using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
namespace BehaviorDesigner.Editor
{
	public class SerializeJSON : UnityEngine.Object
	{
		private static TaskSerializationData taskSerializationData;
		private static FieldSerializationData fieldSerializationData;
		private static VariableSerializationData variableSerializationData;
		public static void Save(BehaviorSource behaviorSource)
		{
			behaviorSource.CheckForSerialization(false, null);
			SerializeJSON.taskSerializationData = new TaskSerializationData();
			SerializeJSON.fieldSerializationData = SerializeJSON.taskSerializationData.fieldSerializationData;
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			if (behaviorSource.EntryTask != null)
			{
				dictionary.Add("EntryTask", SerializeJSON.SerializeTask(behaviorSource.EntryTask, true, ref SerializeJSON.fieldSerializationData.unityObjects));
			}
			if (behaviorSource.RootTask != null)
			{
				dictionary.Add("RootTask", SerializeJSON.SerializeTask(behaviorSource.RootTask, true, ref SerializeJSON.fieldSerializationData.unityObjects));
			}
			if (behaviorSource.DetachedTasks != null && behaviorSource.DetachedTasks.Count > 0)
			{
				Dictionary<string, object>[] array = new Dictionary<string, object>[behaviorSource.DetachedTasks.Count];
				for (int i = 0; i < behaviorSource.DetachedTasks.Count; i++)
				{
					array[i] = SerializeJSON.SerializeTask(behaviorSource.DetachedTasks[i], true, ref SerializeJSON.fieldSerializationData.unityObjects);
				}
				dictionary.Add("DetachedTasks", array);
			}
			if (behaviorSource.Variables != null && behaviorSource.Variables.Count > 0)
			{
				dictionary.Add("Variables", SerializeJSON.SerializeVariables(behaviorSource.Variables, ref SerializeJSON.fieldSerializationData.unityObjects));
			}
			SerializeJSON.taskSerializationData.JSONSerialization = MiniJSON.Serialize(dictionary);
			behaviorSource.TaskData = SerializeJSON.taskSerializationData;
			if (behaviorSource.Owner != null)
			{
				EditorUtility.SetDirty(behaviorSource.Owner.GetObject());
			}
		}
		public static void Save(GlobalVariables variables)
		{
			if (variables == null)
			{
				return;
			}
			SerializeJSON.variableSerializationData = new VariableSerializationData();
			SerializeJSON.fieldSerializationData = SerializeJSON.variableSerializationData.fieldSerializationData;
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("Variables", SerializeJSON.SerializeVariables(variables.Variables, ref SerializeJSON.fieldSerializationData.unityObjects));
			SerializeJSON.variableSerializationData.JSONSerialization = MiniJSON.Serialize(dictionary);
			variables.VariableData = SerializeJSON.variableSerializationData;
			EditorUtility.SetDirty(variables);
		}
		private static Dictionary<string, object>[] SerializeVariables(List<SharedVariable> variables, ref List<UnityEngine.Object> unityObjects)
		{
			Dictionary<string, object>[] array = new Dictionary<string, object>[variables.Count];
			for (int i = 0; i < variables.Count; i++)
			{
				array[i] = SerializeJSON.SerializeVariable(variables[i], ref unityObjects);
			}
			return array;
		}
		public static Dictionary<string, object> SerializeTask(Task task, bool serializeChildren, ref List<UnityEngine.Object> unityObjects)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("ObjectType", task.GetType());
			dictionary.Add("NodeData", SerializeJSON.SerializeNodeData(task.NodeData));
			dictionary.Add("ID", task.ID);
			dictionary.Add("Name", task.FriendlyName);
			dictionary.Add("Instant", task.IsInstant);
			SerializeJSON.SerializeFields(task, ref dictionary, ref unityObjects);
			if (serializeChildren && task is ParentTask)
			{
				ParentTask parentTask = task as ParentTask;
				if (parentTask.Children != null && parentTask.Children.Count > 0)
				{
					Dictionary<string, object>[] array = new Dictionary<string, object>[parentTask.Children.Count];
					for (int i = 0; i < parentTask.Children.Count; i++)
					{
						array[i] = SerializeJSON.SerializeTask(parentTask.Children[i], serializeChildren, ref unityObjects);
					}
					dictionary.Add("Children", array);
				}
			}
			return dictionary;
		}
		private static Dictionary<string, object> SerializeNodeData(NodeData nodeData)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("Offset", nodeData.Offset);
			if (nodeData.Comment.Length > 0)
			{
				dictionary.Add("Comment", nodeData.Comment);
			}
			if (nodeData.IsBreakpoint)
			{
				dictionary.Add("IsBreakpoint", nodeData.IsBreakpoint);
			}
			if (nodeData.Collapsed)
			{
				dictionary.Add("Collapsed", nodeData.Collapsed);
			}
			if (nodeData.Disabled)
			{
				dictionary.Add("Disabled", nodeData.Disabled);
			}
			if (nodeData.ColorIndex != 0)
			{
				dictionary.Add("ColorIndex", nodeData.ColorIndex);
			}
			if (nodeData.WatchedFieldNames != null && nodeData.WatchedFieldNames.Count > 0)
			{
				dictionary.Add("WatchedFields", nodeData.WatchedFieldNames);
			}
			return dictionary;
		}
		private static Dictionary<string, object> SerializeVariable(SharedVariable sharedVariable, ref List<UnityEngine.Object> unityObjects)
		{
			if (sharedVariable == null)
			{
				return null;
			}
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("Type", sharedVariable.GetType());
			dictionary.Add("Name", sharedVariable.Name);
			if (sharedVariable.IsShared)
			{
				dictionary.Add("IsShared", sharedVariable.IsShared);
			}
			if (sharedVariable.IsGlobal)
			{
				dictionary.Add("IsGlobal", sharedVariable.IsGlobal);
			}
			if (sharedVariable.NetworkSync)
			{
				dictionary.Add("NetworkSync", sharedVariable.NetworkSync);
			}
			if (!string.IsNullOrEmpty(sharedVariable.PropertyMapping))
			{
				dictionary.Add("PropertyMapping", sharedVariable.PropertyMapping);
				if (!object.Equals(sharedVariable.PropertyMappingOwner, null))
				{
					dictionary.Add("PropertyMappingOwner", unityObjects.Count);
					unityObjects.Add(sharedVariable.PropertyMappingOwner);
				}
			}
			SerializeJSON.SerializeFields(sharedVariable, ref dictionary, ref unityObjects);
			return dictionary;
		}
		private static void SerializeFields(object obj, ref Dictionary<string, object> dict, ref List<UnityEngine.Object> unityObjects)
		{
			FieldInfo[] allFields = TaskUtility.GetAllFields(obj.GetType());
			for (int i = 0; i < allFields.Length; i++)
			{
				if (!BehaviorDesignerUtility.HasAttribute(allFields[i], typeof(NonSerializedAttribute)) && ((!allFields[i].IsPrivate && !allFields[i].IsFamily) || BehaviorDesignerUtility.HasAttribute(allFields[i], typeof(SerializeField))) && (!(obj is ParentTask) || !allFields[i].Name.Equals("children")))
				{
					if (allFields[i].GetValue(obj) != null)
					{
						if (typeof(IList).IsAssignableFrom(allFields[i].FieldType))
						{
							IList list = allFields[i].GetValue(obj) as IList;
							if (list != null)
							{
								List<object> list2 = new List<object>();
								for (int j = 0; j < list.Count; j++)
								{
									if (list[j] == null || object.ReferenceEquals(list[j], null) || list[j].Equals(null))
									{
										list2.Add(-1);
									}
									else
									{
										Type type = list[j].GetType();
										if (list[j] is Task)
										{
											Task task = list[j] as Task;
											list2.Add(task.ID);
										}
										else if (list[j] is SharedVariable)
										{
											list2.Add(SerializeJSON.SerializeVariable(list[j] as SharedVariable, ref unityObjects));
										}
										else if (list[j] is UnityEngine.Object)
										{
											UnityEngine.Object @object = list[j] as UnityEngine.Object;
											if (!object.ReferenceEquals(@object, null) && @object != null)
											{
												list2.Add(unityObjects.Count);
												unityObjects.Add(@object);
											}
										}
										else if (type.Equals(typeof(LayerMask)))
										{
											list2.Add(((LayerMask)list[j]).value);
										}
										else if (type.IsPrimitive || type.IsEnum || type.Equals(typeof(string)) || type.Equals(typeof(Vector2)) || type.Equals(typeof(Vector3)) || type.Equals(typeof(Vector4)) || type.Equals(typeof(Quaternion)) || type.Equals(typeof(Matrix4x4)) || type.Equals(typeof(Color)) || type.Equals(typeof(Rect)))
										{
											list2.Add(list[j]);
										}
										else
										{
											Dictionary<string, object> item = new Dictionary<string, object>();
											SerializeJSON.SerializeFields(list[j], ref item, ref unityObjects);
											list2.Add(item);
										}
									}
								}
								if (list2 != null)
								{
									dict.Add(allFields[i].FieldType + "," + allFields[i].Name, list2);
								}
							}
						}
						else if (allFields[i].FieldType.Equals(typeof(Task)) || allFields[i].FieldType.IsSubclassOf(typeof(Task)))
						{
							Task task2 = allFields[i].GetValue(obj) as Task;
							if (task2 != null)
							{
								if (BehaviorDesignerUtility.HasAttribute(allFields[i], typeof(InspectTaskAttribute)))
								{
									Dictionary<string, object> dictionary = new Dictionary<string, object>();
									dictionary.Add("ObjectType", task2.GetType());
									SerializeJSON.SerializeFields(task2, ref dictionary, ref unityObjects);
									dict.Add(allFields[i].Name, dictionary);
								}
								else
								{
									dict.Add(allFields[i].FieldType + "," + allFields[i].Name, task2.ID);
								}
							}
						}
						else if (allFields[i].FieldType.Equals(typeof(SharedVariable)) || allFields[i].FieldType.IsSubclassOf(typeof(SharedVariable)))
						{
							dict.Add(allFields[i].FieldType + "," + allFields[i].Name, SerializeJSON.SerializeVariable(allFields[i].GetValue(obj) as SharedVariable, ref unityObjects));
						}
						else if (allFields[i].FieldType.Equals(typeof(UnityEngine.Object)) || allFields[i].FieldType.IsSubclassOf(typeof(UnityEngine.Object)))
						{
							UnityEngine.Object object2 = allFields[i].GetValue(obj) as UnityEngine.Object;
							if (!object.ReferenceEquals(object2, null) && object2 != null)
							{
								dict.Add(allFields[i].FieldType + "," + allFields[i].Name, unityObjects.Count);
								unityObjects.Add(object2);
							}
						}
						else if (allFields[i].FieldType.Equals(typeof(LayerMask)))
						{
							dict.Add(allFields[i].FieldType + "," + allFields[i].Name, ((LayerMask)allFields[i].GetValue(obj)).value);
						}
						else if (allFields[i].FieldType.IsPrimitive || allFields[i].FieldType.IsEnum || allFields[i].FieldType.Equals(typeof(string)) || allFields[i].FieldType.Equals(typeof(Vector2)) || allFields[i].FieldType.Equals(typeof(Vector3)) || allFields[i].FieldType.Equals(typeof(Vector4)) || allFields[i].FieldType.Equals(typeof(Quaternion)) || allFields[i].FieldType.Equals(typeof(Matrix4x4)) || allFields[i].FieldType.Equals(typeof(Color)) || allFields[i].FieldType.Equals(typeof(Rect)))
						{
							dict.Add(allFields[i].FieldType + "," + allFields[i].Name, allFields[i].GetValue(obj));
						}
						else if (allFields[i].FieldType.Equals(typeof(AnimationCurve)))
						{
							AnimationCurve animationCurve = allFields[i].GetValue(obj) as AnimationCurve;
							Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
							if (animationCurve.keys != null)
							{
								Keyframe[] keys = animationCurve.keys;
								List<List<object>> list3 = new List<List<object>>();
								for (int k = 0; k < keys.Length; k++)
								{
									list3.Add(new List<object>
									{
										keys[k].time,
										keys[k].value,
										keys[k].inTangent,
										keys[k].outTangent,
										keys[k].tangentMode
									});
								}
								dictionary2.Add("Keys", list3);
							}
							dictionary2.Add("PreWrapMode", animationCurve.preWrapMode);
							dictionary2.Add("PostWrapMode", animationCurve.postWrapMode);
							dict.Add(allFields[i].FieldType + "," + allFields[i].Name, dictionary2);
						}
						else
						{
							Dictionary<string, object> value = new Dictionary<string, object>();
							SerializeJSON.SerializeFields(allFields[i].GetValue(obj), ref value, ref unityObjects);
							dict.Add(allFields[i].FieldType + "," + allFields[i].Name, value);
						}
					}
				}
			}
		}
	}
}
