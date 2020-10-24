using System;
using System.Collections.Generic;
using System.Linq;

namespace Hl7.Entities
{
    public class Hl7Encoding : IEquatable<Hl7Encoding>
    {
        public readonly char FieldDelimiter;// = '|'; // \F\
        public readonly char ComponentDelimiter;// = '^'; // \S\
        public readonly char RepeatDelimiter;// = '~';  // \R\
        public readonly char EscapeCharacter;// = '\\'; // \E\
        public readonly char SubComponentDelimiter;// = '&'; // \T\
        public readonly IEnumerable<string> SegmentDelimiters;// = Helpers.EncodingHelper.LineSeparators;
        public readonly string DateTimeFormat; //"yyyyMMddHHmmsszzz"

        public string GetPreferredSegmentDelimiter() => SegmentDelimiters.FirstOrDefault();
        public string GetAllDelimiters() => $"{FieldDelimiter}{ComponentDelimiter}{RepeatDelimiter}{EscapeCharacter}{SubComponentDelimiter}";

        //note the hard coding 0-4 below is dangerous, could consider putting in a static method to invoke the other constructor, but since this is internal, leaving it simple for now.
        internal Hl7Encoding(string delimiters, IEnumerable<string>? segmentDelimiters = null, string? dateTimeFormat = null) : this(delimiters[0], delimiters[1], delimiters[2], delimiters[3], delimiters[4], segmentDelimiters, dateTimeFormat) { }

        public Hl7Encoding(char fieldDelimiter = '|', char componentDelimiter = '^', char repeatDelimiter = '~', char escapeCharacter = '\\',
            char subComponentDelimiter = '&', IEnumerable<string>? segmentDelimiters = null, string? dateTimeFormat = null)
        {
            this.FieldDelimiter = fieldDelimiter;
            this.ComponentDelimiter = componentDelimiter;
            this.RepeatDelimiter = repeatDelimiter;
            this.EscapeCharacter = escapeCharacter;
            this.SubComponentDelimiter = subComponentDelimiter;
            this.SegmentDelimiters = segmentDelimiters ?? Helpers.EncodingHelper.LineDelimiters;
            this.DateTimeFormat = dateTimeFormat ?? "yyyyMMddHHmmsszzz";
        }

        public bool Equals(Hl7Encoding other)
        {
            return this.GetAllDelimiters().Equals(other.GetAllDelimiters()) && this.SegmentDelimiters.Equals(other.SegmentDelimiters) &&
                this.DateTimeFormat.Equals(other.DateTimeFormat);
        }
    }
}
