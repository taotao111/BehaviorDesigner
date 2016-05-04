using System;
using UnityEngine;
namespace BehaviorDesigner.Runtime.Tasks
{
	[HelpURL("http://www.opsive.com/assets/BehaviorDesigner/documentation.php?id=53"), TaskDescription("Behavior Reference allows you to run another behavior tree within the current behavior tree."), TaskIcon("BehaviorTreeReferenceIcon.png")]
	public abstract class BehaviorReference : Action
	{
		[RequiredField, Tooltip("External behavior array that this task should reference")]
		public ExternalBehavior[] externalBehaviors;
		[Tooltip("Any variables that should be set for the specific tree")]
		public SharedNamedVariable[] variables;
		[HideInInspector]
		public bool collapsed;
		public virtual ExternalBehavior[] GetExternalBehaviors()
		{
			return this.externalBehaviors;
		}
		public override void OnReset()
		{
			this.externalBehaviors = null;
		}
	}
}
