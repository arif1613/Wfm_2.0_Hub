using System;

namespace CommonSystemTestLibrary.Extensions
{
    public static class StringExtensions
    {
        public static string ToCamelCase(this string str)
        {
            var output = string.Empty;
            var upperCase = true;

            foreach (var c in str)
            {
                if(c!=' ' && c!= '_' && !char.IsLetterOrDigit(c))
                    throw new ArgumentException("Allowed characters are letters, digits, space and underscore", str);

                if (c == ' ' || c == '_')
                    upperCase = true;
                else if (upperCase)
                {
                    output += char.ToUpper(c);
                    upperCase = false;
                }
                else
                    output += c;
            }

            return output;
        }
    }
}
