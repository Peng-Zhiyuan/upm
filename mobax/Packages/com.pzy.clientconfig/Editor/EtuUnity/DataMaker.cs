using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System;
using System.Data;
using CustomLitJson;
using OfficeOpenXml;
//using System.Diagnostics;
using UnityEngine;

namespace EtuUnity
{
    public enum TableType
    {
        Normal,
        Array,
        Nkv,
    }

    public enum TableKind
    {
        Both,
        Client,
        Server,
    }

    public class DataMaker
    {
        // 数据表最小行数
        const int MIN_ROW_COUNT = 5;
        // 字段所在行的索引
        const int FEILD_NAME_ROW_INDEX = 2;
        // 字段类型所在行的索引
        const int FEILD_TYPE_ROW_INDEX = 1;
        // 数据行开始的行索引
        const int VALUE_BEGIN_ROW_INDEX = 4;
        // 描述所在行索引
        const int DES_TYPE_ROW_INDEX = 3;


        private static DataMaker _instance;
        public static DataMaker Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DataMaker();
                }
                return _instance;
            }
        }



        public void Build(string dir, EtuBuildResult result)
        {

            DmHelper.MagicNum = 1;

            DmHelper.DataNameGlobaList.Clear();

            var fileList = Directory.GetFiles(dir);

            foreach (string filePath in fileList)
            {

                var fi = new FileInfo(filePath);
                if (fi.Extension != ".xlsx")
                {
                    continue;
                }
                if (fi.Name.StartsWith("~"))
                {
                    continue;
                }

                string fileNameWithoutExtension = fi.Name.Replace(".xlsx", "");
                var package = OpenExcelFile(filePath);
                if (package.Workbook == null)
                {
                    continue;
                }
                if (package.Workbook.Worksheets == null)
                {
                    continue;
                }
                if (package.Workbook.Worksheets.Count <= 0)
                {
                    continue;
                }



                var hasCache = CacheManager.HasCache(filePath);
                //var hasCache = false;
                if (hasCache)
                {
                    var tableResultList = CacheManager.ReadCache(filePath);
                    foreach (var tableResult in tableResultList)
                    {
                        var tableName = tableResult.name;
                        result.tableResultDic[tableName] = tableResult;
                        result.successCount++;
                    }
                    Debug.Log($"{filePath} read result from cache");
                }
                else
                {
                    var theFileTabelResultList = new List<TableResult>();
                    var theFileExceptionLit = new List<ExceptionInfo>();
                    foreach (var excelSheet in package.Workbook.Worksheets)
                    {
                        try
                        {
                            var tableReuslt = ProcessSheet(fileNameWithoutExtension, excelSheet);
                            // 这个表不是数据表
                            if (tableReuslt == null)
                            {
                                continue;
                            }
                            var tableName = tableReuslt.name;
                            result.tableResultDic[tableName] = tableReuslt;
                            result.successCount++;

                            theFileTabelResultList.Add(tableReuslt);
                        }
                        catch (Exception e)
                        {
                            Debug.Log($"[ETU] {fi.Name} convert fail");
                            Debug.Log(e.Message);
                            Debug.Log(e.StackTrace);
                            result.failCount++;
                            var exceptionInfo = new ExceptionInfo();
                            exceptionInfo.e = e;
                            exceptionInfo.fileName = fileNameWithoutExtension;
                            exceptionInfo.sheet = excelSheet.Name;
                            result.exceptionList.Add(exceptionInfo);

                            theFileExceptionLit.Add(exceptionInfo);
                        }
                    }

                    // 缓存
                    if (theFileExceptionLit.Count == 0)
                    {
                        CacheManager.WriteCache(filePath, theFileTabelResultList);
                    }
                }



                int _lindex = filePath.LastIndexOf('/');
                string _file_name = filePath.Substring(_lindex + 1);
                PlayerPrefs.SetString(_file_name, File.GetLastWriteTime(filePath).Ticks.ToString());
                package.Dispose();
            }

            Debug.Log($"[ETU] success: {result.successCount}, fail: {result.failCount}");

            PlayerPrefs.Save();

        }


        ExcelPackage OpenExcelFile(string file)
        {
            var fileInfo = new FileInfo(file);
            var package = new ExcelPackage(fileInfo);
            return package;
        }

        // pzy: not complete
        // read excel as DataSet
        DataSet ExcelUtility(string file)
        {
            if (!File.Exists(file))
            {
                Console.Write("can't find excelFile" + file);
            }
            FileStream mStream = File.Open(file, FileMode.Open, FileAccess.Read);
            // IExcelDataReader mExcelReader = ExcelReaderFactory.CreateOpenXmlReader(mStream);
            // var dataSet =  mExcelReader.AsDataSet();
            // return dataSet;
            return null;
        }

        public TableResult ProcessSheet(string fileNameWithoutExtension, ExcelWorksheet excelSheet)
        {

            Debug.Log($"file: {fileNameWithoutExtension}");
            Debug.Log($"sheet: {excelSheet.Name}");
            string code = null;
            if (excelSheet.Dimension == null)
            {
                Debug.Log($"sheet.Dimension == null");
                return null;
            }
            if (excelSheet.Dimension.Rows < MIN_ROW_COUNT)
            {
                Debug.Log($"sheet.Dimension.Rows < MIN_ROW_COUNT");
                return null;
            }

            var cell = GetSheetCell(excelSheet, 0, 0);
            string tableName = cell;

            if (string.IsNullOrEmpty(tableName))
            {
                Debug.Log($"string.IsNullOrEmpty(tableName)");
                return null;
            }

            // if(excelSheet.Dimension.Columns > 3)
            // {
            // 	var value = GetSheetCell(excelSheet, 0, 3);
            // 	if(value != "")
            // 	{

            // 		continue;
            // 	}
            // }

            DmHelper.DataBankList.Clear();
            DmHelper.DataNameBankList.Clear();
            var tableType = TableType.Normal;

            var typeString = GetSheetCell(excelSheet, 0, 1);
            if (typeString == "array")
            {
                tableType = TableType.Array;
            }
            else if (typeString == "nkv" || typeString == "kv")
            {
                tableType = TableType.Nkv;
            }
            else
            {
                tableType = TableType.Normal;
            }

            TableKind kind;
            {
                var kindString = GetSheetCell(excelSheet, 0, 2);
                if (kindString == "")
                {
                    kind = TableKind.Both;
                }
                else if (kindString == "client")
                {
                    kind = TableKind.Client;
                }
                else if (kindString == "server")
                {
                    kind = TableKind.Server;
                }
                else
                {
                    kind = TableKind.Both;
                }

            }
            Debug.Log($"kind: {kind}");

            var kvTableValueCSharpTypeString = "";
            if (tableType == TableType.Nkv)
            {
                var define = GetSheetCell(excelSheet, 1, 1);
                if (define == "")
                {
                    define = GetSheetCell(excelSheet, 1, 2);
                }
                if (define == "")
                {
                    define = GetSheetCell(excelSheet, 1, 3);
                }
                if (define == "")
                {
                    define = GetSheetCell(excelSheet, 1, 4);
                }

                if (define == "number" || define == "float")
                {
                    kvTableValueCSharpTypeString = "float";
                }
                else if (define == "int")
                {
                    kvTableValueCSharpTypeString = "int";
                }
                else
                {
                    kvTableValueCSharpTypeString = "string";
                }
            }

            int rowCount = excelSheet.Dimension.Rows;
            int colCount = excelSheet.Dimension.Columns;

            var MAX_COUNT = 200;
            if (colCount > MAX_COUNT)
            {
                throw new Exception($"违章了！表列数最多 {MAX_COUNT}，现在有 {colCount}");
            }

            // 读取字段名称列表
            List<string> feildNameList = new List<string>();

            for (int i = excelSheet.Dimension.Columns - 1; i >= 0; i--)
            {
                string feild = GetSheetCell(excelSheet, FEILD_NAME_ROW_INDEX, i);
                feildNameList.Add(feild);
            }




            string keyFeildType = GetSheetCell(excelSheet, FEILD_TYPE_ROW_INDEX, 0);


            List<string> idList = new List<string>();
            Dictionary<string, List<string>> _dic = new Dictionary<string, List<string>>();

            // 读取所有id
            for (int i = VALUE_BEGIN_ROW_INDEX; i < rowCount; i++)
            {
                var id = GetSheetCell(excelSheet, i, 0);
                if (id == "")
                {
                    continue;
                }

                if (!idList.Contains(id))
                {
                    idList.Add(id);
                }
            }


            // 解析每一行的数据
            var rowList = new List<DataObject>();
            for (int i = VALUE_BEGIN_ROW_INDEX; i < rowCount; i++)
            {
                var id = GetSheetCell(excelSheet, i, 0);
                if (id == "")
                {
                    continue;
                }


                var excelRow = excelSheet.Row(i);
                var defineString = GetSheetCell(excelSheet, FEILD_NAME_ROW_INDEX, 0);
                var rowId = GetSheetCell(excelSheet, i, 0);
                DataObject dataObject = new DataObject(rowId);
                dataObject.isRow = true;
                ParseCellThenRecursiveRight(dataObject, excelSheet, excelRow, 0, defineString, colCount, true, tableName);
                rowList.Add(dataObject);
                //tableRoot.Add(dataObject);

                //if(i == VALUE_BEGIN_ROW_INDEX)
                {
                    if (code == null)
                    {
                        code = dataObject.ToCSharpCode();
                    }
                }
            }

            // 根据类型，拼接表对象
            if (tableName == "skill")
            {
                Debug.Log(tableName);
            }

            var tableObject = Util.CreateTable(rowList, tableType);
            var json = tableObject.ToJson();

            Debug.Log($"tableName: {tableName}");
            Debug.Log($"type: {tableType}");
            Debug.Log(json);
            var jd = JsonMapper.Instance.ToObject(json);

            var clazzName = Util.FirstChaUp(tableName);
            code = GenerateCSharp(tableName, clazzName, tableType, code, fileNameWithoutExtension);
            // generate code





            var tableResult = new TableResult();
            tableResult.code = code;
            tableResult.codeFileName = clazzName + "Row.cs";
            tableResult.name = tableName;
            tableResult.tableType = tableType;
            tableResult.kind = kind;
            tableResult.rowClazzName = clazzName;
            tableResult.kvTableValueCsharpType = kvTableValueCSharpTypeString;
            tableResult.jd = jd;
            //result.tableResultDic[tableName] = tableResult;


            return tableResult;


        }


        private static string GenerateCSharp(string tableName, string clazzName, TableType tableType, string code, string fileNameWithoutExtension)
        {
            if (tableType == TableType.Array)
            {
                string[] _sss = code.Split('\n');
                string _sub_str = "";
                for (int i = 0; i < _sss.Length; ++i)
                {
                    _sub_str += _sss[i];
                    _sub_str += "\n";
                }
                _sub_str = "public partial class " + clazzName + "RowSub\n{\n" + _sub_str + "\n}";
                string _main_str = "";
                //_main_str=_sss[0];
                _main_str = "";
                _main_str += "\n\tpublic List<" + clazzName + "RowSub> Coll;";
                _main_str = "public partial class " + clazzName + "Row\n{\n" + _main_str + "\n}";

                code = _sub_str + "\n" + _main_str;
            }
            else
            {
                code = "public partial class " + clazzName + "Row\n{\n" + code + "\n}";
            }


            string _before_info = "";
            for (int i = 0; i < DmHelper.DataBankList.Count; i++)
            {
                string _key_name = DmHelper.DataNameBankList[i];

                if (DmHelper.DataNameGlobaList.Contains(_key_name))
                {
                    continue;
                }
                else
                {
                    DmHelper.DataNameGlobaList.Add(_key_name);
                }

                string _detail = DmHelper.DataBankList[i].ToCSharpCode();
                string _des_end = DmHelper.DataBankList[i].GetDesEnd();

                _before_info += "public partial class " + _key_name + "\n{\n" + _detail + "\n}\n";
                int _index = code.IndexOf("public " + _key_name);
                if (_index >= 0)
                {
                    int _lindex = code.IndexOf('\n', _index);
                    string _sub_front = code.Substring(0, _lindex);
                    string _sub_end = code.Substring(_lindex, code.Length - _sub_front.Length);
                    if (!string.IsNullOrEmpty(_des_end))
                    {
                        code = _sub_front + "//" + _des_end + _sub_end;
                    }
                }
            }
            _before_info += "\n";


            code = _before_info + code;


            if (code.Contains("List<"))
            {
                code = "using System.Collections.Generic;\n" + code;
            }
            code = $"//===========Generate by etuclisharp=============\n//Excel:" + fileNameWithoutExtension + " TableName:" + tableName + $"\nnamespace EtuUnity\n{{\n{code}}}\n";

            return code;
        }

        // enum ColumType
        // {

        // }

        // private static void IndicateColumnType()
        // {
        //     ArrayStart
        // }

        public static void ParseCellThenRecursiveRight(RjElement element, ExcelWorksheet excelSheet, ExcelRow excelRow, int filedIndex, string defineString, int fieldCount, bool needTakeValue, string tableName)
        {
            // if(tableName == "skill")
            // {
            //     Debug.Log("skill");
            // }
            if (defineString == "" && element.GetClassType() != RjClassType.COLL)
            {
                var hasNextField = filedIndex + 1 < fieldCount;
                if (hasNextField)
                {
                    var nextFieldName = GetSheetCell(excelSheet, FEILD_NAME_ROW_INDEX, filedIndex + 1);
                    ParseCellThenRecursiveRight(element, excelSheet, excelRow, filedIndex + 1, nextFieldName, fieldCount, true, tableName);
                }
                return;
            }
            // filter " and :
            defineString = defineString.Replace("\"", "");
            defineString = defineString.Replace(":", "");

            for (int i = 0; i < defineString.Length; i++)
            {
                if (defineString[i] == '[')
                {
                    string collectionName = defineString.Substring(0, i);
                    string restString = defineString.Substring(i + 1, defineString.Length - i - 1);


                    // 这是一个数组
                    // 获得数组的修饰符
                    var modifier = GetSheetCell(excelSheet, 0, filedIndex);



                    RjCollection collection = new RjCollection(collectionName, modifier);
                    element.AddElement(collection);
                    if (string.IsNullOrEmpty(restString))
                    {
                        //must add the value
                        CreateValue(collection, excelSheet, excelRow, filedIndex, null);
                        var definesString = GetSheetCell(excelSheet, FEILD_NAME_ROW_INDEX, filedIndex + 1);
                        ParseCellThenRecursiveRight(collection, excelSheet, excelRow, filedIndex + 1, definesString, fieldCount, true, tableName);
                    }
                    else
                    {
                        ParseCellThenRecursiveRight(collection, excelSheet, excelRow, filedIndex, restString, fieldCount, true, tableName);
                    }
                    return;
                }
                else if (defineString[i] == '{')
                {
                    var parts = defineString.Split('{');
                    var name = parts[0];
                    var restDefine = parts[1];
                    DataObject obj = new DataObject(name);
                    element.AddElement(obj);
                    ParseCellThenRecursiveRight(obj, excelSheet, excelRow, filedIndex, restDefine, fieldCount, true, tableName);
                    return;
                }
                else if (defineString[i] == ']')
                {
                    if (needTakeValue)
                    {
                        CreateValue(element, excelSheet, excelRow, filedIndex, null);
                    }
                    element = element.GetPerent();
                    if (defineString.Length == 1)
                    {
                        if (filedIndex < fieldCount - 1)
                        {
                            ParseCellThenRecursiveRight(element, excelSheet, excelRow, filedIndex + 1, GetSheetCell(excelSheet, FEILD_NAME_ROW_INDEX, filedIndex + 1), fieldCount, true, tableName);
                        }
                    }
                    else
                    {
                        string _key_after = defineString.Substring(i + 1, defineString.Length - i - 1);
                        ParseCellThenRecursiveRight(element, excelSheet, excelRow, filedIndex, _key_after, fieldCount, false, tableName);
                    }
                    return;
                }
                else if (defineString[i] == '}')
                {
                    string _key_before = defineString.Substring(0, i);
                    string _key_after = defineString.Substring(i + 1, defineString.Length - i - 1);
                    bool _need_take = true;
                    if (!string.IsNullOrEmpty(_key_before))
                    {
                        CreateValue(element, excelSheet, excelRow, filedIndex, _key_before);
                        _need_take = false;
                    }
                    element = element.GetPerent();
                    if (string.IsNullOrEmpty(_key_after))
                    {
                        if (filedIndex < fieldCount - 1)
                        {
                            ParseCellThenRecursiveRight(element, excelSheet, excelRow, filedIndex + 1, GetSheetCell(excelSheet, FEILD_NAME_ROW_INDEX, filedIndex + 1), fieldCount, true, tableName);
                        }
                    }
                    else
                    {
                        ParseCellThenRecursiveRight(element, excelSheet, excelRow, filedIndex, _key_after, fieldCount, _need_take, tableName);
                    }
                    return;
                }
            }

            CreateValue(element, excelSheet, excelRow, filedIndex, defineString);

            {
                var hasNextField = filedIndex + 1 < fieldCount;
                if (hasNextField)
                {
                    ParseCellThenRecursiveRight(element, excelSheet, excelRow, filedIndex + 1, GetSheetCell(excelSheet, FEILD_NAME_ROW_INDEX, filedIndex + 1), fieldCount, true, tableName);
                }
            }

        }


        public static void CreateValue(RjElement element, ExcelWorksheet sheet, ExcelRow row, int collumIndex, string key)
        {


            //var fieldName = GetSheetCell(sheet, FEILD_NAME_ROW_INDEX, collumIndex);
            var typeString = GetSheetCell(sheet, FEILD_TYPE_ROW_INDEX, collumIndex);
            RjValueType type = RjValueType.INT;
            if (typeString == "int")
            {
                type = RjValueType.INT;
            }
            else if (typeString == "float" || typeString == "number")
            {
                type = RjValueType.FLOAT;
            }
            else if (typeString == "bool")
            {
                type = RjValueType.BOOL;
            }
            else
            {
                type = RjValueType.STRING;
            }
            var valueString = GetSheetCell(sheet, row.Row, collumIndex);
            var des = GetSheetCell(sheet, DES_TYPE_ROW_INDEX, collumIndex);
            var rjValue = new RjValue(key, valueString, type, des);
            element.AddElement(rjValue);
        }

        private static string GetSheetCell(ExcelWorksheet sheet, int rowIndexBase0, int colIndexBase0)
        {
            var obj = sheet.GetValue(rowIndexBase0 + 1, colIndexBase0 + 1);
            if (obj == null)
            {
                return "";
            }
            var text = obj.ToString();
            return text;
        }
    }
}