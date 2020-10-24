using System.Diagnostics.CodeAnalysis;

namespace Hl7.Entities
{
    public class Result<T> : ValidationResult
    {
        public readonly T Value;

        //considering this one...
        //public static implicit operator T(ReturnObject<T> v) => v.Value;

        public Result([AllowNull] T value = default, string? error = null) : base(error) => this.Value = value;
        public Result(string error) : this(default, error) { }
    }

    public class ValidationResult
    {
        public readonly string? Error;
        public bool Success => string.IsNullOrWhiteSpace(Error);

        public static implicit operator bool(ValidationResult v) => v.Success;

        public ValidationResult(string? error = null) => this.Error = error;
    }
}
