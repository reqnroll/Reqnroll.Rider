using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Psi;

public class GherkinExamplesBlock() : GherkinElement(GherkinNodeTypes.EXAMPLES_BLOCK)
{

    public IDictionary<string, string> GetExampleData(int exampleIndex)
    {
        var table = this.Children<GherkinTable>().FirstOrDefault();
        if (table == null)
            return ImmutableDictionary<string, string>.Empty;

        return table.GetValuesOfRow(exampleIndex);
    }
}