using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
namespace BehaviorDesigner.Editor
{
	public class BinarySerialization
	{
		private static int fieldIndex;
		private static TaskSerializationData taskSerializationData;
		private static FieldSerializationData fieldSerializationData;
		public static void Save(BehaviorSource behaviorSource)
		{
			BinarySerialization.fieldIndex = 0;
			BinarySerialization.taskSerializationData = new TaskSerializationData();
			BinarySerialization.fieldSerializationData = BinarySerialization.taskSerializationData.fieldSerializationData;
			if (behaviorSource.Variables != null)
			{
				for (int i = 0; i < behaviorSource.Variables.Count; i++)
				{
					BinarySerialization.taskSerializationData.variableStartIndex.Add(BinarySerialization.fieldSerializationData.startIndex.Count);
					BinarySerialization.SaveSharedVariable(behaviorSource.Variables[i], string.Empty);
				}
			}
			if (!object.ReferenceEquals(behaviorSource.EntryTask, null))
			{
				BinarySerialization.SaveTask(behaviorSource.EntryTask, -1);
			}
			if (!object.ReferenceEquals(behaviorSource.RootTask, null))
			{
				BinarySerialization.SaveTask(behaviorSource.RootTask, 0);
			}
			if (behaviorSource.DetachedTasks != null)
			{
				for (int j = 0; j < behaviorSource.DetachedTasks.Count; j++)
				{
					BinarySerialization.SaveTask(behaviorSource.DetachedTasks[j], -1);
				}
			}
			behaviorSource.TaskData = BinarySerialization.taskSerializationData;
			if (behaviorSource.Owner != null)
			{
				EditorUtility.SetDirty(behaviorSource.Owner.GetObject());
			}
		}
		public static void Save(GlobalVariables globalVariables)
		{
			if (globalVariables == null)
			{
				return;
			}
			BinarySerialization.fieldIndex = 0;
			globalVariables.VariableData = new VariableSerializationData();
			if (globalVariables.Variables == null || globalVariables.Variables.Count == 0)
			{
				return;
			}
			BinarySerialization.fieldSerializationData = globalVariables.VariableData.fieldSerializationData;
			for (int i = 0; i < globalVariables.Variables.Count; i++)
			{
				globalVariables.VariableData.variableStartIndex.Add(BinarySerialization.fieldSerializationData.startIndex.Count);
				BinarySerialization.SaveSharedVariable(globalVariables.Variables[i], string.Empty);
			}
			EditorUtility.SetDirty(globalVariables);
		}
		private static void SaveTask(Task task, int parentTaskIndex)
		{
			BinarySerialization.taskSerializationData.types.Add(task.GetType().ToString());
			BinarySerialization.taskSerializationData.parentIndex.Add(parentTaskIndex);
			BinarySerialization.taskSerializationData.startIndex.Add(BinarySerialization.fieldSerializationData.startIndex.Count);
			BinarySerialization.SaveField(typeof(int), "ID", task.ID, null);
			BinarySerialization.SaveField(typeof(string), "FriendlyName", task.FriendlyName, null);
			BinarySerialization.SaveField(typeof(bool), "IsInstant", task.IsInstant, null);
			BinarySerialization.SaveNodeData(task.NodeData);
			BinarySerialization.SaveFields(task, string.Empty);
			if (task is ParentTask)
			{
				ParentTask parentTask = task as ParentTask;
				if (parentTask.Children != null && parentTask.Children.Count > 0)
				{
					for (int i = 0; i < parentTask.Children.Count; i++)
					{
						BinarySerialization.SaveTask(parentTask.Children[i], parentTask.ID);
					}
				}
			}
		}
		private static void SaveNodeData(NodeData nodeData)
		{
			BinarySerialization.SaveField(typeof(Vector2), "NodeDataOffset", nodeData.Offset, null);
			BinarySerialization.SaveField(typeof(string), "NodeDataComment", nodeData.Comment, null);
			BinarySerialization.SaveField(typeof(bool), "NodeDataIsBreakpoint", nodeData.IsBreakpoint, null);
			BinarySerialization.SaveField(typeof(bool), "NodeDataDisabled", nodeData.Disabled, null);
			BinarySerialization.SaveField(typeof(bool), "NodeDataCollapsed", nodeData.Collapsed, null);
			BinarySerialization.SaveField(typeof(int), "NodeDataColorIndex", nodeData.ColorIndex, null);
			BinarySerialization.SaveField(typeof(List<string>), "NodeDataWatchedFields", nodeData.WatchedFieldNames, null);
		}
		private static void SaveSharedVariable(SharedVariable sharedVariable, string namePrefix)
		{
			if (sharedVariable == null)
			{
				return;
			}
			BinarySerialization.SaveField(typeof(string), namePrefix + "Type", sharedVariable.GetType().ToString(), null);
			BinarySerialization.SaveField(typeof(string), namePrefix + "Name", sharedVariable.Name, null);
			if (sharedVariable.IsShared)
			{
				BinarySerialization.SaveField(typeof(bool), namePrefix + "IsShared", sharedVariable.IsShared, null);
			}
			if (sharedVariable.IsGlobal)
			{
				BinarySerialization.SaveField(typeof(bool), namePrefix + "IsGlobal", sharedVariable.IsGlobal, null);
			}
			if (sharedVariable.NetworkSync)
			{
				BinarySerialization.SaveField(typeof(bool), namePrefix + "NetworkSync", sharedVariable.NetworkSync, null);
			}
			if (!string.IsNullOrEmpty(sharedVariable.PropertyMapping))
			{
				BinarySerialization.SaveField(typeof(string), namePrefix + "PropertyMapping", sharedVariable.PropertyMapping, null);
				if (!object.Equals(sharedVariable.PropertyMappingOwner, null))
				{
					BinarySerialization.SaveField(typeof(GameObject), namePrefix + "PropertyMappingOwner", sharedVariable.PropertyMappingOwner, null);
				}
			}
			BinarySerialization.SaveFields(sharedVariable, namePrefix);
		}
		private static void SaveFields(object obj, string namePrefix)
		{
			FieldInfo[] allFields = TaskUtility.GetAllFields(obj.GetType());
			for (int i = 0; i < allFields.Length; i++)
			{
				if (!BehaviorDesignerUtility.HasAttribute(allFields[i], typeof(NonSerializedAttribute)) && ((!allFields[i].IsPrivate && !allFields[i].IsFamily) || BehaviorDesignerUtility.HasAttribute(allFields[i], typeof(SerializeField))) && (!(obj is ParentTask) || !allFields[i].Name.Equals("children")))
				{
					object value = allFields[i].GetValue(obj);
					if (!object.ReferenceEquals(value, null))
					{
						BinarySerialization.SaveField(allFields[i].FieldType, namePrefix + allFields[i].Name, value, allFields[i]);
					}
				}
			}
		}
		private static void SaveField(Type fieldType, string fieldName, object value, FieldInfo fieldInfo = null)
		{
			string text = fieldType.Name + fieldName;
			BinarySerialization.fieldSerializationData.typeName.Add(text);
			BinarySerialization.fieldSerializationData.startIndex.Add(BinarySerialization.fieldIndex);
			if (typeof(IList).IsAssignableFrom(fieldType))
			{
				Type fieldType2;
				if (fieldType.IsArray)
				{
					fieldType2 = fieldType.GetElementType();
				}
				else
				{
					Type type = fieldType;
					while (!type.IsGenericType)
					{
						type = type.BaseType;
					}
					fieldType2 = type.GetGenericArguments()[0];
				}
				IList list = value as IList;
				if (list == null)
				{
					BinarySerialization.AddByteData(typeof(int), BinarySerialization.IntToBytes(0));
				}
				else
				{
					BinarySerialization.AddByteData(typeof(int), BinarySerialization.IntToBytes(list.Count));
					if (list.Count > 0)
					{
						for (int i = 0; i < list.Count; i++)
						{
							if (object.ReferenceEquals(list[i], null))
							{
								BinarySerialization.AddByteData(fieldType2, BinarySerialization.IntToBytes(-1));
							}
							else
							{
								BinarySerialization.SaveField(fieldType2, text + i, list[i], fieldInfo);
							}
						}
					}
				}
			}
			else if (typeof(Task).IsAssignableFrom(fieldType))
			{
				if (fieldInfo != null && BehaviorDesignerUtility.HasAttribute(fieldInfo, typeof(InspectTaskAttribute)))
				{
					BinarySerialization.AddByteData(fieldType, BinarySerialization.StringToBytes(value.GetType().ToString()));
					BinarySerialization.SaveFields(value, text);
				}
				else
				{
					BinarySerialization.AddByteData(fieldType, BinarySerialization.IntToBytes((value as Task).ID));
				}
			}
			else if (typeof(SharedVariable).IsAssignableFrom(fieldType))
			{
				BinarySerialization.SaveSharedVariable(value as SharedVariable, text);
			}
			else if (typeof(UnityEngine.Object).IsAssignableFrom(fieldType))
			{
				BinarySerialization.AddByteData(fieldType, BinarySerialization.IntToBytes(BinarySerialization.fieldSerializationData.unityObjects.Count));
				BinarySerialization.fieldSerializationData.unityObjects.Add(value as UnityEngine.Object);
			}
			else if (fieldType.Equals(typeof(int)) || fieldType.IsEnum)
			{
				BinarySerialization.AddByteData(fieldType, BinarySerialization.IntToBytes((int)value));
			}
			else if (fieldType.Equals(typeof(short)))
			{
				BinarySerialization.AddByteData(fieldType, BinarySerialization.Int16ToBytes((short)value));
			}
			else if (fieldType.Equals(typeof(uint)))
			{
				BinarySerialization.AddByteData(fieldType, BinarySerialization.UIntToBytes((uint)value));
			}
			else if (fieldType.Equals(typeof(float)))
			{
				BinarySerialization.AddByteData(fieldType, BinarySerialization.FloatToBytes((float)value));
			}
			else if (fieldType.Equals(typeof(double)))
			{
				BinarySerialization.AddByteData(fieldType, BinarySerialization.DoubleToBytes((double)value));
			}
			else if (fieldType.Equals(typeof(long)))
			{
				BinarySerialization.AddByteData(fieldType, BinarySerialization.LongToBytes((long)value));
			}
			else if (fieldType.Equals(typeof(bool)))
			{
				BinarySerialization.AddByteData(fieldType, BinarySerialization.BoolToBytes((bool)value));
			}
			else if (fieldType.Equals(typeof(string)))
			{
				BinarySerialization.AddByteData(fieldType, BinarySerialization.StringToBytes((string)value));
			}
			else if (fieldType.Equals(typeof(byte)))
			{
				BinarySerialization.AddByteData(fieldType, BinarySerialization.ByteToBytes((byte)value));
			}
			else if (fieldType.Equals(typeof(Vector2)))
			{
				BinarySerialization.AddByteData(fieldType, BinarySerialization.Vector2ToBytes((Vector2)value));
			}
			else if (fieldType.Equals(typeof(Vector3)))
			{
				BinarySerialization.AddByteData(fieldType, BinarySerialization.Vector3ToBytes((Vector3)value));
			}
			else if (fieldType.Equals(typeof(Vector4)))
			{
				BinarySerialization.AddByteData(fieldType, BinarySerialization.Vector4ToBytes((Vector4)value));
			}
			else if (fieldType.Equals(typeof(Quaternion)))
			{
				BinarySerialization.AddByteData(fieldType, BinarySerialization.QuaternionToBytes((Quaternion)value));
			}
			else if (fieldType.Equals(typeof(Color)))
			{
				BinarySerialization.AddByteData(fieldType, BinarySerialization.ColorToBytes((Color)value));
			}
			else if (fieldType.Equals(typeof(Rect)))
			{
				BinarySerialization.AddByteData(fieldType, BinarySerialization.RectToBytes((Rect)value));
			}
			else if (fieldType.Equals(typeof(Matrix4x4)))
			{
				BinarySerialization.AddByteData(fieldType, BinarySerialization.Matrix4x4ToBytes((Matrix4x4)value));
			}
			else if (fieldType.Equals(typeof(LayerMask)))
			{
				BinarySerialization.AddByteData(fieldType, BinarySerialization.IntToBytes(((LayerMask)value).value));
			}
			else if (fieldType.Equals(typeof(AnimationCurve)))
			{
				BinarySerialization.AddByteData(fieldType, BinarySerialization.AnimationCurveToBytes((AnimationCurve)value));
			}
			else if (fieldType.IsClass || (fieldType.IsValueType && !fieldType.IsPrimitive))
			{
				if (object.ReferenceEquals(value, null))
				{
					value = Activator.CreateInstance(fieldType, true);
				}
				BinarySerialization.SaveFields(value, text);
			}
			else
			{
				Debug.LogError("Missing Serialization for " + fieldType);
			}
		}
		private static byte[] IntToBytes(int value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(bytes);
			}
			return bytes;
		}
		private static byte[] Int16ToBytes(short value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(bytes);
			}
			return bytes;
		}
		private static byte[] UIntToBytes(uint value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(bytes);
			}
			return bytes;
		}
		private static byte[] FloatToBytes(float value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(bytes);
			}
			return bytes;
		}
		private static byte[] DoubleToBytes(double value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(bytes);
			}
			return bytes;
		}
		private static byte[] LongToBytes(long value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(bytes);
			}
			return bytes;
		}
		private static byte[] BoolToBytes(bool value)
		{
			return BitConverter.GetBytes(value);
		}
		private static byte[] StringToBytes(string str)
		{
			if (str == null)
			{
				str = string.Empty;
			}
			return Encoding.UTF8.GetBytes(str);
		}
		private static byte[] ByteToBytes(byte value)
		{
			return new byte[]
			{
				value
			};
		}
		private static ICollection<byte> ColorToBytes(Color color)
		{
			List<byte> list = new List<byte>();
			list.AddRange(BitConverter.GetBytes(color.r));
			list.AddRange(BitConverter.GetBytes(color.g));
			list.AddRange(BitConverter.GetBytes(color.b));
			list.AddRange(BitConverter.GetBytes(color.a));
			return list;
		}
		private static ICollection<byte> Vector2ToBytes(Vector2 vector2)
		{
			List<byte> list = new List<byte>();
			list.AddRange(BitConverter.GetBytes(vector2.x));
			list.AddRange(BitConverter.GetBytes(vector2.y));
			return list;
		}
		private static ICollection<byte> Vector3ToBytes(Vector3 vector3)
		{
			List<byte> list = new List<byte>();
			list.AddRange(BitConverter.GetBytes(vector3.x));
			list.AddRange(BitConverter.GetBytes(vector3.y));
			list.AddRange(BitConverter.GetBytes(vector3.z));
			return list;
		}
		private static ICollection<byte> Vector4ToBytes(Vector4 vector4)
		{
			List<byte> list = new List<byte>();
			list.AddRange(BitConverter.GetBytes(vector4.x));
			list.AddRange(BitConverter.GetBytes(vector4.y));
			list.AddRange(BitConverter.GetBytes(vector4.z));
			list.AddRange(BitConverter.GetBytes(vector4.w));
			return list;
		}
		private static ICollection<byte> QuaternionToBytes(Quaternion quaternion)
		{
			List<byte> list = new List<byte>();
			list.AddRange(BitConverter.GetBytes(quaternion.x));
			list.AddRange(BitConverter.GetBytes(quaternion.y));
			list.AddRange(BitConverter.GetBytes(quaternion.z));
			list.AddRange(BitConverter.GetBytes(quaternion.w));
			return list;
		}
		private static ICollection<byte> RectToBytes(Rect rect)
		{
			List<byte> list = new List<byte>();
			list.AddRange(BitConverter.GetBytes(rect.x));
			list.AddRange(BitConverter.GetBytes(rect.y));
			list.AddRange(BitConverter.GetBytes(rect.width));
			list.AddRange(BitConverter.GetBytes(rect.height));
			return list;
		}
		private static ICollection<byte> Matrix4x4ToBytes(Matrix4x4 matrix4x4)
		{
			List<byte> list = new List<byte>();
			list.AddRange(BitConverter.GetBytes(matrix4x4.m00));
			list.AddRange(BitConverter.GetBytes(matrix4x4.m01));
			list.AddRange(BitConverter.GetBytes(matrix4x4.m02));
			list.AddRange(BitConverter.GetBytes(matrix4x4.m03));
			list.AddRange(BitConverter.GetBytes(matrix4x4.m10));
			list.AddRange(BitConverter.GetBytes(matrix4x4.m11));
			list.AddRange(BitConverter.GetBytes(matrix4x4.m12));
			list.AddRange(BitConverter.GetBytes(matrix4x4.m13));
			list.AddRange(BitConverter.GetBytes(matrix4x4.m20));
			list.AddRange(BitConverter.GetBytes(matrix4x4.m21));
			list.AddRange(BitConverter.GetBytes(matrix4x4.m22));
			list.AddRange(BitConverter.GetBytes(matrix4x4.m23));
			list.AddRange(BitConverter.GetBytes(matrix4x4.m30));
			list.AddRange(BitConverter.GetBytes(matrix4x4.m31));
			list.AddRange(BitConverter.GetBytes(matrix4x4.m32));
			list.AddRange(BitConverter.GetBytes(matrix4x4.m33));
			return list;
		}
		private static ICollection<byte> AnimationCurveToBytes(AnimationCurve animationCurve)
		{
			List<byte> list = new List<byte>();
			Keyframe[] keys = animationCurve.keys;
			if (keys != null)
			{
				list.AddRange(BitConverter.GetBytes(keys.Length));
				for (int i = 0; i < keys.Length; i++)
				{
					list.AddRange(BitConverter.GetBytes(keys[i].time));
					list.AddRange(BitConverter.GetBytes(keys[i].value));
					list.AddRange(BitConverter.GetBytes(keys[i].inTangent));
					list.AddRange(BitConverter.GetBytes(keys[i].outTangent));
					list.AddRange(BitConverter.GetBytes(keys[i].tangentMode));
				}
			}
			else
			{
				list.AddRange(BitConverter.GetBytes(0));
			}
			list.AddRange(BitConverter.GetBytes((int)animationCurve.preWrapMode));
			list.AddRange(BitConverter.GetBytes((int)animationCurve.postWrapMode));
			return list;
		}
		private static void AddByteData(Type fieldType, ICollection<byte> bytes)
		{
			BinarySerialization.fieldSerializationData.dataPosition.Add(BinarySerialization.fieldSerializationData.byteData.Count);
			if (bytes != null)
			{
				BinarySerialization.fieldSerializationData.byteData.AddRange(bytes);
			}
			BinarySerialization.fieldIndex++;
		}
	}
}
