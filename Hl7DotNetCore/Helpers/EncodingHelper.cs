using Hl7.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace Hl7.Helpers
{
    public static class EncodingHelper
    {
        private static readonly ConcurrentDictionary<Hl7Encoding, Lazy<(ImmutableDictionary<string, string> DecodeDict, ImmutableDictionary<string, string> EncodeDict)>> EncodingDictionaries =
            new ConcurrentDictionary<Hl7Encoding, Lazy<(ImmutableDictionary<string, string>, ImmutableDictionary<string, string>)>>();

        internal static readonly string[] LineDelimiters = { "\r\n", "\n\r", "\r", "\n" };

        internal static List<string> Split(this string strStringToSplit, char chSplitBy, StringSplitOptions splitOptions = StringSplitOptions.None) =>
            strStringToSplit.Split(new char[] { chSplitBy }, splitOptions).ToList();

        internal static List<string> SplitLines(this string message, IEnumerable<string>? lineDelimiters = null) =>
            message.Split((lineDelimiters?.ToArray() ?? LineDelimiters), StringSplitOptions.None).Where(m => !string.IsNullOrWhiteSpace(m)).ToList();

        internal static string FormatDate(this DateTime dt, Hl7Encoding encoding) => dt.ToString(encoding.DateTimeFormat);

        private static void GetEncodingDictionaries(Hl7Encoding encoding, out ImmutableDictionary<string, string> decodeDictionary, out ImmutableDictionary<string, string> encodeDictionary)
        {
            var dicts = EncodingDictionaries.GetOrAdd(encoding, k => new Lazy<(ImmutableDictionary<string, string>, ImmutableDictionary<string, string>)>(
                () => GetDicts(encoding), LazyThreadSafetyMode.ExecutionAndPublication));
            decodeDictionary = dicts.Value.DecodeDict;
            encodeDictionary = dicts.Value.EncodeDict;
        }

        private static (ImmutableDictionary<string, string>, ImmutableDictionary<string, string>) GetDicts(Hl7Encoding encoding)
        {
            var encodingPairs = GetEncodingsPairs(encoding);
            var decodeDictionary = encodingPairs.ToImmutableDictionary(k => k.Item1, k => k.Item2);
            var encodeDictionary = encodingPairs.ToImmutableDictionary(k => k.Item2, k => k.Item1);
            return (decodeDictionary, encodeDictionary);
        }

        private static List<(string, string)> GetEncodingsPairs(Hl7Encoding encoding) => new List<(string, string)>()
        {//Note if new angle bracket values with lengths greater than 4 are added to the right column, update the encode method below.
            ("E", encoding.EscapeCharacter.ToString()),
            ("F", encoding.FieldDelimiter.ToString()),
            ("H","<B>"),
            ("N","</B>"),
            ("R", encoding.RepeatDelimiter.ToString()),
            ("S", encoding.ComponentDelimiter.ToString()),
            ("T", encoding.SubComponentDelimiter.ToString()),
            (".br", "<BR/>"),
        };

        public static string Decode(this Hl7Encoding encoding, string encodedValue)
        {
            if (string.IsNullOrWhiteSpace(encodedValue))
                return string.Empty;
            GetEncodingDictionaries(encoding, out var decodeDictionary, out _);
            bool inEscape = false;//this flips on and off, so grab value and inject, then grab value and convert, repeat
            var result = new StringBuilder();
            foreach (string value in encodedValue.Split(encoding.EscapeCharacter))
            {
                if (inEscape)
                {
                    if (decodeDictionary.TryGetValue(value, out string newValue))
                        result.Append(newValue);
                    else if (value.StartsWith("X"))
                        result.Append(((char)int.Parse(value.Substring(1), NumberStyles.AllowHexSpecifier)));//why substring 1?
                    else
                    {//this wasn't really an escape sequence, it just had a slash, add this content and flip even so it stays the same after the lower flip
                        result.Append(value);
                        inEscape = !inEscape;
                    }
                }
                else
                    result.Append(value);
                inEscape = !inEscape;
            }
            return result.ToString();
        }

        public static string Encode(this Hl7Encoding encoding, string val)
        {
            if (string.IsNullOrWhiteSpace(val))
                return string.Empty;
            GetEncodingDictionaries(encoding, out _, out var encodeDictionary);
            var sb = new StringBuilder();
            for (int i = 0; i < val.Length; i++)
            {
                string currentValue = val[i].ToString();
                if (val[i] < 32)//spec allows 1 or more hex characters, so encode them all
                {
                    string hexString = val[i].ToString();
                    while (val[i + 1] < 32)
                    {
                        i++;
                        hexString += val[i];
                    }
                    sb.Append(encoding.WrapStringInEscape(string.Concat("X", EncodeHex(hexString))));
                }
                else
                {
                    if (currentValue == "<")//Special lookahead handling for angle brakets
                    {
                        string workingValue = string.Empty;
                        int j = i;
                        for (; j < val.Length && (j - i < 5); j++)//note hard coded 5, could use encodeDictionary.Keys.Max(f => f.Length) instead, but this is faster provided angle bracket values don't exceed 4 characters total
                        {
                            workingValue += val[j];
                            if (val[j] == '>')
                                break;
                        }
                        if (encodeDictionary.ContainsKey(workingValue))
                        {
                            currentValue = workingValue;
                            i = j;
                        }
                    }
                    if (encodeDictionary.TryGetValue(currentValue, out string newValue))
                        sb.Append(encoding.WrapStringInEscape(newValue));
                    else
                        sb.Append(currentValue);
                }
            }
            return sb.ToString();
        }

        internal static string EncodeHex(string toEncode) => toEncode.Aggregate("", (s, c) => string.Concat(s, EncodeHex(c)));

        internal static string EncodeHex(char c) => string.Format("{0:X3}", (int)c);


        private static string WrapStringInEscape(this Hl7Encoding encoding, string toEscape) => $"{encoding.EscapeCharacter}{toEscape}{encoding.EscapeCharacter}";

    }
}
