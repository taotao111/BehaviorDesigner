using BehaviorDesigner.Runtime;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace BehaviorDesigner.Editor
{
	[CustomEditor(typeof(ExternalBehavior))]
	public class ExternalBehaviorInspector : Editor
	{
		private bool mShowVariables;
		private static List<float> variablePosition;
		private static int selectedVariableIndex = -1;
		private static string selectedVariableName;
		private static int selectedVariableTypeIndex;
		public override void OnInspectorGUI()
		{
			ExternalBehavior externalBehavior = this.target as ExternalBehavior;
			if (externalBehavior == null)
			{
				return;
			}
			if (ExternalBehaviorInspector.DrawInspectorGUI(externalBehavior.BehaviorSource, true, ref this.mShowVariables))
			{
				EditorUtility.SetDirty(externalBehavior);
			}
		}
		public void Reset()
		{
			ExternalBehavior externalBehavior = this.target as ExternalBehavior;
			if (externalBehavior == null)
			{
				return;
			}
			if (externalBehavior.BehaviorSource.Owner == null)
			{
				externalBehavior.BehaviorSource.Owner = externalBehavior;
			}
		}
		public static bool DrawInspectorGUI(BehaviorSource behaviorSource, bool fromInspector, ref bool showVariables)
		{
			EditorGUI.BeginChangeCheck();
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			EditorGUILayout.LabelField("Behavior Name", new GUILayoutOption[]
			{
				GUILayout.Width(120f)
			});
			behaviorSource.behaviorName = EditorGUILayout.TextField(behaviorSource.behaviorName, new GUILayoutOption[0]);
			if (fromInspector && GUILayout.Button("Open", new GUILayoutOption[0]))
			{
				BehaviorDesignerWindow.ShowWindow();
				BehaviorDesignerWindow.instance.LoadBehavior(behaviorSource, false, true);
			}
			GUILayout.EndHorizontal();
			EditorGUILayout.LabelField("Behavior Description", new GUILayoutOption[0]);
			behaviorSource.behaviorDescription = EditorGUILayout.TextArea(behaviorSource.behaviorDescription, new GUILayoutOption[]
			{
				GUILayout.Height(48f)
			});
			if (fromInspector && (showVariables = EditorGUILayout.Foldout(showVariables, "Variables")))
			{
				List<SharedVariable> allVariables = behaviorSource.GetAllVariables();
				if (allVariables != null && VariableInspector.DrawAllVariables(false, behaviorSource, ref allVariables, false, ref ExternalBehaviorInspector.variablePosition, ref ExternalBehaviorInspector.selectedVariableIndex, ref ExternalBehaviorInspector.selectedVariableName, ref ExternalBehaviorInspector.selectedVariableTypeIndex, true, false))
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
			}
			return EditorGUI.EndChangeCheck();
		}
	}
}
