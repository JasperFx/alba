using System;

namespace Alba
{
    public struct Segment
    {
        public static string UnquoteToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token) || !token.StartsWith("\"", StringComparison.Ordinal) || (!token.EndsWith("\"", StringComparison.Ordinal) || token.Length <= 1))
                return token;

            return token.Substring(1, token.Length - 2);
        }

        public Segment(string text)
        {
            var index = text.IndexOf('=');
            if (index < 0)
            {
                Key = text;
                Value = null;
            }
            else
            {
                Key = text.Substring(0, index);
                Value = UnquoteToken(text.Substring(index + 1).Trim());
            }
        }

        public string Key;
        public string Value;
    }
}