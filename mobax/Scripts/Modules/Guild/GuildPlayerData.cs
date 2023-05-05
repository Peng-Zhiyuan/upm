public enum SpringAct
{
       None,
       JoinBattle,
       LeaveBattle,
}
public class GuildPlayerData
{
       public string uid;
       public int pos;
       public SpringAct act;
       public bool inBattle;
       public bool changed;

       public void SetAction(SpringAct param_act)
       {
              if(param_act == 0)
                     return;

              act = param_act;

              if (param_act == SpringAct.JoinBattle)
              {
                     inBattle = true;
                     changed = true;
              }
              else if (param_act == SpringAct.LeaveBattle)
              {
                     inBattle = false;
                     changed = true;
              }
       }

       /*public bool IsInBattle()
       {
              return act == SpringAct.JoinBattle;
       }*/
}