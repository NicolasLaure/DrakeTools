using System;
using System.Collections.Generic;
using System.IO;
using DrakeToolbox.Cast;
using DrakeToolbox.Services;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace DrakeToolbox.Blueprints
{
    public sealed class BlueprintRegistry : IService
    {
        public bool IsPersistent => true;

        private readonly Dictionary<string, BlueprintData> blueprintDatas;

        internal Dictionary<string, BlueprintData> BlueprintDatas => blueprintDatas;

        public List<string> BlueprintsOf(string blueprintTable) => blueprintDatas[blueprintTable].BlueprintIds;
        public List<string> ParametersOf(string blueprintTable) => blueprintDatas[blueprintTable].Parameters;

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

        public ValueType GetDataAt<ValueType>(string blueprintTable, string blueprintId, string parameter)
        {
            return (ValueType)StringCast.Convert(blueprintDatas[blueprintTable][blueprintId, parameter], typeof(ValueType));
        }
    }
}