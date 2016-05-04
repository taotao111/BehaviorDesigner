using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
namespace BehaviorDesigner.Runtime
{
	public class DeserializeJSON : UnityEngine.Object
	{
		private struct TaskField
		{
			public Task task;
			public FieldInfo fieldInfo;
			public TaskField(Task t, FieldInfo f)
			{
				this.task = t;
				this.fieldInfo = f;
			}
		}
		private static Dictionary<DeserializeJSON.TaskField, List<int>> taskIDs = null;
		private static GlobalVariables globalVariables = null;
		private static Dictionary<int, Dictionary<string, object>> serializationCache = new Dictionary<int, Dictionary<string, object>>();
		public static void Load(TaskSerializationData taskData, BehaviorSource behaviorSource)
		{
			behaviorSource.EntryTask = null;
			behaviorSource.RootTask = null;
			behaviorSource.DetachedTasks = null;
			behaviorSource.Variables = null;
			Dictionary<string, object> dictionary;
			if (!DeserializeJSON.serializationCache.TryGetValue(taskData.JSONSerialization.GetHashCode(), out dictionary))
			{
				dictionary = (MiniJSON.Deserialize(taskData.JSONSerialization) as Dictionary<string, object>);
				DeserializeJSON.serializationCache.Add(taskData.JSONSerialization.GetHashCode(), dictionary);
			}
			if (dictionary == null)
			{
				Debug.Log("Failed to deserialize");
				return;
			}
			DeserializeJSON.taskIDs = new Dictionary<DeserializeJSON.TaskField, List<int>>();
			Dictionary<int, Task> dictionary2 = new Dictionary<int, Task>();
			DeserializeJSON.DeserializeVariables(behaviorSource, dictionary, taskData.fieldSerializationData.unityObjects);
			if (dictionary.ContainsKey("EntryTask"))
			{
				behaviorSource.EntryTask = DeserializeJSON.DeserializeTask(behaviorSource, dictionary["EntryTask"] as Dictionary<string, object>, ref dictionary2, taskData.fieldSerializationData.unityObjects);
			}
			if (dictionary.ContainsKey("RootTask"))
			{
				behaviorSource.RootTask = DeserializeJSON.DeserializeTask(behaviorSource, dictionary["RootTask"] as Dictionary<string, object>, ref dictionary2, taskData.fieldSerializationData.unityObjects);
			}
			if (dictionary.ContainsKey("DetachedTasks"))
			{
				List<Task> list = new List<Task>();
				foreach (Dictionary<string, object> dict in dictionary["DetachedTasks"] as IEnumerable)
				{
					list.Add(DeserializeJSON.DeserializeTask(behaviorSource, dict, ref dictionary2, taskData.fieldSerializationData.unityObjects));
				}
				behaviorSource.DetachedTasks = list;
			}
			if (DeserializeJSON.taskIDs != null && DeserializeJSON.taskIDs.Count > 0)
			{
				foreach (DeserializeJSON.TaskField current in DeserializeJSON.taskIDs.Keys)
				{
					List<int> list2 = DeserializeJSON.taskIDs[current];
					Type fieldType = current.fieldInfo.FieldType;
					if (current.fieldInfo.FieldType.IsArray)
					{
						int num = 0;
						for (int i = 0; i < list2.Count; i++)
						{
							Task task = dictionary2[list2[i]];
							if (task.GetType().Equals(fieldType.GetElementType()) || task.GetType().IsSubclassOf(fieldType.GetElementType()))
							{
								num++;
							}
						}
						Array array = Array.CreateInstance(fieldType.GetElementType(), num);
						int num2 = 0;
						for (int j = 0; j < list2.Count; j++)
						{
							Task task2 = dictionary2[list2[j]];
							if (task2.GetType().Equals(fieldType.GetElementType()) || task2.GetType().IsSubclassOf(fieldType.GetElementType()))
							{
								array.SetValue(task2, num2);
								num2++;
							}
						}
						current.fieldInfo.SetValue(current.task, array);
					}
					else
					{
						Task task3 = dictionary2[list2[0]];
						if (task3.GetType().Equals(current.fieldInfo.FieldType) || task3.GetType().IsSubclassOf(current.fieldInfo.FieldType))
						{
							current.fieldInfo.SetValue(current.task, task3);
						}
					}
				}
				DeserializeJSON.taskIDs = null;
			}
		}
		public static void Load(string serialization, GlobalVariables globalVariables)
		{
			if (globalVariables == null)
			{
				return;
			}
			Dictionary<string, object> dictionary = MiniJSON.Deserialize(serialization) as Dictionary<string, object>;
			if (dictionary == null)
			{
				Debug.Log("Failed to deserialize");
				return;
			}
			if (globalVariables.VariableData == null)
			{
				globalVariables.VariableData = new VariableSerializationData();
			}
			DeserializeJSON.DeserializeVariables(globalVariables, dictionary, globalVariables.VariableData.fieldSerializationData.unityObjects);
		}
		private static void DeserializeVariables(IVariableSource variableSource, Dictionary<string, object> dict, List<UnityEngine.Object> unityObjects)
		{
			object obj;
			if (dict.TryGetValue("Variables", out obj))
			{
				List<SharedVariable> list = new List<SharedVariable>();
				IList list2 = obj as IList;
				for (int i = 0; i < list2.Count; i++)
				{
					SharedVariable item = DeserializeJSON.DeserializeSharedVariable(list2[i] as Dictionary<string, object>, variableSource, true, unityObjects);
					list.Add(item);
				}
				variableSource.SetAllVariables(list);
			}
		}
		public static Task DeserializeTask(BehaviorSource behaviorSource, Dictionary<string, object> dict, ref Dictionary<int, Task> IDtoTask, List<UnityEngine.Object> unityObjects)
		{
			Task task = null;
			try
			{
				Type type = TaskUtility.GetTypeWithinAssembly(dict["ObjectType"] as string);
				if (type == null)
				{
					if (dict.ContainsKey("Children"))
					{
						type = typeof(UnknownParentTask);
					}
					else
					{
						type = typeof(UnknownTask);
					}
				}
				task = (TaskUtility.CreateInstance(type) as Task);
			}
			catch (Exception)
			{
			}
			if (task == null)
			{
				Debug.Log("Error: task is null of type " + dict["ObjectType"]);
				return null;
			}
			task.Owner = (behaviorSource.Owner.GetObject() as Behavior);
			task.ID = Convert.ToInt32(dict["ID"]);
			object obj;
			if (dict.TryGetValue("Name", out obj))
			{
				task.FriendlyName = (string)obj;
			}
			if (dict.TryGetValue("Instant", out obj))
			{
				task.IsInstant = Convert.ToBoolean(obj);
			}
			IDtoTask.Add(task.ID, task);
			task.NodeData = DeserializeJSON.DeserializeNodeData(dict["NodeData"] as Dictionary<string, object>, task);
			if (task.GetType().Equals(typeof(UnknownTask)) || task.GetType().Equals(typeof(UnknownParentTask)))
			{
				if (!task.FriendlyName.Contains("Unknown "))
				{
					task.FriendlyName = string.Format("Unknown {0}", task.FriendlyName);
				}
				if (!task.NodeData.Comment.Contains("Loaded from an unknown type. Was a task renamed or deleted?"))
				{
					task.NodeData.Comment = string.Format("Loaded from an unknown type. Was a task renamed or deleted?{0}", (!task.NodeData.Comment.Equals(string.Empty)) ? string.Format("\0{0}", task.NodeData.Comment) : string.Empty);
				}
			}
			DeserializeJSON.DeserializeObject(task, task, dict, behaviorSource, unityObjects);
			if (task is ParentTask && dict.TryGetValue("Children", out obj))
			{
				ParentTask parentTask = task as ParentTask;
				if (parentTask != null)
				{
					foreach (Dictionary<string, object> dict2 in obj as IEnumerable)
					{
						Task child = DeserializeJSON.DeserializeTask(behaviorSource, dict2, ref IDtoTask, unityObjects);
						int index = (parentTask.Children != null) ? parentTask.Children.Count : 0;
						parentTask.AddChild(child, index);
					}
				}
			}
			return task;
		}
		private static NodeData DeserializeNodeData(Dictionary<string, object> dict, Task task)
		{
			NodeData nodeData = new NodeData();
			object obj;
			if (dict.TryGetValue("Offset", out obj))
			{
				nodeData.Offset = DeserializeJSON.StringToVector2((string)obj);
			}
			if (dict.TryGetValue("FriendlyName", out obj))
			{
				task.FriendlyName = (string)obj;
			}
			if (dict.TryGetValue("Comment", out obj))
			{
				nodeData.Comment = (string)obj;
			}
			if (dict.TryGetValue("IsBreakpoint", out obj))
			{
				nodeData.IsBreakpoint = Convert.ToBoolean(obj);
			}
			if (dict.TryGetValue("Collapsed", out obj))
			{
				nodeData.Collapsed = Convert.ToBoolean(obj);
			}
			if (dict.TryGetValue("Disabled", out obj))
			{
				nodeData.Disabled = Convert.ToBoolean(obj);
			}
			if (dict.TryGetValue("ColorIndex", out obj))
			{
				nodeData.ColorIndex = Convert.ToInt32(obj);
			}
			if (dict.TryGetValue("WatchedFields", out obj))
			{
				nodeData.WatchedFieldNames = new List<string>();
				nodeData.WatchedFields = new List<FieldInfo>();
				IList list = obj as IList;
				for (int i = 0; i < list.Count; i++)
				{
					FieldInfo field = task.GetType().GetField((string)list[i], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (field != null)
					{
						nodeData.WatchedFieldNames.Add(field.Name);
						nodeData.WatchedFields.Add(field);
					}
				}
			}
			return nodeData;
		}
		private static SharedVariable DeserializeSharedVariable(Dictionary<string, object> dict, IVariableSource variableSource, bool fromSource, List<UnityEngine.Object> unityObjects)
		{
			if (dict == null)
			{
				return null;
			}
			SharedVariable sharedVariable = null;
			object obj;
			if (!fromSource && variableSource != null && dict.TryGetValue("Name", out obj))
			{
				object value;
				dict.TryGetValue("IsGlobal", out value);
				if (!dict.TryGetValue("IsGlobal", out value) || !Convert.ToBoolean(value))
				{
					sharedVariable = variableSource.GetVariable(obj as string);
				}
				else
				{
					if (DeserializeJSON.globalVariables == null)
					{
						DeserializeJSON.globalVariables = GlobalVariables.Instance;
					}
					if (DeserializeJSON.globalVariables != null)
					{
						sharedVariable = DeserializeJSON.globalVariables.GetVariable(obj as string);
					}
				}
			}
			Type typeWithinAssembly = TaskUtility.GetTypeWithinAssembly(dict["Type"] as string);
			if (typeWithinAssembly == null)
			{
				return null;
			}
			bool flag = true;
			if (sharedVariable == null || !(flag = sharedVariable.GetType().Equals(typeWithinAssembly)))
			{
				sharedVariable = (TaskUtility.CreateInstance(typeWithinAssembly) as SharedVariable);
				sharedVariable.Name = (dict["Name"] as string);
				object obj2;
				if (dict.TryGetValue("IsShared", out obj2))
				{
					sharedVariable.IsShared = Convert.ToBoolean(obj2);
				}
				if (dict.TryGetValue("IsGlobal", out obj2))
				{
					sharedVariable.IsGlobal = Convert.ToBoolean(obj2);
				}
				if (dict.TryGetValue("NetworkSync", out obj2))
				{
					sharedVariable.NetworkSync = Convert.ToBoolean(obj2);
				}
				if (!sharedVariable.IsGlobal && dict.TryGetValue("PropertyMapping", out obj2))
				{
					sharedVariable.PropertyMapping = (obj2 as string);
					if (dict.TryGetValue("PropertyMappingOwner", out obj2))
					{
						sharedVariable.PropertyMappingOwner = (DeserializeJSON.IndexToUnityObject(Convert.ToInt32(obj2), unityObjects) as GameObject);
					}
					sharedVariable.InitializePropertyMapping(variableSource as BehaviorSource);
				}
				if (!flag)
				{
					sharedVariable.IsShared = true;
				}
				DeserializeJSON.DeserializeObject(null, sharedVariable, dict, variableSource, unityObjects);
			}
			return sharedVariable;
		}
		private static void DeserializeObject(Task task, object obj, Dictionary<string, object> dict, IVariableSource variableSource, List<UnityEngine.Object> unityObjects)
		{
			if (dict == null)
			{
				return;
			}
			FieldInfo[] allFields = TaskUtility.GetAllFields(obj.GetType());
			for (int i = 0; i < allFields.Length; i++)
			{
				object obj2;
				if (dict.TryGetValue(allFields[i].FieldType + "," + allFields[i].Name, out obj2) || dict.TryGetValue(allFields[i].Name, out obj2))
				{
					if (typeof(IList).IsAssignableFrom(allFields[i].FieldType))
					{
						IList list = obj2 as IList;
						if (list != null)
						{
							Type type;
							if (allFields[i].FieldType.IsArray)
							{
								type = allFields[i].FieldType.GetElementType();
							}
							else
							{
								Type type2 = allFields[i].FieldType;
								while (!type2.IsGenericType)
								{
									type2 = type2.BaseType;
								}
								type = type2.GetGenericArguments()[0];
							}
							bool flag = type.Equals(typeof(Task)) || type.IsSubclassOf(typeof(Task));
							if (flag)
							{
								if (DeserializeJSON.taskIDs != null)
								{
									List<int> list2 = new List<int>();
									for (int j = 0; j < list.Count; j++)
									{
										list2.Add(Convert.ToInt32(list[j]));
									}
									DeserializeJSON.taskIDs.Add(new DeserializeJSON.TaskField(task, allFields[i]), list2);
								}
							}
							else
							{
								if (allFields[i].FieldType.IsArray)
								{
									Array array = Array.CreateInstance(type, list.Count);
									for (int k = 0; k < list.Count; k++)
									{
										array.SetValue(DeserializeJSON.ValueToObject(task, type, list[k], variableSource, unityObjects), k);
									}
									allFields[i].SetValue(obj, array);
								}
								else
								{
									IList list3;
									if (allFields[i].FieldType.IsGenericType)
									{
										list3 = (TaskUtility.CreateInstance(typeof(List<>).MakeGenericType(new Type[]
										{
											type
										})) as IList);
									}
									else
									{
										list3 = (TaskUtility.CreateInstance(allFields[i].FieldType) as IList);
									}
									for (int l = 0; l < list.Count; l++)
									{
										list3.Add(DeserializeJSON.ValueToObject(task, type, list[l], variableSource, unityObjects));
									}
									allFields[i].SetValue(obj, list3);
								}
							}
						}
					}
					else
					{
						Type fieldType = allFields[i].FieldType;
						if (fieldType.Equals(typeof(Task)) || fieldType.IsSubclassOf(typeof(Task)))
						{
							if (TaskUtility.HasAttribute(allFields[i], typeof(InspectTaskAttribute)))
							{
								Dictionary<string, object> dictionary = obj2 as Dictionary<string, object>;
								Type typeWithinAssembly = TaskUtility.GetTypeWithinAssembly(dictionary["ObjectType"] as string);
								if (typeWithinAssembly != null)
								{
									Task task2 = TaskUtility.CreateInstance(typeWithinAssembly) as Task;
									DeserializeJSON.DeserializeObject(task2, task2, dictionary, variableSource, unityObjects);
									allFields[i].SetValue(task, task2);
								}
							}
							else
							{
								if (DeserializeJSON.taskIDs != null)
								{
									List<int> list4 = new List<int>();
									list4.Add(Convert.ToInt32(obj2));
									DeserializeJSON.taskIDs.Add(new DeserializeJSON.TaskField(task, allFields[i]), list4);
								}
							}
						}
						else
						{
							allFields[i].SetValue(obj, DeserializeJSON.ValueToObject(task, fieldType, obj2, variableSource, unityObjects));
						}
					}
				}
				else
				{
					if (typeof(SharedVariable).IsAssignableFrom(allFields[i].FieldType))
					{
						Type type3 = TaskUtility.SharedVariableToConcreteType(allFields[i].FieldType);
						if (type3 == null)
						{
							return;
						}
						if (dict.TryGetValue(type3 + "," + allFields[i].Name, out obj2))
						{
							SharedVariable sharedVariable = TaskUtility.CreateInstance(allFields[i].FieldType) as SharedVariable;
							sharedVariable.SetValue(DeserializeJSON.ValueToObject(task, type3, obj2, variableSource, unityObjects));
							allFields[i].SetValue(obj, sharedVariable);
						}
						else
						{
							SharedVariable value = TaskUtility.CreateInstance(allFields[i].FieldType) as SharedVariable;
							allFields[i].SetValue(obj, value);
						}
					}
				}
			}
		}
		private static object ValueToObject(Task task, Type type, object obj, IVariableSource variableSource, List<UnityEngine.Object> unityObjects)
		{
			if (type.Equals(typeof(SharedVariable)) || type.IsSubclassOf(typeof(SharedVariable)))
			{
				SharedVariable sharedVariable = DeserializeJSON.DeserializeSharedVariable(obj as Dictionary<string, object>, variableSource, false, unityObjects);
				if (sharedVariable == null)
				{
					sharedVariable = (TaskUtility.CreateInstance(type) as SharedVariable);
				}
				return sharedVariable;
			}
			if (type.Equals(typeof(UnityEngine.Object)) || type.IsSubclassOf(typeof(UnityEngine.Object)))
			{
				return DeserializeJSON.IndexToUnityObject(Convert.ToInt32(obj), unityObjects);
			}
			if (!type.IsPrimitive)
			{
				if (!type.Equals(typeof(string)))
				{
					goto IL_C5;
				}
			}
			try
			{
				object result = Convert.ChangeType(obj, type);
				return result;
			}
			catch (Exception)
			{
				object result = null;
				return result;
			}
			IL_C5:
			if (type.IsSubclassOf(typeof(Enum)))
			{
				try
				{
					object result = Enum.Parse(type, (string)obj);
					return result;
				}
				catch (Exception)
				{
					object result = null;
					return result;
				}
			}
			if (type.Equals(typeof(Vector2)))
			{
				return DeserializeJSON.StringToVector2((string)obj);
			}
			if (type.Equals(typeof(Vector3)))
			{
				return DeserializeJSON.StringToVector3((string)obj);
			}
			if (type.Equals(typeof(Vector4)))
			{
				return DeserializeJSON.StringToVector4((string)obj);
			}
			if (type.Equals(typeof(Quaternion)))
			{
				return DeserializeJSON.StringToQuaternion((string)obj);
			}
			if (type.Equals(typeof(Matrix4x4)))
			{
				return DeserializeJSON.StringToMatrix4x4((string)obj);
			}
			if (type.Equals(typeof(Color)))
			{
				return DeserializeJSON.StringToColor((string)obj);
			}
			if (type.Equals(typeof(Rect)))
			{
				return DeserializeJSON.StringToRect((string)obj);
			}
			if (type.Equals(typeof(LayerMask)))
			{
				return DeserializeJSON.ValueToLayerMask(Convert.ToInt32(obj));
			}
			if (type.Equals(typeof(AnimationCurve)))
			{
				return DeserializeJSON.ValueToAnimationCurve((Dictionary<string, object>)obj);
			}
			object obj2 = TaskUtility.CreateInstance(type);
			DeserializeJSON.DeserializeObject(task, obj2, obj as Dictionary<string, object>, variableSource, unityObjects);
			return obj2;
		}
		private static Vector2 StringToVector2(string vector2String)
		{
			string[] array = vector2String.Substring(1, vector2String.Length - 2).Split(new char[]
			{
				','
			});
			return new Vector2(float.Parse(array[0]), float.Parse(array[1]));
		}
		private static Vector3 StringToVector3(string vector3String)
		{
			string[] array = vector3String.Substring(1, vector3String.Length - 2).Split(new char[]
			{
				','
			});
			return new Vector3(float.Parse(array[0]), float.Parse(array[1]), float.Parse(array[2]));
		}
		private static Vector4 StringToVector4(string vector4String)
		{
			string[] array = vector4String.Substring(1, vector4String.Length - 2).Split(new char[]
			{
				','
			});
			return new Vector4(float.Parse(array[0]), float.Parse(array[1]), float.Parse(array[2]), float.Parse(array[3]));
		}
		private static Quaternion StringToQuaternion(string quaternionString)
		{
			string[] array = quaternionString.Substring(1, quaternionString.Length - 2).Split(new char[]
			{
				','
			});
			return new Quaternion(float.Parse(array[0]), float.Parse(array[1]), float.Parse(array[2]), float.Parse(array[3]));
		}
		private static Matrix4x4 StringToMatrix4x4(string matrixString)
		{
			string[] array = matrixString.Split(null);
			return new Matrix4x4
			{
				m00 = float.Parse(array[0]),
				m01 = float.Parse(array[1]),
				m02 = float.Parse(array[2]),
				m03 = float.Parse(array[3]),
				m10 = float.Parse(array[4]),
				m11 = float.Parse(array[5]),
				m12 = float.Parse(array[6]),
				m13 = float.Parse(array[7]),
				m20 = float.Parse(array[8]),
				m21 = float.Parse(array[9]),
				m22 = float.Parse(array[10]),
				m23 = float.Parse(array[11]),
				m30 = float.Parse(array[12]),
				m31 = float.Parse(array[13]),
				m32 = float.Parse(array[14]),
				m33 = float.Parse(array[15])
			};
		}
		private static Color StringToColor(string colorString)
		{
			string[] array = colorString.Substring(5, colorString.Length - 6).Split(new char[]
			{
				','
			});
			return new Color(float.Parse(array[0]), float.Parse(array[1]), float.Parse(array[2]), float.Parse(array[3]));
		}
		private static Rect StringToRect(string rectString)
		{
			string[] array = rectString.Substring(1, rectString.Length - 2).Split(new char[]
			{
				','
			});
			return new Rect(float.Parse(array[0].Substring(2, array[0].Length - 2)), float.Parse(array[1].Substring(3, array[1].Length - 3)), float.Parse(array[2].Substring(7, array[2].Length - 7)), float.Parse(array[3].Substring(8, array[3].Length - 8)));
		}
		private static LayerMask ValueToLayerMask(int value)
		{
			return new LayerMask
			{
				value = value
			};
		}
		private static AnimationCurve ValueToAnimationCurve(Dictionary<string, object> value)
		{
			AnimationCurve animationCurve = new AnimationCurve();
			object obj;
			if (value.TryGetValue("Keys", out obj))
			{
				List<object> list = obj as List<object>;
				for (int i = 0; i < list.Count; i++)
				{
					List<object> list2 = list[i] as List<object>;
					animationCurve.AddKey(new Keyframe((float)Convert.ChangeType(list2[0], typeof(float)), (float)Convert.ChangeType(list2[1], typeof(float)), (float)Convert.ChangeType(list2[2], typeof(float)), (float)Convert.ChangeType(list2[3], typeof(float)))
					{
						tangentMode = (int)Convert.ChangeType(list2[4], typeof(int))
					});
				}
			}
			if (value.TryGetValue("PreWrapMode", out obj))
			{
				animationCurve.preWrapMode = (WrapMode)((int)Enum.Parse(typeof(WrapMode), (string)obj));
			}
			if (value.TryGetValue("PostWrapMode", out obj))
			{
				animationCurve.postWrapMode = (WrapMode)((int)Enum.Parse(typeof(WrapMode), (string)obj));
			}
			return animationCurve;
		}
		private static UnityEngine.Object IndexToUnityObject(int index, List<UnityEngine.Object> unityObjects)
		{
			if (index < 0 || index >= unityObjects.Count)
			{
				return null;
			}
			return unityObjects[index];
		}
	}
}
