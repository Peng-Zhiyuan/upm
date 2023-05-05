using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BattleEngine.View;
using UnityEngine;

namespace BattleSystem.ProjectCore
{
    public class HeroFormationInfo
    {
        public int HeroId;
        public int PosIndex; // 站位
        public StageHeroPoint HeroPoint;
    }

    public class HeroFormationInfoWave
    {
        public int Wave;
        public List<HeroFormationInfo> MainHeroInfoList; // 主阵容具体数据（包含调整编队后的数据）
        public List<HeroFormationInfo> SubHeroInfoList; // 替补阵容具体数据（包含调整编队后的数据）
        public HeroFormationInfo LeadInfo; // 主角具体数据（包含调整编队后的数据）
    }

    public class StageInfo
    {
        public int StageId; // 关卡id
        public int SceneId; // 场景id
        public EPveMapType MapType; // 关卡地图类型
        public List<int> MainHero; // 主阵容
        public List<int> SubHero; // 替补阵容
        public List<int> Items; // 战术道具

        public StageCatInfo CatInfo;

        public List<MapPartConfig> MapParts; // 肉鸽地图块信息
        public List<MapEffectBase> MapEffects; // 肉鸽地图块信息
        public List<EnvironmentPartConfig> EnvironmentParts; // 怪物信息，wave=0 ,monsters=[]
        public List<StageMonsterInfo> MonsterInfos; // 怪物信息，wave=0 ,monsters=[]
        public List<HeroFormationInfoWave> formationInfos; // 真正的编队数据根据波次

        public int PveBattleMapId; // PVE大地图Id

        public Vector3 CameraRotation;
        public Vector3 CameraPos;
        public int FormationIndex; // 编队Index

        public bool OnlyPlot; // 纯剧情

        public int GetHeroID(StageHeroPoint hero)
        {
            if (hero.heroType == StageHeroPoint.StageHeroType.Main)
                return MainHero[hero.index];
            else if (hero.heroType == StageHeroPoint.StageHeroType.Sub)
                return SubHero[hero.index - 3];
            else
                return hero.heroId;
        }

        //
        // // 获取主阵容角色的实际站位
        // public  Vector3 GetMainHeroRealPos(List<StageHeroPoint> heroList,int posIndex)
        // {
        //     var mainId = this.MainHeroId[posIndex];
        //     // 过滤0的
        //     var realMainHeroList = this.MainHeroId.FindAll(val => val != 0);
        //     // 如果超出了realMainHeroList的长度 则说明是有问题的
        //     if (posIndex < 0 || posIndex > realMainHeroList.Count - 1) return Vector3.zero;
        //     var mainHero = heroList.FindAll(val => val.heroType == StageHeroPoint.StageHeroType.Main);
        //     
        //     if (realMainHeroList.Count<=1)
        //     {
        //         return mainHero[1].pos;
        //     }
        //     else if (realMainHeroList.Count <=2)
        //     {
        //         // 这里需要计算2个点之间的距离
        //     }
        //     else
        //     {
        //         return mainHero[posIndex].pos;
        //     }
        // }
    }

    // public class StageHeroInfo
    // {
    //     public int HeroId;
    //     public Vector3 Pos;
    // }

    public static class StageBattleUtil
    {
        //是否是boss战
        public static bool IsBossWave()
        {
            var stageRow = StaticData.StageTable.TryGet(Battle.Instance.CopyId);
            if (stageRow == null)
                return false;
            if (stageRow.Boss == 0)
                return false;
            return Battle.Instance.Wave == Battle.Instance.MaxWave;
        }

        private static Tuple<List<int>, List<int>> GetHeroIds(StageMonsterInfo monsterInfo, List<int> mainHero,
            List<int> subHero)
        {
            var mainHeroIds = new List<int>();
            var subHeroIds = new List<int>();
            // 临时处理  只要配置了一个use的直接全用配置数据  否则用传入编队
            var isUse = monsterInfo.heros.Find(val => val.heroUse == StageHeroPoint.StageHeroUse.Use) != null;
            if (isUse)
            {
                foreach (var heroInfo in monsterInfo.heros)
                {
                    if (heroInfo.heroType == StageHeroPoint.StageHeroType.Main)
                    {
                        mainHeroIds.Add(heroInfo.heroId);
                    }
                    else if (heroInfo.heroType == StageHeroPoint.StageHeroType.Sub)
                    {
                        subHeroIds.Add(heroInfo.heroId);
                    }
                }
            }
            else
            {
                mainHeroIds = mainHero;
                subHeroIds = subHero;
            }

            return new Tuple<List<int>, List<int>>(mainHeroIds, subHeroIds);
        }

