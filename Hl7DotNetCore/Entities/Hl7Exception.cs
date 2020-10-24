using System;

namespace Hl7.Entities
{
    public class Hl7Exception : Exception
    {
        public readonly int ErrorCode;
        public Hl7Exception(string message, int errorCode = 0, Exception? innerException = null) : base(message, innerException) => this.ErrorCode = errorCode;
        public override string ToString() => $"{ErrorCode}: {Message} {Environment.NewLine}{base.ToString()}";
    }

    public class Hl7ParsingErrorException : Hl7Exception
    {
        private const string PARSING_ERROR = "Parsing Error";
        public Hl7ParsingErrorException(string message = PARSING_ERROR, Exception? innerException = null) : base(message, 1, innerException) { }
    }
}