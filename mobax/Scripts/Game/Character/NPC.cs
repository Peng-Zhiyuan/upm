using UnityEngine;
using System.Collections;

public class NPC : Creature
{
    /*public NPC(string param_ID, int configID) : base(param_ID, configID)
    {
    }*/

    public SceneObjectType sceneObjectType
    {
        get { return SceneObjectType.NPC; }
    }

}