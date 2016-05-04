using System;
namespace BehaviorDesigner.Runtime.Tasks
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class TaskCategoryAttribute : Attribute
	{
		public readonly string mCategory;
		public string Category
		{
			get
			{
				return this.mCategory;
			}
		}
		public TaskCategoryAttribute(string category)
		{
			this.mCategory = category;
		}
	}
}
