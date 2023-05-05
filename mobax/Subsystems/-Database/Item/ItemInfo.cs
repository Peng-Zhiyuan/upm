using UnityEngine;
using CustomLitJson;
using Sirenix.OdinInspector;

public class ItemInfo
{
    // 子类型动态附加数据
    [HideInInspector]
    public JsonData attach;

    [ShowInInspector]
    public string AttachToString
    {
        get
        {
            var ret = JsonMapper.Instance.ToJson(attach);
            return ret;
        }
    }

    // 服务器的 bag，就是 itype
    public int bag;

    public int IType
    {
        get
        {
            return bag;
        }
    }


    // 数据行 id
    public int id;

    // 实例 id
    public string _id;

    // 所属用户 id
    public string uid;

    // 个数
    public int val;

    // 是否被使用了
    public string used;


    /** 使用的英雄 */
    public string UsedHero => used;

    /** 是否挂在英雄身上了 */
    public bool IsUsed => !string.IsNullOrEmpty(UsedHero);
    

    public T TryGetAttachField<T>(string key, T _default)
    {
        if(this.attach == null)
        {
            return _default;
        }
        if(!this.attach.IsObject)
        {
            return _default;
        }
        var ret = this.attach.TryGet<T>(key, _default);
        return ret;
    }

    public int HeroLv
    {
        get
        {
            var ret = attach.TryGet("lv", 1);
            return ret;
        }
    }

    public int HeroStar
    {
        get
        {
            var ret = attach.TryGet("star", 1);
            return ret;
        }
    }

    public int HeroBreak
    {
        get
        {
            var ret = attach.TryGet("break", 1);
            return ret;
        }
    }

    public int HeroAttackLevel
    {
        get
        {
            var jd = attach.TryGet<JsonData>("skillLv", null);
            if (jd == null)
            {
                return 1;
            }
            var element = jd[0];
            return element.ToInt();
        }
    }

    public int HeroCommonSkillLevel
    {
        get
        {
            var jd = attach.TryGet<JsonData>("skillLv", null);
            if (jd == null)
            {
                return 1;
            }
            var element = jd[1];
            return element.ToInt();
        }
    }

    public int HeroUniqueSkillLevel
    {
        get
        {
            var jd = attach.TryGet<JsonData>("skillLv", null);
            if (jd == null)
            {
                return 1;
            }
            var element = jd[2];
            return element.ToInt();
        }
    }


    public int HeroBoosterLv
    {
        get
        {
            var jd = attach.TryGet<JsonData>("booster", null);
            return jd.TryGet("lv", 1);
        }
    }

    public int HeroBoosterSrc
    {
        get
        {
            var jd = attach.TryGet<JsonData>("booster", null);
            return jd.TryGet("src", -1);
        }
    }

    public int HeroBoosterDst
    {
        get
        {
            var jd = attach.TryGet<JsonData>("booster", null);
            return jd.TryGet("dst", -1);
        }
    }

    public int HeroBoosterNum
    {
        get
        {
            var jd = attach.TryGet<JsonData>("booster", null);
            return jd.TryGet("num", 0);
        }
    }

    public bool HeroCollection
    {
        get
        {
            var ret = attach.TryGet("collection", false);
            return ret;
        }
    }

    public bool HeroExplore
    {
        get
        {
            var ret = attach.TryGet("explore", false);
            return ret;
        }
    }
    
    public string PuzzleBind
    {
        get
        {
            var ret = attach.TryGet<string>("bind", null);
            return ret;
        }
    }
    public int PuzzleLv
    {
        get
        {
            var ret = attach.TryGet("lv", 1);
            return ret;
        }
        set
        {
            attach["lv"] = value;
        }
    }

    public int PuzzleExp
    {
        get
        {
            var ret = attach.TryGet("exp", 0);
            return ret;
        }

    }

    public int PuzzleShap
    {
        get
        {
            var ret = attach.TryGet("shap", -1);
            return ret;
        }
        set
        {
            attach["shap"] = value;
        }
    }

    public Vector2 PuzzlePos
    {
        get
        {
            var array = attach.TryGet<JsonData>("pos", null);
            if(array == null)
            {
                return new Vector2(0, 0);
            }
            else
            {
                var x = array[0].ToInt();
                var y = array[1].ToInt();
                return new Vector2(x, y);
            }
        }
    }


    public int PuzzleId
    {
        get
        {
            var jd = attach.TryGet<JsonData>("attr", null);
            var ret = jd.TryGet("id", -1);
            return ret;
        }
    }

    public int PuzzleQlv
    {
        get
        {
            var jd = attach.TryGet<JsonData>("attr", null);
            var ret = jd.TryGet("qlv", 1);
            return ret;
        }
    }
}