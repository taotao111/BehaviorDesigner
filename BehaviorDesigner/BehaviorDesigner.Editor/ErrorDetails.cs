using BehaviorDesigner.Runtime.Tasks;
using System;
using UnityEngine;
namespace BehaviorDesigner.Editor
{
	[Serializable]
	public class ErrorDetails
	{
		public enum ErrorType
		{
			RequiredField,
			SharedVariable,
			MissingChildren,
			UnknownTask
		}
		[SerializeField]
		private ErrorDetails.ErrorType type;
		[SerializeField]
		private NodeDesigner nodeDesigner;
		[SerializeField]
		private string taskFriendlyName;
		[SerializeField]
		private string taskType;
		[SerializeField]
		private string fieldName;
		public ErrorDetails.ErrorType Type
		{
			get
			{
				return this.type;
			}
		}
		public NodeDesigner NodeDesigner
		{
			get
			{
				return this.nodeDesigner;
			}
		}
		public string TaskFriendlyName
		{
			get
			{
				return this.taskFriendlyName;
			}
		}
		public string TaskType
		{
			get
			{
				return this.taskType;
			}
		}
		public string FieldName
		{
			get
			{
				return this.fieldName;
			}
		}
		public ErrorDetails(ErrorDetails.ErrorType type, Task task, string fieldName)
		{
			this.type = type;
			this.nodeDesigner = (task.NodeData.NodeDesigner as NodeDesigner);
			this.taskFriendlyName = task.FriendlyName;
			this.taskType = task.GetType().ToString();
			this.fieldName = fieldName;
		}
	}
}
