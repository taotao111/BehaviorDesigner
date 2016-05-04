using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
namespace BehaviorDesigner.Editor
{
	public static class ErrorCheck
	{
		public static List<ErrorDetails> CheckForErrors(BehaviorSource behaviorSource)
		{
			if (behaviorSource == null || behaviorSource.EntryTask == null)
			{
				return null;
			}
			List<ErrorDetails> result = null;
			ErrorCheck.CheckTaskForErrors(behaviorSource.EntryTask, ref result);
			if (behaviorSource.RootTask == null)
			{
				ErrorCheck.AddError(ref result, ErrorDetails.ErrorType.MissingChildren, behaviorSource.EntryTask, null);
			}
			if (behaviorSource.RootTask != null)
			{
				ErrorCheck.CheckTaskForErrors(behaviorSource.RootTask, ref result);
			}
			return result;
		}
		private static void CheckTaskForErrors(Task task, ref List<ErrorDetails> errorDetails)
		{
			if (task.NodeData.Disabled)
			{
				return;
			}
			if (task is UnknownTask || task is UnknownParentTask)
			{
				ErrorCheck.AddError(ref errorDetails, ErrorDetails.ErrorType.UnknownTask, task, null);
			}
			if (task.GetType().GetCustomAttributes(typeof(SkipErrorCheckAttribute), false).Length == 0)
			{
				FieldInfo[] allFields = TaskUtility.GetAllFields(task.GetType());
				for (int i = 0; i < allFields.Length; i++)
				{
					FieldInfo fieldInfo = allFields[i];
					object value = fieldInfo.GetValue(task);
					if (TaskUtility.HasAttribute(fieldInfo, typeof(RequiredFieldAttribute)) && !ErrorCheck.IsRequiredFieldValid(fieldInfo.FieldType, value))
					{
						ErrorCheck.AddError(ref errorDetails, ErrorDetails.ErrorType.RequiredField, task, fieldInfo.Name);
					}
					if (fieldInfo.FieldType.Equals(typeof(SharedVariable)) || fieldInfo.FieldType.IsSubclassOf(typeof(SharedVariable)))
					{
						SharedVariable sharedVariable = value as SharedVariable;
						if (sharedVariable != null && sharedVariable.IsShared && string.IsNullOrEmpty(sharedVariable.Name) && !TaskUtility.HasAttribute(fieldInfo, typeof(SharedRequiredAttribute)))
						{
							ErrorCheck.AddError(ref errorDetails, ErrorDetails.ErrorType.SharedVariable, task, fieldInfo.Name);
						}
					}
				}
			}
			if (task is ParentTask && task.NodeData.NodeDesigner != null && !(task.NodeData.NodeDesigner as NodeDesigner).IsEntryDisplay)
			{
				ParentTask parentTask = task as ParentTask;
				if (parentTask.Children == null || parentTask.Children.Count == 0)
				{
					ErrorCheck.AddError(ref errorDetails, ErrorDetails.ErrorType.MissingChildren, task, null);
				}
				else
				{
					for (int j = 0; j < parentTask.Children.Count; j++)
					{
						ErrorCheck.CheckTaskForErrors(parentTask.Children[j], ref errorDetails);
					}
				}
			}
		}
		private static void AddError(ref List<ErrorDetails> errorDetails, ErrorDetails.ErrorType type, Task task, string fieldName)
		{
			if (errorDetails == null)
			{
				errorDetails = new List<ErrorDetails>();
			}
			errorDetails.Add(new ErrorDetails(type, task, fieldName));
		}
		public static bool IsRequiredFieldValid(Type fieldType, object value)
		{
			if (value == null || value.Equals(null))
			{
				return false;
			}
			if (typeof(IList).IsAssignableFrom(fieldType))
			{
				IList list = value as IList;
				if (list.Count == 0)
				{
					return false;
				}
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i] == null || list[i].Equals(null))
					{
						return false;
					}
				}
			}
			return true;
		}
	}
}
