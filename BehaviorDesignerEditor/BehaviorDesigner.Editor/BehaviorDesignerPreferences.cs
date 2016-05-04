using BehaviorDesigner.Runtime;
using System;
using UnityEditor;
using UnityEngine;
namespace BehaviorDesigner.Editor
{
	public class BehaviorDesignerPreferences : Editor
	{
		private static string[] prefString;
		private static string[] serializationString = new string[]
		{
			"Binary",
			"JSON"
		};
		private static string[] PrefString
		{
			get
			{
				if (BehaviorDesignerPreferences.prefString == null)
				{
					BehaviorDesignerPreferences.InitPrefString();
				}
				return BehaviorDesignerPreferences.prefString;
			}
		}
		public static void InitPrefernces()
		{
			if (!EditorPrefs.HasKey(BehaviorDesignerPreferences.PrefString[0]))
			{
				BehaviorDesignerPreferences.SetBool(BDPreferences.ShowWelcomeScreen, true);
			}
			if (!EditorPrefs.HasKey(BehaviorDesignerPreferences.PrefString[1]))
			{
				BehaviorDesignerPreferences.SetBool(BDPreferences.ShowSceneIcon, true);
			}
			if (!EditorPrefs.HasKey(BehaviorDesignerPreferences.PrefString[2]))
			{
				BehaviorDesignerPreferences.SetBool(BDPreferences.ShowHierarchyIcon, true);
			}
			if (!EditorPrefs.HasKey(BehaviorDesignerPreferences.PrefString[3]))
			{
				BehaviorDesignerPreferences.SetBool(BDPreferences.OpenInspectorOnTaskSelection, false);
			}
			if (!EditorPrefs.HasKey(BehaviorDesignerPreferences.PrefString[3]))
			{
				BehaviorDesignerPreferences.SetBool(BDPreferences.OpenInspectorOnTaskSelection, false);
			}
			if (!EditorPrefs.HasKey(BehaviorDesignerPreferences.PrefString[5]))
			{
				BehaviorDesignerPreferences.SetBool(BDPreferences.FadeNodes, true);
			}
			if (!EditorPrefs.HasKey(BehaviorDesignerPreferences.PrefString[6]))
			{
				BehaviorDesignerPreferences.SetBool(BDPreferences.EditablePrefabInstances, false);
			}
			if (!EditorPrefs.HasKey(BehaviorDesignerPreferences.PrefString[7]))
			{
				BehaviorDesignerPreferences.SetBool(BDPreferences.PropertiesPanelOnLeft, true);
			}
			if (!EditorPrefs.HasKey(BehaviorDesignerPreferences.PrefString[8]))
			{
				BehaviorDesignerPreferences.SetBool(BDPreferences.MouseWhellScrolls, false);
			}
			if (!EditorPrefs.HasKey(BehaviorDesignerPreferences.PrefString[9]))
			{
				BehaviorDesignerPreferences.SetBool(BDPreferences.FoldoutFields, true);
			}
			if (!EditorPrefs.HasKey(BehaviorDesignerPreferences.PrefString[10]))
			{
				BehaviorDesignerPreferences.SetBool(BDPreferences.CompactMode, false);
			}
			if (!EditorPrefs.HasKey(BehaviorDesignerPreferences.PrefString[11]))
			{
				BehaviorDesignerPreferences.SetBool(BDPreferences.SnapToGrid, true);
			}
			if (!EditorPrefs.HasKey(BehaviorDesignerPreferences.PrefString[12]))
			{
				BehaviorDesignerPreferences.SetBool(BDPreferences.ShowTaskDescription, true);
			}
			if (!EditorPrefs.HasKey(BehaviorDesignerPreferences.PrefString[13]))
			{
				BehaviorDesignerPreferences.SetBool(BDPreferences.BinarySerialization, true);
			}
			if (!EditorPrefs.HasKey(BehaviorDesignerPreferences.PrefString[14]))
			{
				BehaviorDesignerPreferences.SetBool(BDPreferences.ErrorChecking, true);
			}
			if (!EditorPrefs.HasKey(BehaviorDesignerPreferences.PrefString[15]))
			{
				BehaviorDesignerPreferences.SetBool(BDPreferences.UpdateCheck, true);
			}
			if (!EditorPrefs.HasKey(BehaviorDesignerPreferences.PrefString[16]))
			{
				BehaviorDesignerPreferences.SetBool(BDPreferences.AddGameGUIComponent, false);
			}
			if (!EditorPrefs.HasKey(BehaviorDesignerPreferences.PrefString[17]))
			{
				BehaviorDesignerPreferences.SetInt(BDPreferences.GizmosViewMode, 2);
			}
			if (BehaviorDesignerPreferences.GetBool(BDPreferences.EditablePrefabInstances) && BehaviorDesignerPreferences.GetBool(BDPreferences.BinarySerialization))
			{
				BehaviorDesignerPreferences.SetBool(BDPreferences.BinarySerialization, false);
			}
		}
		private static void InitPrefString()
		{
			BehaviorDesignerPreferences.prefString = new string[18];
			for (int i = 0; i < BehaviorDesignerPreferences.prefString.Length; i++)
			{
				BehaviorDesignerPreferences.prefString[i] = string.Format("BehaviorDesigner{0}", (BDPreferences)i);
			}
		}
		public static void DrawPreferencesPane(PreferenceChangeHandler callback)
		{
			BehaviorDesignerPreferences.DrawBoolPref(BDPreferences.ShowWelcomeScreen, "Show welcome screen", callback);
			BehaviorDesignerPreferences.DrawBoolPref(BDPreferences.ShowSceneIcon, "Show Behavior Designer icon in the scene", callback);
			BehaviorDesignerPreferences.DrawBoolPref(BDPreferences.ShowHierarchyIcon, "Show Behavior Designer icon in the hierarchy window", callback);
			BehaviorDesignerPreferences.DrawBoolPref(BDPreferences.OpenInspectorOnTaskSelection, "Open inspector on single task selection", callback);
			BehaviorDesignerPreferences.DrawBoolPref(BDPreferences.OpenInspectorOnTaskDoubleClick, "Open inspector on task double click", callback);
			BehaviorDesignerPreferences.DrawBoolPref(BDPreferences.FadeNodes, "Fade tasks after they are done running", callback);
			BehaviorDesignerPreferences.DrawBoolPref(BDPreferences.EditablePrefabInstances, "Allow edit of prefab instances", callback);
			BehaviorDesignerPreferences.DrawBoolPref(BDPreferences.PropertiesPanelOnLeft, "Position properties panel on the left", callback);
			BehaviorDesignerPreferences.DrawBoolPref(BDPreferences.MouseWhellScrolls, "Mouse wheel scrolls graph view", callback);
			BehaviorDesignerPreferences.DrawBoolPref(BDPreferences.FoldoutFields, "Grouped fields start visible", callback);
			BehaviorDesignerPreferences.DrawBoolPref(BDPreferences.CompactMode, "Compact mode", callback);
			BehaviorDesignerPreferences.DrawBoolPref(BDPreferences.SnapToGrid, "Snap to grid", callback);
			BehaviorDesignerPreferences.DrawBoolPref(BDPreferences.ShowTaskDescription, "Show selected task description", callback);
			BehaviorDesignerPreferences.DrawBoolPref(BDPreferences.ErrorChecking, "Realtime error checking", callback);
			BehaviorDesignerPreferences.DrawBoolPref(BDPreferences.UpdateCheck, "Check for updates", callback);
			BehaviorDesignerPreferences.DrawBoolPref(BDPreferences.AddGameGUIComponent, "Add Game GUI Component", callback);
			bool @bool = BehaviorDesignerPreferences.GetBool(BDPreferences.BinarySerialization);
			if (EditorGUILayout.Popup("Serialization", (!@bool) ? 1 : 0, BehaviorDesignerPreferences.serializationString, new GUILayoutOption[0]) != ((!@bool) ? 1 : 0))
			{
				BehaviorDesignerPreferences.SetBool(BDPreferences.BinarySerialization, !@bool);
				callback(BDPreferences.BinarySerialization, !@bool);
			}
			int @int = BehaviorDesignerPreferences.GetInt(BDPreferences.GizmosViewMode);
			int num = (int)((Behavior.GizmoViewMode)EditorGUILayout.EnumPopup("Gizmos View Mode", (Behavior.GizmoViewMode)@int, new GUILayoutOption[0]));
			if (num != @int)
			{
				BehaviorDesignerPreferences.SetInt(BDPreferences.GizmosViewMode, num);
				callback(BDPreferences.GizmosViewMode, num);
			}
			if (GUILayout.Button("Restore to Defaults", EditorStyles.miniButtonMid, new GUILayoutOption[0]))
			{
				BehaviorDesignerPreferences.ResetPrefs();
			}
		}
		private static void DrawBoolPref(BDPreferences pref, string text, PreferenceChangeHandler callback)
		{
			bool @bool = BehaviorDesignerPreferences.GetBool(pref);
			bool flag = GUILayout.Toggle(@bool, text, new GUILayoutOption[0]);
			if (flag != @bool)
			{
				BehaviorDesignerPreferences.SetBool(pref, flag);
				callback(pref, flag);
				if (pref == BDPreferences.EditablePrefabInstances && flag && BehaviorDesignerPreferences.GetBool(BDPreferences.BinarySerialization))
				{
					BehaviorDesignerPreferences.SetBool(BDPreferences.BinarySerialization, false);
					callback(BDPreferences.BinarySerialization, false);
				}
				else if (pref == BDPreferences.BinarySerialization && flag && BehaviorDesignerPreferences.GetBool(BDPreferences.EditablePrefabInstances))
				{
					BehaviorDesignerPreferences.SetBool(BDPreferences.EditablePrefabInstances, false);
					callback(BDPreferences.EditablePrefabInstances, false);
				}
			}
		}
		private static void ResetPrefs()
		{
			BehaviorDesignerPreferences.SetBool(BDPreferences.ShowWelcomeScreen, true);
			BehaviorDesignerPreferences.SetBool(BDPreferences.ShowSceneIcon, true);
			BehaviorDesignerPreferences.SetBool(BDPreferences.ShowHierarchyIcon, true);
			BehaviorDesignerPreferences.SetBool(BDPreferences.OpenInspectorOnTaskSelection, false);
			BehaviorDesignerPreferences.SetBool(BDPreferences.OpenInspectorOnTaskDoubleClick, false);
			BehaviorDesignerPreferences.SetBool(BDPreferences.FadeNodes, true);
			BehaviorDesignerPreferences.SetBool(BDPreferences.EditablePrefabInstances, false);
			BehaviorDesignerPreferences.SetBool(BDPreferences.PropertiesPanelOnLeft, true);
			BehaviorDesignerPreferences.SetBool(BDPreferences.MouseWhellScrolls, false);
			BehaviorDesignerPreferences.SetBool(BDPreferences.FoldoutFields, true);
			BehaviorDesignerPreferences.SetBool(BDPreferences.CompactMode, false);
			BehaviorDesignerPreferences.SetBool(BDPreferences.SnapToGrid, true);
			BehaviorDesignerPreferences.SetBool(BDPreferences.ShowTaskDescription, true);
			BehaviorDesignerPreferences.SetBool(BDPreferences.BinarySerialization, true);
			BehaviorDesignerPreferences.SetBool(BDPreferences.ErrorChecking, true);
			BehaviorDesignerPreferences.SetBool(BDPreferences.UpdateCheck, true);
			BehaviorDesignerPreferences.SetBool(BDPreferences.AddGameGUIComponent, false);
			BehaviorDesignerPreferences.SetInt(BDPreferences.GizmosViewMode, 2);
		}
		public static void SetBool(BDPreferences pref, bool value)
		{
			EditorPrefs.SetBool(BehaviorDesignerPreferences.PrefString[(int)pref], value);
		}
		public static bool GetBool(BDPreferences pref)
		{
			return EditorPrefs.GetBool(BehaviorDesignerPreferences.PrefString[(int)pref]);
		}
		public static void SetInt(BDPreferences pref, int value)
		{
			EditorPrefs.SetInt(BehaviorDesignerPreferences.PrefString[(int)pref], value);
		}
		public static int GetInt(BDPreferences pref)
		{
			return EditorPrefs.GetInt(BehaviorDesignerPreferences.PrefString[(int)pref]);
		}
	}
}
