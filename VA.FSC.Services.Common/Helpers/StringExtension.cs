using System;
using System.Text.RegularExpressions;

namespace VA.FSC.Services.Common.Helpers
{
    /// <summary>
    /// String Extension
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// Strips out any non-alphanumeric characters
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string AntiLogForging(this string value)
        {
            var safeValue = Regex.Replace(value, @"[^A-Za-z0-9$]", "");
            return safeValue;
        }

        public static String CleanString(this string aString)
        {
            if (aString == null)
                return null;

            String cleanString = "";

            for (int i = 0; i < aString.Length; ++i)
            {
                cleanString += cleanChar(aString[i]);
            }
            return cleanString;
        }

        private static char cleanChar(char aChar)
        {

            //--> 0 - 9
            for (int i = 48; i < 58; ++i)
            {
                if (aChar == i) return (char)i;
            }

            //--> 'A' - 'Z'
            for (int i = 65; i < 91; ++i)
            {
                if (aChar == i) return (char)i;
            }

            //--> 'a' - 'z'
            for (int i = 97; i < 123; ++i)
            {
                if (aChar == i) return (char)i;
            }


            //--> other valid characters
            switch (aChar)
            {
                case '=':
                    return '=';
                case '"':
                    return '"';
                case '<':
                    return '<';
                case '>':
                    return '>';
                case '/':
                    return '/';
                case '\\':
                    return '\\';

                case '[':
                    return '[';
                case ']':
                    return ']';
                case '(':
                    return '(';
                case ')':
                    return ')';
                case '{':
                    return '{';
                case '}':
                    return '}';

                case '.':
                    return '.';
                case ',':
                    return ',';
                case ';':
                    return ';';
                case ':':
                    return ':';

                case '-':
                    return '-';
                case '_':
                    return '_';

                case '*':
                    return '*';
                case '&':
                    return '&';
                case '^':
                    return '^';
                case '|':
                    return '|';
                case '$':
                    return '$';
                case '#':
                    return '#';
                case '@':
                    return '@';
                case '!':
                    return '!';
                case ' ':
                    return ' ';
            }

            return '%';
        }

    }
}
