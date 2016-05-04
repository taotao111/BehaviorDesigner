using System;
using UnityEngine;
namespace BehaviorDesigner.Runtime
{
	[Serializable]
	public abstract class ExternalBehavior : ScriptableObject, IBehavior
	{
		[SerializeField]
		private BehaviorSource mBehaviorSource;
		public BehaviorSource BehaviorSource
		{
			get
			{
				return this.mBehaviorSource;
			}
			set
			{
				this.mBehaviorSource = value;
			}
		}
		public BehaviorSource GetBehaviorSource()
		{
			return this.mBehaviorSource;
		}
		public void SetBehaviorSource(BehaviorSource behaviorSource)
		{
			this.mBehaviorSource = behaviorSource;
		}
		public UnityEngine.Object GetObject()
		{
			return this;
		}
		public string GetOwnerName()
		{
			return base.name;
		}
		public SharedVariable GetVariable(string name)
		{
			this.mBehaviorSource.CheckForSerialization(false, null);
			return this.mBehaviorSource.GetVariable(name);
		}
		public void SetVariable(string name, SharedVariable item)
		{
			this.mBehaviorSource.CheckForSerialization(false, null);
			this.mBehaviorSource.SetVariable(name, item);
		}
		public void SetVariableValue(string name, object value)
		{
			SharedVariable variable = this.GetVariable(name);
			if (variable != null)
			{
				variable.SetValue(value);
				variable.ValueChanged();
			}
		}
        public virtual new int GetInstanceID()
        {
            return base.GetInstanceID();
        }
    }
}
