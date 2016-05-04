using BehaviorDesigner.Editor;
using BehaviorDesigner.Runtime;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[CustomObjectDrawer(typeof(GenericVariable))]
public class SharedGenericVariableDrawer : ObjectDrawer
{
	private static string[] variableNames;
	public override void OnGUI(GUIContent label)
	{
		GenericVariable genericVariable = this.value as GenericVariable;
		EditorGUILayout.BeginVertical(new GUILayoutOption[0]);
		if (FieldInspector.DrawFoldout(genericVariable.GetHashCode(), label))
		{
			EditorGUI.indentLevel++;
			if (SharedGenericVariableDrawer.variableNames == null)
			{
				List<Type> list = VariableInspector.FindAllSharedVariableTypes(true);
				SharedGenericVariableDrawer.variableNames = new string[list.Count];
				for (int i = 0; i < list.Count; i++)
				{
					SharedGenericVariableDrawer.variableNames[i] = list[i].Name.Remove(0, 6);
				}
			}
			int num = 0;
			string value = genericVariable.type.Remove(0, 6);
			for (int j = 0; j < SharedGenericVariableDrawer.variableNames.Length; j++)
			{
				if (SharedGenericVariableDrawer.variableNames[j].Equals(value))
				{
					num = j;
					break;
				}
			}
			int num2 = EditorGUILayout.Popup("Type", num, SharedGenericVariableDrawer.variableNames, BehaviorDesignerUtility.SharedVariableToolbarPopup, new GUILayoutOption[0]);
			Type type = VariableInspector.FindAllSharedVariableTypes(true)[num2];
			if (num2 != num)
			{
				num = num2;
				genericVariable.value = (Activator.CreateInstance(type) as SharedVariable);
			}
			GUILayout.Space(3f);
			genericVariable.type = "Shared" + SharedGenericVariableDrawer.variableNames[num];
			genericVariable.value = FieldInspector.DrawSharedVariable(null, new GUIContent("Value"), null, type, genericVariable.value);
			EditorGUI.indentLevel--;
		}
		EditorGUILayout.EndVertical();
	}
}
