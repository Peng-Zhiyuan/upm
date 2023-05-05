using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleSystem.Core;
using System;
public class RoleAgent : MonoBehaviour
{
    public SceneObjectType sceneObjectType;
    public string _id;
    public void SetCreature(Creature curCre)
    {
        var characterController = this.GetComponent<CharacterController>();
        if (characterController != null)
        {
            Debug.LogError("characterController");
            this.sceneObjectType = curCre.sceneObjectType;
            this._id = curCre.ID;
            characterController.radius = 0.5f;
            //characterController.enabled = curCre.sceneObjectType == SceneObjectType.Player || curCre.sceneObjectType == SceneObjectType.NPC;
        }
    }
    

    private RecycledGameObject emoteObject = null;
}
