using System;

namespace Silky.Http.Core;

   internal static class HttpCharacters
    {
        private static readonly int _tableSize = 128;

        private static readonly bool[] _alphaNumeric = InitializeAlphaNumeric();
        private static readonly bool[] _fieldValue = InitializeFieldValue();
        private static readonly bool[] _token = InitializeToken();

        private static bool[] InitializeAlphaNumeric()
        {
            // ALPHA and DIGIT https://tools.ietf.org/html/rfc5234#appendix-B.1
            var alphaNumeric = new bool[_tableSize];
            for (var c = '0'; c <= '9'; c++)
            {
                alphaNumeric[c] = true;
            }
            for (var c = 'A'; c <= 'Z'; c++)
            {
                alphaNumeric[c] = true;
            }
            for (var c = 'a'; c <= 'z'; c++)
            {
                alphaNumeric[c] = true;
            }
            return alphaNumeric;
        }

        private static bool[] InitializeFieldValue()
        {
            // field-value https://tools.ietf.org/html/rfc7230#section-3.2
            var fieldValue = new bool[_tableSize];
            for (var c = 0x20; c <= 0x7e; c++) // VCHAR and SP
            {
                fieldValue[c] = true;
            }
            return fieldValue;
        }

        private static bool[] InitializeToken()
        {
            // tchar https://tools.ietf.org/html/rfc7230#appendix-B
            var token = new bool[_tableSize];
            Array.Copy(_alphaNumeric, token, _tableSize);
            token['!'] = true;
            token['#'] = true;
            token['$'] = true;
            token['%'] = true;
            token['&'] = true;
            token['\''] = true;
            token['*'] = true;
            token['+'] = true;
            token['-'] = true;
            token['.'] = true;
            token['^'] = true;
            token['_'] = true;
            token['`'] = true;
            token['|'] = true;
            token['~'] = true;
            return token;
        }

        public static int IndexOfInvalidTokenChar(string s)
        {
            var token = _token;

            for (var i = 0; i < s.Length; i++)
            {
                var c = s[i];
                if (c >= (uint)token.Length || !token[c])
                {
                    return i;
                }
            }

            return -1;
        }

        public static int IndexOfInvalidFieldValueChar(string s)
        {
            var fieldValue = _fieldValue;

            for (var i = 0; i < s.Length; i++)
            {
                var c = s[i];
                if (c >= (uint)fieldValue.Length || !fieldValue[c])
                {
                    return i;
                }
            }

            return -1;
        }
    }