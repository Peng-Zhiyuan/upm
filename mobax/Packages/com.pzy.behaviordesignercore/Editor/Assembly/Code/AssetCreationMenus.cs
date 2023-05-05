namespace BehaviorDesigner.Editor
{
    using BehaviorDesigner.Runtime;
    using System;
    using UnityEditor;

    public class AssetCreationMenus
    {
        [UnityEditor.MenuItem("Assets/Create/Behavior Designer/C# Action Task")]
        public static void CreateCSharpActionTask()
        {
            AssetCreator.ShowWindow(AssetCreator.AssetClassType.Action);
        }

        [UnityEditor.MenuItem("Assets/Create/Behavior Designer/C# Conditional Task")]
        public static void CreateCSharpConditionalTask()
        {
            AssetCreator.ShowWindow(AssetCreator.AssetClassType.Conditional);
        }

        [UnityEditor.MenuItem("Assets/Create/Behavior Designer/External Behavior Tree")]
        public static void CreateExternalBehaviorTree()
        {
            AssetCreator.CreateAsset(BehaviorDesigner.Runtime.TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.ExternalBehaviorTree"), "NewExternalBehavior");
        }

        [UnityEditor.MenuItem("Assets/Create/Behavior Designer/Shared Variable")]
        public static void CreateSharedVariable()
        {
            AssetCreator.ShowWindow(AssetCreator.AssetClassType.SharedVariable);
        }
    }
}

