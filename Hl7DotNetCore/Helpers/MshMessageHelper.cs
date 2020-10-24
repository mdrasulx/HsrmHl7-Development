using Hl7.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using static Hl7.Helpers.Helper;

namespace Hl7.Helpers
{
    //consider whether we want an MSH segment class instead to embody most of this functionality.
    public static class MshMessageHelper
    {
        //note that the indexes are 0 based, and that the name does not count as a field, so delimiter is field 0, and rest go from there
        public static string? GetDelimiters(this Message message) => message?.GetMshSegment()?.GetDelimiters();
        public static string? GetSendingApplication(this Message message) => message?.GetMshSegment()?.GetSendingApplication();
        public static string? GetSendingFacility(this Message message) => message?.GetMshSegment()?.GetSendingFacility();
        public static string? GetReceivingApplication(this Message message) => message?.GetMshSegment()?.GetReceivingApplication();
        public static string? GetReceivingFacility(this Message message) => message?.GetMshSegment()?.GetReceivingFacility();
        public static DateTime? GetMessageDateTime(this Message message) => message?.GetMshSegment()?.GetMessageDateTime();
        public static string? GetSecurity(this Message message) => message?.GetMshSegment()?.GetSecurity();
        public static string? GetMessageType(this Message message) => message?.GetMshSegment()?.GetMessageType();
        public static Result<string?> TryGetMessageType(this Message message) => GetResult(message, MshSegmentHelper.TryGetMessageType);
        public static string? GetMessageControlId(this Message message) => message?.GetMshSegment()?.GetMessageControlId();
        public static Result<string?> TryGetMessageControlId(this Message message) => GetResult(message, MshSegmentHelper.TryGetMessageControlId);
        public static string? GetProcessingId(this Message message) => message?.GetMshSegment()?.GetProcessingId();
        public static Result<string?> TryGetProcessingId(this Message message) => GetResult(message, MshSegmentHelper.TryGetProcessingId);
        public static string? GetVersionId(this Message message) => message?.GetMshSegment()?.GetVersionId();
        public static Result<string?> TryGetVersionId(this Message message) => GetResult(message, MshSegmentHelper.TryGetVersionId);
        public static string? GetSequenceNumber(this Message message) => message?.GetMshSegment()?.GetSequenceNumber();
        public static string? GetContinuationPointer(this Message message) => message?.GetMshSegment()?.GetContinuationPointer();
        public static string? GetAcceptAckType(this Message message) => message?.GetMshSegment()?.GetAcceptAckType();
        public static string? GetAppAckType(this Message message) => message?.GetMshSegment()?.GetAppAckType();
        public static string? GetCountryCode(this Message message) => message?.GetMshSegment()?.GetCountryCode();
        public static string? GetCharacterSet(this Message message) => message?.GetMshSegment()?.GetCharacterSet();
        public static string? GetPrincipalLanguage(this Message message) => message?.GetMshSegment()?.GetPrincipalLanguage();

        private static Result<string?> GetResult(this Message message, Func<BaseSegment, Result<string?>> Getter)
        {
            if (message == null)
                return ErrorReturn<string?>("Tried to get property on a null message");
            BaseSegment? mshSegment = message.GetMshSegment();
            if (mshSegment == null)
                return ErrorReturn<string?>("Tried to get MSH property, but no MSH segment present in message");
            return Getter.Invoke(mshSegment);
        }

        internal static BaseSegment CreateResponseMshSegment(this Message message, string messageType = MessageHelper.ACK)
        {
            List<string?> newValues = new List<string?>(){//SegmentHelper.MSH, message.GetDelimiters()--these first two fields are handled in parse fields below
                message.GetReceivingApplication(), message.GetReceivingFacility(),message.GetSendingApplication(), message.GetSendingFacility(),//flip applications and facilities
                message.GetMessageDateTime()?.FormatDate(message.Encoding), string.Empty /*security*/, messageType, message.GetMessageControlId(), message.GetProcessingId(),
                message.GetVersionId(),string.Empty/*sequence number*/, string.Empty/*continuation pointer*/, string.Empty/*acceptacktype*/, string.Empty /*appacktype*/,
                message.GetCountryCode(),message.GetCharacterSet(),message.GetPrincipalLanguage()};
            return SegmentHelper.CreateSegment(message.Encoding, SegmentHelper.MSH, FieldHelper.ParseFields(newValues, message.Encoding, true, false, false));
        }

        internal static BaseSegment? GetMshSegment(this Message message) =>
            message.GetSegmentCollections().FirstOrDefault(s => s.SegmentName == SegmentHelper.MSH)?.Segments.FirstOrDefault();

        internal static Result<string> TryGetMessageFormat(this Message message)
        {
            if (message == null)
                return ErrorReturn<string>("Attempted to get MSH segment, but message is null");
            string? messageStructure = GetMessageType(message);
            if (string.IsNullOrWhiteSpace(messageStructure))
                return ErrorReturn<string>("MSH 9.3 not found");
            List<string> structure = EncodingHelper.Split(messageStructure, message.Encoding.ComponentDelimiter);
            if (structure.Count >= 3)
                messageStructure = structure[2];
            else if (structure[0] == MessageHelper.ACK)
                messageStructure = structure[0];
            else if (structure.Count == 2)
                messageStructure = structure[0] + "_" + structure[1];
            else
                return ErrorReturn<string>("Message Type & Trigger Event value not found in message");
            if (string.IsNullOrWhiteSpace(messageStructure))
                return ErrorReturn<string>("MSH 9.3 not properly formatted");
            return new Result<string>(messageStructure!, null);
        }
    }
}
