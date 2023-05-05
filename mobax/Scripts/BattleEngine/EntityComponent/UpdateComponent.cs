namespace BattleEngine.Logic
{
    [EnableUpdate]
    public class UpdateComponent : Component
    {
        public override bool Enable { get; set; } = true;

        public override void Update(float deltaTime)
        {
            Entity.LogicUpdate(deltaTime);
        }
    }
}