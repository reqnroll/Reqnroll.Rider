using System;
using System.Collections.Generic;
using System.Text;

namespace RE
{
	partial class CharFA<TAccept>
	{
		/// <summary>
		/// Reduces the complexity of the graph, and returns the result as a new graph
		/// </summary>
		/// <returns>A new graph with a complexity of 1</returns>
		public CharFA<TAccept> Reduce(IProgress<CharFAProgress> progress=null)
		{
			var fa = Clone();
			while (true)
			{
				var cc = fa.FillClosure().Count;
				fa.Finalize();
				fa = fa.ToDfa(progress);
				fa.TrimDuplicates(progress);
				if (fa.FillClosure().Count == cc)
					return fa;
			}
		}
	}
}
