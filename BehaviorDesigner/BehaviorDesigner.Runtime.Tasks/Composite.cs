using System;
using UnityEngine;
namespace BehaviorDesigner.Runtime.Tasks
{
	public abstract class Composite : ParentTask
	{
		[Tooltip("Specifies the type of conditional abort. More information is located at http://www.opsive.com/assets/BehaviorDesigner/documentation.php?id=89."), SerializeField]
		protected AbortType abortType;
		public AbortType AbortType
		{
			get
			{
				return this.abortType;
			}
		}
	}
}
