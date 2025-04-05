using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Psi;

public class GherkinTable() : GherkinElement(GherkinNodeTypes.TABLE)
{

    public IEnumerable<string> GetColumnNames()
    {
        if (!(FirstChild is GherkinTableHeaderRow header))
            return EmptyList<string>.Instance;
        return header.Children<GherkinTableCell>().Select(x => x.GetText().ToString());
    }

    public IDictionary<string, string> GetValuesOfRow(int rowIndex)
    {
        var row = this.Children<GherkinTableRow>().Skip(rowIndex).FirstOrDefault();
        if (row == null)
            return ImmutableDictionary<string, string>.Empty;

        var columnNames = GetColumnNames();

        var data = new Dictionary<string, string>();
        var cells = row.ChildrenEnumerator<GherkinTableCell>();
        foreach (var columnName in columnNames)
        {
            if (!cells.MoveNext())
                break;
            data[columnName] = cells.Current.GetText();
        }

        return data;
    }
}