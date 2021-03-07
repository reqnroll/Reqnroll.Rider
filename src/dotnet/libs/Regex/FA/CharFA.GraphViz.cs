using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace RE
{
	// provides rendering support using GraphViz
	partial class CharFA<TAccept>
	{
		#region DotGraphOptions
		/// <summary>
		/// Represents optional rendering parameters for a dot graph.
		/// </summary>
		public sealed class DotGraphOptions
		{
			/// <summary>
			/// The resolution, in dots-per-inch to render at
			/// </summary>
			public int Dpi { get; set; } = 300;
			/// <summary>
			/// The prefix used for state labels
			/// </summary>
			public string StatePrefix { get; set; } = "q";

			/// <summary>
			/// If non-null, specifies a debug render using the specified input string.
			/// </summary>
			/// <remarks>The debug render is useful for tracking the transitions in a state machine</remarks>
			public IEnumerable<char> DebugString { get; set; } = null;
			/// <summary>
			/// If non-null, specifies the source NFA from which this DFA was derived - used for debug view
			/// </summary>
			public CharFA<TAccept> DebugSourceNfa { get; set; } = null;
		}
		#endregion
		/// <summary>
		/// Writes a Graphviz dot specification to the specified <see cref="TextWriter"/>
		/// </summary>
		/// <param name="writer">The writer</param>
		/// <param name="options">A <see cref="DotGraphOptions"/> instance with any options, or null to use the defaults</param>
		public void WriteDotTo(TextWriter writer, DotGraphOptions options = null)
		{
			_WriteDotTo(FillClosure(), writer, options);
		}
		/// <summary>
		/// Writes a Graphviz dot specification of the specified closure to the specified <see cref="TextWriter"/>
		/// </summary>
		/// <param name="closure">The closure of all states</param>
		/// <param name="writer">The writer</param>
		/// <param name="options">A <see cref="DotGraphOptions"/> instance with any options, or null to use the defaults</param>
		static void _WriteDotTo(IList<CharFA<TAccept>> closure, TextWriter writer, DotGraphOptions options = null)
		{
			if (null == options) options = new DotGraphOptions();
			string spfx = null == options.StatePrefix ? "q" : options.StatePrefix;
			writer.WriteLine("digraph FA {");
			writer.WriteLine("rankdir=LR");
			writer.WriteLine("node [shape=circle]");
			var finals = new List<CharFA<TAccept>>();
			var neutrals = new List<CharFA<TAccept>>();
			var accepting = FillAcceptingStates(closure, null);
			foreach (var ffa in closure)
				if (ffa.IsFinal && !ffa.IsAccepting)
					finals.Add(ffa);

			IList<CharFA<TAccept>> fromStates = null;
			IList<CharFA<TAccept>> toStates = null;

			char tchar = default(char);
			if (null != options.DebugString)
			{
				toStates = closure[0].FillEpsilonClosure();
				if (null == fromStates)
					fromStates = toStates;
				foreach (char ch in options.DebugString)
				{
					tchar = ch;
					toStates = FillMove(fromStates, ch);
					if (0 == toStates.Count)
						break;
					fromStates = toStates;
				}
			}
			if (null != toStates)
			{
				toStates = FillEpsilonClosure(toStates, null);
			}
			int i = 0;
			foreach (var ffa in closure)
			{
				if (!finals.Contains(ffa))
				{
					if (ffa.IsAccepting)
						accepting.Add(ffa);
					else if (ffa.IsNeutral)
						neutrals.Add(ffa);
				}
				var rngGrps = ffa.FillInputTransitionRangesGroupedByState(null);
				foreach (var rngGrp in rngGrps)
				{
					var di = closure.IndexOf(rngGrp.Key);
					writer.Write(spfx);
					writer.Write(i);
					writer.Write("->");
					writer.Write(spfx);
					writer.Write(di.ToString());
					writer.Write(" [label=\"");
					var sb = new StringBuilder();
					foreach (var range in rngGrp.Value)
						_AppendRangeTo(sb, range);
					if (sb.Length != 1 || " " == sb.ToString())
					{
						writer.Write('[');
						writer.Write(_EscapeLabel(sb.ToString()));
						writer.Write(']');
					}
					else
						writer.Write(_EscapeLabel(sb.ToString()));
					writer.WriteLine("\"]");
				}
				// do epsilons
				foreach (var fffa in ffa.EpsilonTransitions)
				{
					writer.Write(spfx);
					writer.Write(i);
					writer.Write("->");
					writer.Write(spfx);
					writer.Write(closure.IndexOf(fffa));
					writer.WriteLine(" [style=dashed,color=gray]");
				}
				++i;
			}
			string delim = "";
			i = 0;
			foreach (var ffa in closure)
			{
				writer.Write(spfx);
				writer.Write(i);
				writer.Write(" [");
				if (null != options.DebugString)
				{
					if (null != toStates && toStates.Contains(ffa))
						writer.Write("color=green,");
					else if (null != fromStates && fromStates.Contains(ffa) && (null == toStates || !toStates.Contains(ffa)))
						writer.Write("color=darkgreen,");
				}
				writer.Write("label=<");
				writer.Write("<TABLE BORDER=\"0\"><TR><TD>");
				writer.Write(spfx);
				writer.Write("<SUB>");
				writer.Write(i);
				writer.Write("</SUB></TD></TR>");

				if (null != options.DebugSourceNfa && null != ffa.Tag)
				{
					var tags = ffa.Tag as IEnumerable;
					if (null != tags || ffa.Tag is CharFA<TAccept>)
					{
						writer.Write("<TR><TD>{");
						if (null == tags)
						{
							writer.Write(" q<SUB>");
							writer.Write(options.DebugSourceNfa.FillClosure().IndexOf((CharFA<TAccept>)ffa.Tag).ToString());
							writer.Write("</SUB>");
						}
						else
						{
							delim = "";
							foreach (var tag in tags)
							{
								writer.Write(delim);
								if (tag is CharFA<TAccept>)
								{
									writer.Write(delim);
									writer.Write(" q<SUB>");
									writer.Write(options.DebugSourceNfa.FillClosure().IndexOf((CharFA<TAccept>)tag).ToString());
									writer.Write("</SUB>");
									// putting a comma here is what we'd like
									// but it breaks dot no matter how its encoded
									delim = @" ";
								}
							}
						}
						writer.Write(" }</TD></TR>");
					}

				}
				if (ffa.IsAccepting)
				{
					writer.Write("<TR><TD>");
					writer.Write(Convert.ToString(ffa.AcceptSymbol).Replace("\"", "&quot;"));
					writer.Write("</TD></TR>");

				}
				writer.Write("</TABLE>");
				writer.Write(">");
				bool isfinal = false;
				if (accepting.Contains(ffa) || (isfinal = finals.Contains(ffa)))
					writer.Write(",shape=doublecircle");
				if (isfinal || neutrals.Contains(ffa))
				{
					if ((null == fromStates || !fromStates.Contains(ffa)) &&
						(null == toStates || !toStates.Contains(ffa)))
					{
						writer.Write(",color=gray");
					}
				}
				writer.WriteLine("]");
				++i;
			}
			delim = "";
			if (0 < accepting.Count)
			{
				foreach (var ntfa in accepting)
				{
					writer.Write(delim);
					writer.Write(spfx);
					writer.Write(closure.IndexOf(ntfa));
					delim = ",";
				}
				writer.WriteLine(" [shape=doublecircle]");
			}
			delim = "";
			if (0 < neutrals.Count)
			{

				foreach (var ntfa in neutrals)
				{
					if ((null == fromStates || !fromStates.Contains(ntfa)) &&
						(null == toStates || !toStates.Contains(ntfa))
						)
					{
						writer.Write(delim);
						writer.Write(spfx);
						writer.Write(closure.IndexOf(ntfa));
						delim = ",";
					}
				}
				writer.WriteLine(" [color=gray]");
				delim = "";
			}
			delim = "";
			if (0 < finals.Count)
			{
				foreach (var ntfa in finals)
				{
					writer.Write(delim);
					writer.Write(spfx);
					writer.Write(closure.IndexOf(ntfa));
					delim = ",";
				}
				writer.WriteLine(" [shape=doublecircle,color=gray]");
			}

			writer.WriteLine("}");
		}
		/// <summary>
		/// Renders Graphviz output for this machine to the specified file
		/// </summary>
		/// <param name="filename">The output filename. The format to render is indicated by the file extension.</param>
		/// <param name="options">A <see cref="DotGraphOptions"/> instance with any options, or null to use the defaults</param>
		public void RenderToFile(string filename, DotGraphOptions options = null)
		{
			if (null == options)
				options = new DotGraphOptions();
			string args = "-T";
			string ext = Path.GetExtension(filename);
			if (0 == string.Compare(".png", ext, StringComparison.InvariantCultureIgnoreCase))
				args += "png";
			else if (0 == string.Compare(".jpg", ext, StringComparison.InvariantCultureIgnoreCase))
				args += "jpg";
			else if (0 == string.Compare(".bmp", ext, StringComparison.InvariantCultureIgnoreCase))
				args += "bmp";
			else if (0 == string.Compare(".svg", ext, StringComparison.InvariantCultureIgnoreCase))
				args += "svg";
			if (0 < options.Dpi)
				args += " -Gdpi=" + options.Dpi.ToString();

			args += " -o\"" + filename + "\"";

			var psi = new ProcessStartInfo("dot", args)
			{
				CreateNoWindow = true,
				UseShellExecute = false,
				RedirectStandardInput = true
			};
			using (var proc = Process.Start(psi))
			{
				WriteDotTo(proc.StandardInput, options);
				proc.StandardInput.Close();
				proc.WaitForExit();
			}

		}
		/// <summary>
		/// Renders Graphviz output for this machine to a stream
		/// </summary>
		/// <param name="format">The output format. The format to render can be any supported dot output format. See dot command line documation for details.</param>
		/// <param name="copy">True to copy the stream, otherwise false</param>
		/// <param name="options">A <see cref="DotGraphOptions"/> instance with any options, or null to use the defaults</param>
		/// <returns>A stream containing the output. The caller is expected to close the stream when finished.</returns>
		public Stream RenderToStream(string format, bool copy = false, DotGraphOptions options = null)
		{
			if (null == options)
				options = new DotGraphOptions();
			string args = "-T";
			args += string.Concat(" ", format);
			if (0 < options.Dpi)
				args += " -Gdpi=" + options.Dpi.ToString();

			var psi = new ProcessStartInfo("dot", args)
			{
				CreateNoWindow = true,
				UseShellExecute = false,
				RedirectStandardInput = true,
				RedirectStandardOutput = true
			};
			using (var proc = Process.Start(psi))
			{
				WriteDotTo(proc.StandardInput, options);
				proc.StandardInput.Close();
				if (!copy)
					return proc.StandardOutput.BaseStream;
				else
				{
					MemoryStream stm = new MemoryStream();
					proc.StandardOutput.BaseStream.CopyTo(stm);
					proc.StandardOutput.BaseStream.Close();
					proc.WaitForExit();
					return stm;
				}
			}
		}
		static void _AppendRangeTo(StringBuilder builder, CharRange range)
		{
			_AppendRangeCharTo(builder, range.First);
			if (0 == range.Last.CompareTo(range.First)) return;
			if (range.Last == range.First + 1) // spit out 1 length ranges as two chars
			{
				_AppendRangeCharTo(builder, range.Last);
				return;
			}
			builder.Append('-');
			_AppendRangeCharTo(builder, range.Last);
		}
		static void _AppendRangeCharTo(StringBuilder builder, char rangeChar)
		{
			switch (rangeChar)
			{
				case '-':
				case '\\':
					builder.Append('\\');
					builder.Append(rangeChar);
					return;
				case '\t':
					builder.Append("\\t");
					return;
				case '\n':
					builder.Append("\\n");
					return;
				case '\r':
					builder.Append("\\r");
					return;
				case '\0':
					builder.Append("\\0");
					return;
				case '\f':
					builder.Append("\\f");
					return;
				case '\v':
					builder.Append("\\v");
					return;
				case '\b':
					builder.Append("\\b");
					return;
				default:
					if (!char.IsLetterOrDigit(rangeChar) && !char.IsSeparator(rangeChar) && !char.IsPunctuation(rangeChar) && !char.IsSymbol(rangeChar))
					{

						builder.Append("\\u");
						builder.Append(unchecked((ushort)rangeChar).ToString("x4"));

					}
					else
						builder.Append(rangeChar);
					break;
			}
		}
		static string _EscapeLabel(string label)
		{
			if (string.IsNullOrEmpty(label)) return label;

			string result = label.Replace("\\", @"\\");
			result = result.Replace("\"", "\\\"");
			result = result.Replace("\n", "\\n");
			result = result.Replace("\r", "\\r");
			result = result.Replace("\0", "\\0");
			result = result.Replace("\v", "\\v");
			result = result.Replace("\t", "\\t");
			result = result.Replace("\f", "\\f");
			return result;
		}

	}
}
