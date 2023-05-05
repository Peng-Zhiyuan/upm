using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;


public class PlotPreview : MonoBehaviour
{
    public AnimationClip anima1;
    public AnimationClip anima2;
 
    private Animator animator;
 
    AnimatorOverrideController overrideController;
 
  
    void Start () {
 
        animator = GetComponent<Animator>();
 
        RuntimeAnimatorController runtimeAnimatorController = animator.runtimeAnimatorController;
 
        overrideController = new AnimatorOverrideController();
        overrideController.runtimeAnimatorController = runtimeAnimatorController;
 
        // overrideController["run1"] = anima1;
        // overrideController["1501011_Bs_02"] = 
        animator.Play("run1");
        animator.Play("1501011_Bs_02");
    }
	
    // Update is called once per frame
    void Update () {
 
        if (Input.GetKeyDown(KeyCode.A))
        {
            animator.runtimeAnimatorController = overrideController;
        }
    }

}