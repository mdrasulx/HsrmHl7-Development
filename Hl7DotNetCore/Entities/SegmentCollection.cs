using Hl7.Entities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Hl7.Entities
{
    public class SegmentCollection
    {
        public readonly string SegmentName;
        public readonly int SegmentOrdinal;//ordinal of this segment collection among all the segment collections
        public readonly ImmutableList<BaseSegment> Segments;

        public bool IsEnumeratedSegmentCollection { get => Segments.All(s => s.Ordinal.HasValue); }

        public SegmentCollection(string segmentName, int ordinal, IEnumerable<BaseSegment> segments) =>
            (SegmentName, SegmentOrdinal, Segments) = (segmentName, ordinal, segments.ToImmutableList());
    }
}
