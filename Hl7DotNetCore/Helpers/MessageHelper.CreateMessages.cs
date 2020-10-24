using Hl7.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using static Hl7.Helpers.SegmentHelper;
using static Hl7.Helpers.Helper;

namespace Hl7.Helpers
{
    public static partial class MessageHelper
    {
        internal const string ACK = "ACK";
        internal const string MSA = "MSA"; // message acknowledgement segment name
        internal const string AA = "AA";//Ack -- Application Accept - MSA-1 response code
        internal const string AE = "AE";//Nack -- Application Error - MSA-1 response code
        internal const string AR = "AR";//NACK -- Application Reject - MSA-1 response code

        public static Result<Message> CreateBasicMessage(Hl7Encoding encoding, string messageType, string controlId, string processingId, string versionId, string? sendingApplication = null,
            string? sendingFacility = null, string? receivingApplication = null, string? receivingFacility = null, DateTime? messageDateTime = null, string? security = null,
            string? sequenceNumber = null, string? continuationPointer = null, string? acceptAckType = null, string? countryCode = null, string? characterSet = null,
            string? principalLanguage = null)
        {
            if (ValidateRequiredFields(encoding, messageType, controlId, processingId, versionId).DecomposeResult(out string? missingFields))
                return ErrorReturn<Message>($"Missing required argument to create message required fields: {missingFields}");
            List<BaseField> fields = new List<BaseField>()
            {
                new DelimitersField(encoding),//required
                new ValueField(sendingApplication ?? string.Empty,encoding),
                new ValueField(sendingFacility ?? string.Empty,encoding),
                new ValueField(receivingApplication ?? string.Empty,encoding),
                new ValueField(receivingFacility ?? string.Empty,encoding),
                new ValueField(messageDateTime?.FormatDate(encoding) ?? string.Empty,encoding),
                new ValueField(security ?? string.Empty,encoding),
                new ValueField(messageType,encoding),//required
                new ValueField(controlId,encoding),//required
                new ValueField(processingId,encoding),//required
                new ValueField(versionId,encoding),//required
                new ValueField(sequenceNumber ?? string.Empty,encoding),
                new ValueField(continuationPointer ?? string.Empty,encoding),
                new ValueField(acceptAckType ?? string.Empty,encoding),
                new ValueField(countryCode ?? string.Empty,encoding),
                new ValueField(characterSet ?? string.Empty,encoding),
                new ValueField(principalLanguage ?? string.Empty,encoding),
            };
            //remove trailing empties beyond versionid
            for (int i = fields.Count; i > 10 && string.IsNullOrEmpty(fields[i].GetRawValue()); i--)
                fields.RemoveAt(i);
            return CreateMessageFromSegment(new Segment(encoding, SegmentHelper.MSH, fields));
        }

        private static ValidationResult ValidateRequiredFields(Hl7Encoding encoding, string messageType, string controlId, string processingId, string versionId)
        {
            List<string> results = new List<string>();
            results.Add(encoding == null ? "encoding" : string.Empty);
            results.Add(string.IsNullOrWhiteSpace(messageType) ? "messageType" : string.Empty);
            results.Add(string.IsNullOrWhiteSpace(controlId) ? "controlId" : string.Empty);
            results.Add(string.IsNullOrWhiteSpace(processingId) ? "processingId" : string.Empty);
            results.Add(string.IsNullOrWhiteSpace(versionId) ? "versionId" : string.Empty);
            results.RemoveAll(r => string.IsNullOrWhiteSpace(r));
            return new ValidationResult(string.Join(',', results));
        }

        public static Result<Message> CreateBasicMessage(string sendingApplication, string sendingFacility, string receivingApplication, string receivingFacility,
            string security, string messageType, string messageControlId, string processingId, string version, Hl7Encoding encoding) =>
            CreateBasicMessage(encoding, messageType, messageControlId, processingId, version, sendingApplication, sendingFacility, receivingApplication,
                receivingFacility, DateTime.UtcNow, security);

        public static Result<Message> CreateMessageFromSegmentString(Hl7Encoding encoding, string segmentString, string segmentName) =>
            CreateMessageFromSegment(CreateSegment(encoding, segmentName, segmentString));

        public static Result<Message> CreateMessageFromSegment(BaseSegment segment) =>
            CreateMessageFromSegmentCollection(new SegmentCollection(segment.Name, 1, new List<BaseSegment> { segment }));

        private static Result<Message> CreateMessageFromSegmentCollection(SegmentCollection segment) =>
            CreateMessageFromSegmentCollections(new List<SegmentCollection> { segment });

        // NOTE requires at least 1 segment.
        private static Result<Message> CreateMessageFromSegmentCollections(IEnumerable<SegmentCollection> segments) =>
            new Result<Message>(new Message(segments, segments.First().Segments.First().Encoding));

        /// <summary>
        /// Builds the acknowledgement message for this message
        /// </summary>
        /// <returns>An ACK message if success, otherwise null</returns>
        public static Result<Message> GetAck(this Message message, string? code = null) => CreateAckorNackMessage(message, code);

        /// <summary>
        /// Builds a negative ack for this message
        /// </summary>
        /// <param name="code">ack code like AR, AE</param>
        /// <param name="errMsg">error message to be sent with NACK</param>
        /// <returns>A NACK message if success, otherwise null</returns>
        public static Result<Message> GetNack(this Message message, string errMsg, string? code = null) => CreateAckorNackMessage(message, code, errMsg);

        /// <summary>
        /// Builds an ACK or NACK message for this message
        /// </summary>
        /// <param name="code">ack code like AA, AR, AE</param>
        /// <param name="errMsg">error message to be sent with NACK -- infers that this is a NACK</param>
        /// <returns>An ACK or NACK message if success, otherwise null</returns>
        private static Result<Message> CreateAckorNackMessage(this Message message, string? code = null, string? nackErrorMessage = null)
        {
            BaseSegment msh = MshMessageHelper.CreateResponseMshSegment(message);
            string responseCode = code ?? (string.IsNullOrWhiteSpace(nackErrorMessage) ? (message.GetAcceptAckType() ?? AA) : AE);//is acceptacktype a good default?
            List<BaseField> fields = FieldHelper.ParseFields(new string?[] { responseCode, message.GetMessageControlId() ?? string.Empty, nackErrorMessage },
                message.Encoding, false, false);
            BaseSegment msa = SegmentHelper.CreateSegment(message.Encoding, MessageHelper.MSA, fields);
            return CreateMessageFromSegmentCollections(new List<SegmentCollection> { new SegmentCollection(msh.Name, 1, new List<BaseSegment> { msh }), new SegmentCollection(msa.Name, 2, new List<BaseSegment> { msa }) });
        }
    }
}
