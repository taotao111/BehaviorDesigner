using System;
namespace BehaviorDesigner.Runtime.Tasks
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class TaskDescriptionAttribute : Attribute
	{
		public readonly string mDescription;
		public string Description
		{
			get
			{
				return this.mDescription;
			}
		}
		public TaskDescriptionAttribute(string description)
		{
			this.mDescription = description;
		}
	}
}
