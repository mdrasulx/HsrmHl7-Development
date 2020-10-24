using Hl7.Entities;
using System.Collections.Generic;
using System.Linq;
using static Hl7.Helpers.ComponentHelper;
using static Hl7.Helpers.Helper;
using static Hl7.Helpers.SegmentHelper;

namespace Hl7.Helpers
{
    public static class FieldHelper
    {
        internal static List<BaseField> ParseFields(string value, Hl7Encoding encoding)
        {
            string name = GetSegmentName(value);
            IEnumerable<string> allFields = EncodingHelper.Split(value, encoding.FieldDelimiter);
            return ParseFields(allFields, encoding, name == MSH);
        }

        internal static List<BaseField> ParseFields(IEnumerable<string?> allFields, Hl7Encoding encoding, bool includeEncoding = false, bool fieldsIncludesName = true, bool skipEncodingField = true)
        {
            List<BaseField> fields = new List<BaseField>();
            if (fieldsIncludesName && allFields.Count() > 1)
                allFields = allFields.Skip(1);///skip the name
            if (includeEncoding)
            {//handle the special delimiters field for MSH
                fields.Add(new DelimitersField(encoding));
                if(skipEncodingField)
                    allFields = allFields.Skip(1);
            }
            fields.AddRange(allFields.Where(f => f != null).Select(f => CreateField(encoding, f!)));
            return fields;
        }

        internal static BaseField CreateField(Hl7Encoding encoding, string value)
        {
            if (value.Contains(encoding.RepeatDelimiter))
                return new RepetitionField(EncodingHelper.Split(value, encoding.RepeatDelimiter).ConvertAll(f => CreateField(encoding, f)), encoding);
            if (value.Contains(encoding.ComponentDelimiter))
                return new ComponentField(EncodingHelper.Split(value, encoding.ComponentDelimiter).ConvertAll(c => CreateComponent(encoding, c)), encoding);
            return new ValueField(EncodingHelper.Decode(encoding, value), encoding);
        }

        internal static Result<BaseField> TrySetField(BaseField field, ElementIndex elementIndex, string newValue, bool navigateSubField = true)
        {
            BaseField? returnValue = null;
            bool subFieldWasSet = false;
            if (navigateSubField && elementIndex.SubFieldIndex?.HasValue == true)
            {
                if (!(field is RepetitionField repField))
                    return ErrorReturn<BaseField>($"failed to find RepetitionField on message with {elementIndex}");
                List<BaseField>? fields = repField.Repetitions?.ToList();
                if (fields == null)
                    return ErrorReturn<BaseField>($"Attempted to set field, but there was no field collection found at {elementIndex}");
                if (!TryGetField(fields!, elementIndex.SubFieldIndex, out int foundIndex).DecomposeResult(out BaseField innerField, out string? error))
                    return ErrorReturn<BaseField>(error ?? "Unknown error getting field");
                if (!TrySetField(innerField, elementIndex, newValue, false).DecomposeResult(out var result, out error))
                    return ErrorReturn<BaseField>(error ?? "Unknown error setting field");
                fields[foundIndex - 1] = result;
                returnValue = new RepetitionField(repField, fields);
                subFieldWasSet = true;
            }
            if (!subFieldWasSet)//prevents trying to do this twice in recursion scenarios for RepetitionFields
            {
                if (elementIndex.ComponentIndex?.HasValue == true)
                {
                    if (!(field is ComponentField componentField))
                        return ErrorReturn<BaseField>($"failed to find component({elementIndex.ComponentIndex}) on message with {elementIndex}, as the field was not found");
                    if (!TryGetComponent(componentField, elementIndex.ComponentIndex, out int fieldIndex).DecomposeResult(out BaseComponent component, out string? error))
                        return ErrorReturn <BaseField>(error ?? "unknown error getting component");
                    if (!TrySetComponent(component, newValue, elementIndex).DecomposeResult(out var result, out error))
                        return ErrorReturn<BaseField>(error ?? "Unknown error setting component");
                    List<BaseComponent> components = componentField.Components.ToList();
                    components[fieldIndex - 1] = result;
                    returnValue = new ComponentField(componentField, components);
                }
                else//probably value field - but override the whole value anyway
                    returnValue = CreateField(field.Encoding, newValue);
            }
            if (returnValue == null)
                return ErrorReturn<BaseField>($"attempted to set field {elementIndex}, but failed to make any updates");
            return new Result<BaseField>(returnValue!, null);
        }

