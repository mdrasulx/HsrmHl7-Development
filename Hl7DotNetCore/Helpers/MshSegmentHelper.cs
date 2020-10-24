using Hl7.Entities;
using System;
using System.Globalization;
using System.Linq;
using static Hl7.Helpers.Helper;

namespace Hl7.Helpers
{
    //consider whether we want an MSH segment class instead to embody most of this functionality.
    public static class MshSegmentHelper
    {
        //note that the indexes are 0 based, and that the name does not count as a field, so delimiter is field 0, and rest go from there
        public static string? GetDelimiters(this BaseSegment segment) => segment?.Encoding?.GetAllDelimiters();//consider ensuring the field is delimiters field?
        public static string? GetSendingApplication(this BaseSegment segment) => GetStringProperty(segment, 1, 180);
        public static string? GetSendingFacility(this BaseSegment segment) => GetStringProperty(segment, 2, 180);
        public static string? GetReceivingApplication(this BaseSegment segment) => GetStringProperty(segment, 3, 180);
        public static string? GetReceivingFacility(this BaseSegment segment) => GetStringProperty(segment, 4, 180);
        public static DateTime? GetMessageDateTime(this BaseSegment segment) =>
            GetMshProperty<DateTime>(segment, 5, 26, s => DateTime.ParseExact(s, segment.Encoding.DateTimeFormat, CultureInfo.InvariantCulture));
        public static string? GetSecurity(this BaseSegment segment) => GetStringProperty(segment, 6, 40);
        //ordinal wise (field 7), GetMessageType is below, it requires more code, So it is located there.
        public static string? GetMessageControlId(this BaseSegment segment) => GetStringProperty(segment, 8, 20, true);//required
        public static Result<string?> TryGetMessageControlId(this BaseSegment segment) => TryGetStringProperty(segment, 8, 20, true);//required
        public static string? GetProcessingId(this BaseSegment segment) => GetStringProperty(segment, 9, 3, true);//required
        public static Result<string?> TryGetProcessingId(this BaseSegment segment) => TryGetStringProperty(segment, 9, 3, true);//required
        public static string? GetVersionId(this BaseSegment segment) => GetStringProperty(segment, 10, 8, true);//required
        public static Result<string?> TryGetVersionId(this BaseSegment segment) => TryGetStringProperty(segment, 10, 8, true);//required
        public static string? GetSequenceNumber(this BaseSegment segment) => GetStringProperty(segment, 11, 15);
        public static string? GetContinuationPointer(this BaseSegment segment) => GetStringProperty(segment, 12, 180);
        public static string? GetAcceptAckType(this BaseSegment segment) => GetStringProperty(segment, 13, 2);
        public static string? GetAppAckType(this BaseSegment segment) => GetStringProperty(segment, 14, 2);
        public static string? GetCountryCode(this BaseSegment segment) => GetStringProperty(segment, 15, 2);
        public static string? GetCharacterSet(this BaseSegment segment) => GetStringProperty(segment, 16, 6);
        public static string? GetPrincipalLanguage(this BaseSegment segment) => GetStringProperty(segment, 17, 3);

        public static Result<string?> TryGetMessageType(this BaseSegment segment)
        {
            var field = segment?.FieldList?.ElementAtOrDefault(7);
            if (field == null)
                return ErrorReturn<string?>("Did not find the required segment type field on the MSH segment of the segment");
            return new Result<string?>(value:field.GetRawValue());//this could be a value field, or a componentized field (probably not a delimited field), in any case return the raw value
        }

        public static string? GetMessageType(this BaseSegment segment) => InvokeProperty<string?>(() => TryGetMessageType(segment), "Did not find segment type");

        delegate Result<T> PropInvoker<T>();

        private static T InvokeProperty<T>(PropInvoker<T> toInvoke, string defaultErrorBaseSegment)
        {
            if (!toInvoke.Invoke().DecomposeResult(out T result, out string? error))
                throw new Hl7.Entities.Hl7ParsingErrorException(error ?? defaultErrorBaseSegment);
            return result;
        }

        private static Result<string?> TryGetStringProperty(this BaseSegment segment, int index, int width, bool required = false, bool UseEmptyStringForNoValue = true) =>
            TryGetMshProperty<string?>(segment, index, width, required, s => s, UseEmptyStringForNoValue ? string.Empty : null);

        private static T GetMshProperty<T>(this BaseSegment segment, int index, int width, Func<string, T> parser, bool required = false, T defaultValue = default) =>
            InvokeProperty<T>(() => TryGetMshProperty<T>(segment, index, width, required, parser, defaultValue), $"unknown error parsing MSH value at index {index}");

        private static string? GetStringProperty(this BaseSegment segment, int index, int width, bool required = false) =>
            InvokeProperty(() => TryGetStringProperty(segment, index, width, required), $"unknown error parsing MSH string value at index {index}");

        private static Result<T> TryGetMshProperty<T>(this BaseSegment segment, int index, int width, bool required, Func<string, T> parser, T defaultValue = default) //T is type, index is 0 based
        {
            if (segment == null)
                return ErrorReturn<T>("Attempted to get MSH property on a null segment");
            if (!segment.IsMsh())
                return ErrorReturn<T>("Attempted to get MSH property on a segment that was not MSH");
            var field = segment.FieldList?.ElementAtOrDefault(index);
            if (field == null)
            {
                if (required)
                    return ErrorReturn<T>("requested required field not found");
                return new Result<T>(defaultValue);
            }
            if (!(field is ValueField vf))
                return ErrorReturn<T>("Attempted to get MSH property on a non-value field");
            string workingValue = vf.value;
            workingValue = workingValue.Substring(0, workingValue.Length > width ? width : workingValue.Length);
            if (workingValue.Length <= 0)
            {
                if (required)
                    return ErrorReturn<T>("Attempted to get required MSH property, but the field was empty");
                return new Result<T>(defaultValue);//treat emptry strings differently? 
            }
            return new Result<T>(parser.Invoke(workingValue));
        }

        private static bool IsMsh(this BaseSegment segment) => segment.Name == SegmentHelper.MSH;
    }
}
