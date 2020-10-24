using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Hl7.Entities
{
    public abstract class BaseSegment : MessageElement
    {
        public readonly ImmutableList<BaseField> FieldList;
        public int? Ordinal => GetOrdinalInternal(FieldList); // how this segment fits in as a numbered segment in the collection of the same segment types
        public readonly string Name;
        public string? Id => GetIdInternal(FieldList);//identifier of the segment (for most segment types). this is the second value in the segment after the name.

        internal BaseSegment(Hl7Encoding encoding, string name, IEnumerable<BaseField> fieldList) : base(encoding) => 
            (Name, FieldList) = (name, fieldList.ToImmutableList());

        private static int? GetOrdinalInternal(IEnumerable<BaseField> fieldList)
        {
            string? id = GetIdInternal(fieldList);
            if (!string.IsNullOrWhiteSpace(id) && int.TryParse(id, out int res))
                return res;
            return null;
        }

        private static string? GetIdInternal(IEnumerable<BaseField> fieldList)
        {
            if (fieldList != null && fieldList.Count() > 0 && fieldList.First() is ValueField vf)
                return vf.value;
            return null;
        }
    }

    public class Segment : BaseSegment
    {
        public Segment(Hl7Encoding encoding, string name, IEnumerable<BaseField> fieldList) : base(encoding, name, fieldList) { }
        internal Segment(Segment segment, IEnumerable<BaseField> fieldList) : this(segment.Encoding, segment.Name, fieldList) { }
        internal override string GetRawValue() =>
            string.Concat(Name, Encoding.FieldDelimiter, string.Join(Encoding.FieldDelimiter.ToString(), this.FieldList.Select(field => field.GetRawValue()).Where(v => v != null)));
    }
}
