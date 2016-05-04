using BehaviorDesigner.Runtime;
using System;
using UnityEditor;
using UnityEngine;
namespace BehaviorDesigner.Editor
{
	[InitializeOnLoad]
	public class GizmoManager
	{
		private static string currentScene;
		static GizmoManager()
		{
			GizmoManager.currentScene = EditorApplication.currentScene;
			EditorApplication.hierarchyWindowChanged = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.hierarchyWindowChanged, new EditorApplication.CallbackFunction(GizmoManager.HierarchyChange));
			if (!Application.isPlaying)
			{
				GizmoManager.UpdateAllGizmos();
				EditorApplication.playmodeStateChanged = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.playmodeStateChanged, new EditorApplication.CallbackFunction(GizmoManager.UpdateAllGizmos));
			}
		}
		public static void UpdateAllGizmos()
		{
			Behavior[] array = UnityEngine.Object.FindObjectsOfType<Behavior>();
			for (int i = 0; i < array.Length; i++)
			{
				GizmoManager.UpdateGizmo(array[i]);
			}
		}
		public static void UpdateGizmo(Behavior behavior)
		{
			behavior.gizmoViewMode = (Behavior.GizmoViewMode)BehaviorDesignerPreferences.GetInt(BDPreferences.GizmosViewMode);
			behavior.showBehaviorDesignerGizmo = BehaviorDesignerPreferences.GetBool(BDPreferences.ShowSceneIcon);
		}
		public static void HierarchyChange()
		{
			BehaviorManager instance = BehaviorManager.instance;
			if (Application.isPlaying)
			{
				if (instance != null)
				{
					instance.onEnableBehavior = new BehaviorManager.BehaviorManagerHandler(GizmoManager.UpdateBehaviorManagerGizmos);
				}
			}
			else if (GizmoManager.currentScene != EditorApplication.currentScene)
			{
				GizmoManager.currentScene = EditorApplication.currentScene;
				GizmoManager.UpdateAllGizmos();
			}
		}
		private static void UpdateBehaviorManagerGizmos()
		{
			BehaviorManager instance = BehaviorManager.instance;
			if (instance != null)
			{
				for (int i = 0; i < instance.BehaviorTrees.Count; i++)
				{
					GizmoManager.UpdateGizmo(instance.BehaviorTrees[i].behavior);
				}
			}
		}
	}
}
