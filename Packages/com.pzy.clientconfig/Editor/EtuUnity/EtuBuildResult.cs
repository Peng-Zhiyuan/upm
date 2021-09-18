using System.Collections.Generic;
using CustomLitJson;

public class EtuBuildResult
{
	public int successCount;
	public int failCount;

	public Dictionary<string, TableResult> tableResultDic = new Dictionary<string, TableResult>();
    public List<ExceptionInfo> exceptionList = new List<ExceptionInfo>();
}