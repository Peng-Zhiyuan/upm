using BattleEngine.Logic;

namespace Game.Bubble
{
    public class BubbleManager : Singleton<BubbleManager>
    {
        public void TriggerEvent(EmojiEvent evt)
        {
            switch (evt)
            {
                case EmojiEvent.EnterLevel:
                {
                    
                    break;
                }
            }
        }
    }
}