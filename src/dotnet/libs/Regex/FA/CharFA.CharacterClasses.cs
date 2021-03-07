using System;
using System.Collections.Generic;
using System.Text;

namespace RE
{
	// provides character class support. 
	// Just populates the posix character 
	// classes and exposes a 
	// CharacterClasses property
	partial class CharFA<TAccept>
	{
		static IDictionary<string, IList<CharRange>> _charClasses = _GetCharacterClasses();
		/// <summary>
		/// Retrieves a dictionary indicating the character classes supported by this library
		/// </summary>
		public static IDictionary<string, IList<CharRange>> CharacterClasses
					=> _charClasses;
		// build the character classes
		static IDictionary<string, IList<CharRange>> _GetCharacterClasses()
		{
			var result = new Dictionary<string, IList<CharRange>>();
			result.Add("alnum",
				new List<CharRange>(
					new CharRange[] {
						new CharRange('A','Z'),
						new CharRange('a', 'z'),
						new CharRange('0', '9')
					}));
			result.Add("alpha",
				new List<CharRange>(
					new CharRange[] {
						new CharRange('A','Z'),
						new CharRange('a', 'z')
					}));
			result.Add("ascii",
				new List<CharRange>(
					new CharRange[] {
						new CharRange('\0','\x7F')
					}));
			result.Add("blank",
				new List<CharRange>(
					new CharRange[] {
						new CharRange(' ',' '),
						new CharRange('\t','\t')
					}));
			result.Add("cntrl",
				new List<CharRange>(
					new CharRange[] {
						new CharRange('\0','\x1F'),
						new CharRange('\x7F','\x7F')
					}));
			result.Add("digit",
				new List<CharRange>(
					new CharRange[] {
						new CharRange('0', '9')
					}));
			result.Add("^digit", new List<CharRange>(CharRange.NotRanges(result["digit"])));
			result.Add("graph",
				new List<CharRange>(
					new CharRange[] {
						new CharRange('\x21', '\x7E')
					}));
			result.Add("lower",
				new List<CharRange>(
					new CharRange[] {
						new CharRange('a', 'z')
					}));
			result.Add("print",
				new List<CharRange>(
					new CharRange[] {
						new CharRange('\x20', '\x7E')
					}));
			// [!"\#$%&'()*+,\-./:;<=>?@\[\\\]^_`{|}~]	
			result.Add("punct",
				new List<CharRange>(
					CharRange.GetRanges("!\"#$%&\'()*+,-./:;<=>?@[\\]^_`{|}~")
					));
			//[ \t\r\n\v\f]
			result.Add("space",
					new List<CharRange>(
						CharRange.GetRanges(" \t\r\n\v\f")
						));
			result.Add("^space", new List<CharRange>(CharRange.NotRanges(result["space"])));
			result.Add("upper",
				new List<CharRange>(
					new CharRange[] {
						new CharRange('A', 'Z')
					}));
			result.Add("word",
				new List<CharRange>(
					new CharRange[] {
						new CharRange('0', '9'),
						new CharRange('A', 'Z'),
						new CharRange('_', '_'),
						new CharRange('a', 'z')
					}));
			result.Add("^word", new List<CharRange>(CharRange.NotRanges(result["word"])));
			result.Add("xdigit",
				new List<CharRange>(
					new CharRange[] {
						new CharRange('0', '9'),
						new CharRange('A', 'F'),
						new CharRange('a', 'f')
					}));
			return result;
		}

	}
}
