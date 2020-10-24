using Hl7.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Hl7.Helpers.Helper;
using static Hl7.Helpers.SegmentHelper;

namespace Hl7.Helpers
{
    public static partial class MessageHelper
    {
        /// <summary>
        /// Parse the HL7 message in text format, throws HL7Exception if error occurs
        /// </summary>
        /// <returns>boolean</returns>
        public static Message ParseMessage(string message, Hl7Encoding? encoding = null, bool validateOutMatchesIn = false, string[]? newLineStrings = null, string? dateTimeFormat = null)
        {
            if (!TryParseMessage(message, encoding, validateOutMatchesIn, newLineStrings, dateTimeFormat).DecomposeResult(out var result, out string? error))
                throw new Hl7ParsingErrorException(error ?? "Unknown error parsing message");
            return result;
        }

        public static Result<Message> TryParseMessage(string? message, Hl7Encoding? encoding = null, bool validateOutMatchesIn = false, string[]? newLineStrings = null, string? dateTimeFormat = null)
        {
            if (string.IsNullOrWhiteSpace(message))
                return ErrorReturn<Message>("Attempted to parse null or empty message");
            if (!GetEncodingAndSegmentsFromMessage(message.Trim(), encoding, newLineStrings, dateTimeFormat).DecomposeResult(out var encAndSeg, out string? error))
                return ErrorReturn<Message>(error ?? "Unknown error parsing out segments and encoding");
            Message returnValue = new Message(ParseSegments(encAndSeg.Encoding, encAndSeg.AllSegments), encAndSeg.Encoding);
            if (!ValidateMessageProperties(returnValue).DecomposeResult(out error))
                return ErrorReturn<Message>(error ?? "Unknown error attempting to validate message properties");
            //Note that there are scenarios that can cause the comparison below to fail:
            //mixed line endings -- Consider coercing the message to standardize the new lines to the preferred newline in encoding
            //Unencoded characters that should be escaped - this could be complex to solve, consider encoding the input?
            //new lines other than at the start or end of message will be ignored when serialized - consider preserving empty segments? Or removing on source before comparing?
            //trailing white spaces on segments could cause mismatch -- trim that off the received message?
            if (validateOutMatchesIn && !message.Trim().Equals(SerializeMessage(returnValue).Trim()))
                return ErrorReturn<Message>("Comparison of serialized message and original message showed a difference");//TODO show the difference?
            return new Result<Message>(returnValue, null);
        }

        private static Result<(Hl7Encoding Encoding, List<string> AllSegments)> GetEncodingAndSegmentsFromMessage(string message, Hl7Encoding? encoding = null, string[]? newLineStrings = null, string? dateTimeFormat = null)
        {
            if (!ValidateMessage(message).DecomposeResult(out string? error))
                return ErrorReturn<(Hl7Encoding, List<string>)>(error ?? "unknown error getting encoding and segments");
            IEnumerable<string>? segmentDelimiters = encoding?.SegmentDelimiters ?? newLineStrings;
            if (segmentDelimiters == null && !FindSegmentDelimiters(message).DecomposeResult(out segmentDelimiters, out _))
                segmentDelimiters = EncodingHelper.LineDelimiters;//note if there are no newlines we will process the whole message as one segment, should be fine, default the line endings for responses
            List<string> allSegments = EncodingHelper.SplitLines(message, segmentDelimiters);
            if (encoding == null && !TryFindMessageEncoding(allSegments.FirstOrDefault(), segmentDelimiters, dateTimeFormat).DecomposeResult(out encoding, out error))
                return ErrorReturn<(Hl7Encoding, List<string>)>(error ?? "Unknown error finding encoding");
            if (!ValidateSegments(allSegments, encoding).DecomposeResult(out error))
                return ErrorReturn<(Hl7Encoding, List<string>)>(error ?? "Unknown Error validating segments");
            return new Result<(Hl7Encoding, List<string>)>((encoding, allSegments));
        }

        private static Result<IEnumerable<string>> FindSegmentDelimiters(string message)
        {
            Dictionary<string, int> counts = EncodingHelper.LineDelimiters.ToDictionary(k => k, c => 0);
            Parallel.ForEach(EncodingHelper.LineDelimiters, s => counts[s] += (message.Length - message.Replace(s, String.Empty).Length) / s.Length);
            var result = counts.OrderByDescending(s => s.Value).ThenByDescending(s => s.Key.Length);//highest count first, if tie, take the longer name (eg /r/n, /r and /n will have an equal count when /r/n is used)
            return new Result<IEnumerable<string>>(result.Select(v => v.Key));
        }

