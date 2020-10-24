using Hl7.Helpers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Hl7.Entities
{
    public abstract class BaseField : MessageElement
    {
        public BaseField(Hl7Encoding encoding) : base(encoding) { }
    }

    public class DelimitersField : BaseField
    {
        public DelimitersField(Hl7Encoding encoding) : base(encoding) { }
        internal DelimitersField(DelimitersField field) : this(field.Encoding) { }
        internal override string GetRawValue() => this.Encoding.GetAllDelimiters().Remove(0, 1);//remove the field delimiter character, it's always first, and we inject one when encoding between each value
    }

    public class ValueField : BaseField
    {
        public readonly string value;
        public ValueField(string value, Hl7Encoding encoding) : base(encoding) => this.value = value;
        internal ValueField(ValueField field, string value) : this(value, field.Encoding) { }
        internal override string GetRawValue() => this.Encoding.Encode(this.value);
    }

    public class RepetitionField : BaseField
    {
        public readonly ImmutableList<BaseField> Repetitions;
        public RepetitionField(IEnumerable<BaseField> repetitions, Hl7Encoding encoding) : base(encoding) => this.Repetitions = repetitions.ToImmutableList();
        internal RepetitionField(RepetitionField field, IEnumerable<BaseField> repetitions) : this(repetitions, field.Encoding) { }
        internal override string GetRawValue() => string.Join(this.Encoding.RepeatDelimiter.ToString(), this.Repetitions.Select(r => r.GetRawValue()));
    }

    public class ComponentField : BaseField
    {
        public readonly ImmutableList<BaseComponent> Components;
        public ComponentField(IEnumerable<BaseComponent> components, Hl7Encoding encoding) : base(encoding) => this.Components = components.ToImmutableList();
        internal ComponentField(ComponentField field, IEnumerable<BaseComponent> components) : this(components, field.Encoding) { }
        internal override string GetRawValue() => string.Join(this.Encoding.ComponentDelimiter.ToString(), this.Components.Select(com => com.GetRawValue()));
    }
}
