using BehaviorDesigner.Runtime;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace BehaviorDesigner.Editor
{
	[CustomEditor(typeof(Behavior))]
	public class BehaviorInspector : Editor
	{
		private bool mShowOptions = true;
		private bool mShowVariables;
		private static List<float> variablePosition;
		private static int selectedVariableIndex = -1;
		private static string selectedVariableName;
		private static int selectedVariableTypeIndex;
		private void OnEnable()
		{
			Behavior behavior = this.target as Behavior;
			if (behavior == null)
			{
				return;
			}
			GizmoManager.UpdateGizmo(behavior);
		}
		public override void OnInspectorGUI()
		{
			Behavior behavior = this.target as Behavior;
			if (behavior == null)
			{
				return;
			}
			bool flag = false;
			if (BehaviorInspector.DrawInspectorGUI(behavior, base.serializedObject, true, ref flag, ref this.mShowOptions, ref this.mShowVariables))
			{
				EditorUtility.SetDirty(behavior);
				if (flag && BehaviorDesignerWindow.instance != null && behavior.GetBehaviorSource().BehaviorID == BehaviorDesignerWindow.instance.ActiveBehaviorID)
				{
					BehaviorDesignerWindow.instance.LoadBehavior(behavior.GetBehaviorSource(), false, false);
				}
			}
		}
		public static bool DrawInspectorGUI(Behavior behavior, SerializedObject serializedObject, bool fromInspector, ref bool externalModification, ref bool showOptions, ref bool showVariables)
		{
			EditorGUI.BeginChangeCheck();
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			EditorGUILayout.LabelField("Behavior Name", new GUILayoutOption[]
			{
				GUILayout.Width(120f)
			});
			behavior.GetBehaviorSource().behaviorName = EditorGUILayout.TextField(behavior.GetBehaviorSource().behaviorName, new GUILayoutOption[0]);
			if (fromInspector && GUILayout.Button("Open", new GUILayoutOption[0]))
			{
				BehaviorDesignerWindow.ShowWindow();
				BehaviorDesignerWindow.instance.LoadBehavior(behavior.GetBehaviorSource(), false, true);
			}
			GUILayout.EndHorizontal();
			EditorGUILayout.LabelField("Behavior Description", new GUILayoutOption[0]);
			behavior.GetBehaviorSource().behaviorDescription = EditorGUILayout.TextArea(behavior.GetBehaviorSource().behaviorDescription, BehaviorDesignerUtility.TaskInspectorCommentGUIStyle, new GUILayoutOption[]
			{
				GUILayout.Height(48f)
			});
			serializedObject.Update();
			GUI.enabled = (PrefabUtility.GetPrefabType(behavior) != PrefabType.PrefabInstance || BehaviorDesignerPreferences.GetBool(BDPreferences.EditablePrefabInstances));
			SerializedProperty serializedProperty = serializedObject.FindProperty("externalBehavior");
			ExternalBehavior externalBehavior = serializedProperty.objectReferenceValue as ExternalBehavior;
			EditorGUILayout.PropertyField(serializedProperty, true, new GUILayoutOption[0]);
			serializedObject.ApplyModifiedProperties();
			if ((!object.ReferenceEquals(behavior.ExternalBehavior, null) && !behavior.ExternalBehavior.Equals(externalBehavior)) || (!object.ReferenceEquals(externalBehavior, null) && !externalBehavior.Equals(behavior.ExternalBehavior)))
			{
				if (!object.ReferenceEquals(behavior.ExternalBehavior, null))
				{
					behavior.ExternalBehavior.BehaviorSource.Owner = behavior.ExternalBehavior;
					behavior.ExternalBehavior.BehaviorSource.CheckForSerialization(true, behavior.GetBehaviorSource());
				}
				else
				{
					behavior.GetBehaviorSource().EntryTask = null;
					behavior.GetBehaviorSource().RootTask = null;
					behavior.GetBehaviorSource().DetachedTasks = null;
					behavior.GetBehaviorSource().Variables = null;
					behavior.GetBehaviorSource().CheckForSerialization(true, null);
					behavior.GetBehaviorSource().Variables = null;
					if (BehaviorDesignerPreferences.GetBool(BDPreferences.BinarySerialization))
					{
						BinarySerialization.Save(behavior.GetBehaviorSource());
					}
					else
					{
						SerializeJSON.Save(behavior.GetBehaviorSource());
					}
				}
				externalModification = true;
			}
			GUI.enabled = true;
			serializedProperty = serializedObject.FindProperty("group");
			EditorGUILayout.PropertyField(serializedProperty, true, new GUILayoutOption[0]);
			if (fromInspector && (showVariables = EditorGUILayout.Foldout(showVariables, "Variables")))
			{
				EditorGUI.indentLevel++;
				List<SharedVariable> allVariables = behavior.GetAllVariables();
				BehaviorSource behaviorSource = behavior.GetBehaviorSource();
				bool flag = false;
				if (!Application.isPlaying && behavior.ExternalBehavior != null)
				{
					behaviorSource.CheckForSerialization(true, null);
					flag = true;
				}
				bool flag2 = false;
				if (VariableInspector.SyncVariables(behaviorSource, allVariables))
				{
					flag2 = true;
				}
				if (allVariables != null && allVariables.Count > 0)
				{
					List<SharedVariable> allVariables2 = behaviorSource.GetAllVariables();
					if (VariableInspector.DrawAllVariables(false, behaviorSource, ref allVariables2, false, ref BehaviorInspector.variablePosition, ref BehaviorInspector.selectedVariableIndex, ref BehaviorInspector.selectedVariableName, ref BehaviorInspector.selectedVariableTypeIndex, false, true))
					{
						flag2 = true;
					}
				}
				else
				{
					EditorGUILayout.LabelField("There are no variables to display", new GUILayoutOption[0]);
				}
				if (flag)
				{
					ExternalBehavior externalBehavior2 = (behaviorSource.Owner as Behavior).ExternalBehavior;
					externalBehavior2.BehaviorSource.Owner = externalBehavior2;
					externalBehavior2.BehaviorSource.CheckForSerialization(true, behaviorSource);
				}
				if (flag2)
				{
					if (BehaviorDesignerPreferences.GetBool(BDPreferences.BinarySerialization))
					{
						BinarySerialization.Save(behaviorSource);
					}
					else
					{
						SerializeJSON.Save(behaviorSource);
					}
				}
				EditorGUI.indentLevel--;
			}
			if (!fromInspector || (showOptions = EditorGUILayout.Foldout(showOptions, "Options")))
			{
				if (fromInspector)
				{
					EditorGUI.indentLevel++;
				}
				serializedProperty = serializedObject.FindProperty("startWhenEnabled");
				EditorGUILayout.PropertyField(serializedProperty, true, new GUILayoutOption[0]);
				serializedProperty = serializedObject.FindProperty("pauseWhenDisabled");
				EditorGUILayout.PropertyField(serializedProperty, true, new GUILayoutOption[0]);
				serializedProperty = serializedObject.FindProperty("restartWhenComplete");
				EditorGUILayout.PropertyField(serializedProperty, true, new GUILayoutOption[0]);
				serializedProperty = serializedObject.FindProperty("resetValuesOnRestart");
				EditorGUILayout.PropertyField(serializedProperty, true, new GUILayoutOption[0]);
				serializedProperty = serializedObject.FindProperty("logTaskChanges");
				EditorGUILayout.PropertyField(serializedProperty, true, new GUILayoutOption[0]);
				if (fromInspector)
				{
					EditorGUI.indentLevel--;
				}
			}
			if (EditorGUI.EndChangeCheck())
			{
				serializedObject.ApplyModifiedProperties();
				return true;
			}
			return false;
		}
	}
}
