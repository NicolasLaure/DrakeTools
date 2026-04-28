using System.Collections.Generic;
using System.IO;
using DrakeToolbox.Services;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace DrakeTools.Blueprints
{
    public sealed class BlueprintRegistry : IService
    {
        public bool IsPersistent => true;

        private readonly Dictionary<string, BlueprintData> blueprintDatas;

        public BlueprintRegistry(string blueprintPath)
        {
            blueprintDatas = new Dictionary<string, BlueprintData>();

            using (FileStream file = new FileStream(blueprintPath, FileMode.Open, FileAccess.Read))
            {
                IWorkbook workbook = new XSSFWorkbook(file);
                for (int i = 0; i < workbook.NumberOfSheets; i++)
                {
                    blueprintDatas.Add(workbook.GetSheetName(i), new BlueprintData(workbook.GetSheetAt(i)));
                }
            }
        }

        public List<string> BlueprintsOf(string blueprintTable) => blueprintDatas[blueprintTable].BlueprintIds;
        public List<string> ParametersOf(string blueprintTable) => blueprintDatas[blueprintTable].Parameters;
    }

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