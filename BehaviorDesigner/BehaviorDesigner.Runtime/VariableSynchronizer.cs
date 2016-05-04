using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;
namespace BehaviorDesigner.Runtime
{
	[AddComponentMenu("Behavior Designer/Variable Synchronizer")]
	public class VariableSynchronizer : MonoBehaviour
	{
		public enum SynchronizationType
		{
			BehaviorDesigner,
			Property,
			Animator,
			PlayMaker,
			uFrame
		}
		public enum AnimatorParameterType
		{
			Bool,
			Float,
			Integer
		}
		[Serializable]
		public class SynchronizedVariable
		{
			public VariableSynchronizer.SynchronizationType synchronizationType;
			public bool setVariable;
			public Behavior behavior;
			public string variableName;
			public bool global;
			public Component targetComponent;
			public string targetName;
			public bool targetGlobal;
			public SharedVariable targetSharedVariable;
			public Action<object> setDelegate;
			public Func<object> getDelegate;
			public Animator animator;
			public VariableSynchronizer.AnimatorParameterType animatorParameterType;
			public int targetID;
			public Action<VariableSynchronizer.SynchronizedVariable> thirdPartyTick;
			public Enum variableType;
			public object thirdPartyVariable;
			public SharedVariable sharedVariable;
			public SynchronizedVariable(VariableSynchronizer.SynchronizationType synchronizationType, bool setVariable, Behavior behavior, string variableName, bool global, Component targetComponent, string targetName, bool targetGlobal)
			{
				this.synchronizationType = synchronizationType;
				this.setVariable = setVariable;
				this.behavior = behavior;
				this.variableName = variableName;
				this.global = global;
				this.targetComponent = targetComponent;
				this.targetName = targetName;
				this.targetGlobal = targetGlobal;
			}
		}
		[SerializeField]
		private UpdateIntervalType updateInterval;
		[SerializeField]
		private float updateIntervalSeconds;
		private WaitForSeconds updateWait;
		[SerializeField]
		private List<VariableSynchronizer.SynchronizedVariable> synchronizedVariables = new List<VariableSynchronizer.SynchronizedVariable>();
		public UpdateIntervalType UpdateInterval
		{
			get
			{
				return this.updateInterval;
			}
			set
			{
				this.updateInterval = value;
				this.UpdateIntervalChanged();
			}
		}
		public float UpdateIntervalSeconds
		{
			get
			{
				return this.updateIntervalSeconds;
			}
			set
			{
				this.updateIntervalSeconds = value;
				this.UpdateIntervalChanged();
			}
		}
		public List<VariableSynchronizer.SynchronizedVariable> SynchronizedVariables
		{
			get
			{
				return this.synchronizedVariables;
			}
			set
			{
				this.synchronizedVariables = value;
				base.enabled = true;
			}
		}
		private void UpdateIntervalChanged()
		{
			base.StopCoroutine("CoroutineUpdate");
			if (this.updateInterval == UpdateIntervalType.EveryFrame)
			{
				base.enabled = true;
			}
			else
			{
				if (this.updateInterval == UpdateIntervalType.SpecifySeconds)
				{
					if (Application.isPlaying)
					{
						this.updateWait = new WaitForSeconds(this.updateIntervalSeconds);
						base.StartCoroutine("CoroutineUpdate");
					}
					base.enabled = false;
				}
				else
				{
					base.enabled = false;
				}
			}
		}
		public void Awake()
		{
			for (int i = this.synchronizedVariables.Count - 1; i > -1; i--)
			{
				VariableSynchronizer.SynchronizedVariable synchronizedVariable = this.synchronizedVariables[i];
				if (synchronizedVariable.global)
				{
					synchronizedVariable.sharedVariable = GlobalVariables.Instance.GetVariable(synchronizedVariable.variableName);
				}
				else
				{
					synchronizedVariable.sharedVariable = synchronizedVariable.behavior.GetVariable(synchronizedVariable.variableName);
				}
				string text = string.Empty;
				if (synchronizedVariable.sharedVariable == null)
				{
					text = "the SharedVariable can't be found";
				}
				else
				{
					switch (synchronizedVariable.synchronizationType)
					{
					case VariableSynchronizer.SynchronizationType.BehaviorDesigner:
					{
						Behavior behavior = synchronizedVariable.targetComponent as Behavior;
						if (behavior == null)
						{
							text = "the target component is not of type Behavior Tree";
						}
						else
						{
							if (synchronizedVariable.targetGlobal)
							{
								synchronizedVariable.targetSharedVariable = GlobalVariables.Instance.GetVariable(synchronizedVariable.targetName);
							}
							else
							{
								synchronizedVariable.targetSharedVariable = behavior.GetVariable(synchronizedVariable.targetName);
							}
							if (synchronizedVariable.targetSharedVariable == null)
							{
								text = "the target SharedVariable cannot be found";
							}
						}
						break;
					}
					case VariableSynchronizer.SynchronizationType.Property:
					{
						PropertyInfo property = synchronizedVariable.targetComponent.GetType().GetProperty(synchronizedVariable.targetName);
						if (property == null)
						{
							text = "the property " + synchronizedVariable.targetName + " doesn't exist";
						}
						else
						{
							if (synchronizedVariable.setVariable)
							{
								MethodInfo getMethod = property.GetGetMethod();
								if (getMethod == null)
								{
									text = "the property has no get method";
								}
								else
								{
									synchronizedVariable.getDelegate = VariableSynchronizer.CreateGetDelegate(synchronizedVariable.targetComponent, getMethod);
								}
							}
							else
							{
								MethodInfo setMethod = property.GetSetMethod();
								if (setMethod == null)
								{
									text = "the property has no set method";
								}
								else
								{
									synchronizedVariable.setDelegate = VariableSynchronizer.CreateSetDelegate(synchronizedVariable.targetComponent, setMethod);
								}
							}
						}
						break;
					}
					case VariableSynchronizer.SynchronizationType.Animator:
						synchronizedVariable.animator = (synchronizedVariable.targetComponent as Animator);
						if (synchronizedVariable.animator == null)
						{
							text = "the component is not of type Animator";
						}
						else
						{
							synchronizedVariable.targetID = Animator.StringToHash(synchronizedVariable.targetName);
							Type propertyType = synchronizedVariable.sharedVariable.GetType().GetProperty("Value").PropertyType;
							if (propertyType.Equals(typeof(bool)))
							{
								synchronizedVariable.animatorParameterType = VariableSynchronizer.AnimatorParameterType.Bool;
							}
							else
							{
								if (propertyType.Equals(typeof(float)))
								{
									synchronizedVariable.animatorParameterType = VariableSynchronizer.AnimatorParameterType.Float;
								}
								else
								{
									if (propertyType.Equals(typeof(int)))
									{
										synchronizedVariable.animatorParameterType = VariableSynchronizer.AnimatorParameterType.Integer;
									}
									else
									{
										text = "there is no animator parameter type that can synchronize with " + propertyType;
									}
								}
							}
						}
						break;
					case VariableSynchronizer.SynchronizationType.PlayMaker:
					{
						Type typeWithinAssembly = TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.VariableSynchronizer_PlayMaker");
						if (typeWithinAssembly != null)
						{
							MethodInfo method = typeWithinAssembly.GetMethod("Start");
							if (method != null)
							{
								int num = (int)method.Invoke(null, new object[]
								{
									synchronizedVariable
								});
								if (num == 1)
								{
									text = "the PlayMaker NamedVariable cannot be found";
								}
								else
								{
									if (num == 2)
									{
										text = "the Behavior Designer SharedVariable is not the same type as the PlayMaker NamedVariable";
									}
									else
									{
										MethodInfo method2 = typeWithinAssembly.GetMethod("Tick");
										if (method2 != null)
										{
											synchronizedVariable.thirdPartyTick = (Action<VariableSynchronizer.SynchronizedVariable>)Delegate.CreateDelegate(typeof(Action<VariableSynchronizer.SynchronizedVariable>), method2);
										}
									}
								}
							}
						}
						else
						{
							text = "has the PlayMaker classes been imported?";
						}
						break;
					}
					case VariableSynchronizer.SynchronizationType.uFrame:
					{
						Type typeWithinAssembly2 = TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.VariableSynchronizer_uFrame");
						if (typeWithinAssembly2 != null)
						{
							MethodInfo method3 = typeWithinAssembly2.GetMethod("Start");
							if (method3 != null)
							{
								int num2 = (int)method3.Invoke(null, new object[]
								{
									synchronizedVariable
								});
								if (num2 == 1)
								{
									text = "the uFrame property cannot be found";
								}
								else
								{
									if (num2 == 2)
									{
										text = "the Behavior Designer SharedVariable is not the same type as the uFrame property";
									}
									else
									{
										MethodInfo method4 = typeWithinAssembly2.GetMethod("Tick");
										if (method4 != null)
										{
											synchronizedVariable.thirdPartyTick = (Action<VariableSynchronizer.SynchronizedVariable>)Delegate.CreateDelegate(typeof(Action<VariableSynchronizer.SynchronizedVariable>), method4);
										}
									}
								}
							}
						}
						else
						{
							text = "has the uFrame classes been imported?";
						}
						break;
					}
					}
				}
				if (!string.IsNullOrEmpty(text))
				{
					UnityEngine.Debug.LogError(string.Format("Unable to synchronize {0}: {1}", synchronizedVariable.sharedVariable.Name, text));
					this.synchronizedVariables.RemoveAt(i);
				}
			}
			if (this.synchronizedVariables.Count == 0)
			{
				base.enabled = false;
				return;
			}
			this.UpdateIntervalChanged();
		}
		public void Update()
		{
			this.Tick();
		}
		[DebuggerHidden]
		private IEnumerator CoroutineUpdate()
		{
            //VariableSynchronizer.<CoroutineUpdate>c__Iterator2 <CoroutineUpdate>c__Iterator = new VariableSynchronizer.<CoroutineUpdate>c__Iterator2();
            //<CoroutineUpdate>c__Iterator.<>f__this = this;
            //return <CoroutineUpdate>c__Iterator;
            Tick();
            yield return updateWait;
		}
		public void Tick()
		{
			for (int i = 0; i < this.synchronizedVariables.Count; i++)
			{
				VariableSynchronizer.SynchronizedVariable synchronizedVariable = this.synchronizedVariables[i];
				switch (synchronizedVariable.synchronizationType)
				{
				case VariableSynchronizer.SynchronizationType.BehaviorDesigner:
					if (synchronizedVariable.setVariable)
					{
						synchronizedVariable.sharedVariable.SetValue(synchronizedVariable.targetSharedVariable.GetValue());
					}
					else
					{
						synchronizedVariable.targetSharedVariable.SetValue(synchronizedVariable.sharedVariable.GetValue());
					}
					break;
				case VariableSynchronizer.SynchronizationType.Property:
					if (synchronizedVariable.setVariable)
					{
						synchronizedVariable.sharedVariable.SetValue(synchronizedVariable.getDelegate());
					}
					else
					{
						synchronizedVariable.setDelegate(synchronizedVariable.sharedVariable.GetValue());
					}
					break;
				case VariableSynchronizer.SynchronizationType.Animator:
					if (synchronizedVariable.setVariable)
					{
						switch (synchronizedVariable.animatorParameterType)
						{
						case VariableSynchronizer.AnimatorParameterType.Bool:
							synchronizedVariable.sharedVariable.SetValue(synchronizedVariable.animator.GetBool(synchronizedVariable.targetID));
							break;
						case VariableSynchronizer.AnimatorParameterType.Float:
							synchronizedVariable.sharedVariable.SetValue(synchronizedVariable.animator.GetFloat(synchronizedVariable.targetID));
							break;
						case VariableSynchronizer.AnimatorParameterType.Integer:
							synchronizedVariable.sharedVariable.SetValue(synchronizedVariable.animator.GetInteger(synchronizedVariable.targetID));
							break;
						}
					}
					else
					{
						switch (synchronizedVariable.animatorParameterType)
						{
						case VariableSynchronizer.AnimatorParameterType.Bool:
							synchronizedVariable.animator.SetBool(synchronizedVariable.targetID, (bool)synchronizedVariable.sharedVariable.GetValue());
							break;
						case VariableSynchronizer.AnimatorParameterType.Float:
							synchronizedVariable.animator.SetFloat(synchronizedVariable.targetID, (float)synchronizedVariable.sharedVariable.GetValue());
							break;
						case VariableSynchronizer.AnimatorParameterType.Integer:
							synchronizedVariable.animator.SetInteger(synchronizedVariable.targetID, (int)synchronizedVariable.sharedVariable.GetValue());
							break;
						}
					}
					break;
				case VariableSynchronizer.SynchronizationType.PlayMaker:
				case VariableSynchronizer.SynchronizationType.uFrame:
					synchronizedVariable.thirdPartyTick(synchronizedVariable);
					break;
				}
			}
		}
		private static Func<object> CreateGetDelegate(object instance, MethodInfo method)
		{
			ConstantExpression instance2 = Expression.Constant(instance);
			MethodCallExpression expression = Expression.Call(instance2, method);
			return Expression.Lambda<Func<object>>(Expression.TypeAs(expression, typeof(object)), new ParameterExpression[0]).Compile();
		}
		private static Action<object> CreateSetDelegate(object instance, MethodInfo method)
		{
			ConstantExpression instance2 = Expression.Constant(instance);
			ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "p");
			UnaryExpression unaryExpression = Expression.Convert(parameterExpression, method.GetParameters()[0].ParameterType);
			MethodCallExpression body = Expression.Call(instance2, method, new Expression[]
			{
				unaryExpression
			});
			return Expression.Lambda<Action<object>>(body, new ParameterExpression[]
			{
				parameterExpression
			}).Compile();
		}
	}
}