        /// <summary>
        /// 对比heroPoints和heroIds找寻真正的位置信息
        /// </summary>
        /// <param name="heroIds"></param>
        /// <param name="heroPoints"></param>
        private static List<HeroFormationInfo> GetHeroFormationInfo(List<int> heroIds, List<StageHeroPoint> heroPoints)
        {
            // 找到当前所有的不为0的heroId数组
            var realHeroIds = heroIds.FindAll(val => val != 0);
            var formationInfoList = new List<HeroFormationInfo>();
            if (heroPoints.Count <= 0
                || realHeroIds.Count <= 0) return formationInfoList;
            if (realHeroIds.Count <= 1)
            {
                formationInfoList.Add(new HeroFormationInfo()
                {
                    HeroId = realHeroIds.First(), PosIndex = 1,
                    HeroPoint = heroPoints.Count > 1 ? heroPoints[1] : heroPoints[0]
                });
            }
            else if (realHeroIds.Count <= 2)
            {
                // 配置的数量多则使用算法
                if (heroPoints.Count > realHeroIds.Count)
                {
                    for (int index = 0; index < 2; index++)
                    {
                        StageHeroPoint heroPoint = new StageHeroPoint();
                        var next = index + 1;
                        if (index <= heroPoints.Count - 1
                            && next <= heroPoints.Count - 1)
                        {
                            var curInfo = heroPoints[index];
                            var nextInfo = heroPoints[next];
                            var posX = (curInfo.pos.x + nextInfo.pos.x) / 2;
                            var posY = (curInfo.pos.y + nextInfo.pos.y) / 2;
                            var posZ = (curInfo.pos.z + nextInfo.pos.z) / 2;
                            var pos = new Vector3(posX, posY, posZ);
                            heroPoint.heroId = curInfo.heroId;
                            heroPoint.index = curInfo.index;
                            heroPoint.dir = curInfo.dir;
                            heroPoint.wave = curInfo.wave;
                            heroPoint.heroType = curInfo.heroType;
                            heroPoint.heroUse = curInfo.heroUse;
                            heroPoint.rotationY = curInfo.rotationY;
                            heroPoint.finalRotationY = curInfo.finalRotationY;
                            heroPoint.pos = pos;
                            formationInfoList.Add(new HeroFormationInfo()
                                {HeroId = realHeroIds[index], PosIndex = index, HeroPoint = heroPoint});
                        }
                    }
                }
                else
                {
                    for (var index = 0; index < heroPoints.Count; index++)
                    {
                        formationInfoList.Add(new HeroFormationInfo()
                            {HeroId = realHeroIds[index], PosIndex = index, HeroPoint = heroPoints[index]});
                    }
                }
            }
            else
            {
                for (var index = 0; index < heroPoints.Count; index++)
                {
                    formationInfoList.Add(new HeroFormationInfo()
                        {HeroId = realHeroIds[index], PosIndex = index, HeroPoint = heroPoints[index]});
                }
            }

            return formationInfoList;
        }

        public static int GetChapterId(int stageId)
        {
            var chapterId = -1;
            var rows = StaticData.StoryAreaTable.ElementList;
            foreach (var row in rows)
            {
                foreach (var ss in row.Levelareas)
                {
                    if (ss == stageId)
                    {
                        chapterId = row.Id;
                        break;
                    }
                }
            }

            return chapterId;
        }

        public static StageMonsterInfo GetMonsterInfo(List<StageMonsterInfo> infos, int wave)
        {
            return infos.Find(val => val.wave.Equals(wave));
        }

        public static HeroFormationInfoWave GetFormationInfoByWave(List<HeroFormationInfoWave> infos, int wave)
        {
            return infos.Find(val => val.Wave.Equals(wave));
        }

