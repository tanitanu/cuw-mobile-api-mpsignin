using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace United.Utility.Extensions
{
    public static class StringExtension
    {
        private static Regex WordRegex = new Regex(@"\p{Lu}\p{Ll}+|\p{Lu}+(?!\p{Ll})|\p{Ll}+|\d+");

        public static string ToPascalCase(this string input)
        {
            return WordRegex.Replace(input, EvaluatePascal);
        }

        public static string ToCamelCase(this string input)
        {
            string pascal = ToPascalCase(input);
            return WordRegex.Replace(pascal, EvaluateFirstCamel, 1);
        }

        private static string EvaluateFirstCamel(Match match)
        {
            return match.Value.ToLower();
        }

        private static string EvaluatePascal(Match match)
        {
            string value = match.Value;
            int valueLength = value.Length;

            if (valueLength == 1)
                return value.ToUpper();
            else
            {
                if (valueLength <= 2 && IsWordUpper(value))
                    return value;
                else
                    return value.Substring(0, 1).ToUpper() + value.Substring(1, valueLength - 1).ToLower();
            }
        }


        public static string ToBase64(this string text)
        {
            return ToBase64(text, Encoding.UTF8);
        }

        public static string ToBase64(this string text, Encoding encoding)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            byte[] textAsBytes = encoding.GetBytes(text);
            return Convert.ToBase64String(textAsBytes);
        }

        public static bool TryParseBase64(this string text, out string decodedText)
        {
            return TryParseBase64(text, Encoding.UTF8, out decodedText);
        }

        public static bool TryParseBase64(this string text, Encoding encoding, out string decodedText)
        {
            if (string.IsNullOrEmpty(text))
            {
                decodedText = text;
                return false;
            }

            try
            {
                byte[] textAsBytes = Convert.FromBase64String(text);
                decodedText = encoding.GetString(textAsBytes);
                return true;
            }
            catch (Exception)
            {
                decodedText = null;
                return false;
            }
        }

        private static bool IsWordUpper(string word)
        {
            bool result = true;

            foreach (char c in word)
            {
                if (Char.IsLower(c))
                {
                    result = false;
                    break;
                }
            }

            return result;
        }
        public static string Mask(this string source, int start, int maskLength)
        {
            return source.Mask(start, maskLength, '*');
        }

        public static string Mask(this string source, int start, int maskLength, char maskCharacter)
        {
            if (start > source.Length - 1)
            {
                throw new ArgumentException("Start position is greater than string length");
            }

            if (maskLength > source.Length)
            {
                throw new ArgumentException("Mask length is greater than string length");
            }

            if (start + maskLength > source.Length)
            {
                throw new ArgumentException("Start position and mask length imply more characters than are present");
            }

            string mask = new string(maskCharacter, maskLength);
            string unMaskStart = source.Substring(0, start);
            string unMaskEnd = source.Substring(start + maskLength, source.Length - maskLength);

            return unMaskStart + mask + unMaskEnd;
        }

        public static string RemoveSpecialCharacters(this string str)
        {
            var sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

    }
}
