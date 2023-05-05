using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorExtra : MonoBehaviour
{
    void OnDisable()
    {
        var animator = this.GetComponent<Animator>();
        animator.CrossFade("Default", 0f);
        animator.Update(0f);
    }

}
