using System.Collections.Generic;
using NPOI.SS.UserModel;

namespace DrakeToolbox.Blueprints
{
    internal sealed class BlueprintData
    {
        internal string this[string blueprintID, string parameter] => rawContent[blueprintIds.IndexOf(blueprintID) + 1, parameters.IndexOf(parameter) + 1];

        private readonly string[,] rawContent;
        private readonly List<string> blueprintIds;
        private readonly List<string> parameters;

        public List<string> BlueprintIds => blueprintIds;
        public List<string> Parameters => parameters;

        public BlueprintData(ISheet sheet)
        {
            int maxRow = 0;
            int maxCol = 0;

            for (int row = sheet.FirstRowNum; row <= sheet.LastRowNum; row++)
            {
                IRow sheetRow = sheet.GetRow(row);
                if (sheetRow == null)
                    continue;

                for (int col = sheetRow.FirstCellNum; col < sheetRow.LastCellNum; col++)
                {
                    ICell cell = sheetRow.GetCell(col);
                    if (cell == null)
                        continue;

                    if (cell.CellType == CellType.Blank)
                        continue;

                    if (row + 1 > maxRow)
                        maxRow = row + 1;

                    if (col + 1 > maxCol)
                        maxCol = col + 1;
                }
            }

            rawContent = new string[maxRow, maxCol];
            for (int row = sheet.FirstRowNum; row <= sheet.LastRowNum; row++)
            {
                IRow sheetRow = sheet.GetRow(row);
                if (sheetRow == null)
                    continue;

                for (int col = sheetRow.FirstCellNum; col < sheetRow.LastCellNum; col++)
                {
                    ICell cell = sheetRow.GetCell(col);
                    if (cell == null)
                        continue;

                    if (cell.CellType == CellType.Blank)
                        continue;

                    rawContent[row, col] = cell.ToString();
                }
            }

            blueprintIds = new List<string>();
            for (int i = 1; i < rawContent.GetLength(0); i++)
            {
                blueprintIds.Add(rawContent[i, 0]);
            }

            parameters = new List<string>();
            for (int i = 1; i < rawContent.GetLength(1); i++)
            {
                parameters.Add(rawContent[0, i]);
            }
        }
    }
}