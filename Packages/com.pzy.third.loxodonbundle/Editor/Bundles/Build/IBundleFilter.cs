namespace Loxodon.Framework.Bundles.Editors
{
    public interface IBundleFilter
    {
        bool IsValid(BundleInfo bundleInfo);
    }
}
