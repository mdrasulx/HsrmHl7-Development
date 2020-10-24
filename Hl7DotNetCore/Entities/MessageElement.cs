namespace Hl7.Entities
{
    public abstract class MessageElement
    {
        public readonly Hl7Encoding Encoding;
        protected MessageElement(Hl7Encoding encoding) => this.Encoding = encoding;
        internal abstract string GetRawValue();
    }
}
