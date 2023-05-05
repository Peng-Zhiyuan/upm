using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalizationService : Service
{
    public override void OnCreate()
    {
        FontLocalizer.onGetFont = OnGetFont;
    }

    Font OnGetFont(string language)
    {

        if(language == "cn")
        {
            //var ret = BucketManager.Stuff.Font.Get<Font>("TsangerYuYangT-W05.ttf");
            var ret = BucketManager.Stuff.Font.Get<Font>("SourceHanSansCN-Bold.otf");
            return ret;
        }
        else if(language == "en")
        {
            // GenBkBasB.ttf: Copyright (c) SIL International, 2003-2013.
            //var ret = BucketManager.Stuff.Font.Get<Font>("gentium-book-basic.bold.ttf");
            var ret = BucketManager.Stuff.Font.Get<Font>("SourceHanSansCN-Bold.otf");
            //var ret = BucketManager.Stuff.Font.Get<Font>("TsangerYuYangT-W05.ttf");
            return ret;
        }
        else if(language == "jp")
        {
            // MPLUS1p-Bold.ttf: Copyright 2016 The M+ Project Authors.
            var ret = BucketManager.Stuff.Font.Get<Font>("MPLUS1p-Bold.ttf");
            return ret;
        }
        else
        {
            var ret = BucketManager.Stuff.Font.Get<Font>("SourceHanSansCN-Bold.otf");
            return ret;
        }
       
    }

}
