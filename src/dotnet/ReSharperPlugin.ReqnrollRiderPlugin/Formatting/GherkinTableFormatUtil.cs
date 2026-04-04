using System;
using System.Linq;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Impl.CodeStyle;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Resources.Shell;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Formatting;

public static class GherkinTableFormatUtil
{
    private record CellData(int Width, bool IsNumeric, bool IsHeaderRow);
    private record ColumnData(int Width, bool IsNumeric);

    public static void AlignTables(ITreeNode firstElement, FmtSettingsHolder<GherkinFormatSettingsKey> formatterSettings)
    {
        var tablePaddingSize = formatterSettings.Settings.GetValue(key => key.TableCellPaddingSize);
        var rightAlignNumericCells = formatterSettings.Settings.GetValue(key => key.TableCellRightAlignNumericContent);

        var gherkinFile = firstElement.GetContainingNode<GherkinFile>();
        if (gherkinFile == null)
            return;

        foreach (var table in gherkinFile.Descendants<GherkinTable>())
        {
            var rows = table.Children<ITreeNode>()
                .Where(static child => child is GherkinTableHeaderRow or GherkinTableRow)
                .ToArray();

            if (rows.Length == 0)
                continue;

            var columnData = rows
                .Select(row => row
                    .Children<GherkinTableCell>()
                    .Select(cell => new CellData(cell.GetText().Length, decimal.TryParse(cell.GetText(), out _), row is GherkinTableHeaderRow))
                    .ToArray())
                .Aggregate(
                    Array.Empty<ColumnData>(),
                    static (colData, cellData) =>
                    {
                        var currentSize = colData.Length;
                        var newSize = cellData.Length;

                        if (currentSize < newSize)
                        {
                            Array.Resize(ref colData, cellData.Length);
                            for (var i = currentSize; i < newSize; i++)
                            {
                                // Simpler to assume all columns are numeric columns to begin with if they're headers
                                // If they're not, then the rows will override that value
                                colData[i] = new ColumnData(cellData[i].Width, IsNumeric: cellData[i].IsNumeric || cellData[i].IsHeaderRow);
                            }
                        }

                        for (var i = 0; i < cellData.Length; i++)
                        {
                            if (colData[i].Width < cellData[i].Width)
                                colData[i] = colData[i] with { Width = cellData[i].Width };
                            if (colData[i].IsNumeric && !cellData[i].IsNumeric && !cellData[i].IsHeaderRow) // Only care about IsNumeric when it's not a header row
                                colData[i] = colData[i] with { IsNumeric = false };
                        }

                        return colData;
                    });

            foreach (var row in rows)
            {
                AlignRow(row, columnData, tablePaddingSize, rightAlignNumericCells);
            }
        }
    }

    private static void AlignRow(ITreeNode row, ColumnData[] columnData, int tablePaddingSize, bool rightAlignNumericCells)
    {
        var cells = row.Children<GherkinTableCell>().ToArray();

        for (var i = 0; i < cells.Length; i++)
        {
            var cell = cells[i];
            var cellText = cell.GetText();
            var width = columnData[i].Width;
            var isHeaderRow = row is GherkinTableHeaderRow;

            var isRightAligned = rightAlignNumericCells && columnData[i].IsNumeric && !isHeaderRow;

            var extraPadding = Math.Max(0, width - cellText.Length);
            var leftPadding = tablePaddingSize + (isRightAligned ? extraPadding : 0);
            var rightPadding = tablePaddingSize + (isRightAligned ? 0 : extraPadding);

            var leftPipe = FindPipeBefore(cell);
            var rightPipe = FindPipeAfter(cell);

            if (leftPipe != null)
                SetWhitespaceBetween(leftPipe, cell, leftPadding);
            if (rightPipe != null)
                SetWhitespaceBetween(cell, rightPipe, rightPadding);
        }
    }

    private static void SetWhitespaceBetween(ITreeNode left, ITreeNode right, int leftPadding)
    {
        var whitespace = new string(' ', leftPadding);
        var newWhitespace = GherkinTokenTypes.WHITE_SPACE.CreateLeafElement(whitespace);

        var current = left.NextSibling;
        ITreeNode firstToRemove = null;
        ITreeNode lastToRemove = null;

        while (current != null && current != right)
        {
            if (current.NodeType == GherkinTokenTypes.WHITE_SPACE)
            {
                firstToRemove ??= current;
                lastToRemove = current;
            }

            current = current.NextSibling;
        }

        using (WriteLockCookie.Create(left.IsPhysical()))
        {
            if (firstToRemove != null)
            {
                ModificationUtil.ReplaceChild(firstToRemove, newWhitespace);

                if (lastToRemove == firstToRemove)
                    return;

                var node = newWhitespace.NextSibling;
                while (node != null && node != right)
                {
                    var next = node.NextSibling;
                    if (node.NodeType == GherkinTokenTypes.WHITE_SPACE)
                        ModificationUtil.DeleteChild(node);
                    node = next;
                }
            }
            else
            {
                ModificationUtil.AddChildAfter(left, newWhitespace);
            }
        }
    }

    private static ITreeNode FindPipeBefore(ITreeNode cell)
    {
        while (cell.PrevSibling is not null && cell.PrevSibling.NodeType != GherkinTokenTypes.PIPE)
            cell = cell.PrevSibling;
        return cell.PrevSibling;
    }

    private static ITreeNode FindPipeAfter(ITreeNode cell)
    {
        while (cell.NextSibling is not null && cell.NextSibling.NodeType != GherkinTokenTypes.PIPE)
            cell = cell.NextSibling;
        return cell.NextSibling;
    }
}