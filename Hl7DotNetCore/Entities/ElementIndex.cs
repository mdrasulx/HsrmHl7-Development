using System.Collections.Generic;
using System.Text;

namespace Hl7.Entities
{
    public abstract class BaseSpecificElementIndex
    {
        public abstract bool HasValue { get; }
        internal abstract int? GetIndex();

        public BaseSpecificElementIndex() { }
    }

    public class SpecificElementIndex : BaseSpecificElementIndex
    {
        public readonly int? NullableValue;
        public int Value => NullableValue ?? -1;
        public override bool HasValue => NullableValue.HasValue;

        public static implicit operator SpecificElementIndex?(int? value) => value == null ? null : new SpecificElementIndex(value);
        public static implicit operator int? (SpecificElementIndex value) => value?.NullableValue;

        public SpecificElementIndex(int? index) : base() => this.NullableValue = index;

        internal override int? GetIndex() => NullableValue;
        public override string? ToString() => NullableValue?.ToString();
    }

    public class SpecificElementIndexLookAhead : BaseSpecificElementIndex
    {
        public readonly int? LookAheadIndex;
        public readonly string LookAheadValue;
        public override bool HasValue => !string.IsNullOrEmpty(LookAheadValue);

        public SpecificElementIndexLookAhead(string lookAheadValue, int? lookAheadIndex = null) : base() => (LookAheadIndex, LookAheadValue) = (lookAheadIndex, lookAheadValue);

        internal override int? GetIndex() => LookAheadIndex;
        public override string ToString() => $"{(LookAheadIndex.HasValue ? LookAheadIndex.ToString() : string.Empty)}:{LookAheadValue ?? string.Empty}";
    }

    public abstract class BaseSegmentIndex : BaseSpecificElementIndex{}

    public class SegmentOrdinalIndex : BaseSegmentIndex
    {
        public readonly int Ordinal;
        public override bool HasValue => true;

        public SegmentOrdinalIndex(int ordinal) => Ordinal = ordinal;

        internal override int? GetIndex() => Ordinal;

        public override string ToString() => Ordinal.ToString();
    }

    public abstract class BaseSegmentNamedIndex : BaseSegmentIndex
    {
        public readonly string SegmentName;
        public readonly int? MultipleInstanceOrdinal;
        public override bool HasValue => !string.IsNullOrWhiteSpace(SegmentName);

        public BaseSegmentNamedIndex(string segmentName, int? multipleInstanceOrdinal = null) : base() => (SegmentName, MultipleInstanceOrdinal) = (segmentName, multipleInstanceOrdinal);

        public override string ToString() => $"{SegmentName}{(MultipleInstanceOrdinal.HasValue ? $":{MultipleInstanceOrdinal}" : string.Empty)}";
    }

    public class SegmentNamedIndex : BaseSegmentNamedIndex
    {
        public override bool HasValue => base.HasValue;

        public SegmentNamedIndex(string segmentName, int? multipleInstanceOrdinal = null) : base(segmentName, multipleInstanceOrdinal) { }

        internal override int? GetIndex() => null;
        public override string ToString() => base.ToString();
    }

    public class SegmentNameAndOrdinalIndex : BaseSegmentNamedIndex
    {
        public readonly int? SegmentOrdinal;
        public override bool HasValue => base.HasValue && SegmentOrdinal.HasValue;

        public SegmentNameAndOrdinalIndex(string segmentName, int? segmentOrdinal = null, int? multipleInstanceOrdinal = null) : base(segmentName, multipleInstanceOrdinal) => SegmentOrdinal = segmentOrdinal;

        internal override int? GetIndex() => SegmentOrdinal;
        public override string ToString() => $"{base.ToString()}-{SegmentOrdinal?.ToString()}";
    }

    public class SegmentNameAndIdIndex : BaseSegmentNamedIndex
    {
        public readonly string SegmentId;
        public override bool HasValue => base.HasValue && !string.IsNullOrEmpty(SegmentId);

        public SegmentNameAndIdIndex(string segmentName, string segmentId, int? multipleInstanceOrdinal = null) : base(segmentName, multipleInstanceOrdinal) => SegmentId = segmentId;

        internal override int? GetIndex() => null;
        public override string ToString() => $"{base.ToString()}-{SegmentId?.ToString()}";
    }

    public class ElementIndex
    {
        public readonly BaseSegmentIndex SegmentIndex;
        public readonly BaseSpecificElementIndex? FieldIndex;
        public readonly BaseSpecificElementIndex? SubFieldIndex;
        public readonly BaseSpecificElementIndex? ComponentIndex;
        public readonly int? SubComponentIndex;

        public ElementIndex(BaseSegmentIndex segmentIndex, BaseSpecificElementIndex? fieldIndex = null, BaseSpecificElementIndex? subFieldIndex = null, BaseSpecificElementIndex? componentIndex = null, int? subComponentIndex = null)
            : this(null, segmentIndex, fieldIndex, subFieldIndex, componentIndex, subComponentIndex) { }

        public ElementIndex(ElementIndex? index, BaseSegmentIndex segmentIndex, BaseSpecificElementIndex? fieldIndex = null, BaseSpecificElementIndex? subFieldIndex = null, BaseSpecificElementIndex? componentIndex = null, int? subComponentIndex = null)
        {
            this.SegmentIndex = segmentIndex;
            this.FieldIndex = fieldIndex ?? index?.FieldIndex;
            this.SubFieldIndex = subFieldIndex ?? index?.SubFieldIndex;
            this.ComponentIndex = componentIndex ?? index?.ComponentIndex;
            this.SubComponentIndex = subComponentIndex ?? index?.SubComponentIndex;
        }

        internal IEnumerable<int?> GetIntIndexes() => new int?[] { SegmentIndex?.GetIndex(), FieldIndex?.GetIndex(), SubFieldIndex?.GetIndex(), ComponentIndex?.GetIndex(), SubComponentIndex };

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder($"SegmentIndex: {SegmentIndex}");
            if (FieldIndex != null && FieldIndex.HasValue) sb.Append($", fieldIndex: {FieldIndex}");
            if (SubFieldIndex != null && SubFieldIndex.HasValue) sb.Append($", SubFieldIndex: {SubFieldIndex}");
            if (ComponentIndex != null && ComponentIndex.HasValue) sb.Append($", ComponentIndex: {ComponentIndex}");
            if (SubComponentIndex.HasValue) sb.Append($", SubComponentIndex: {SubComponentIndex}");
            return sb.ToString();
        }
    }
}
