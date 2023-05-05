using System.Collections.Generic;
using BattleEngine.View;

namespace BattleSystem.ProjectCore
{
    /// <summary>
    /// 项目级战斗核心的启动参数
    /// </summary>
    public class BattleCoreParam
    {
        public BattleModeType mode;

        /// <summary>
        /// 模式数据结构--通用结构(pveModeParam)
        /// </summary>
        public PveModeParam pveParam;

        /// <summary>
        /// 额外指定参数结构--模式自己传入
        /// </summary>
        public object modeParam;

        //战斗入口来源
        public BattleEnterSource source = BattleEnterSource.None;

        //回放模式
        public bool isBattleReport = false;

        public List<FormationHeroInfo> memebers;
    }
}