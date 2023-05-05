using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Threading.Tasks;

public partial class MovieFloating : Floating
{
    public void ChangeMovie(string videoAddress, Action onComplete = null, Action onUnsupport = null, bool fadeIn = true, bool isLoop = false)
    {
        //var url = VideoManager.Stuff.GetRealPathForAddress(videoAddress);
        this.MovieView.ChangeMovie(videoAddress, () => { }, () =>
                        {
                            onComplete?.Invoke();
                        }, onUnsupport, isLoop
        );
    }
}