using UnityEngine;

using BehaviorDesigner.Runtime;
// 怪物类
// 通用怪物的基类，如果有很特殊的怪物或大boss就继承这个类
public class Monster : Creature
{
	//private MonCtrl_AI _ai = new MonCtrl_AI();
	private Behavior bTree = null;

	/*public Monster(string param_ID, int configID) : base(param_ID, configID)
	{

		//////////////////////////
		//需要派生的控制器在这里创建
		//////////////////////////

		//bTree = this.gameObject.AddComponent<BehaviorTree>();
		//bTree.SetBehaviorSource = "EnemyBehaviour";
	}*/

	protected void InitializeGameObject()
	{
		base.InitializeGameObject();
	}

	public void ChangeState(State state)
	{
		//_stateMachine.ChangeState((int)state, null);
	}

	public SceneObjectType sceneObjectType
    {
        get { return SceneObjectType.Monster; }
    }

	/*public void Update(float param_deltaTime)
	{
		//base.Update(param_deltaTime);

		//_ai.Update(param_deltaTime);
	}*/

	protected void DestroyObject()
	{
		//base.DestroyObject();
	}
}
