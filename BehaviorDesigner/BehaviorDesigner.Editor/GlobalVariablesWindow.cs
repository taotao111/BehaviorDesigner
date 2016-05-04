using BehaviorDesigner.Runtime;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace BehaviorDesigner.Editor
{
	public class GlobalVariablesWindow : EditorWindow
	{
		private string mVariableName = string.Empty;
		private int mVariableTypeIndex;
		private Vector2 mScrollPosition = Vector2.zero;
		private bool mFocusNameField;
		[SerializeField]
		private float mVariableStartPosition = -1f;
		[SerializeField]
		private List<float> mVariablePosition;
		[SerializeField]
		private int mSelectedVariableIndex = -1;
		[SerializeField]
		private string mSelectedVariableName;
		[SerializeField]
		private int mSelectedVariableTypeIndex;
		private GlobalVariables mVariableSource;
		public static GlobalVariablesWindow instance;
		[MenuItem("Tools/Behavior Designer/Global Variables", false, 1)]
		public static void ShowWindow()
		{
			GlobalVariablesWindow window = EditorWindow.GetWindow<GlobalVariablesWindow>(false, "Global Variables");
			window.minSize = new Vector2(300f, 410f);
			window.maxSize = new Vector2(300f, 3.40282347E+38f);
			window.wantsMouseMove = true;
		}
		public void OnFocus()
		{
			GlobalVariablesWindow.instance = this;
			this.mVariableSource = GlobalVariables.Instance;
			if (this.mVariableSource != null)
			{
				this.mVariableSource.CheckForSerialization(!Application.isPlaying);
			}
			FieldInspector.Init();
		}
		public void OnGUI()
		{
			if (this.mVariableSource == null)
			{
				this.mVariableSource = GlobalVariables.Instance;
			}
			if (VariableInspector.DrawVariables(this.mVariableSource, true, null, ref this.mVariableName, ref this.mFocusNameField, ref this.mVariableTypeIndex, ref this.mScrollPosition, ref this.mVariablePosition, ref this.mVariableStartPosition, ref this.mSelectedVariableIndex, ref this.mSelectedVariableName, ref this.mSelectedVariableTypeIndex))
			{
				this.SerializeVariables();
			}
			if (Event.current.type == EventType.MouseDown && VariableInspector.LeftMouseDown(this.mVariableSource, null, Event.current.mousePosition, this.mVariablePosition, this.mVariableStartPosition, this.mScrollPosition, ref this.mSelectedVariableIndex, ref this.mSelectedVariableName, ref this.mSelectedVariableTypeIndex))
			{
				Event.current.Use();
				base.Repaint();
			}
		}
		private void SerializeVariables()
		{
			if (this.mVariableSource == null)
			{
				this.mVariableSource = GlobalVariables.Instance;
			}
			if (BehaviorDesignerPreferences.GetBool(BDPreferences.BinarySerialization))
			{
				BinarySerialization.Save(this.mVariableSource);
			}
			else
			{
				SerializeJSON.Save(this.mVariableSource);
			}
		}
	}
}
