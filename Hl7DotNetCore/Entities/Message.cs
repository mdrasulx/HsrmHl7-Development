using Hl7.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Hl7.Entities
{
    public class Message : MessageElement
    {
        private readonly ImmutableList<SegmentCollection> InternalSegmentCollections;

        public List<SegmentCollection> GetSegmentCollections() => InternalSegmentCollections.ToList();

        public Message(IEnumerable<SegmentCollection> segments, Hl7Encoding encoding) : base(encoding) =>
            InternalSegmentCollections = segments.ToImmutableList();

        internal override string GetRawValue() => this.GetAllSegmentsInOrder().Aggregate(string.Empty, (s, y) => string.Concat(s, y.GetRawValue(), Encoding.GetPreferredSegmentDelimiter()));
    }
}
