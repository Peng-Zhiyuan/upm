public class GuildLocalPlayer : GuildPlayer
{
        public GuildLocalPlayer(string uid, int ConfigID, bool isHigh = false, bool isSpring = false, bool isSelf = true) : base(uid, ConfigID, isHigh, isSpring, isSelf)
        {
        }

        public override GuildPlayerType GetType()
        {
                return GuildPlayerType.LocalPlayer;
        }

        public override void Update()
        {
                base.Update();
                
                
        }

        private void SyncPosition()
        {
                
        }

}