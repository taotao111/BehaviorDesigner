using System;
using UnityEditor;
namespace BehaviorDesigner.Editor
{
	public class AssetCreationMenus
	{
		[MenuItem("Assets/Create/Behavior Designer/C# Action Task")]
		public static void CreateCSharpActionTask()
		{
			AssetCreator.ShowWindow(AssetCreator.AssetClassType.Action, true);
		}
		[MenuItem("Assets/Create/Behavior Designer/C# Conditional Task")]
		public static void CreateCSharpConditionalTask()
		{
			AssetCreator.ShowWindow(AssetCreator.AssetClassType.Conditional, true);
		}
		[MenuItem("Assets/Create/Behavior Designer/Unityscript Action Task")]
		public static void CreateUnityscriptActionTask()
		{
			AssetCreator.ShowWindow(AssetCreator.AssetClassType.Action, false);
		}
		[MenuItem("Assets/Create/Behavior Designer/Unityscript Conditional Task")]
		public static void CreateUnityscriptConditionalTask()
		{
			AssetCreator.ShowWindow(AssetCreator.AssetClassType.Conditional, false);
		}
		[MenuItem("Assets/Create/Behavior Designer/Shared Variable")]
		public static void CreateSharedVariable()
		{
			AssetCreator.ShowWindow(AssetCreator.AssetClassType.SharedVariable, true);
		}
		[MenuItem("Assets/Create/Behavior Designer/External Behavior Tree")]
		public static void CreateExternalBehaviorTree()
		{
			Type type = Type.GetType("BehaviorDesigner.Runtime.ExternalBehaviorTree, Assembly-CSharp");
			if (type == null)
			{
				type = Type.GetType("BehaviorDesigner.Runtime.ExternalBehaviorTree, Assembly-CSharp-firstpass");
			}
			AssetCreator.CreateAsset(type, "NewExternalBehavior");
		}
	}
}
