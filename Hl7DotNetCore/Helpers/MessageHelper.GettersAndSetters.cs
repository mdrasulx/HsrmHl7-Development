using Hl7.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static Hl7.Helpers.FieldHelper;
using static Hl7.Helpers.Helper;
using static Hl7.Helpers.SegmentHelper;

namespace Hl7.Helpers
{
    public static partial class MessageHelper
    {
        private static readonly Regex ValidSegmentName = new Regex("[A-Z][A-Z][A-Z1-9]");

        /// <summary>
        /// Get the Value of specific Field/Component/SubCpomponent, throws error if field/component index is not valid
        /// </summary>
        /// <param name="target">Field/Component position in format SEGMENTNAME.FieldIndex.ComponentIndex.SubComponentIndex example PID.5.2</param>
        /// <returns>Value of specified field/component/subcomponent</returns>
        public static string? GetValue(this Message message, string target)
        {
            if (!TryGetValue(message, target).DecomposeResult(out string? result, out string? error))
                throw new Hl7Exception(error ?? "unknown error in GetValue");
            return result;
        }

        public static Result<string?> TryGetValue(this Message message, string target, bool errorOnNotFound = false)
        {
            var result = TryGetMessageElement(message, target);
            if (errorOnNotFound && !result)
                return ErrorReturn<string?>(result.Error ?? "unknown error trying to get string value");
            string? returnValue = result ? result.Value.GetRawValue() : null; // consider string.empty instead of null for reverse compatability.
            return new Result<string?>(returnValue, null);
        }

        public static Result<MessageElement> TryGetMessageElement(this Message message, string valueFormat)
        {
            if (!TryParseIndexesFromValueFormat(valueFormat).DecomposeResult(out var elementIndex, out string? error))
                return ErrorReturn<MessageElement>(error ?? "unknown error parsing indexes");
            return TryGetSpecifiedMessageElement(message, elementIndex);
        }

        public static Result<MessageElement> TryGetSpecifiedMessageElement(this Message message, ElementIndex elementIndex)
        {
            var valResult = ValidateNameAndIndexes(elementIndex);
            if (!valResult)
                return ErrorReturn<MessageElement>(valResult.Error ?? $"Request format is not valid: segmentName: {elementIndex}");
            var segmentAndList = TryGetSegmentAndSegmentCollectionAndIndex(message, elementIndex.SegmentIndex);
            if (!segmentAndList)
                return ErrorReturn<MessageElement>(segmentAndList.Error ?? "Did not find segment and index");
            if (elementIndex.FieldIndex?.HasValue == true)
            {
                if (segmentAndList.Value.Segment == null)
                    return ErrorReturn<MessageElement>($"failed to find field {elementIndex} as the segment was not found");
                return TryGetField(segmentAndList.Value.Segment, elementIndex);
            }
            else
            {
                if (segmentAndList.Value.Segment == null)
                    return ErrorReturn<MessageElement>($"Did not find a segment for {elementIndex}");
                return new Result<MessageElement>(segmentAndList.Value.Segment, null);
            }
        }

        public static Result<Message> TrySetValues(this Message message, IEnumerable<(string Target,string Value)> targetsAndValues)
        {
            Message? returnValue = null;
            List<string> errors = new List<string>();
            foreach(var (Target, Value) in targetsAndValues)
            {
                if (!TrySetValue(returnValue ?? message, Target, Value).DecomposeResult(out returnValue, out string? error))
                    errors.Add(error ?? "unknown error setting value");
            }
            if (errors.Any())
                return ErrorReturn<Message>(string.Concat("Error(s) encountered when trying to set values:", string.Join(Environment.NewLine, errors)));
            return new Result<Message>(returnValue ?? message);
        }

        public static Result<Message> TrySetValue(this Message message, string updateTarget, string newValue)
        {
            if (!TryParseIndexesFromValueFormat(updateTarget).DecomposeResult(out var result, out string? error))
                return ErrorReturn<Message>(error ?? "unknown error parsing valueFormat in TrySetValue");
            return TrySetSpecifiedValue(message, newValue, result);
        }

        public static Result<Message> TrySetSpecifiedValue(this Message message, string newValue, BaseSegmentIndex segmentIndex, BaseSpecificElementIndex? fieldIndex = null, BaseSpecificElementIndex? subFieldIndex = null, BaseSpecificElementIndex? componentIndex = null, int? subComponentIndex = null)
        {
            return TrySetSpecifiedValue(message, newValue, new ElementIndex(segmentIndex, fieldIndex, subFieldIndex, componentIndex, subComponentIndex));
        }

        public static Result<Message> TrySetSpecifiedValue(this Message message, string newValue, ElementIndex elementIndex)
        {
            var valResult = ValidateNameAndIndexes(elementIndex);
            if (!valResult)
                return ErrorReturn<Message>(valResult.Error ?? $"Request format is not valid: {elementIndex}");
            if (!TryGetSegmentAndSegmentCollectionAndIndex(message, elementIndex!.SegmentIndex).DecomposeResult(out var segAndList, out string? error))
                return ErrorReturn<Message>(error ?? "unknown error getting segment and segment list");
            if (!TrySetSegment(segAndList.Segment, elementIndex, newValue).DecomposeResult(out var result, out error))
                return ErrorReturn<Message>(error ?? "unknown error setting segment");
            if (segAndList.Segment == null)
                return ErrorReturn<Message>("Did not find Segment");
            var segmentList = segAndList.SegmentCollection.Segments.ToList();
            if (segmentList == null)
                return ErrorReturn<Message>("Did not find SegmentList");
            segmentList[segmentList.IndexOf(segAndList.Segment)] = result;
            var segmentCollections = message.GetSegmentCollections();
            segmentCollections[segAndList.index] = new SegmentCollection(result.Name, segAndList.SegmentCollection.SegmentOrdinal, segmentList);
            return new Result<Message>(new Message(segmentCollections, message.Encoding), null);
        }

        private static ValidationResult ValidateNameAndIndexes(ElementIndex elementIndex)
        {
            if (elementIndex == null)
                return new ValidationResult("ElementIndex was null");
            if (elementIndex.SegmentIndex is BaseSegmentNamedIndex bsni && !ValidSegmentName.IsMatch(bsni.SegmentName))
                return new ValidationResult($"Invalid Segment Name {bsni.SegmentName}");
            if (!elementIndex.GetIntIndexes().All(i => ValidateIndex(i)))//valid indexes
                return new ValidationResult($"One or more indexes invalid {elementIndex}");
            if (elementIndex.SubFieldIndex?.HasValue == true && !elementIndex.FieldIndex?.HasValue == true)// no subfields without fields
                return new ValidationResult($"Specified a subfield without a field {elementIndex}");
            if (elementIndex.ComponentIndex?.HasValue == true && !elementIndex.FieldIndex?.HasValue == true)// no components without fields
                return new ValidationResult($"Specified a component without a field {elementIndex}");
            if (elementIndex.SubComponentIndex.HasValue && !elementIndex.ComponentIndex?.HasValue == true)// no sub components without components
                return new ValidationResult($"Specified a subcomponent without a component {elementIndex}");
            return new ValidationResult();
        }

        private static bool ValidateIndex(int? index) => index.HasValue ? index > 0 && index < 1000 : true;
    }
}
