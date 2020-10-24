using Hl7.Helpers;

namespace Hl7.Entities
{
    public abstract class BaseSubComponent : MessageElement
    {
        public BaseSubComponent(Hl7Encoding encoding) : base(encoding) { }
    }

    public class SubComponent : BaseSubComponent
    {
        private readonly string value;
        public SubComponent(string value, Hl7Encoding encoding) : base(encoding) => this.value = value;
        internal SubComponent(BaseSubComponent subComponent, string value) : this(value, subComponent.Encoding) { }
        internal override string GetRawValue() => base.Encoding.Encode(this.value);
    }
}
