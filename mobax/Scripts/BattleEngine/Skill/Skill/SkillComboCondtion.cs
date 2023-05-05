namespace BattleEngine.Logic
{
    public abstract class Chunk
    {
        public int id;
        public string name;
    }

    public abstract class TODO : Chunk
    {
        public abstract int Execute(object context, object[] args);
    }

    public class PHASE : TODO
    {
        public override int Execute(object context, object[] args)
        {
            var actor = context as CombatActorEntity;
            int nowPhase = 1;
            int targetPhase = int.Parse(args[0] as string);
            return nowPhase == targetPhase ? 0 : 1;
        }
    }
}