using Hl7.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Hl7.Helpers
{
    public static class Helper
    {
        internal static ValidationResult ErrorReturn(string error) => new ValidationResult(error);

        internal static Result<T> ErrorReturn<T>(string error) => new Result<T>(error);

        internal static Result<ElementIndex> TryParseIndexesFromValueFormat(string format)
        {
            BaseSpecificElementIndex? fieldIndex = null, subFieldIndex = null, componentIndex = null;
            int? subComponentIndex = null;
            List<string> allComponents = EncodingHelper.Split(format, '.');
            if (allComponents.Count > 4 || allComponents.Count == 0)
                return ErrorReturn<ElementIndex>($"Expected 1-4 values in the valueFormat field of ParseIndexesFromValueFormat, expected: SegmentName.FieldIndex.ComponentIndex.SubComponentIndex found {format}.");
            if (!GetSegmentIndex(allComponents[0]).DecomposeResult(out var segmentIndex, out string? error))
                return ErrorReturn<ElementIndex>(error ?? "unknown error parsing segment name");
            if (allComponents.Count > 1)
            {
                string field = allComponents[1];
                if (field.Contains('-'))
                {
                    List<string> fieldParts = EncodingHelper.Split(field, '-');
                    if (fieldParts.Count != 2)
                        return ErrorReturn<ElementIndex>($"Expected exactly 2 ints seperated by a dash in the field position found {field} instead");
                    field = fieldParts[0];
                    if (!GetElementIndex(fieldParts[1]).DecomposeResult(out subFieldIndex, out error))
                        return ErrorReturn<ElementIndex>(error ?? "unknown error parsing indexes for subfield");
                }
                if (!GetElementIndex(field).DecomposeResult(out fieldIndex, out error))
                    return ErrorReturn<ElementIndex>(error ?? "unknown error parsing indexes for field");
                if (allComponents.Count > 2)
                {
                    if (!GetElementIndex(allComponents[2]).DecomposeResult(out componentIndex, out error))
                        return ErrorReturn<ElementIndex>(error ?? "unknown error parsing indexes for component");
                    subComponentIndex = ParseIntFromStringList(allComponents, 3);
                }
            }
            return new Result<ElementIndex>(new ElementIndex(segmentIndex, fieldIndex, subFieldIndex, componentIndex, subComponentIndex), null);
        }

        private static Result<BaseSpecificElementIndex> GetElementIndex(string input)
        {
            if (input.Contains('['))
            {//using lookahead
                string[] splits = input.Split('[', ']');
                if (splits.Length != 3 || splits[0] != string.Empty || splits[2] != string.Empty)
                    return ErrorReturn<BaseSpecificElementIndex>("Attempted to parse lookahead value, but did not find correct format");
                splits = splits[1].Split(':');
                if (splits.Length != 1 && splits.Length != 2)
                    return ErrorReturn<BaseSpecificElementIndex>("Attempted to parse lookahead value, but did not find correct format");
                int? index = null;
                if (!string.IsNullOrWhiteSpace(splits[0]))
                {
                    if (!int.TryParse(splits[0], out int indexVal))
                        return ErrorReturn<BaseSpecificElementIndex>($"Failed to parse element as integer {splits[0]}, from input {input}");
                    else
                        index = indexVal;
                }
                return new Result<BaseSpecificElementIndex>(new SpecificElementIndexLookAhead(splits[1], index));
            }
            else
            {
                if (!int.TryParse(input, out int indexVal))
                    return ErrorReturn<BaseSpecificElementIndex>($"Failed to parse element as integer from input {input}");
                return new Result<BaseSpecificElementIndex>(new SpecificElementIndex(indexVal));
            }
        }

        internal static Result<BaseSegmentIndex> GetSegmentIndex(string nameIn)
        {
            string? segIndex = null, colIndexStr = null;
            if (nameIn.Contains('-'))
            {
                List<string> segmentParts = EncodingHelper.Split(nameIn, '-');
                if (segmentParts.Count != 2)
                    return ErrorReturn<BaseSegmentIndex>($"Expected only one dash in index similiar to x-y or x:y-z in the segmentName position found {nameIn} instead");
                segIndex = segmentParts[1];
            }
            List<string> multiSegmentParts = nameIn.Split(':', '-').ToList();
            if (nameIn.Contains(':'))
            {
                if (nameIn.Contains('-') && multiSegmentParts.Count != 3)
                    return ErrorReturn<BaseSegmentIndex>($"Expected exactly 3 values in the format x:y-z in the segementname position, found {nameIn} instead");
                if (!nameIn.Contains('-') && multiSegmentParts.Count != 2)
                    return ErrorReturn<BaseSegmentIndex>($"Expected exactly 2 values in the format x:y in the segementname position, found {nameIn} instead");
                colIndexStr = multiSegmentParts[1];
            }
            int? colIndex = null;
            if (!string.IsNullOrWhiteSpace(colIndexStr))
            {
                if (int.TryParse(colIndexStr, out int colIndexInt))
                    colIndex = colIndexInt;
                else
                    return ErrorReturn<BaseSegmentIndex>($"Could not parse segment collection index as int value {colIndexStr} from {nameIn}");
            }
            if(int.TryParse(multiSegmentParts[0], out int segIntIndex))
            {
                if (segIndex != null)
                    return ErrorReturn<BaseSegmentIndex>($"attempted to use a segment indexer ({segIndex}), with a segment ordinal ({segIntIndex}), you can't use a segment ordinal with a segment index");
                if (colIndexStr != null)
                    return ErrorReturn<BaseSegmentIndex>($"attempted to use a multisegment ordinal ({segIndex}), with a segment ordinal ({segIntIndex}), you can't use a multisegment ordinal with a segment index");
                return new Result<BaseSegmentIndex>(new SegmentOrdinalIndex(segIntIndex));
            }
            if (segIndex != null)
            {
                if (int.TryParse(segIndex, out int index))
                    return new Result<BaseSegmentIndex>(new SegmentNameAndOrdinalIndex(multiSegmentParts[0], index, colIndex));
                else
                    return new Result<BaseSegmentIndex>(new SegmentNameAndIdIndex(multiSegmentParts[0], segIndex, colIndex));
            }
            return new Result<BaseSegmentIndex>(new SegmentNamedIndex(multiSegmentParts[0], colIndex));
        }

        internal static int? ParseIntFromStringList(List<string> strings, int index) => ParseIntNullable(strings?.ElementAtOrDefault(index));

        internal static int? ParseIntNullable(string? str) => int.TryParse(str, out int parsedInt) ? parsedInt : (int?)null;

        public static bool DecomposeResult<T>(this Result<T> obj, out T value, out string? error)
        {
            value = obj.Value;
            error = obj.Error;
            return obj.Success;
        }

        public static bool DecomposeResult(this ValidationResult obj, out string? error)
        {
            error = obj.Error;
            return obj.Success;
        }
    }
}