        internal static Result<MessageElement> TryGetField(BaseSegment segment, ElementIndex elementIndex)
        {
            if (!TryGetField(segment.FieldList, elementIndex.FieldIndex, out _).DecomposeResult(out BaseField field, out string? error))
                ErrorReturn(error ?? "unknown error trying to get field");
            if (elementIndex.SubFieldIndex?.HasValue == true)
            {
                if (!(field is RepetitionField rf))
                    return ErrorReturn <MessageElement>($"failed to find RepititionField({elementIndex.SubFieldIndex}) on message with {elementIndex}, as the field was not a repetition field");
                if (!TryGetField(rf.Repetitions, elementIndex.SubFieldIndex, out _).DecomposeResult(out field, out error))
                    ErrorReturn(error ?? "unknown error trying to get subfield");
                if (field == null)
                    return ErrorReturn<MessageElement>($"failed to find RepititionField({elementIndex.SubFieldIndex}) on message with {elementIndex}");
            }
            if (elementIndex.ComponentIndex?.HasValue == true)
            {
                if (field == null)
                    return ErrorReturn<MessageElement>($"failed to find component({elementIndex.ComponentIndex}) on message with {elementIndex}, as the field was not found");
                return TryGetComponent(field, elementIndex);
            }
            else
                return new Result<MessageElement>(field, null);
            
        }

        internal static Result<BaseField> TryGetField(IEnumerable<BaseField> fieldCollection, BaseSpecificElementIndex? index, out int foundIndex)
        {
            foundIndex = 1;
            if (fieldCollection == null)
                return ErrorReturn<BaseField>("Attempted to get a field from a null field collection");
            if (index == null)
                return ErrorReturn<BaseField>("Attempted to get a field with a null index to find it");
            if (index is SpecificElementIndexLookAhead lookAhead)
            {
                //look through all elements, find their propery if lookadheadindex and see if it equals lookaheadvalue, if so return index from there
                if (lookAhead.LookAheadIndex.HasValue)
                {
                    if (string.IsNullOrEmpty(lookAhead.LookAheadValue))
                        return ErrorReturn<BaseField>("attempted to look for a lookahead without a lookahead value");
                    else
                    {
                        for (; foundIndex < fieldCollection.Count(); foundIndex++)
                        {
                            var element = fieldCollection.ElementAt(foundIndex - 1);
                            if (element is RepetitionField rf)
                            {//FUTURE allow complex sub comparisons -- turtles all the way down
                                if (rf.Repetitions[lookAhead.LookAheadIndex.Value - 1].GetRawValue() == lookAhead.LookAheadValue)
                                    return new Result<BaseField>(rf);
                            }
                            else if (element is ComponentField cf)
                            {//FUTURE allow complex sub comparisons -- turtles all the way down
                                if (cf.Components[lookAhead.LookAheadIndex.Value - 1].GetRawValue() == lookAhead.LookAheadValue)
                                    return new Result<BaseField>(cf);
                            }
                            else if (!(element is DelimitersField || element is ValueField))
                                return ErrorReturn<BaseField>("Attempted to look for a lookahead on a field that was not of the right type");
                        }
                        return ErrorReturn<BaseField>($"Did not find field at {index}");
                    }
                }
                else //no index, just find the item with the value
                {
                    foundIndex = fieldCollection.TakeWhile(f => f.GetRawValue() != lookAhead.LookAheadValue).Count() + 1;
                    return new Result<BaseField>(fieldCollection.ElementAt(foundIndex - 1));
                }
            }
            else if (index is SpecificElementIndex spi)
            {
                foundIndex = spi.Value;
                return new Result<BaseField>(fieldCollection.ElementAtOrDefault(spi.Value - 1));
            }
            else
                return ErrorReturn<BaseField>($"Unknown elementIndex type: {index.GetType()}");
        }
    }
}