        public static int GetHeroId(StageHeroPoint hero, List<StageHeroPoint> heroList, List<int> mainHeroIds,
            List<int> subHeroIds)
        {
            var mainHero = heroList.FindAll(val => val.heroType == StageHeroPoint.StageHeroType.Main);
            var subHero = heroList.FindAll(val => val.heroType == StageHeroPoint.StageHeroType.Sub);
            int index = -1;
            index = mainHero.FindIndex(val => val.heroId.Equals(hero.heroId));
            if (index >= 0)
            {
                return mainHeroIds[index];
            }

            index = subHero.FindIndex(val => val.heroId.Equals(hero.heroId));
            if (index >= 0)
            {
                return subHeroIds[index];
            }

            return hero.heroId;
        }

        public static StageHeroPoint GetStageHeroDataByPos(StageHeroPoint.StageHeroType type, int posIndex,
            List<StageHeroPoint> heroList)
        {
            var mainHero = heroList.FindAll(val => val.heroType == StageHeroPoint.StageHeroType.Main);
            var subHero = heroList.FindAll(val => val.heroType == StageHeroPoint.StageHeroType.Sub);
            var leader = heroList.FindAll(val => val.heroType == StageHeroPoint.StageHeroType.Leader);
            StageHeroPoint result = null;
            switch (type)
            {
                case StageHeroPoint.StageHeroType.Leader:
                    result = leader.Count > 0 ? leader.First() : null;
                    break;
                case StageHeroPoint.StageHeroType.Main:
                    if (posIndex <= mainHero.Count - 1
                        && posIndex >= 0)
                    {
                        result = mainHero[posIndex];
                    }

                    break;
                case StageHeroPoint.StageHeroType.Sub:
                    if (posIndex <= subHero.Count - 1
                        && posIndex >= 0)
                    {
                        result = subHero[posIndex];
                    }

                    break;
            }

            return result;
        }

        public static int GetHeroIndex(StageHeroPoint hero, List<StageHeroPoint> heroList)
        {
            var mainHero = heroList.FindAll(val => val.heroType == StageHeroPoint.StageHeroType.Main);
            var subHero = heroList.FindAll(val => val.heroType == StageHeroPoint.StageHeroType.Sub);
            int index = -1;
            index = mainHero.FindIndex(val => val.heroId.Equals(hero.heroId));
            if (index >= 0)
            {
                return index;
            }

            index = subHero.FindIndex(val => val.heroId.Equals(hero.heroId));
            if (index >= 0)
            {
                return index;
            }

            return index;
        }

        public static bool StoryOpen = true;
        private static readonly int Speed = Animator.StringToHash("Speed");

        public static async Task TriggerStory(int stageId, EPlotEventType type, Action callback)
        {
            var row = StaticData.StageTable.TryGet(stageId);
            if (row == null
                || !StoryOpen
                || row.Chat <= 0)
            {
                if (callback != null)
                    callback.Invoke();
                return;
            }

            BattleStateManager.Instance.StoryPlay = true;
            var plotInfo = new PlotInfo
            {
                StageId = stageId,
                EventType = type,
                OnComp = delegate
                {
                    BattleStateManager.Instance.StoryPlay = false;
                    if (callback != null)
                        callback.Invoke();
                }
            };
            PlotPipelineManager.Stuff.OnHideAction = () =>
            {
                if (Battle.Instance.IsFight)
                    BattleSpecialEventManager.Instance.RemoveEvent(SpecailEventType.HideUI);
            };
            PlotPipelineManager.Stuff.OnShowAction = () =>
            {
                if (Battle.Instance.IsFight)
                    BattleSpecialEventManager.Instance.AddEvent(SpecailEventType.HideUI);
            };
            await PlotPipelineManager.Stuff.StartPipelineAsync(stageId, type, false);
            BattleStateManager.Instance.StoryPlay = false;
            if (callback != null)
                callback.Invoke();
        }

        public static float GetMapRotationY(MapWaveModelData data)
        {
            var rotationY = 0f;
            switch (data.Dir)
            {
                case EMapModelDir.Bottom:
                    rotationY = 180;
                    break;
                case EMapModelDir.Top:
                    rotationY = 0;
                    break;
                case EMapModelDir.Left:
                    rotationY = 270;
                    break;
                case EMapModelDir.Right:
                    rotationY = 90;
                    break;
            }

            return rotationY + data.RotationY;
        }
    }
}