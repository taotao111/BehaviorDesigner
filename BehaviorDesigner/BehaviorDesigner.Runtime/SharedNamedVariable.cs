using System;
namespace BehaviorDesigner.Runtime
{
	[Serializable]
	public class SharedNamedVariable : SharedVariable<NamedVariable>
	{
		public SharedNamedVariable()
		{
			this.mValue = new NamedVariable();
		}
		public static implicit operator SharedNamedVariable(NamedVariable value)
		{
			return new SharedNamedVariable
			{
				mValue = value
			};
		}
	}
}
