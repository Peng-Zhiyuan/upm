using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantData 
{
    public int id;
    public long time;
    public int num;//领取次数
    public long lastTime;//上一次领取时间
    public int[] buff;
}
