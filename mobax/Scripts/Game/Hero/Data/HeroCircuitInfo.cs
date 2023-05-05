using System.Collections.Generic;
using System.Linq;
using CustomLitJson;

public class HeroCircuitInfo
{
    #region Basic Attributes From Table

    /** 配置 */
    public PuzzleRow Conf { get; }
    /** 配置ID */
    public int ConfId => Conf.Id;
    /** 导师ID */
    public int SourceHero => Conf.heroId;

    #endregion
    
    #region Basic Attributes From Server

    /** 服务端数据 */
    public ItemInfo ItemInfo { get; }
    /** 拼图解锁 **/
    public bool Unlocked => null != ItemInfo;
    /** Instance Id */
    public string InstanceId => ItemInfo._id;
    /** 等级 */
    public int Level => LevelConfig?.Level ?? 0;
    /** 经验 */
    public int Exp => ItemInfo.attach.TryGet("exp", 0);
    /** 拼图技能 */
    public int Skill => ItemInfo.attach.TryGet("skill", 0);
    /** 拼图获得时间 */
    public int Time => ItemInfo.attach.TryGet("time", 0);
    /** 品阶 */
    public int Qlv => LevelConfig?.Qlv ?? 0;
    /** 绑定英雄的instance id（非配表id） */
    public string HeroInstanceId => ItemInfo.UsedHero;
    /** Hero配表id */
    public int HeroId => string.IsNullOrEmpty(HeroInstanceId) ? 0 : HeroHelper.InstanceIdToRowId(HeroInstanceId);
    /** 导师拼图绑定的ID */
    public string BindHero => ItemInfo.attach.TryGet("bind", "");
    /** 是否是绑定拼图（导师拼图） */
    public bool Bind => !string.IsNullOrEmpty(BindHero);
    /** 属性数组 {int id, int qlv}[] */
    public JsonData Attrs => ItemInfo.attach.TryGet<JsonData>("attr", null);
    /** 形状 */
    public int Shape
    {
        get => ItemInfo.attach.TryGet("shape", 0);
        set => ItemInfo.attach["shape"] = value;
    }

    public PuzzleShapeRow ShapeConf => StaticData.PuzzleShapeTable.TryGet(Shape);

    /** 标记源，用于旋转后判断是否还是同一个拼图 */
    public int Flag => GetFlag(ShapeConf);

    /** 颜色 */
    public int Color
    {
        get
        {
            var skillRow = StaticData.SkillTable.TryGet(Skill);
            return skillRow?.Colls.First().colorType ?? 1;
        }
    }
    
    public int KeywordId
    {
        get
        {
            var skillRow = StaticData.SkillTable.TryGet(Skill);
            return skillRow?.Colls.First().Affix ?? 1;
        }
    }

    /** 是否待进阶状态 */
    public bool InAdvance
    {
        get
        {
            if (!Unlocked) return false;

            var levelCfg = StaticData.PuzzleLevelTable.TryGet(LevelId);
            return levelCfg.Advance == 1;
        }
    }

    /** 是否满级 */
    public bool LevelMax
    {
        get
        {
            if (!Unlocked) return true;

            var levelCfg = StaticData.PuzzleLevelTable.TryGet(LevelId);
            return levelCfg.Next == 0;
        }
    }
    
    public int LevelId
    {
        get
        {
            if (!Unlocked) return 0;

            var levelId = ItemInfo.attach.TryGet("lv", 0);
            return levelId;
        }
    }

    public bool SkillUnlocked
    {
        get
        {
            int unlockQlv = StaticData.BaseTable.TryGet(Bind ? "memoryPuzzleSkillUnlock" : "PuzzleSkillUnlock");
            return Qlv >= unlockQlv;
        }
    }
    
    public PuzzleLevelRow LevelConfig => LevelId == 0 ? null : StaticData.PuzzleLevelTable.TryGet(LevelId);
    
    public Pos Coordinate
    {
        get
        {
            var array = ItemInfo.attach.TryGet<JsonData>("pos", null);
            return null == array ? default : new Pos((int) array[0], (int) array[1]);
        }

        set
        {
            JsonData array = ItemInfo.attach["pos"] ??= new JsonData();
            var x = value.X;
            var y = value.Y;
            if (array.GetJsonType() == default)
            {
                array.Add(x);
                array.Add(y);
            }
            else
            {
                array[0] = x;
                array[1] = y;
            }
        }
    }

    public List<Pos> Nodes => ShapeConf.Dots;

    #endregion
    
    public HeroCircuitInfo(ItemInfo itemInfo)
    {
        ItemInfo = itemInfo;
        Conf = StaticData.PuzzleTable.TryGet(itemInfo.id);
    }
    
    /** 方向旋转 */
    public bool Turn()
    {
        var newShape = ShapeConf.Shapenext;
        if (Shape == newShape) return false;
        
        Shape = newShape;
        return true;
    }

    public static int GetFlag(PuzzleShapeRow shapeConf)
    {
        return shapeConf.Qlv * 10 + shapeConf.Shapetype;
    }
}