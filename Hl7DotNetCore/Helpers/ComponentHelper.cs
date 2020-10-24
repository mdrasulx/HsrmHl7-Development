using Hl7.Entities;
using System.Collections.Generic;
using System.Linq;
using static Hl7.Helpers.Helper;

namespace Hl7.Helpers
{
    public static class ComponentHelper
    {
        internal static BaseComponent CreateComponent(Hl7Encoding encoding, string value)
        {
            if (value.Contains(encoding.SubComponentDelimiter.ToString()))
                return new ComplexComponent(EncodingHelper.Split(value, encoding.SubComponentDelimiter).ConvertAll(s => new SubComponent(encoding.Decode(s), encoding)), encoding);
            return new SimpleComponent(value, encoding);
        }

        internal static Result<BaseComponent> TrySetComponent(BaseComponent? component, string newValue, ElementIndex elementIndex)
        {
            if (elementIndex.SubComponentIndex.HasValue)
            {
                if(component == null || !(component is ComplexComponent complexComponent))
                    return ErrorReturn<BaseComponent>($"failed to find subComponent on message with {elementIndex}, as the component was not found");
                List<BaseSubComponent> subComponents = complexComponent?.SubComponents != null ? complexComponent.SubComponents.ToList() : new List<BaseSubComponent>();
                BaseSubComponent? subComponent = subComponents?.ElementAtOrDefault(elementIndex.SubComponentIndex.Value - 1);
                if (subComponent == null)
                    return ErrorReturn<BaseComponent>($"failed to find subComponent {elementIndex}");
                subComponents![elementIndex.SubComponentIndex.Value - 1] = new SubComponent(subComponent, newValue);//set the right type of subcomponent if we ever add others.
                return new Result<BaseComponent>(new ComplexComponent(complexComponent!, subComponents));
            }
            else// set the component, probably a simple component, but override with value no matter what
                return new Result<BaseComponent>(CreateComponent(component!.Encoding, newValue));
        }

        internal static Result<MessageElement> TryGetComponent(BaseField field, ElementIndex elementIndex)
        {
            if (!(field is ComponentField componentField))
                return ErrorReturn<MessageElement>("Unable to get component: field is not a component field");
            if(elementIndex?.ComponentIndex == null)
                return ErrorReturn<MessageElement>("Unable to get component: component index or element index are null");
            if (!TryGetComponent(componentField, elementIndex.ComponentIndex, out _).DecomposeResult(out BaseComponent component, out string? error))
                return ErrorReturn<MessageElement>(error ?? "unknown error when attempting to get the component");
            if (component == null)
                return ErrorReturn<MessageElement>($"failed to find subComponent on message with {elementIndex}, as the component was not found");
            if (elementIndex.SubComponentIndex.HasValue)
            {
                BaseSubComponent? subComponent = (component as ComplexComponent)?.SubComponents?.ElementAtOrDefault(elementIndex.SubComponentIndex.Value - 1);
                if (subComponent == null)
                    return ErrorReturn<MessageElement>($"failed to find subComponent on message with {elementIndex}");
                return new Result<MessageElement>(subComponent!);
            }
            return new Result<MessageElement>(component!);
        }

        internal static Result<BaseComponent> TryGetComponent(ComponentField field, BaseSpecificElementIndex index, out int foundIndex)
        {
            foundIndex = 0;
            if (index is SpecificElementIndexLookAhead lookAhead)
            {
                //look through all elements, find their propery if lookadheadindex and see if it equals lookaheadvalue, if so return index from there
                if (lookAhead.LookAheadIndex.HasValue)
                {
                    if (string.IsNullOrEmpty(lookAhead.LookAheadValue))
                        return ErrorReturn<BaseComponent>("attempted to look for a lookahead without a lookahead value");
                    else
                    {
                        foundIndex = field.Components.Select(c => c as ComplexComponent).TakeWhile(c => c?.SubComponents[lookAhead.LookAheadIndex.Value].GetRawValue() == lookAhead.LookAheadValue).Count() + 1;
                        return new Result<BaseComponent>(field.Components.ElementAtOrDefault(foundIndex - 1));
                    }
                }
                else //no index, just find the item with the value
                {
                    foundIndex = field.Components.TakeWhile(f => f.GetRawValue() != lookAhead.LookAheadValue).Count() + 1;
                    return new Result<BaseComponent>(field.Components.ElementAtOrDefault(foundIndex - 1));
                }
            }
            else if (index is SpecificElementIndex spi)
            {
                foundIndex = spi.Value;
                return new Result<BaseComponent>(field.Components.ElementAtOrDefault(foundIndex - 1));
            }
            else
            {
                return ErrorReturn<BaseComponent>($"Attempted to get component, but index type {index?.GetType()} was unknown");
            }
        }
    }
}
