using System;
using System.Reflection;
using UnityEngine;
namespace BehaviorDesigner.Runtime
{
	public abstract class SharedVariable
	{
		[SerializeField]
		private bool mIsShared;
		[SerializeField]
		private bool mIsGlobal;
		[SerializeField]
		private string mName;
		[SerializeField]
		private string mPropertyMapping;
		[SerializeField]
		private GameObject mPropertyMappingOwner;
		[SerializeField]
		private bool mNetworkSync;
		public bool IsShared
		{
			get
			{
				return this.mIsShared;
			}
			set
			{
				this.mIsShared = value;
			}
		}
		public bool IsGlobal
		{
			get
			{
				return this.mIsGlobal;
			}
			set
			{
				this.mIsGlobal = value;
			}
		}
		public string Name
		{
			get
			{
				return this.mName;
			}
			set
			{
				this.mName = value;
			}
		}
		public string PropertyMapping
		{
			get
			{
				return this.mPropertyMapping;
			}
			set
			{
				this.mPropertyMapping = value;
			}
		}
		public GameObject PropertyMappingOwner
		{
			get
			{
				return this.mPropertyMappingOwner;
			}
			set
			{
				this.mPropertyMappingOwner = value;
			}
		}
		public bool NetworkSync
		{
			get
			{
				return this.mNetworkSync;
			}
			set
			{
				this.mNetworkSync = value;
			}
		}
		public bool IsNone
		{
			get
			{
				return this.mIsShared && string.IsNullOrEmpty(this.mName);
			}
		}
		public void ValueChanged()
		{
		}
		public virtual void InitializePropertyMapping(BehaviorSource behaviorSource)
		{
		}
		public abstract object GetValue();
		public abstract void SetValue(object value);
	}
	public abstract class SharedVariable<T> : SharedVariable
	{
		private Func<T> mGetter;
		private Action<T> mSetter;
		[SerializeField]
		protected T mValue;
		public T Value
		{
			get
			{
				return (this.mGetter == null) ? this.mValue : this.mGetter();
			}
			set
			{
				bool flag = !object.Equals(this.Value, value);
				if (this.mSetter != null)
				{
					this.mSetter(value);
				}
				else
				{
					this.mValue = value;
				}
				if (flag)
				{
					base.ValueChanged();
				}
			}
		}
		public override void InitializePropertyMapping(BehaviorSource behaviorSource)
		{
			if (!Application.isPlaying || !(behaviorSource.Owner.GetObject() is Behavior))
			{
				return;
			}
			if (!string.IsNullOrEmpty(base.PropertyMapping))
			{
				string[] array = base.PropertyMapping.Split(new char[]
				{
					'/'
				});
				GameObject gameObject;
				if (!object.Equals(base.PropertyMappingOwner, null))
				{
					gameObject = base.PropertyMappingOwner;
				}
				else
				{
					gameObject = (behaviorSource.Owner.GetObject() as Behavior).gameObject;
				}
				Component component = gameObject.GetComponent(TaskUtility.GetTypeWithinAssembly(array[0]));
				Type type = component.GetType();
				PropertyInfo property = type.GetProperty(array[1]);
				if (property != null)
				{
					MethodInfo methodInfo = property.GetGetMethod();
					if (methodInfo != null)
					{
						this.mGetter = (Func<T>)Delegate.CreateDelegate(typeof(Func<T>), component, methodInfo);
					}
					methodInfo = property.GetSetMethod();
					if (methodInfo != null)
					{
						this.mSetter = (Action<T>)Delegate.CreateDelegate(typeof(Action<T>), component, methodInfo);
					}
				}
			}
		}
		public override object GetValue()
		{
			return this.Value;
		}
		public override void SetValue(object value)
		{
			if (this.mSetter != null)
			{
				this.mSetter((T)((object)value));
			}
			else
			{
				this.mValue = (T)((object)value);
			}
		}
		public override string ToString()
		{
			string arg_2E_0;
			if (this.Value == null)
			{
				arg_2E_0 = "(null)";
			}
			else
			{
				T value = this.Value;
				arg_2E_0 = value.ToString();
			}
			return arg_2E_0;
		}
	}
}