        /// <summary>
        /// Serialize the message in text format
        /// </summary>
        /// <returns>string with HL7 message</returns>
        public static string SerializeMessage(this Message message) => message.GetRawValue();//Remove trailing new lines with a trim?

        /// <summary>
        /// check if specified field has components
        /// </summary>
        /// <param name="valueFormat">Field/Component position in format SEGMENTNAME.FieldIndex.ComponentIndex.SubComponentIndex example PID.5.2</param>
        /// <returns>boolean</returns>
        public static Result<bool> IsComponentized(this Message message, string valueFormat) => GetBoolValue(message, valueFormat, (BaseField f) => f is ComponentField);

        /// <summary>
        /// check if specified fields has repeatitions
        /// </summary>
        /// 
        /// <param name="valueFormat">Field/Component position in format SEGMENTNAME.FieldIndex.ComponentIndex.SubComponentIndex example PID.5.2</param>
        /// <returns>boolean</returns>
        public static Result<bool> HasRepetitions(this Message message, string valueFormat) => GetBoolValue(message, valueFormat, (BaseField f) => f is RepetitionField);

        /// <summary>
        /// check if specified component has sub components
        /// </summary>
        /// <param name="valueFormat">Field/Component position in format SEGMENTNAME.FieldIndex.ComponentIndex.SubComponentIndex example PID.5.2</param>
        /// <returns>boolean</returns>
        public static Result<bool> IsSubComponentized(this Message message, string valueFormat) => GetBoolValue(message, valueFormat, (BaseComponent c) => c is ComplexComponent);

        private static Result<bool> GetBoolValue<T>(this Message message, string valueFormat, Func<T,bool> func) where T : MessageElement
        {
            if (!TryGetMessageElement(message, valueFormat).DecomposeResult(out var result, out string? error))
                return new Result<bool>(error ?? "unknown error getting bool property value");
            if (!(result is T entity))
                return new Result<bool>($"did not find entity when determining bool value with format: {valueFormat}");
            return new Result<bool>(func(entity));
        }

        private static ValidationResult ValidateMessageProperties(Message message)
        {
            if (!message.TryGetMessageFormat().DecomposeResult(out string? error))
                return ErrorReturn(error ?? "Unknown error getting message format");
            if (!message.TryGetMessageControlId().DecomposeResult(out error))
                return ErrorReturn($"MSH.10 - Message Control ID not found {error ?? string.Empty}");
            if (!message.TryGetProcessingId().DecomposeResult(out error))
                return ErrorReturn($"MSH.11 - Processing ID not found {error ?? string.Empty}");
            return new ValidationResult();
        }

        private static ValidationResult ValidateMessage(string? message)
        {
            if (string.IsNullOrEmpty(message))
                return ErrorReturn("No Message Found");
            if (message.Length < 20)//check message length - MSH+Delimeters+12Fields in MSH
                return ErrorReturn($"Message Length too short: {message.Length} chars.");
            if (!message.StartsWith(SegmentHelper.MSH))//check if message starts with header segment
                return ErrorReturn("MSH segment not found at the beginning of the message");
            return new ValidationResult();
        }

        private static ValidationResult ValidateSegments(List<string> segments, Hl7Encoding encoding)
        {
            if (segments == null || segments.Count == 0 || segments[0].Count(f => f == encoding.FieldDelimiter) < 11)// Count field separators, MSH.12 is required so there should be at least 11 field separators in MSH
                return ErrorReturn("first segment (MSH) doesn't contain all the required fields");
            foreach (string strSegment in segments.Where(s => !string.IsNullOrWhiteSpace(s)))
            {
                if (!ValidSegmentName.IsMatch(SegmentHelper.GetSegmentName(strSegment)))
                    return ErrorReturn($"Invalid segment name found: {strSegment}");
                if (encoding.FieldDelimiter != strSegment[3])
                    return ErrorReturn($"Invalid segment found: {strSegment}");
            }
            return new ValidationResult();
        }

        private static Result<Hl7Encoding> TryFindMessageEncoding(string mshSegment, IEnumerable<string>? lineSeparators = null, string? dateTimeFormat = null)
        {
            if(mshSegment.Substring(0, 3) != MSH)
                return ErrorReturn<Hl7Encoding>("failed to deduce message encoding, first line was not MSH");
            if(mshSegment.Length < 8)
                return ErrorReturn<Hl7Encoding>("failed to deduce message encoding, first line was not long enough to contain delimiters");
            return new Result<Hl7Encoding>(new Hl7Encoding(mshSegment.Substring(3, 5), lineSeparators, dateTimeFormat), null);
        }
    }
}
