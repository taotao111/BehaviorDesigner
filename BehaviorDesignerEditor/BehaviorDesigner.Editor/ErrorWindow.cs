using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace BehaviorDesigner.Editor
{
	public class ErrorWindow : EditorWindow
	{
		private List<ErrorDetails> mErrorDetails;
		private Vector2 mScrollPosition;
		public static ErrorWindow instance;
		public List<ErrorDetails> ErrorDetails
		{
			set
			{
				this.mErrorDetails = value;
			}
		}
		[MenuItem("Tools/Behavior Designer/Error List", false, 2)]
		public static void ShowWindow()
		{
			ErrorWindow window = EditorWindow.GetWindow<ErrorWindow>(false, "Error List");
			window.minSize = new Vector2(400f, 200f);
			window.wantsMouseMove = true;
		}
		public void OnFocus()
		{
			ErrorWindow.instance = this;
			if (BehaviorDesignerWindow.instance != null)
			{
				this.mErrorDetails = BehaviorDesignerWindow.instance.ErrorDetails;
			}
		}
		public void OnGUI()
		{
			this.mScrollPosition = EditorGUILayout.BeginScrollView(this.mScrollPosition, new GUILayoutOption[0]);
			if (this.mErrorDetails != null && this.mErrorDetails.Count > 0)
			{
				for (int i = 0; i < this.mErrorDetails.Count; i++)
				{
					ErrorDetails errorDetails = this.mErrorDetails[i];
					if (errorDetails != null && !(errorDetails.NodeDesigner == null) && errorDetails.NodeDesigner.Task != null)
					{
						string label = string.Empty;
						switch (errorDetails.Type)
						{
						case BehaviorDesigner.Editor.ErrorDetails.ErrorType.RequiredField:
							label = string.Format("The task {0} ({1}, index {2}) requires a value for the field {3}.", new object[]
							{
								errorDetails.TaskFriendlyName,
								errorDetails.TaskType,
								errorDetails.NodeDesigner.Task.ID,
								BehaviorDesignerUtility.SplitCamelCase(errorDetails.FieldName)
							});
							break;
						case BehaviorDesigner.Editor.ErrorDetails.ErrorType.SharedVariable:
							label = string.Format("The task {0} ({1}, index {2}) has a Shared Variable field ({3}) that is marked as shared but is not referencing a Shared Variable.", new object[]
							{
								errorDetails.TaskFriendlyName,
								errorDetails.TaskType,
								errorDetails.NodeDesigner.Task.ID,
								BehaviorDesignerUtility.SplitCamelCase(errorDetails.FieldName)
							});
							break;
						case BehaviorDesigner.Editor.ErrorDetails.ErrorType.MissingChildren:
							label = string.Format("The {0} task ({1}, index {2}) is a parent task which does not have any children", errorDetails.TaskFriendlyName, errorDetails.TaskType, errorDetails.NodeDesigner.Task.ID);
							break;
						case BehaviorDesigner.Editor.ErrorDetails.ErrorType.UnknownTask:
							label = string.Format("The task at index {0} is unknown. Has a task been renamed or deleted?", errorDetails.NodeDesigner.Task.ID);
							break;
						}
						EditorGUILayout.LabelField(label, (i % 2 != 0) ? BehaviorDesignerUtility.ErrorListDarkBackground : BehaviorDesignerUtility.ErrorListLightBackground, new GUILayoutOption[]
						{
							GUILayout.Height(30f),
							GUILayout.Width((float)(Screen.width - 7))
						});
					}
				}
			}
			else if (!BehaviorDesignerPreferences.GetBool(BDPreferences.ErrorChecking))
			{
				EditorGUILayout.LabelField("Enable realtime error checking from the preferences to view the errors.", BehaviorDesignerUtility.ErrorListLightBackground, new GUILayoutOption[0]);
			}
			else
			{
				EditorGUILayout.LabelField("The behavior tree has no errors.", BehaviorDesignerUtility.ErrorListLightBackground, new GUILayoutOption[0]);
			}
			EditorGUILayout.EndScrollView();
		}
	}
}
