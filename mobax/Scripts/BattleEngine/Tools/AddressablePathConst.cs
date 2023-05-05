public static class AddressablePathConst
{
    //Behaviors
    public static string BehaviorsXmlPath = "{0}.xml";
    public static string BehaviorsBsonPath = "{0}.bson.bytes";

    public static string SkillConfServerPath = "../Assets/res/$Data_ahead/SkillConfigs/JSON/{0}.json";
    public static string SkillConfPath = "{0}.asset";

    public static string SkillEditorPathParse(string res)
    {
        if (string.IsNullOrEmpty(res))
        {
            return "";
        }
        if (res.Contains("/"))
        {
            string[] path = res.Split('/');
            return path[path.Length - 1];
        }
        else
        {
            return res;
        }
    }
}