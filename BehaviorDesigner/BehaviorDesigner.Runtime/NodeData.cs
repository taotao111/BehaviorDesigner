using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
namespace BehaviorDesigner.Runtime
{
	[Serializable]
	public class NodeData
	{
		[SerializeField]
		private object nodeDesigner;
		[SerializeField]
		private Vector2 offset;
		[SerializeField]
		private string friendlyName = string.Empty;
		[SerializeField]
		private string comment = string.Empty;
		[SerializeField]
		private bool isBreakpoint;
		[SerializeField]
		private Texture icon;
		[SerializeField]
		private bool collapsed;
		[SerializeField]
		private bool disabled;
		[SerializeField]
		private int colorIndex;
		[SerializeField]
		private List<string> watchedFieldNames;
		private List<FieldInfo> watchedFields;
		private float pushTime = -1f;
		private float popTime = -1f;
		private float interruptTime = -1f;
		private bool isReevaluating;
		private TaskStatus executionStatus;
		public object NodeDesigner
		{
			get
			{
				return this.nodeDesigner;
			}
			set
			{
				this.nodeDesigner = value;
			}
		}
		public Vector2 Offset
		{
			get
			{
				return this.offset;
			}
			set
			{
				this.offset = value;
			}
		}
		public string FriendlyName
		{
			get
			{
				return this.friendlyName;
			}
			set
			{
				this.friendlyName = value;
			}
		}
		public string Comment
		{
			get
			{
				return this.comment;
			}
			set
			{
				this.comment = value;
			}
		}
		public bool IsBreakpoint
		{
			get
			{
				return this.isBreakpoint;
			}
			set
			{
				this.isBreakpoint = value;
			}
		}
		public Texture Icon
		{
			get
			{
				return this.icon;
			}
			set
			{
				this.icon = value;
			}
		}
		public bool Collapsed
		{
			get
			{
				return this.collapsed;
			}
			set
			{
				this.collapsed = value;
			}
		}
		public bool Disabled
		{
			get
			{
				return this.disabled;
			}
			set
			{
				this.disabled = value;
			}
		}
		public int ColorIndex
		{
			get
			{
				return this.colorIndex;
			}
			set
			{
				this.colorIndex = value;
			}
		}
		public List<string> WatchedFieldNames
		{
			get
			{
				return this.watchedFieldNames;
			}
			set
			{
				this.watchedFieldNames = value;
			}
		}
		public List<FieldInfo> WatchedFields
		{
			get
			{
				return this.watchedFields;
			}
			set
			{
				this.watchedFields = value;
			}
		}
		public float PushTime
		{
			get
			{
				return this.pushTime;
			}
			set
			{
				this.pushTime = value;
			}
		}
		public float PopTime
		{
			get
			{
				return this.popTime;
			}
			set
			{
				this.popTime = value;
			}
		}
		public float InterruptTime
		{
			get
			{
				return this.interruptTime;
			}
			set
			{
				this.interruptTime = value;
			}
		}
		public bool IsReevaluating
		{
			get
			{
				return this.isReevaluating;
			}
			set
			{
				this.isReevaluating = value;
			}
		}
		public TaskStatus ExecutionStatus
		{
			get
			{
				return this.executionStatus;
			}
			set
			{
				this.executionStatus = value;
			}
		}
		public void InitWatchedFields(Task task)
		{
			if (this.watchedFieldNames != null && this.watchedFieldNames.Count > 0)
			{
				this.watchedFields = new List<FieldInfo>();
				for (int i = 0; i < this.watchedFieldNames.Count; i++)
				{
					FieldInfo field = task.GetType().GetField(this.watchedFieldNames[i], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (field != null)
					{
						this.watchedFields.Add(field);
					}
				}
			}
		}
		public void CopyFrom(NodeData nodeData, Task task)
		{
			this.nodeDesigner = nodeData.NodeDesigner;
			this.offset = nodeData.Offset;
			this.comment = nodeData.Comment;
			this.isBreakpoint = nodeData.IsBreakpoint;
			this.collapsed = nodeData.Collapsed;
			this.disabled = nodeData.Disabled;
			if (nodeData.WatchedFields != null && nodeData.WatchedFields.Count > 0)
			{
				this.watchedFields = new List<FieldInfo>();
				this.watchedFieldNames = new List<string>();
				for (int i = 0; i < nodeData.watchedFields.Count; i++)
				{
					FieldInfo field = task.GetType().GetField(nodeData.WatchedFields[i].Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (field != null)
					{
						this.watchedFields.Add(field);
						this.watchedFieldNames.Add(field.Name);
					}
				}
			}
		}
		public bool ContainsWatchedField(FieldInfo field)
		{
			return this.watchedFields != null && this.watchedFields.Contains(field);
		}
		public void AddWatchedField(FieldInfo field)
		{
			if (this.watchedFields == null)
			{
				this.watchedFields = new List<FieldInfo>();
				this.watchedFieldNames = new List<string>();
			}
			this.watchedFields.Add(field);
			this.watchedFieldNames.Add(field.Name);
		}
		public void RemoveWatchedField(FieldInfo field)
		{
			if (this.watchedFields != null)
			{
				this.watchedFields.Remove(field);
				this.watchedFieldNames.Remove(field.Name);
			}
		}
		private static Vector2 StringToVector2(string vector2String)
		{
			string[] array = vector2String.Substring(1, vector2String.Length - 2).Split(new char[]
			{
				','
			});
			return new Vector3(float.Parse(array[0]), float.Parse(array[1]));
		}
	}
}
