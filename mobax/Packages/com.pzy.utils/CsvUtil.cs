using System.Text;
using System.Collections.Generic;

public static class CsvUtil
{
    /// <summary>
    /// 返回行的数组
    /// </summary>
    /// <param name="csvString"></param>
    /// <returns></returns>
    public static List<List<string>> ParseCsv(string csvString)
    {
        var table = new List<List<string>>();
        var cellOfCurrentLine = new List<string>();
        var tokenSb = new StringBuilder();
        var inQoute = false;
        for(int i = 0; i < csvString.Length; i++)
        {
            var ch = csvString[i];
            if(ch == '"')
            {
                if(!inQoute)
                {
                    inQoute = true;
                    continue;
                }
                else
                {
                    // 判断是否转意双引号
                    var hasNext = csvString.Length > i + 1;
                    if(hasNext)
                    {
                        var nextCh = csvString[i + 1];
                        if(nextCh == '"')
                        {
                            // 内容：引号
                            i++;
                            tokenSb.Append('"');
                            continue;
                        }
                    }
                    // 结束引用
                    inQoute = false;
                    continue;
                }
            }

            if(ch == ',')
            {
                if(!inQoute)
                {
                    // token 结束
                    var token = tokenSb.ToString();
                    cellOfCurrentLine.Add(token);
                    tokenSb.Clear();
                    continue;
                }
                else
                {
                    // 内容：逗号
                    tokenSb.Append(ch);
                    continue;
                }
            }

            if(ch == '\n')
            {
                if(inQoute)
                {
                    // 内容：换行
                    tokenSb.Append(ch);
                    continue;
                }
                else
                {
                    // token 结束
                    var token = tokenSb.ToString();
                    cellOfCurrentLine.Add(token);
                    tokenSb.Clear();

                    // 一行结束
                    table.Add(cellOfCurrentLine);
                    cellOfCurrentLine = new List<string>();
                    continue;
                }
            }

            if(ch == '\r')
            {
                // 丢弃
                continue;
            }

            tokenSb.Append(ch);
        }

        // token 结束
        {
            var token = tokenSb.ToString();
            cellOfCurrentLine.Add(token);
            tokenSb.Clear();

            // 一行结束
            table.Add(cellOfCurrentLine);
        }


        return table;
    }

    public static string ToCsv(List<List<string>> lists){
        StringBuilder sb = new StringBuilder();
        foreach(var line in lists){
            for(int i = 0; i < line.Count; i++){
                sb.Append(GetBlock(line[i]));
                if(i != line.Count - 1){
                    sb.Append(",");
                }
            }
            sb.Append("\n");
        }
        return sb.ToString();
    }

    static string GetBlock(string s){
        if(s.Contains(",")){
            return $"\"{s}\"";
        }
        else{
            return s;
        }

    }

    public static bool IsRectangle(List<List<string>> lists){
        int count = lists[0].Count;
        foreach(var i in lists){
            if(i.Count != count){
                return false;
            }
        }
        return true;
    }

    public static List<List<string>> Transposition(List<List<string>> lists){
        if(!IsRectangle(lists)){
            throw new System.Exception("[CsvUtil] List is not rectangle");
        }
        var newList = GetRectStringListList(lists[0].Count, lists.Count);
        for(int i = 0; i < lists.Count; i++){
            for(int j = 0; j < lists[0].Count; j++){
                newList[j][i] = lists[i][j];
            }
        }

        return newList;
    }

    public static List<List<string>> GetRectStringListList(int row, int col){
        List<List<string>> temp = new List<List<string>>();
        for(int i = 0; i < row; i++){
            var line = new List<string>();
            for(int j = 0; j < col; j++){
                line.Add("");
            }
            temp.Add(line);
        }
        return temp;
    }
}