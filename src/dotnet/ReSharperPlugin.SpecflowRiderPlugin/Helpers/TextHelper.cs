using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.ReSharper.Psi.JavaScript.Util.Literals;
using JetBrains.Util;

namespace ReSharperPlugin.SpecflowRiderPlugin.Helpers
{
    public class TextHelper
    {
        public static string ToKebabCase(string input)
        {
            var tokens = TokenizeLower(input);
            return string.Join("-", tokens);
        }
        public static string ToPascalCase(string input)
        {
            var tokens = TokenizeLower(input);
            return string.Join("", tokens.Select(x => x[0].ToUpperFast() + x.Substring(1)));
        }

        public static List<string> TokenizeLower(string input)
        {
            var tokens = new List<string>();
            var sb = new StringBuilder();
            foreach (var c in input)
            {
                if (c.IsLetterFast() && c.IsUpperFast())
                    AddToken(tokens, sb);

                if (c == '-' || c == '_' || c == ' ')
                    AddToken(tokens, sb);

                if (c.IsLetterFast())
                    sb.Append(c.ToLowerFast());
                else if (c.IsDigit())
                    sb.Append(c);
            }

            AddToken(tokens, sb);

            return tokens;
        }

        private static void AddToken(List<string> tokens, StringBuilder sb)
        {
            if (sb.Length > 0)
                tokens.Add(sb.ToString());
            sb.Clear();
        }
    }
}
