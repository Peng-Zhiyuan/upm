namespace Loxodon.Framework.Bundles
{
    public class LocalPathInfoParser : IPathInfoParser
    {
        public virtual AssetPathInfo Parse(string path)
        {
            UnityEngine.Debug.Log(path);
            return new AssetPathInfo("", path);
        }
    }
}
