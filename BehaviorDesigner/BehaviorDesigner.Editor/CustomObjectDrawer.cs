using System;
namespace BehaviorDesigner.Editor
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
	public sealed class CustomObjectDrawer : Attribute
	{
		private Type type;
		public Type Type
		{
			get
			{
				return this.type;
			}
		}
		public CustomObjectDrawer(Type type)
		{
			this.type = type;
		}
	}
}
