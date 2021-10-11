using System;
using System.Collections.Generic;

namespace Loxodon.Framework.Bundles.Editors
{
    [System.Obsolete("This class is deprecated. Use PublishBundleModifier instead.", false)]
    public class InvertedIndexBundleModifier : IBundleModifier
    {
        private List<string> needIndexedBundleNames;
        private Func<BundleInfo, bool> match;
        public InvertedIndexBundleModifier(List<string> needIndexedBundleNames)
        {
            this.needIndexedBundleNames = needIndexedBundleNames;
            this.match = info => this.needIndexedBundleNames.Contains(info.FullName);
        }

        public InvertedIndexBundleModifier(Func<BundleInfo, bool> match)
        {
            this.match = match;
        }

        public virtual void Modify(BundleData bundleData)
        {
            BundleInfo bundleInfo = bundleData.BundleInfo;
            if (match != null)
                bundleInfo.Published = match(bundleInfo);
        }
    }
}
