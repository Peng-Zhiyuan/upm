using System;
using System.Collections.Generic;

namespace Loxodon.Framework.Bundles.Editors
{
    public class PublishBundleModifier : IBundleModifier
    {
        private List<string> needPublishedBundleNames;
        private Func<BundleInfo, bool> match;
        public PublishBundleModifier(List<string> needPublishedBundleNames)
        {
            this.needPublishedBundleNames = needPublishedBundleNames;
            this.match = info => this.needPublishedBundleNames.Contains(info.FullName);
        }

        public PublishBundleModifier(Func<BundleInfo, bool> match)
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
