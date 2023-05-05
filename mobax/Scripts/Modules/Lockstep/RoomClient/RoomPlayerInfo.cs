using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProtoBuf;

/*public struct HeroData
{
    public string uid;
    public string _id;
    public int id;
    public int[] attr;
}*/
public struct RoomPlayerInfo
{
    public string uid;
    public string name;
    public int randomSeed;
    public List<Hero> heros;
}