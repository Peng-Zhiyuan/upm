/*
using FoW;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FOWHelper
{
    public static void AddDefaultFOW (GameObject go)
    {
        var fowUnit = go.AddComponent<FogOfWarUnit>();
        fowUnit.shapeType = FogOfWarShapeType.Circle;
        fowUnit.offset = new Vector2(-5, -0.2f);
        fowUnit.boxSize = new Vector2(10, 10);
        fowUnit.circleRadius = 7;
    } 

    
    public static void EnableFog (bool enable)
    {
        
    }
    public static void SetFOWUnit (Creature creature)
    {
        if (creature == null)
            return;

        var fowUnit = creature.gameObject.GetComponent<FogOfWarUnit>();
        if ((creature.sceneObjectType == SceneObjectType.Player || creature.sceneObjectType == SceneObjectType.NPC) && 
            fowUnit == null) {
            AddDefaultFOW(creature.gameObject);
        } 

        if (fowUnit != null) {
            fowUnit.enabled = creature.sceneObjectType == SceneObjectType.Player || creature.sceneObjectType == SceneObjectType.NPC;
        }
    }
    static FogOfWarTeam FOWTeam = null;
    static float minFogStrength = 0.2f;
    public static bool IsInFog (Vector3 pos)
    {
        if (FOWTeam == null)
            FOWTeam = FogOfWarTeam.GetTeam(0);
        if (FOWTeam != null)
            return FOWTeam.GetFogValue (pos) < (byte)(minFogStrength * 255);
        return true;
    }
}


*/