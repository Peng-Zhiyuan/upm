using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class NativeBuilderPlugin
{
    public bool markedFail;

    public virtual void OnPreBuild() { }
    public virtual void OnPostBuild() { }

    public virtual void OnFirstSceneProccessed() { }

}
