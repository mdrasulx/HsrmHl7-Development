using Hl7.Entities;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using static Hl7.Helpers.FieldHelper;
using static Hl7.Helpers.Helper;

namespace Hl7.Helpers
{
    public static class SegmentHelper
    {
        public const string MSH = "MSH";

        public static BaseSegment AddNewField(this BaseSegment segment, string content, bool isDelimiters = false, int position = 0)
        {
            if (isDelimiters)
                return segment.AddNewField(new DelimitersField(segment.Encoding), position);
            return segment.AddNewField(CreateField(segment.Encoding, content), position);
        }

        public static BaseSegment AddNewField(this BaseSegment segment, BaseField field, int position = 0)
        {
            if (position <= 0)
                return CreateSegment(segment, segment.FieldList.Add(field));
            else
                return CreateSegment(segment, segment.FieldList.Insert(position - 1, field));
        }

        internal static string GetSegmentName(string value) => value.Substring(0, 3);

        internal static Result<BaseSegment> TrySetSegment(BaseSegment? segment, ElementIndex elementIndex, string newValue)
        {
            if (segment == null)
                return ErrorReturn<BaseSegment>($"failed to find field({elementIndex.FieldIndex} on message with {elementIndex}, as the segment was not found");
            if (elementIndex.FieldIndex?.HasValue == true)
            {
                List<BaseField> fields = segment.FieldList.ToList();
                if (!TryGetField(fields, elementIndex.FieldIndex, out int foundIndex).DecomposeResult(out BaseField field, out string? error))
                    return ErrorReturn<BaseSegment>(error ?? "unknown error getting field");
                if (!TrySetField(field, elementIndex, newValue).DecomposeResult(out BaseField result, out error))
                    return ErrorReturn<BaseSegment>(error ?? "unknown error setting field");
                fields[foundIndex - 1] = result;
                return new Result<BaseSegment>(CreateSegment(segment, fields));
            }
            else
                return new Result<BaseSegment>(CreateSegment(segment, newValue));
        }

        internal static List<SegmentCollection> ParseSegments(Hl7Encoding definedEncoding, List<string> allSegments)
        {
            var returnValue = new List<SegmentCollection>();
            if (allSegments == null)
                return returnValue;
            short ordinal = 1;
            string previous = string.Empty;
            List<BaseSegment> currentSegments = new List<BaseSegment>();
            foreach (string strSegment in allSegments.Where(s => !string.IsNullOrWhiteSpace(s)))
            {
                BaseSegment newSegment = CreateSegment(strSegment, definedEncoding);
                previous = previous == string.Empty ? newSegment.Name : previous;
                if (previous != newSegment.Name)
                {
                    returnValue.Add(new SegmentCollection(previous, ordinal, currentSegments));
                    currentSegments = new List<BaseSegment>();
                    ordinal++;
                    previous = newSegment.Name;
                }
                currentSegments.Add(newSegment);
            }
            returnValue.Add(new SegmentCollection(previous, ordinal, currentSegments));
            return returnValue;
        }

        public static BaseSegment CreateSegment(string value, Hl7Encoding encoding) =>
            CreateSegment(encoding, SegmentHelper.GetSegmentName(value), value);

        public static BaseSegment CreateSegment(BaseSegment segment, IEnumerable<BaseField> fields) =>
            CreateSegment(segment.Encoding, segment.Name, fields);

        public static BaseSegment CreateSegment(BaseSegment segment, string value) =>
            CreateSegment(segment.Encoding, segment.Name, ParseFields(value, segment.Encoding));

        public static BaseSegment CreateSegment(Hl7Encoding encoding, string name, string value) =>
            CreateSegment(encoding, name, ParseFields(value, encoding));

        public static BaseSegment CreateSegment(Hl7Encoding encoding, string name, IEnumerable<BaseField> fields) =>
            new Segment(encoding, name, fields);

