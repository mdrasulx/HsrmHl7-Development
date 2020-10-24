using Hl7.Helpers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Hl7.Entities
{
    public abstract class BaseComponent : MessageElement
    {
        public BaseComponent(Hl7Encoding encoding) : base(encoding) { }
    }

    public class SimpleComponent : BaseComponent
    {
        private readonly string value;
        public SimpleComponent(string value, Hl7Encoding encoding) : base(encoding) => this.value = value;
        internal SimpleComponent(SimpleComponent component, string value) : this(value, component.Encoding) { }
        internal override string GetRawValue() => this.Encoding.Encode(this.value);
    }

    public class ComplexComponent : BaseComponent
    {
        public readonly ImmutableList<BaseSubComponent> SubComponents;
        public ComplexComponent(IEnumerable<BaseSubComponent> subComponents, Hl7Encoding encoding) : base(encoding) => this.SubComponents = subComponents.ToImmutableList();
        internal ComplexComponent(ComplexComponent component, IEnumerable<BaseSubComponent> subComponents) : this(subComponents, component.Encoding) { }
        internal override string GetRawValue() => string.Join(Encoding.SubComponentDelimiter.ToString(), SubComponents.Select(sc => sc.GetRawValue()));
    }
}
