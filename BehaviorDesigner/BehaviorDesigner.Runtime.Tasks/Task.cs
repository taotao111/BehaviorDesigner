using System;
using System.Collections;
using UnityEngine;
namespace BehaviorDesigner.Runtime.Tasks
{
	public abstract class Task
	{
		protected GameObject gameObject;
		protected Transform transform;
		[SerializeField]
		private NodeData nodeData;
		[SerializeField]
		private Behavior owner;
		[SerializeField]
		private int id = -1;
		[SerializeField]
		private string friendlyName = string.Empty;
		[SerializeField]
		private bool instant = true;
		private int referenceID = -1;
		public GameObject GameObject
		{
			set
			{
				this.gameObject = value;
			}
		}
		public Transform Transform
		{
			set
			{
				this.transform = value;
			}
		}
		public NodeData NodeData
		{
			get
			{
				return this.nodeData;
			}
			set
			{
				this.nodeData = value;
			}
		}
		public Behavior Owner
		{
			get
			{
				return this.owner;
			}
			set
			{
				this.owner = value;
			}
		}
		public int ID
		{
			get
			{
				return this.id;
			}
			set
			{
				this.id = value;
			}
		}
		public string FriendlyName
		{
			get
			{
				return this.friendlyName;
			}
			set
			{
				this.friendlyName = value;
			}
		}
		public bool IsInstant
		{
			get
			{
				return this.instant;
			}
			set
			{
				this.instant = value;
			}
		}
		public int ReferenceID
		{
			get
			{
				return this.referenceID;
			}
			set
			{
				this.referenceID = value;
			}
		}
		public virtual void OnAwake()
		{
		}
		public virtual void OnStart()
		{
		}
		public virtual TaskStatus OnUpdate()
		{
			return TaskStatus.Success;
		}
		public virtual void OnLateUpdate()
		{
		}
		public virtual void OnFixedUpdate()
		{
		}
		public virtual void OnEnd()
		{
		}
		public virtual void OnPause(bool paused)
		{
		}
		public virtual float GetPriority()
		{
			return 0f;
		}
		public virtual void OnBehaviorRestart()
		{
		}
		public virtual void OnBehaviorComplete()
		{
		}
		public virtual void OnReset()
		{
		}
		public virtual void OnDrawGizmos()
		{
		}
		protected void StartCoroutine(string methodName)
		{
			this.Owner.StartTaskCoroutine(this, methodName);
		}
		protected Coroutine StartCoroutine(IEnumerator routine)
		{
			return this.Owner.StartCoroutine(routine);
		}
		protected Coroutine StartCoroutine(string methodName, object value)
		{
			return this.Owner.StartTaskCoroutine(this, methodName, value);
		}
		protected void StopCoroutine(string methodName)
		{
			this.Owner.StopTaskCoroutine(methodName);
		}
		protected void StopAllCoroutines()
		{
			this.Owner.StopAllTaskCoroutines();
		}
		public virtual void OnCollisionEnter(Collision collision)
		{
		}
		public virtual void OnCollisionExit(Collision collision)
		{
		}
		public virtual void OnCollisionStay(Collision collision)
		{
		}
		public virtual void OnTriggerEnter(Collider other)
		{
		}
		public virtual void OnTriggerExit(Collider other)
		{
		}
		public virtual void OnTriggerStay(Collider other)
		{
		}
		public virtual void OnCollisionEnter2D(Collision2D collision)
		{
		}
		public virtual void OnCollisionExit2D(Collision2D collision)
		{
		}
		public virtual void OnCollisionStay2D(Collision2D collision)
		{
		}
		public virtual void OnTriggerEnter2D(Collider2D other)
		{
		}
		public virtual void OnTriggerExit2D(Collider2D other)
		{
		}
		public virtual void OnTriggerStay2D(Collider2D other)
		{
		}
		public virtual void OnControllerColliderHit(ControllerColliderHit hit)
		{
		}
		protected T GetComponent<T>() where T : Component
		{
			return this.gameObject.GetComponent<T>();
		}
		protected Component GetComponent(Type type)
		{
			return this.gameObject.GetComponent(type);
		}
		protected GameObject GetDefaultGameObject(GameObject go)
		{
			if (go == null)
			{
				return this.gameObject;
			}
			return go;
		}
	}
}
