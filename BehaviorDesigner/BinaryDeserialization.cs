using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
public static class BinaryDeserialization
{
	private class ObjectFieldMap
	{
		public object obj;
		public FieldInfo fieldInfo;
		public ObjectFieldMap(object o, FieldInfo f)
		{
			this.obj = o;
			this.fieldInfo = f;
		}
	}
	private class ObjectFieldMapComparer : IEqualityComparer<BinaryDeserialization.ObjectFieldMap>
	{
		public bool Equals(BinaryDeserialization.ObjectFieldMap a, BinaryDeserialization.ObjectFieldMap b)
		{
			return !object.ReferenceEquals(a, null) && !object.ReferenceEquals(b, null) && a.obj.Equals(b.obj) && a.fieldInfo.Equals(b.fieldInfo);
		}
		public int GetHashCode(BinaryDeserialization.ObjectFieldMap a)
		{
			return (a == null) ? 0 : (a.obj.ToString().GetHashCode() + a.fieldInfo.ToString().GetHashCode());
		}
	}
	private static GlobalVariables globalVariables;
	private static Dictionary<BinaryDeserialization.ObjectFieldMap, List<int>> taskIDs;
	public static void Load(BehaviorSource behaviorSource)
	{
		BinaryDeserialization.Load(behaviorSource.TaskData, behaviorSource);
	}
	public static void Load(TaskSerializationData taskData, BehaviorSource behaviorSource)
	{
		behaviorSource.EntryTask = null;
		behaviorSource.RootTask = null;
		behaviorSource.DetachedTasks = null;
		behaviorSource.Variables = null;
		FieldSerializationData fieldSerializationData;
		if (taskData == null || (fieldSerializationData = taskData.fieldSerializationData).byteData == null || fieldSerializationData.byteData.Count == 0)
		{
			return;
		}
		fieldSerializationData.byteDataArray = fieldSerializationData.byteData.ToArray();
		BinaryDeserialization.taskIDs = null;
		if (taskData.variableStartIndex != null)
		{
			List<SharedVariable> list = new List<SharedVariable>();
			Dictionary<string, int> dictionary = ObjectPool.Get<Dictionary<string, int>>();
			for (int i = 0; i < taskData.variableStartIndex.Count; i++)
			{
				int num = taskData.variableStartIndex[i];
				int num2;
				if (i + 1 < taskData.variableStartIndex.Count)
				{
					num2 = taskData.variableStartIndex[i + 1];
				}
				else
				{
					if (taskData.startIndex != null && taskData.startIndex.Count > 0)
					{
						num2 = taskData.startIndex[0];
					}
					else
					{
						num2 = fieldSerializationData.startIndex.Count;
					}
				}
				dictionary.Clear();
				for (int j = num; j < num2; j++)
				{
					dictionary.Add(fieldSerializationData.typeName[j], fieldSerializationData.startIndex[j]);
				}
				SharedVariable sharedVariable = BinaryDeserialization.BytesToSharedVariable(fieldSerializationData, dictionary, fieldSerializationData.byteDataArray, taskData.variableStartIndex[i], behaviorSource, false, string.Empty);
				if (sharedVariable != null)
				{
					list.Add(sharedVariable);
				}
			}
			ObjectPool.Return<Dictionary<string, int>>(dictionary);
			behaviorSource.Variables = list;
		}
		List<Task> list2 = new List<Task>();
		if (taskData.types != null)
		{
			for (int k = 0; k < taskData.types.Count; k++)
			{
				BinaryDeserialization.LoadTask(taskData, fieldSerializationData, ref list2, ref behaviorSource);
			}
		}
		if (taskData.parentIndex.Count != list2.Count)
		{
			Debug.LogError("Deserialization Error: parent index count does not match task list count");
			return;
		}
		for (int l = 0; l < taskData.parentIndex.Count; l++)
		{
			if (taskData.parentIndex[l] == -1)
			{
				if (behaviorSource.EntryTask == null)
				{
					behaviorSource.EntryTask = list2[l];
				}
				else
				{
					if (behaviorSource.DetachedTasks == null)
					{
						behaviorSource.DetachedTasks = new List<Task>();
					}
					behaviorSource.DetachedTasks.Add(list2[l]);
				}
			}
			else
			{
				if (taskData.parentIndex[l] == 0)
				{
					behaviorSource.RootTask = list2[l];
				}
				else
				{
					if (taskData.parentIndex[l] != -1)
					{
						ParentTask parentTask = list2[taskData.parentIndex[l]] as ParentTask;
						if (parentTask != null)
						{
							int index = (parentTask.Children != null) ? parentTask.Children.Count : 0;
							parentTask.AddChild(list2[l], index);
						}
					}
				}
			}
		}
		if (BinaryDeserialization.taskIDs != null)
		{
			foreach (BinaryDeserialization.ObjectFieldMap current in BinaryDeserialization.taskIDs.Keys)
			{
				List<int> list3 = BinaryDeserialization.taskIDs[current];
				Type fieldType = current.fieldInfo.FieldType;
				if (typeof(IList).IsAssignableFrom(fieldType))
				{
					if (fieldType.IsArray)
					{
						Type elementType = fieldType.GetElementType();
						Array array = Array.CreateInstance(elementType, list3.Count);
						for (int m = 0; m < array.Length; m++)
						{
							array.SetValue(list2[list3[m]], m);
						}
						current.fieldInfo.SetValue(current.obj, array);
					}
					else
					{
						Type type = fieldType.GetGenericArguments()[0];
						IList list4 = TaskUtility.CreateInstance(typeof(List<>).MakeGenericType(new Type[]
						{
							type
						})) as IList;
						for (int n = 0; n < list3.Count; n++)
						{
							list4.Add(list2[list3[n]]);
						}
						current.fieldInfo.SetValue(current.obj, list4);
					}
				}
				else
				{
					current.fieldInfo.SetValue(current.obj, list2[list3[0]]);
				}
			}
		}
	}
	public static void Load(GlobalVariables globalVariables)
	{
		if (globalVariables == null)
		{
			return;
		}
		globalVariables.Variables = null;
		FieldSerializationData fieldSerializationData;
		if (globalVariables.VariableData == null || (fieldSerializationData = globalVariables.VariableData.fieldSerializationData).byteData == null || fieldSerializationData.byteData.Count == 0)
		{
			return;
		}
		VariableSerializationData variableData = globalVariables.VariableData;
		fieldSerializationData.byteDataArray = fieldSerializationData.byteData.ToArray();
		if (variableData.variableStartIndex != null)
		{
			List<SharedVariable> list = new List<SharedVariable>();
			Dictionary<string, int> dictionary = ObjectPool.Get<Dictionary<string, int>>();
			for (int i = 0; i < variableData.variableStartIndex.Count; i++)
			{
				int num = variableData.variableStartIndex[i];
				int num2;
				if (i + 1 < variableData.variableStartIndex.Count)
				{
					num2 = variableData.variableStartIndex[i + 1];
				}
				else
				{
					num2 = fieldSerializationData.startIndex.Count;
				}
				dictionary.Clear();
				for (int j = num; j < num2; j++)
				{
					dictionary.Add(fieldSerializationData.typeName[j], fieldSerializationData.startIndex[j]);
				}
				SharedVariable sharedVariable = BinaryDeserialization.BytesToSharedVariable(fieldSerializationData, dictionary, fieldSerializationData.byteDataArray, variableData.variableStartIndex[i], globalVariables, false, string.Empty);
				if (sharedVariable != null)
				{
					list.Add(sharedVariable);
				}
			}
			ObjectPool.Return<Dictionary<string, int>>(dictionary);
			globalVariables.Variables = list;
		}
	}
	private static void LoadTask(TaskSerializationData taskSerializationData, FieldSerializationData fieldSerializationData, ref List<Task> taskList, ref BehaviorSource behaviorSource)
	{
		int count = taskList.Count;
		Type type = TaskUtility.GetTypeWithinAssembly(taskSerializationData.types[count]);
		if (type == null)
		{
			bool flag = false;
			for (int i = 0; i < taskSerializationData.parentIndex.Count; i++)
			{
				if (count == taskSerializationData.parentIndex[i])
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				type = typeof(UnknownParentTask);
			}
			else
			{
				type = typeof(UnknownTask);
			}
		}
		Task task = TaskUtility.CreateInstance(type) as Task;
		task.Owner = (behaviorSource.Owner.GetObject() as Behavior);
		taskList.Add(task);
		int num = taskSerializationData.startIndex[count];
		int num2;
		if (count + 1 < taskSerializationData.startIndex.Count)
		{
			num2 = taskSerializationData.startIndex[count + 1];
		}
		else
		{
			num2 = fieldSerializationData.startIndex.Count;
		}
		Dictionary<string, int> dictionary = ObjectPool.Get<Dictionary<string, int>>();
		dictionary.Clear();
		for (int j = num; j < num2; j++)
		{
			dictionary.Add(fieldSerializationData.typeName[j], fieldSerializationData.startIndex[j]);
		}
		task.ID = (int)BinaryDeserialization.LoadField(fieldSerializationData, dictionary, typeof(int), "ID", null, null, null);
		task.FriendlyName = (string)BinaryDeserialization.LoadField(fieldSerializationData, dictionary, typeof(string), "FriendlyName", null, null, null);
		task.IsInstant = (bool)BinaryDeserialization.LoadField(fieldSerializationData, dictionary, typeof(bool), "IsInstant", null, null, null);
		BinaryDeserialization.LoadNodeData(fieldSerializationData, dictionary, taskList[count]);
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
		BinaryDeserialization.LoadFields(fieldSerializationData, dictionary, taskList[count], string.Empty, behaviorSource);
		ObjectPool.Return<Dictionary<string, int>>(dictionary);
	}
	private static void LoadNodeData(FieldSerializationData fieldSerializationData, Dictionary<string, int> fieldIndexMap, Task task)
	{
		NodeData nodeData = new NodeData();
		nodeData.Offset = (Vector2)BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof(Vector2), "NodeDataOffset", null, null, null);
		nodeData.Comment = (string)BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof(string), "NodeDataComment", null, null, null);
		nodeData.IsBreakpoint = (bool)BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof(bool), "NodeDataIsBreakpoint", null, null, null);
		nodeData.Disabled = (bool)BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof(bool), "NodeDataDisabled", null, null, null);
		nodeData.Collapsed = (bool)BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof(bool), "NodeDataCollapsed", null, null, null);
		object obj = BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof(int), "NodeDataColorIndex", null, null, null);
		if (obj != null)
		{
			nodeData.ColorIndex = (int)obj;
		}
		obj = BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof(List<string>), "NodeDataWatchedFields", null, null, null);
		if (obj != null)
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
		task.NodeData = nodeData;
	}
	private static void LoadFields(FieldSerializationData fieldSerializationData, Dictionary<string, int> fieldIndexMap, object obj, string namePrefix, IVariableSource variableSource)
	{
		FieldInfo[] allFields = TaskUtility.GetAllFields(obj.GetType());
		for (int i = 0; i < allFields.Length; i++)
		{
			if (!TaskUtility.HasAttribute(allFields[i], typeof(NonSerializedAttribute)) && ((!allFields[i].IsPrivate && !allFields[i].IsFamily) || TaskUtility.HasAttribute(allFields[i], typeof(SerializeField))) && (!(obj is ParentTask) || !allFields[i].Name.Equals("children")))
			{
				object obj2 = BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, allFields[i].FieldType, namePrefix + allFields[i].Name, variableSource, obj, allFields[i]);
				if (obj2 != null && !object.ReferenceEquals(obj2, null) && !obj2.Equals(null))
				{
					allFields[i].SetValue(obj, obj2);
				}
			}
		}
	}
	private static object LoadField(FieldSerializationData fieldSerializationData, Dictionary<string, int> fieldIndexMap, Type fieldType, string fieldName, IVariableSource variableSource, object obj = null, FieldInfo fieldInfo = null)
	{
		string text = fieldType.Name + fieldName;
		int num;
		if (fieldIndexMap.TryGetValue(text, out num))
		{
			object obj2 = null;
			if (typeof(IList).IsAssignableFrom(fieldType))
			{
				int num2 = BinaryDeserialization.BytesToInt(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[num]);
				if (fieldType.IsArray)
				{
					Type elementType = fieldType.GetElementType();
					if (elementType == null)
					{
						return null;
					}
					Array array = Array.CreateInstance(elementType, num2);
					for (int i = 0; i < num2; i++)
					{
						object obj3 = BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, elementType, text + i, variableSource, obj, fieldInfo);
						array.SetValue((!object.ReferenceEquals(obj3, null) && !obj3.Equals(null)) ? obj3 : null, i);
					}
					obj2 = array;
				}
				else
				{
					Type type = fieldType;
					while (!type.IsGenericType)
					{
						type = type.BaseType;
					}
					Type type2 = type.GetGenericArguments()[0];
					IList list;
					if (fieldType.IsGenericType)
					{
						list = (TaskUtility.CreateInstance(typeof(List<>).MakeGenericType(new Type[]
						{
							type2
						})) as IList);
					}
					else
					{
						list = (TaskUtility.CreateInstance(fieldType) as IList);
					}
					for (int j = 0; j < num2; j++)
					{
						object obj4 = BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, type2, text + j, variableSource, obj, fieldInfo);
						list.Add((!object.ReferenceEquals(obj4, null) && !obj4.Equals(null)) ? obj4 : null);
					}
					obj2 = list;
				}
			}
			else
			{
				if (typeof(Task).IsAssignableFrom(fieldType))
				{
					if (fieldInfo != null && TaskUtility.HasAttribute(fieldInfo, typeof(InspectTaskAttribute)))
					{
						string text2 = BinaryDeserialization.BytesToString(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[num], BinaryDeserialization.GetFieldSize(fieldSerializationData, num));
						if (!string.IsNullOrEmpty(text2))
						{
							Type typeWithinAssembly = TaskUtility.GetTypeWithinAssembly(text2);
							if (typeWithinAssembly != null)
							{
								obj2 = TaskUtility.CreateInstance(typeWithinAssembly);
								BinaryDeserialization.LoadFields(fieldSerializationData, fieldIndexMap, obj2, text, variableSource);
							}
						}
					}
					else
					{
						if (BinaryDeserialization.taskIDs == null)
						{
							BinaryDeserialization.taskIDs = new Dictionary<BinaryDeserialization.ObjectFieldMap, List<int>>(new BinaryDeserialization.ObjectFieldMapComparer());
						}
						int item = BinaryDeserialization.BytesToInt(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[num]);
						BinaryDeserialization.ObjectFieldMap key = new BinaryDeserialization.ObjectFieldMap(obj, fieldInfo);
						if (BinaryDeserialization.taskIDs.ContainsKey(key))
						{
							BinaryDeserialization.taskIDs[key].Add(item);
						}
						else
						{
							List<int> list2 = new List<int>();
							list2.Add(item);
							BinaryDeserialization.taskIDs.Add(key, list2);
						}
					}
				}
				else
				{
					if (typeof(SharedVariable).IsAssignableFrom(fieldType))
					{
						obj2 = BinaryDeserialization.BytesToSharedVariable(fieldSerializationData, fieldIndexMap, fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[num], variableSource, true, text);
					}
					else
					{
						if (typeof(UnityEngine.Object).IsAssignableFrom(fieldType))
						{
							int index = BinaryDeserialization.BytesToInt(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[num]);
							obj2 = BinaryDeserialization.IndexToUnityObject(index, fieldSerializationData);
						}
						else
						{
							if (fieldType.Equals(typeof(int)) || fieldType.IsEnum)
							{
								obj2 = BinaryDeserialization.BytesToInt(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[num]);
							}
							else
							{
								if (fieldType.Equals(typeof(uint)))
								{
									obj2 = BinaryDeserialization.BytesToUInt(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[num]);
								}
								else
								{
									if (fieldType.Equals(typeof(float)))
									{
										obj2 = BinaryDeserialization.BytesToFloat(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[num]);
									}
									else
									{
										if (fieldType.Equals(typeof(double)))
										{
											obj2 = BinaryDeserialization.BytesToDouble(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[num]);
										}
										else
										{
											if (fieldType.Equals(typeof(long)))
											{
												obj2 = BinaryDeserialization.BytesToLong(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[num]);
											}
											else
											{
												if (fieldType.Equals(typeof(bool)))
												{
													obj2 = BinaryDeserialization.BytesToBool(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[num]);
												}
												else
												{
													if (fieldType.Equals(typeof(string)))
													{
														obj2 = BinaryDeserialization.BytesToString(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[num], BinaryDeserialization.GetFieldSize(fieldSerializationData, num));
													}
													else
													{
														if (fieldType.Equals(typeof(byte)))
														{
															obj2 = BinaryDeserialization.BytesToByte(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[num]);
														}
														else
														{
															if (fieldType.Equals(typeof(Vector2)))
															{
																obj2 = BinaryDeserialization.BytesToVector2(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[num]);
															}
															else
															{
																if (fieldType.Equals(typeof(Vector3)))
																{
																	obj2 = BinaryDeserialization.BytesToVector3(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[num]);
																}
																else
																{
																	if (fieldType.Equals(typeof(Vector4)))
																	{
																		obj2 = BinaryDeserialization.BytesToVector4(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[num]);
																	}
																	else
																	{
																		if (fieldType.Equals(typeof(Quaternion)))
																		{
																			obj2 = BinaryDeserialization.BytesToQuaternion(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[num]);
																		}
																		else
																		{
																			if (fieldType.Equals(typeof(Color)))
																			{
																				obj2 = BinaryDeserialization.BytesToColor(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[num]);
																			}
																			else
																			{
																				if (fieldType.Equals(typeof(Rect)))
																				{
																					obj2 = BinaryDeserialization.BytesToRect(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[num]);
																				}
																				else
																				{
																					if (fieldType.Equals(typeof(Matrix4x4)))
																					{
																						obj2 = BinaryDeserialization.BytesToMatrix4x4(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[num]);
																					}
																					else
																					{
																						if (fieldType.Equals(typeof(AnimationCurve)))
																						{
																							obj2 = BinaryDeserialization.BytesToAnimationCurve(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[num]);
																						}
																						else
																						{
																							if (fieldType.Equals(typeof(LayerMask)))
																							{
																								obj2 = BinaryDeserialization.BytesToLayerMask(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[num]);
																							}
																							else
																							{
																								if (fieldType.IsClass || (fieldType.IsValueType && !fieldType.IsPrimitive))
																								{
																									obj2 = TaskUtility.CreateInstance(fieldType);
																									BinaryDeserialization.LoadFields(fieldSerializationData, fieldIndexMap, obj2, text, variableSource);
																									return obj2;
																								}
																							}
																						}
																					}
																				}
																			}
																		}
																	}
																}
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
			return obj2;
		}
		if (typeof(SharedVariable).IsAssignableFrom(fieldType))
		{
			Type type3 = TaskUtility.SharedVariableToConcreteType(fieldType);
			if (type3 == null)
			{
				return null;
			}
			text = type3.Name + fieldName;
			if (fieldIndexMap.ContainsKey(text))
			{
				SharedVariable sharedVariable = TaskUtility.CreateInstance(fieldType) as SharedVariable;
				sharedVariable.SetValue(BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, type3, fieldName, variableSource, null, null));
				return sharedVariable;
			}
		}
		if (typeof(SharedVariable).IsAssignableFrom(fieldType))
		{
			return TaskUtility.CreateInstance(fieldType);
		}
		return null;
	}
	private static int GetFieldSize(FieldSerializationData fieldSerializationData, int fieldIndex)
	{
		return ((fieldIndex + 1 >= fieldSerializationData.dataPosition.Count) ? fieldSerializationData.byteData.Count : fieldSerializationData.dataPosition[fieldIndex + 1]) - fieldSerializationData.dataPosition[fieldIndex];
	}
	private static int BytesToInt(byte[] bytes, int dataPosition)
	{
		if (BitConverter.IsLittleEndian)
		{
			Array.Reverse(bytes, dataPosition, 4);
		}
		return BitConverter.ToInt32(bytes, dataPosition);
	}
	private static uint BytesToUInt(byte[] bytes, int dataPosition)
	{
		if (BitConverter.IsLittleEndian)
		{
			Array.Reverse(bytes, dataPosition, 4);
		}
		return BitConverter.ToUInt32(bytes, dataPosition);
	}
	private static float BytesToFloat(byte[] bytes, int dataPosition)
	{
		if (BitConverter.IsLittleEndian)
		{
			Array.Reverse(bytes, dataPosition, 4);
		}
		return BitConverter.ToSingle(bytes, dataPosition);
	}
	private static double BytesToDouble(byte[] bytes, int dataPosition)
	{
		if (BitConverter.IsLittleEndian)
		{
			Array.Reverse(bytes, dataPosition, 8);
		}
		return BitConverter.ToDouble(bytes, dataPosition);
	}
	private static long BytesToLong(byte[] bytes, int dataPosition)
	{
		if (BitConverter.IsLittleEndian)
		{
			Array.Reverse(bytes, dataPosition, 8);
		}
		return BitConverter.ToInt64(bytes, dataPosition);
	}
	private static bool BytesToBool(byte[] bytes, int dataPosition)
	{
		return BitConverter.ToBoolean(bytes, dataPosition);
	}
	private static string BytesToString(byte[] bytes, int dataPosition, int dataSize)
	{
		if (dataSize == 0)
		{
			return string.Empty;
		}
		return Encoding.UTF8.GetString(bytes, dataPosition, dataSize);
	}
	private static byte BytesToByte(byte[] bytes, int dataPosition)
	{
		return bytes[dataPosition];
	}
	private static Color BytesToColor(byte[] bytes, int dataPosition)
	{
		Color black = Color.black;
		black.r = BitConverter.ToSingle(bytes, dataPosition);
		black.g = BitConverter.ToSingle(bytes, dataPosition + 4);
		black.b = BitConverter.ToSingle(bytes, dataPosition + 8);
		black.a = BitConverter.ToSingle(bytes, dataPosition + 12);
		return black;
	}
	private static Vector2 BytesToVector2(byte[] bytes, int dataPosition)
	{
		Vector2 zero = Vector2.zero;
		zero.x = BitConverter.ToSingle(bytes, dataPosition);
		zero.y = BitConverter.ToSingle(bytes, dataPosition + 4);
		return zero;
	}
	private static Vector3 BytesToVector3(byte[] bytes, int dataPosition)
	{
		Vector3 zero = Vector3.zero;
		zero.x = BitConverter.ToSingle(bytes, dataPosition);
		zero.y = BitConverter.ToSingle(bytes, dataPosition + 4);
		zero.z = BitConverter.ToSingle(bytes, dataPosition + 8);
		return zero;
	}
	private static Vector4 BytesToVector4(byte[] bytes, int dataPosition)
	{
		Vector4 zero = Vector4.zero;
		zero.x = BitConverter.ToSingle(bytes, dataPosition);
		zero.y = BitConverter.ToSingle(bytes, dataPosition + 4);
		zero.z = BitConverter.ToSingle(bytes, dataPosition + 8);
		zero.w = BitConverter.ToSingle(bytes, dataPosition + 12);
		return zero;
	}
	private static Quaternion BytesToQuaternion(byte[] bytes, int dataPosition)
	{
		Quaternion identity = Quaternion.identity;
		identity.x = BitConverter.ToSingle(bytes, dataPosition);
		identity.y = BitConverter.ToSingle(bytes, dataPosition + 4);
		identity.z = BitConverter.ToSingle(bytes, dataPosition + 8);
		identity.w = BitConverter.ToSingle(bytes, dataPosition + 12);
		return identity;
	}
	private static Rect BytesToRect(byte[] bytes, int dataPosition)
	{
		return new Rect
		{
			x = BitConverter.ToSingle(bytes, dataPosition),
			y = BitConverter.ToSingle(bytes, dataPosition + 4),
			width = BitConverter.ToSingle(bytes, dataPosition + 8),
			height = BitConverter.ToSingle(bytes, dataPosition + 12)
		};
	}
	private static Matrix4x4 BytesToMatrix4x4(byte[] bytes, int dataPosition)
	{
		Matrix4x4 identity = Matrix4x4.identity;
		identity.m00 = BitConverter.ToSingle(bytes, dataPosition);
		identity.m01 = BitConverter.ToSingle(bytes, dataPosition + 4);
		identity.m02 = BitConverter.ToSingle(bytes, dataPosition + 8);
		identity.m03 = BitConverter.ToSingle(bytes, dataPosition + 12);
		identity.m10 = BitConverter.ToSingle(bytes, dataPosition + 16);
		identity.m11 = BitConverter.ToSingle(bytes, dataPosition + 20);
		identity.m12 = BitConverter.ToSingle(bytes, dataPosition + 24);
		identity.m13 = BitConverter.ToSingle(bytes, dataPosition + 28);
		identity.m20 = BitConverter.ToSingle(bytes, dataPosition + 32);
		identity.m21 = BitConverter.ToSingle(bytes, dataPosition + 36);
		identity.m22 = BitConverter.ToSingle(bytes, dataPosition + 40);
		identity.m23 = BitConverter.ToSingle(bytes, dataPosition + 44);
		identity.m30 = BitConverter.ToSingle(bytes, dataPosition + 48);
		identity.m31 = BitConverter.ToSingle(bytes, dataPosition + 52);
		identity.m32 = BitConverter.ToSingle(bytes, dataPosition + 56);
		identity.m33 = BitConverter.ToSingle(bytes, dataPosition + 60);
		return identity;
	}
	private static AnimationCurve BytesToAnimationCurve(byte[] bytes, int dataPosition)
	{
		AnimationCurve animationCurve = new AnimationCurve();
		int num = BitConverter.ToInt32(bytes, dataPosition);
		for (int i = 0; i < num; i++)
		{
			animationCurve.AddKey(new Keyframe
			{
				time = BitConverter.ToSingle(bytes, dataPosition + 4),
				value = BitConverter.ToSingle(bytes, dataPosition + 8),
				inTangent = BitConverter.ToSingle(bytes, dataPosition + 12),
				outTangent = BitConverter.ToSingle(bytes, dataPosition + 16),
				tangentMode = BitConverter.ToInt32(bytes, dataPosition + 20)
			});
			dataPosition += 20;
		}
		animationCurve.preWrapMode = (WrapMode)BitConverter.ToInt32(bytes, dataPosition + 4);
		animationCurve.postWrapMode = (WrapMode)BitConverter.ToInt32(bytes, dataPosition + 8);
		return animationCurve;
	}
	private static LayerMask BytesToLayerMask(byte[] bytes, int dataPosition)
	{
		return new LayerMask
		{
			value = BinaryDeserialization.BytesToInt(bytes, dataPosition)
		};
	}
	private static UnityEngine.Object IndexToUnityObject(int index, FieldSerializationData activeFieldSerializationData)
	{
		if (index < 0 || index >= activeFieldSerializationData.unityObjects.Count)
		{
			return null;
		}
		return activeFieldSerializationData.unityObjects[index];
	}
	private static SharedVariable BytesToSharedVariable(FieldSerializationData fieldSerializationData, Dictionary<string, int> fieldIndexMap, byte[] bytes, int dataPosition, IVariableSource variableSource, bool fromField, string namePrefix)
	{
		SharedVariable sharedVariable = null;
		string text = (string)BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof(string), namePrefix + "Type", null, null, null);
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		string name = (string)BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof(string), namePrefix + "Name", null, null, null);
		bool flag = Convert.ToBoolean(BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof(bool), namePrefix + "IsShared", null, null, null));
		bool flag2 = Convert.ToBoolean(BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof(bool), namePrefix + "IsGlobal", null, null, null));
		if (flag && fromField)
		{
			if (!flag2)
			{
				sharedVariable = variableSource.GetVariable(name);
			}
			else
			{
				if (BinaryDeserialization.globalVariables == null)
				{
					BinaryDeserialization.globalVariables = GlobalVariables.Instance;
				}
				if (BinaryDeserialization.globalVariables != null)
				{
					sharedVariable = BinaryDeserialization.globalVariables.GetVariable(name);
				}
			}
		}
		Type typeWithinAssembly = TaskUtility.GetTypeWithinAssembly(text);
		if (typeWithinAssembly == null)
		{
			return null;
		}
		bool flag3 = true;
		if (sharedVariable == null || !(flag3 = sharedVariable.GetType().Equals(typeWithinAssembly)))
		{
			sharedVariable = (TaskUtility.CreateInstance(typeWithinAssembly) as SharedVariable);
			sharedVariable.Name = name;
			sharedVariable.IsShared = flag;
			sharedVariable.IsGlobal = flag2;
			sharedVariable.NetworkSync = Convert.ToBoolean(BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof(bool), namePrefix + "NetworkSync", null, null, null));
			if (!flag2)
			{
				sharedVariable.PropertyMapping = (string)BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof(string), namePrefix + "PropertyMapping", null, null, null);
				sharedVariable.PropertyMappingOwner = (GameObject)BinaryDeserialization.LoadField(fieldSerializationData, fieldIndexMap, typeof(GameObject), namePrefix + "PropertyMappingOwner", null, null, null);
				sharedVariable.InitializePropertyMapping(variableSource as BehaviorSource);
			}
			if (!flag3)
			{
				sharedVariable.IsShared = true;
			}
			BinaryDeserialization.LoadFields(fieldSerializationData, fieldIndexMap, sharedVariable, namePrefix, variableSource);
		}
		return sharedVariable;
	}
}