        internal static List<BaseSegment> GetAllSegmentsInOrder(this Message message)
        {
            List<BaseSegment> returnValue = new List<BaseSegment>();
            var segColls = message?.GetSegmentCollections();
            if (segColls != null)
            {
                foreach (var segCol in segColls.OrderBy(s => s.SegmentOrdinal))
                    returnValue.AddRange(segCol.Segments.OrderBy(s => s.Ordinal));
            }
            return returnValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message">The message to which to add the segment</param>
        /// <param name="newSegment">The segment to add</param>
        /// <param name="segmentCollectionOrdinal">The location of the segment collection in the overall collection of segment collections, if null
        /// the first one will be used </param>
        /// <returns> A new version of the message</returns>
        public static Result<Message> AddNewSegment(this Message message, BaseSegment newSegment, int? segmentCollectionOrdinal = null)
        {
            var segmentCollections = message.GetSegmentCollections();
            var segmentCollection = segmentCollections.FirstOrDefault(sc => sc.SegmentName == newSegment.Name && (segmentCollectionOrdinal == null || sc.SegmentOrdinal == segmentCollectionOrdinal));
            if (segmentCollection != null)
            {//existing, add the segment to it
                List<BaseSegment> newSegments;
                if (newSegment.Ordinal.HasValue)
                {//inject into the right place
                    newSegments = segmentCollection.Segments.Where(s => s.Ordinal < newSegment.Ordinal).ToList();
                    newSegments.Add(newSegment);
                    newSegments.AddRange(segmentCollection.Segments.Where(s => s.Ordinal >= newSegment.Ordinal).Select(s => SetSegmentOrdinal(s, s.Ordinal + 1)));
                }
                else
                {
                    newSegments = segmentCollection.Segments.ToList();
                    newSegments.Add(newSegment);
                }
                segmentCollections[segmentCollections.IndexOf(segmentCollection)] = new SegmentCollection(segmentCollection.SegmentName, segmentCollection.SegmentOrdinal, newSegments);
                return new Result<Message>(new Message(segmentCollections, segmentCollections.First().Segments.First().Encoding));
            }
            else
            {//this is a new segment collection, if it has an ordinal use that, if no ordinal, then put at the end
                int? segmentOrdinal = segmentCollections.Max(s => s.SegmentOrdinal) + 1;
                if (segmentCollectionOrdinal.HasValue && segmentCollectionOrdinal > segmentOrdinal)
                    return ErrorReturn<Message>($"requested segment collection ordinal for new segment ({segmentCollectionOrdinal}) is beyond the size of the maximum + 1 ({segmentOrdinal})");
                segmentOrdinal = segmentCollectionOrdinal ?? segmentOrdinal;
                //renumber the segments as needed, and add our new segment
                var newSegmentList = segmentCollections.Where(s => s.SegmentOrdinal < segmentOrdinal).ToList();
                newSegmentList.Add(new SegmentCollection(newSegment.Name, segmentOrdinal.Value, new List<BaseSegment> { newSegment }));
                newSegmentList.AddRange(segmentCollections.Where(s => s.SegmentOrdinal >= segmentOrdinal).Select(s => new SegmentCollection(s.SegmentName, s.SegmentOrdinal + 1, s.Segments)));
                return new Result<Message>(new Message(newSegmentList, message.Encoding));
            }
        }

        public static BaseSegment SetSegmentOrdinal(this BaseSegment segment, int? ordinal)
        {
            List<BaseField> fieldList = segment.FieldList.ToList();
            fieldList[0] = new ValueField(ordinal.HasValue ? ordinal.Value.ToString() : string.Empty, segment.Encoding);
            //NOTE add logic here if we have other segment types
            return new Segment(segment.Encoding, segment.Name, fieldList);
        }

        internal static Result<(SegmentCollection collection, int index)> FindSegmentCollection(List<SegmentCollection> segmentCollections, BaseSegmentNamedIndex segmentIndex)
        {
            var collectionsWithName = segmentCollections.FindAll(sc => sc.SegmentName == segmentIndex.SegmentName);//grab the ones with this name
            if (segmentIndex.MultipleInstanceOrdinal.HasValue && collectionsWithName.Count() < segmentIndex.MultipleInstanceOrdinal)
                return ErrorReturn<(SegmentCollection collection, int index)>($"attempted to retrieve a segment collection with an index that is too large index:{segmentIndex}, record count: {collectionsWithName.Count()}");
            var collection = collectionsWithName[segmentIndex.MultipleInstanceOrdinal.HasValue ? segmentIndex.MultipleInstanceOrdinal.Value - 1 : 0];
            return new Result<(SegmentCollection collection, int index)>((collection, segmentCollections.IndexOf(collection)));
        }

        internal static Result<(BaseSegment? Segment, SegmentCollection SegmentCollection, int index)> TryGetSegmentAndSegmentCollectionAndIndex(Message message, BaseSegmentIndex segmentIndex)
        {
            var segmentCollections = message.GetSegmentCollections();
            if (segmentIndex is BaseSegmentNamedIndex bsni)
            {
                if (!FindSegmentCollection(segmentCollections, bsni).DecomposeResult(out var segCollectionResults, out string? error))
                    return ErrorReturn<(BaseSegment?, SegmentCollection, int)>(error ?? "Unknown error finding segment collection");
                if (bsni is SegmentNamedIndex sni)
                    return new Result<(BaseSegment?, SegmentCollection, int)>((segCollectionResults.collection.Segments?.FirstOrDefault(), segCollectionResults.collection, segCollectionResults.index));
                if (bsni is SegmentNameAndOrdinalIndex sii && sii.SegmentOrdinal.HasValue)
                    return new Result<(BaseSegment?, SegmentCollection, int)>((segCollectionResults.collection.Segments?.ElementAtOrDefault(sii.SegmentOrdinal.Value - 1), segCollectionResults.collection, segCollectionResults.index));
                else if (bsni is SegmentNameAndIdIndex ssi && !string.IsNullOrWhiteSpace(ssi.SegmentId))
                    return new Result<(BaseSegment?, SegmentCollection, int)>((segCollectionResults.collection.Segments?.FirstOrDefault(s => s.Id == ssi.SegmentId), segCollectionResults.collection, segCollectionResults.index));
                return ErrorReturn<(BaseSegment?, SegmentCollection, int)>($"Did not find specified segment {segmentIndex}");
            }
            else if (segmentIndex is SegmentOrdinalIndex soi)
            {
                var segs = GetAllSegmentsInOrder(message);
                if (segs.Count < soi.Ordinal)
                    return ErrorReturn<(BaseSegment? Segment, SegmentCollection SegmentCollection, int index)>($"Requested a segment with index {soi.Ordinal}, but max index is {segs.Count}");
                //find the segment and assocated collection for it.
                var seg = segs[soi.Ordinal - 1];
                var coll = message.GetSegmentCollections().First(s => s.Segments.Contains(seg));
                return new Result<(BaseSegment? Segment, SegmentCollection SegmentCollection, int index)>((seg, coll, segmentCollections.IndexOf(coll)));
            }
            return ErrorReturn<(BaseSegment? Segment, SegmentCollection SegmentCollection, int index)>($"attempted TryGetSegmentAndSegmentCollectionAndIndex with unknown index type: {segmentIndex.GetType().Name}");
        }
    }
}
