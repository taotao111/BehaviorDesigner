using BehaviorDesigner.Runtime;
using System;
using UnityEditor;
using UnityEngine;
namespace BehaviorDesigner.Editor
{
	[InitializeOnLoad]
	public class HierarchyIcon : ScriptableObject
	{
		private static Texture2D icon;
		static HierarchyIcon()
		{
			HierarchyIcon.icon = (AssetDatabase.LoadAssetAtPath("Assets/Gizmos/Behavior Designer Hier Icon.png", typeof(Texture2D)) as Texture2D);
			if (HierarchyIcon.icon != null)
			{
				EditorApplication.hierarchyWindowItemOnGUI = (EditorApplication.HierarchyWindowItemCallback)Delegate.Combine(EditorApplication.hierarchyWindowItemOnGUI, new EditorApplication.HierarchyWindowItemCallback(HierarchyIcon.HierarchyWindowItemOnGUI));
			}
		}
		private static void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
		{
			if (BehaviorDesignerPreferences.GetBool(BDPreferences.ShowHierarchyIcon))
			{
				GameObject gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
				if (gameObject != null && gameObject.GetComponent<Behavior>() != null)
				{
					Rect position = new Rect(selectionRect);
					position.x = position.width + (selectionRect.x - 16f);
					position.width = 16f;
					position.height = 16f;
					GUI.DrawTexture(position, HierarchyIcon.icon);
				}
			}
		}
	}
}
