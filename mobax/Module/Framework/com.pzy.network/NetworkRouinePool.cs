using System.Collections.Generic;

public static class NetworkRoutinePool
{
    private static readonly Queue<NetworkRoutine> routinePool = new Queue<NetworkRoutine>();
    public static NetworkRoutine Reuse()
	{
		NetworkRoutine routine = null;
		if(routinePool.Count > 0) 
		{
			routine = routinePool.Dequeue();
		}
		else 
		{
			routine = new NetworkRoutine();
		}
        routine.OnReuse();
		return routine;	
	}

    public static void Recycle(NetworkRoutine routine)
    {
        routine.OnRecycle();
        routinePool.Enqueue(routine);
    }

}