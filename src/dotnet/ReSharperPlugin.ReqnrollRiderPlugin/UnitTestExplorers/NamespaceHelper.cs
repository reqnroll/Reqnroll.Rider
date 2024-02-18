using System.Collections.Generic;
using System.Text;
using JetBrains.Util;

namespace ReSharperPlugin.ReqnrollRiderPlugin.UnitTestExplorers;

public static class NamespaceHelper
{
    public static void ConvertFolderNameToToIdentifierPart(string name, StringBuilder sb)
    {
        var lastIsSeparator = true;
        if (name.Length > 0 && name[0].IsDigitFast())
            sb.Append('_');

        foreach (var c in name)
        {
            switch (c)
            {
                case '\'':
                case '"':
                case ' ':
                case '+':
                case '\t':
                    lastIsSeparator = true;
                    break;
                case '.':
                case '-':
                case '_':
                    lastIsSeparator = true;
                    sb.Append('_');
                    break;
                default:
                    if (!c.IsIdentifierPart() && AccentReplacements.TryGetValue(c, out var replacement))
                    {
                        if (lastIsSeparator)
                        {
                            lastIsSeparator = false;
                            sb.Append(replacement.Capitalize());
                        }
                        else
                            sb.Append(replacement);
                    }
                    else
                    {
                        if (lastIsSeparator)
                        {
                            lastIsSeparator = false;
                            sb.Append(c.ToUpperFast());
                        }
                        else
                            sb.Append(c);
                    }
                    break;
            }

        }
    }

    private static readonly Dictionary<char, string> AccentReplacements = new()
    {
        {'\u00C0', "A"},
        {'\u00C1', "A"},
        {'\u00C2', "A"},
        {'\u00C3', "A"},
        {'\u00C4', "A"},
        {'\u00C5', "A"},
        {'\u00C6', "AE"},
        {'\u00C7', "C"},
        {'\u00C8', "E"},
        {'\u00C9', "E"},
        {'\u00CA', "E"},
        {'\u00CB', "E"},
        {'\u00CC', "I"},
        {'\u00CD', "I"},
        {'\u00CE', "I"},
        {'\u00CF', "I"},
        {'\u00D0', "D"},
        {'\u00D1', "N"},
        {'\u00D2', "O"},
        {'\u00D3', "O"},
        {'\u00D4', "O"},
        {'\u00D5', "O"},
        {'\u00D6', "O"},
        {'\u00D8', "O"},
        {'\u00D9', "U"},
        {'\u00DA', "U"},
        {'\u00DB', "U"},
        {'\u00DC', "U"},
        {'\u00DD', "Y"},
        {'\u00DF', "B"},
        {'\u00E0', "a"},
        {'\u00E1', "a"},
        {'\u00E2', "a"},
        {'\u00E3', "a"},
        {'\u00E4', "a"},
        {'\u00E5', "a"},
        {'\u00E6', "ae"},
        {'\u00E7', "c"},
        {'\u00E8', "e"},
        {'\u00E9', "e"},
        {'\u00EA', "e"},
        {'\u00EB', "e"},
        {'\u00EC', "i"},
        {'\u00ED', "i"},
        {'\u00EE', "i"},
        {'\u00EF', "i"},
        //{'\u00F0', "d"},
        {'\u00F1', "n"},
        {'\u00F2', "o"},
        {'\u00F3', "o"},
        {'\u00F4', "o"},
        {'\u00F5', "o"},
        {'\u00F6', "o"},
        {'\u00F8', "o"},
        {'\u00F9', "u"},
        {'\u00FA', "u"},
        {'\u00FB', "u"},
        {'\u00FC', "u"},
        {'\u00FD', "y"},
        {'\u00FF', "y"},


        {'\u0104', "A"},
        {'\u0141', "L"},
        {'\u013D', "L"},
        {'\u015A', "S"},
        {'\u0160', "S"},
        {'\u015E', "S"},
        {'\u0164', "T"},
        {'\u0179', "Z"},
        {'\u017D', "Z"},
        {'\u017B', "Z"},
        {'\u0105', "a"},
        {'\u0142', "l"},
        {'\u013E', "l"},
        {'\u015B', "s"},
        {'\u0161', "s"},
        {'\u015F', "s"},
        {'\u0165', "t"},
        {'\u017A', "z"},
        {'\u017E', "z"},
        {'\u017C', "z"},
        {'\u0154', "R"},
        {'\u0102', "A"},
        {'\u0139', "L"},
        {'\u0106', "C"},
        {'\u010C', "C"},
        {'\u0118', "E"},
        {'\u011A', "E"},
        {'\u010E', "D"},
        {'\u0110', "D"},
        {'\u0143', "N"},
        {'\u0147', "N"},
        {'\u0150', "O"},
        {'\u0158', "R"},
        {'\u016E', "U"},
        {'\u0170', "U"},
        {'\u0162', "T"},
        {'\u0155', "r"},
        {'\u0103', "a"},
        {'\u013A', "l"},
        {'\u0107', "c"},
        {'\u010D', "c"},
        {'\u0119', "e"},
        {'\u011B', "e"},
        {'\u010F', "d"},
        {'\u0111', "d"},
        {'\u0144', "n"},
        {'\u0148', "n"},
        {'\u0151', "o"},
        {'\u0159', "r"},
        {'\u016F', "u"},
        {'\u0171', "u"},
        {'\u0163', "t"},
    };
}
