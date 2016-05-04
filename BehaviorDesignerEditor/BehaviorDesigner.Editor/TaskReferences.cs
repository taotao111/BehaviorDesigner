using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
namespace BehaviorDesigner.Editor
{
	public class TaskReferences : MonoBehaviour
	{
		public static void CheckReferences(BehaviorSource behaviorSource)
		{
			if (behaviorSource.RootTask != null)
			{
				TaskReferences.CheckReferences(behaviorSource, behaviorSource.RootTask);
			}
			if (behaviorSource.DetachedTasks != null)
			{
				for (int i = 0; i < behaviorSource.DetachedTasks.Count; i++)
				{
					TaskReferences.CheckReferences(behaviorSource, behaviorSource.DetachedTasks[i]);
				}
			}
		}
		private static void CheckReferences(BehaviorSource behaviorSource, Task task)
		{
			FieldInfo[] allFields = TaskUtility.GetAllFields(task.GetType());
			for (int i = 0; i < allFields.Length; i++)
			{
				if (!allFields[i].FieldType.IsArray && (allFields[i].FieldType.Equals(typeof(Task)) || allFields[i].FieldType.IsSubclassOf(typeof(Task))))
				{
					Task task2 = allFields[i].GetValue(task) as Task;
					if (task2 != null)
					{
						Task task3 = TaskReferences.FindReferencedTask(behaviorSource, task2);
						if (task3 != null)
						{
							allFields[i].SetValue(task, task3);
						}
					}
				}
				else if (allFields[i].FieldType.IsArray && (allFields[i].FieldType.GetElementType().Equals(typeof(Task)) || allFields[i].FieldType.GetElementType().IsSubclassOf(typeof(Task))))
				{
					Task[] array = allFields[i].GetValue(task) as Task[];
					if (array != null)
					{
						IList list = Activator.CreateInstance(typeof(List<>).MakeGenericType(new Type[]
						{
							allFields[i].FieldType.GetElementType()
						})) as IList;
						for (int j = 0; j < array.Length; j++)
						{
							Task task4 = TaskReferences.FindReferencedTask(behaviorSource, array[j]);
							if (task4 != null)
							{
								list.Add(task4);
							}
						}
						Array array2 = Array.CreateInstance(allFields[i].FieldType.GetElementType(), list.Count);
						list.CopyTo(array2, 0);
						allFields[i].SetValue(task, array2);
					}
				}
			}
			if (task.GetType().IsSubclassOf(typeof(ParentTask)))
			{
				ParentTask parentTask = task as ParentTask;
				if (parentTask.Children != null)
				{
					for (int k = 0; k < parentTask.Children.Count; k++)
					{
						TaskReferences.CheckReferences(behaviorSource, parentTask.Children[k]);
					}
				}
			}
		}
		private static Task FindReferencedTask(BehaviorSource behaviorSource, Task referencedTask)
		{
			int iD = referencedTask.ID;
			Task result;
			if (behaviorSource.RootTask != null && (result = TaskReferences.FindReferencedTask(behaviorSource.RootTask, iD)) != null)
			{
				return result;
			}
			if (behaviorSource.DetachedTasks != null)
			{
				for (int i = 0; i < behaviorSource.DetachedTasks.Count; i++)
				{
					if ((result = TaskReferences.FindReferencedTask(behaviorSource.DetachedTasks[i], iD)) != null)
					{
						return result;
					}
				}
			}
			return null;
		}
		private static Task FindReferencedTask(Task task, int referencedTaskID)
		{
			if (task.ID == referencedTaskID)
			{
				return task;
			}
			if (task.GetType().IsSubclassOf(typeof(ParentTask)))
			{
				ParentTask parentTask = task as ParentTask;
				if (parentTask.Children != null)
				{
					for (int i = 0; i < parentTask.Children.Count; i++)
					{
						Task result;
						if ((result = TaskReferences.FindReferencedTask(parentTask.Children[i], referencedTaskID)) != null)
						{
							return result;
						}
					}
				}
			}
			return null;
		}
		public static void CheckReferences(Behavior behavior, List<Task> taskList)
		{
			for (int i = 0; i < taskList.Count; i++)
			{
				TaskReferences.CheckReferences(behavior, taskList[i], taskList);
			}
		}
		private static void CheckReferences(Behavior behavior, Task task, List<Task> taskList)
		{
			if (TaskUtility.CompareType(task.GetType(), "BehaviorDesigner.Runtime.Tasks.ConditionalEvaluator"))
			{
				FieldInfo field = task.GetType().GetField("conditionalTask");
				object value = field.GetValue(task);
				if (value != null)
				{
					task = (value as Task);
				}
			}
			FieldInfo[] allFields = TaskUtility.GetAllFields(task.GetType());
			for (int i = 0; i < allFields.Length; i++)
			{
				if (!allFields[i].FieldType.IsArray && (allFields[i].FieldType.Equals(typeof(Task)) || allFields[i].FieldType.IsSubclassOf(typeof(Task))))
				{
					Task task2 = allFields[i].GetValue(task) as Task;
					if (task2 != null && !task2.Owner.Equals(behavior))
					{
						Task task3 = TaskReferences.FindReferencedTask(task2, taskList);
						if (task3 != null)
						{
							allFields[i].SetValue(task, task3);
						}
					}
				}
				else if (allFields[i].FieldType.IsArray && (allFields[i].FieldType.GetElementType().Equals(typeof(Task)) || allFields[i].FieldType.GetElementType().IsSubclassOf(typeof(Task))))
				{
					Task[] array = allFields[i].GetValue(task) as Task[];
					if (array != null)
					{
						IList list = Activator.CreateInstance(typeof(List<>).MakeGenericType(new Type[]
						{
							allFields[i].FieldType.GetElementType()
						})) as IList;
						for (int j = 0; j < array.Length; j++)
						{
							Task task4 = TaskReferences.FindReferencedTask(array[j], taskList);
							if (task4 != null)
							{
								list.Add(task4);
							}
						}
						Array array2 = Array.CreateInstance(allFields[i].FieldType.GetElementType(), list.Count);
						list.CopyTo(array2, 0);
						allFields[i].SetValue(task, array2);
					}
				}
			}
		}
		private static Task FindReferencedTask(Task referencedTask, List<Task> taskList)
		{
			int referenceID = referencedTask.ReferenceID;
			for (int i = 0; i < taskList.Count; i++)
			{
				if (taskList[i].ReferenceID == referenceID)
				{
					return taskList[i];
				}
			}
			return null;
		}
	}
}
