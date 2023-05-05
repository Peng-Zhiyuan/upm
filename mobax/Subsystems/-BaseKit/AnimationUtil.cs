using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class AnimationUtil
{
   public static async Task WaitAnimationAsync(Animation animation)
   {
      while(animation.isPlaying)
      {
        await Task.Delay(100);
      }
   }
}
   
