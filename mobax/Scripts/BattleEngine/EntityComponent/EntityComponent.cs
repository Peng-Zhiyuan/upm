namespace BattleEngine.Logic
{
    public class EntityComponent<TEntity> : Component where TEntity : Entity
    {
        public TEntity OwnerEntity { get; set; }
    }
}