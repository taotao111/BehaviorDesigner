using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
namespace BehaviorDesigner.Runtime
{
	[Serializable]
	public abstract class Behavior : MonoBehaviour, IBehavior
	{
		public enum EventTypes
		{
			OnCollisionEnter,
			OnCollisionStay,
			OnCollisionExit,
			OnTriggerEnter,
			OnTriggerStay,
			OnTriggerExit,
			OnCollisionEnter2D,
			OnCollisionStay2D,
			OnCollisionExit2D,
			OnTriggerEnter2D,
			OnTriggerStay2D,
			OnTriggerExit2D,
			OnControllerColliderHit,
			OnLateUpdate,
			OnFixedUpdate,
			None
		}
		public enum GizmoViewMode
		{
			Running,
			Always,
			Selected,
			Never
		}
		public delegate void BehaviorHandler();
		[SerializeField]
		private bool startWhenEnabled = true;
		[SerializeField]
		private bool pauseWhenDisabled;
		[SerializeField]
		private bool restartWhenComplete;
		[SerializeField]
		private bool logTaskChanges;
		[SerializeField]
		private int group;
		[SerializeField]
		private bool resetValuesOnRestart;
		[SerializeField]
		private ExternalBehavior externalBehavior;
		private bool hasInheritedVariables;
		[SerializeField]
		private BehaviorSource mBehaviorSource;
		private bool isPaused;
		private TaskStatus executionStatus;
		private bool initialized;
		private List<Dictionary<string, object>> defaultValues;
		private Dictionary<string, object> defaultVariableValues;
		private bool[] hasEvent = new bool[15];
		private Dictionary<string, List<TaskCoroutine>> activeTaskCoroutines;
		private Dictionary<Type, Dictionary<string, Delegate>> eventTable;
		[NonSerialized]
		public Behavior.GizmoViewMode gizmoViewMode;
		[NonSerialized]
		public bool showBehaviorDesignerGizmo = true;
        public event Behavior.BehaviorHandler OnBehaviorStart;
        public event Behavior.BehaviorHandler OnBehaviorRestart;
        public event Behavior.BehaviorHandler OnBehaviorEnd;
		public bool StartWhenEnabled
		{
			get
			{
				return this.startWhenEnabled;
			}
			set
			{
				this.startWhenEnabled = value;
			}
		}
		public bool PauseWhenDisabled
		{
			get
			{
				return this.pauseWhenDisabled;
			}
			set
			{
				this.pauseWhenDisabled = value;
			}
		}
		public bool RestartWhenComplete
		{
			get
			{
				return this.restartWhenComplete;
			}
			set
			{
				this.restartWhenComplete = value;
			}
		}
		public bool LogTaskChanges
		{
			get
			{
				return this.logTaskChanges;
			}
			set
			{
				this.logTaskChanges = value;
			}
		}
		public int Group
		{
			get
			{
				return this.group;
			}
			set
			{
				this.group = value;
			}
		}
		public bool ResetValuesOnRestart
		{
			get
			{
				return this.resetValuesOnRestart;
			}
			set
			{
				this.resetValuesOnRestart = value;
			}
		}
		public ExternalBehavior ExternalBehavior
		{
			get
			{
				return this.externalBehavior;
			}
			set
			{
				if (this.externalBehavior != value)
				{
					if (BehaviorManager.instance != null)
					{
						BehaviorManager.instance.DisableBehavior(this);
					}
					this.mBehaviorSource.HasSerialized = false;
				}
				this.externalBehavior = value;
				if (this.startWhenEnabled)
				{
					this.EnableBehavior();
				}
			}
		}
		public bool HasInheritedVariables
		{
			get
			{
				return this.hasInheritedVariables;
			}
			set
			{
				this.hasInheritedVariables = value;
			}
		}
		public string BehaviorName
		{
			get
			{
				return this.mBehaviorSource.behaviorName;
			}
			set
			{
				this.mBehaviorSource.behaviorName = value;
			}
		}
		public string BehaviorDescription
		{
			get
			{
				return this.mBehaviorSource.behaviorDescription;
			}
			set
			{
				this.mBehaviorSource.behaviorDescription = value;
			}
		}
		public TaskStatus ExecutionStatus
		{
			get
			{
				return this.executionStatus;
			}
			set
			{
				this.executionStatus = value;
			}
		}
		public bool[] HasEvent
		{
			get
			{
				return this.hasEvent;
			}
		}
		public Behavior()
		{
			this.mBehaviorSource = new BehaviorSource(this);
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
			return base.gameObject.name;
		}
		public void Start()
		{
			if (this.startWhenEnabled)
			{
				this.EnableBehavior();
			}
			this.initialized = true;
			for (int i = 0; i < 15; i++)
			{
				this.hasEvent[i] = this.TaskContainsMethod(((Behavior.EventTypes)i).ToString(), this.mBehaviorSource.RootTask);
			}
		}
		private bool TaskContainsMethod(string methodName, Task task)
		{
			if (task == null)
			{
				return false;
			}
			MethodInfo method = task.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (method != null && task.GetType().Equals(method.DeclaringType))
			{
				return true;
			}
			if (task is ParentTask)
			{
				ParentTask parentTask = task as ParentTask;
				if (parentTask.Children != null)
				{
					for (int i = 0; i < parentTask.Children.Count; i++)
					{
						if (this.TaskContainsMethod(methodName, parentTask.Children[i]))
						{
							return true;
						}
					}
				}
			}
			return false;
		}
		public void EnableBehavior()
		{
			Behavior.CreateBehaviorManager();
			if (BehaviorManager.instance != null)
			{
				BehaviorManager.instance.EnableBehavior(this);
			}
		}
		public void DisableBehavior()
		{
			if (BehaviorManager.instance != null)
			{
				BehaviorManager.instance.DisableBehavior(this, this.pauseWhenDisabled);
				this.isPaused = this.pauseWhenDisabled;
			}
		}
		public void DisableBehavior(bool pause)
		{
			if (BehaviorManager.instance != null)
			{
				BehaviorManager.instance.DisableBehavior(this, pause);
				this.isPaused = pause;
			}
		}
		public void OnEnable()
		{
			if (BehaviorManager.instance != null && this.isPaused)
			{
				BehaviorManager.instance.EnableBehavior(this);
				this.isPaused = false;
			}
			else
			{
				if (this.startWhenEnabled && this.initialized)
				{
					this.EnableBehavior();
				}
			}
		}
		public void OnDisable()
		{
			this.DisableBehavior();
		}
		public SharedVariable GetVariable(string name)
		{
			this.CheckForSerialization();
			return this.mBehaviorSource.GetVariable(name);
		}
		public void SetVariable(string name, SharedVariable item)
		{
			this.CheckForSerialization();
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
			else
			{
				Debug.LogError("Error: No variable exists with name " + name);
			}
		}
		public List<SharedVariable> GetAllVariables()
		{
			this.CheckForSerialization();
			return this.mBehaviorSource.GetAllVariables();
		}
		public void CheckForSerialization()
		{
			if (this.externalBehavior != null)
			{
				List<SharedVariable> list = null;
				bool force = false;
				if (!this.hasInheritedVariables)
				{
					this.mBehaviorSource.CheckForSerialization(false, null);
					list = this.mBehaviorSource.GetAllVariables();
					this.hasInheritedVariables = true;
					force = true;
				}
				this.externalBehavior.BehaviorSource.Owner = this.ExternalBehavior;
				this.externalBehavior.BehaviorSource.CheckForSerialization(force, this.GetBehaviorSource());
				if (list != null)
				{
					for (int i = 0; i < list.Count; i++)
					{
						if (list[i] != null)
						{
							this.mBehaviorSource.SetVariable(list[i].Name, list[i]);
						}
					}
				}
			}
			else
			{
				this.mBehaviorSource.CheckForSerialization(false, null);
			}
		}
		public void OnCollisionEnter(Collision collision)
		{
			if (this.hasEvent[0] && BehaviorManager.instance != null)
			{
				BehaviorManager.instance.BehaviorOnCollisionEnter(collision, this);
			}
		}
		public void OnCollisionExit(Collision collision)
		{
			if (this.hasEvent[2] && BehaviorManager.instance != null)
			{
				BehaviorManager.instance.BehaviorOnCollisionExit(collision, this);
			}
		}
		public void OnCollisionStay(Collision collision)
		{
			if (this.hasEvent[1] && BehaviorManager.instance != null)
			{
				BehaviorManager.instance.BehaviorOnCollisionStay(collision, this);
			}
		}
		public void OnTriggerEnter(Collider other)
		{
			if (this.hasEvent[3] && BehaviorManager.instance != null)
			{
				BehaviorManager.instance.BehaviorOnTriggerEnter(other, this);
			}
		}
		public void OnTriggerExit(Collider other)
		{
			if (this.hasEvent[5] && BehaviorManager.instance != null)
			{
				BehaviorManager.instance.BehaviorOnTriggerExit(other, this);
			}
		}
		public void OnTriggerStay(Collider other)
		{
			if (this.hasEvent[4] && BehaviorManager.instance != null)
			{
				BehaviorManager.instance.BehaviorOnTriggerStay(other, this);
			}
		}
		public void OnCollisionEnter2D(Collision2D collision)
		{
			if (this.hasEvent[6] && BehaviorManager.instance != null)
			{
				BehaviorManager.instance.BehaviorOnCollisionEnter2D(collision, this);
			}
		}
		public void OnCollisionExit2D(Collision2D collision)
		{
			if (this.hasEvent[8] && BehaviorManager.instance != null)
			{
				BehaviorManager.instance.BehaviorOnCollisionExit2D(collision, this);
			}
		}
		public void OnCollisionStay2D(Collision2D collision)
		{
			if (this.hasEvent[7] && BehaviorManager.instance != null)
			{
				BehaviorManager.instance.BehaviorOnCollisionStay2D(collision, this);
			}
		}
		public void OnTriggerEnter2D(Collider2D other)
		{
			if (this.hasEvent[9] && BehaviorManager.instance != null)
			{
				BehaviorManager.instance.BehaviorOnTriggerEnter2D(other, this);
			}
		}
		public void OnTriggerExit2D(Collider2D other)
		{
			if (this.hasEvent[11] && BehaviorManager.instance != null)
			{
				BehaviorManager.instance.BehaviorOnTriggerExit2D(other, this);
			}
		}
		public void OnTriggerStay2D(Collider2D other)
		{
			if (this.hasEvent[10] && BehaviorManager.instance != null)
			{
				BehaviorManager.instance.BehaviorOnTriggerStay2D(other, this);
			}
		}
		public void OnControllerColliderHit(ControllerColliderHit hit)
		{
			if (this.hasEvent[12] && BehaviorManager.instance != null)
			{
				BehaviorManager.instance.BehaviorOnControllerColliderHit(hit, this);
			}
		}
		public void OnDrawGizmos()
		{
			this.DrawTaskGizmos(false);
		}
		public void OnDrawGizmosSelected()
		{
			if (this.showBehaviorDesignerGizmo)
			{
				Gizmos.DrawIcon(base.transform.position, "Behavior Designer Scene Icon.png");
			}
			this.DrawTaskGizmos(true);
		}
		private void DrawTaskGizmos(bool selected)
		{
			if (this.gizmoViewMode == Behavior.GizmoViewMode.Never || (this.gizmoViewMode == Behavior.GizmoViewMode.Selected && !selected))
			{
				return;
			}
			if (this.gizmoViewMode == Behavior.GizmoViewMode.Running || this.gizmoViewMode == Behavior.GizmoViewMode.Always || (Application.isPlaying && this.ExecutionStatus == TaskStatus.Running) || !Application.isPlaying)
			{
				this.CheckForSerialization();
				this.DrawTaskGizmos(this.mBehaviorSource.RootTask);
				List<Task> detachedTasks = this.mBehaviorSource.DetachedTasks;
				if (detachedTasks != null)
				{
					for (int i = 0; i < detachedTasks.Count; i++)
					{
						this.DrawTaskGizmos(detachedTasks[i]);
					}
				}
			}
		}
		private void DrawTaskGizmos(Task task)
		{
			if (task == null)
			{
				return;
			}
			if (this.gizmoViewMode == Behavior.GizmoViewMode.Running && !task.NodeData.IsReevaluating && (task.NodeData.IsReevaluating || task.NodeData.ExecutionStatus != TaskStatus.Running))
			{
				return;
			}
			task.OnDrawGizmos();
			if (task is ParentTask)
			{
				ParentTask parentTask = task as ParentTask;
				if (parentTask.Children != null)
				{
					for (int i = 0; i < parentTask.Children.Count; i++)
					{
						this.DrawTaskGizmos(parentTask.Children[i]);
					}
				}
			}
		}
		public T FindTask<T>() where T : Task
		{
			return this.FindTask<T>(this.mBehaviorSource.RootTask);
		}
		private T FindTask<T>(Task task) where T : Task
		{
			if (task.GetType().Equals(typeof(T)))
			{
				return (T)((object)task);
			}
			ParentTask parentTask;
			if ((parentTask = (task as ParentTask)) != null && parentTask.Children != null)
			{
				for (int i = 0; i < parentTask.Children.Count; i++)
				{
					T result = (T)((object)null);
					if ((result = this.FindTask<T>(parentTask.Children[i])) != null)
					{
						return result;
					}
				}
			}
			return (T)((object)null);
		}
		public List<T> FindTasks<T>() where T : Task
		{
			this.CheckForSerialization();
			List<T> result = new List<T>();
			this.FindTasks<T>(this.mBehaviorSource.RootTask, ref result);
			return result;
		}
		private void FindTasks<T>(Task task, ref List<T> taskList) where T : Task
		{
			if (typeof(T).IsAssignableFrom(task.GetType()))
			{
				taskList.Add((T)((object)task));
			}
			ParentTask parentTask;
			if ((parentTask = (task as ParentTask)) != null && parentTask.Children != null)
			{
				for (int i = 0; i < parentTask.Children.Count; i++)
				{
					this.FindTasks<T>(parentTask.Children[i], ref taskList);
				}
			}
		}
		public Task FindTaskWithName(string taskName)
		{
			return this.FindTaskWithName(taskName, this.mBehaviorSource.RootTask);
		}
		private Task FindTaskWithName(string taskName, Task task)
		{
			if (task.FriendlyName.Equals(taskName))
			{
				return task;
			}
			ParentTask parentTask;
			if ((parentTask = (task as ParentTask)) != null && parentTask.Children != null)
			{
				for (int i = 0; i < parentTask.Children.Count; i++)
				{
					Task result;
					if ((result = this.FindTaskWithName(taskName, parentTask.Children[i])) != null)
					{
						return result;
					}
				}
			}
			return null;
		}
		public List<Task> FindTasksWithName(string taskName)
		{
			List<Task> result = new List<Task>();
			this.FindTasksWithName(taskName, this.mBehaviorSource.RootTask, ref result);
			return result;
		}
		private void FindTasksWithName(string taskName, Task task, ref List<Task> taskList)
		{
			if (task.FriendlyName.Equals(taskName))
			{
				taskList.Add(task);
			}
			ParentTask parentTask;
			if ((parentTask = (task as ParentTask)) != null && parentTask.Children != null)
			{
				for (int i = 0; i < parentTask.Children.Count; i++)
				{
					this.FindTasksWithName(taskName, parentTask.Children[i], ref taskList);
				}
			}
		}
		public List<Task> GetActiveTasks()
		{
			if (BehaviorManager.instance == null)
			{
				return null;
			}
			return BehaviorManager.instance.GetActiveTasks(this);
		}
		public Coroutine StartTaskCoroutine(Task task, string methodName)
		{
			MethodInfo method = task.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (method == null)
			{
				Debug.LogError("Unable to start coroutine " + methodName + ": method not found");
				return null;
			}
			if (this.activeTaskCoroutines == null)
			{
				this.activeTaskCoroutines = new Dictionary<string, List<TaskCoroutine>>();
			}
			TaskCoroutine taskCoroutine = new TaskCoroutine(this, (IEnumerator)method.Invoke(task, new object[0]), methodName);
			if (this.activeTaskCoroutines.ContainsKey(methodName))
			{
				List<TaskCoroutine> list = this.activeTaskCoroutines[methodName];
				list.Add(taskCoroutine);
				this.activeTaskCoroutines[methodName] = list;
			}
			else
			{
				List<TaskCoroutine> list2 = new List<TaskCoroutine>();
				list2.Add(taskCoroutine);
				this.activeTaskCoroutines.Add(methodName, list2);
			}
			return taskCoroutine.Coroutine;
		}
		public Coroutine StartTaskCoroutine(Task task, string methodName, object value)
		{
			MethodInfo method = task.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (method == null)
			{
				Debug.LogError("Unable to start coroutine " + methodName + ": method not found");
				return null;
			}
			if (this.activeTaskCoroutines == null)
			{
				this.activeTaskCoroutines = new Dictionary<string, List<TaskCoroutine>>();
			}
			TaskCoroutine taskCoroutine = new TaskCoroutine(this, (IEnumerator)method.Invoke(task, new object[]
			{
				value
			}), methodName);
			if (this.activeTaskCoroutines.ContainsKey(methodName))
			{
				List<TaskCoroutine> list = this.activeTaskCoroutines[methodName];
				list.Add(taskCoroutine);
				this.activeTaskCoroutines[methodName] = list;
			}
			else
			{
				List<TaskCoroutine> list2 = new List<TaskCoroutine>();
				list2.Add(taskCoroutine);
				this.activeTaskCoroutines.Add(methodName, list2);
			}
			return taskCoroutine.Coroutine;
		}
		public void StopTaskCoroutine(string methodName)
		{
			if (!this.activeTaskCoroutines.ContainsKey(methodName))
			{
				return;
			}
			List<TaskCoroutine> list = this.activeTaskCoroutines[methodName];
			for (int i = 0; i < list.Count; i++)
			{
				list[i].Stop();
			}
		}
		public void StopAllTaskCoroutines()
		{
			base.StopAllCoroutines();
			foreach (KeyValuePair<string, List<TaskCoroutine>> current in this.activeTaskCoroutines)
			{
				List<TaskCoroutine> value = current.Value;
				for (int i = 0; i < value.Count; i++)
				{
					value[i].Stop();
				}
			}
		}
		public void TaskCoroutineEnded(TaskCoroutine taskCoroutine, string coroutineName)
		{
			if (this.activeTaskCoroutines.ContainsKey(coroutineName))
			{
				List<TaskCoroutine> list = this.activeTaskCoroutines[coroutineName];
				if (list.Count == 1)
				{
					this.activeTaskCoroutines.Remove(coroutineName);
				}
				else
				{
					list.Remove(taskCoroutine);
					this.activeTaskCoroutines[coroutineName] = list;
				}
			}
		}
		public void OnBehaviorStarted()
		{
			if (this.OnBehaviorStart != null)
			{
				this.OnBehaviorStart();
			}
		}
		public void OnBehaviorRestarted()
		{
			if (this.OnBehaviorRestart != null)
			{
				this.OnBehaviorRestart();
			}
		}
		public void OnBehaviorEnded()
		{
			if (this.OnBehaviorEnd != null)
			{
				this.OnBehaviorEnd();
			}
		}
		private void RegisterEvent(string name, Delegate handler)
		{
			if (this.eventTable == null)
			{
				this.eventTable = new Dictionary<Type, Dictionary<string, Delegate>>();
			}
			Dictionary<string, Delegate> dictionary;
			if (!this.eventTable.TryGetValue(handler.GetType(), out dictionary))
			{
				dictionary = new Dictionary<string, Delegate>();
				this.eventTable.Add(handler.GetType(), dictionary);
			}
			Delegate a;
			if (dictionary.TryGetValue(name, out a))
			{
				dictionary[name] = Delegate.Combine(a, handler);
			}
			else
			{
				dictionary.Add(name, handler);
			}
		}
		public void RegisterEvent(string name, System.Action handler)
		{
			this.RegisterEvent(name, handler);
		}
		public void RegisterEvent<T>(string name, Action<T> handler)
		{
			this.RegisterEvent(name, handler);
		}
		public void RegisterEvent<T, U>(string name, Action<T, U> handler)
		{
			this.RegisterEvent(name, handler);
		}
		public void RegisterEvent<T, U, V>(string name, Action<T, U, V> handler)
		{
			this.RegisterEvent(name, handler);
		}
		private Delegate GetDelegate(string name, Type type)
		{
			Dictionary<string, Delegate> dictionary;
			Delegate result;
			if (this.eventTable != null && this.eventTable.TryGetValue(type, out dictionary) && dictionary.TryGetValue(name, out result))
			{
				return result;
			}
			return null;
		}
		public void SendEvent(string name)
		{
			System.Action action = this.GetDelegate(name, typeof(System.Action)) as System.Action;
			if (action != null)
			{
				action();
			}
		}
		public void SendEvent<T>(string name, T arg1)
		{
			Action<T> action = this.GetDelegate(name, typeof(Action<T>)) as Action<T>;
			if (action != null)
			{
				action(arg1);
			}
		}
		public void SendEvent<T, U>(string name, T arg1, U arg2)
		{
			Action<T, U> action = this.GetDelegate(name, typeof(Action<T, U>)) as Action<T, U>;
			if (action != null)
			{
				action(arg1, arg2);
			}
		}
		public void SendEvent<T, U, V>(string name, T arg1, U arg2, V arg3)
		{
			Action<T, U, V> action = this.GetDelegate(name, typeof(Action<T, U, V>)) as Action<T, U, V>;
			if (action != null)
			{
				action(arg1, arg2, arg3);
			}
		}
		private void UnregisterEvent(string name, Delegate handler)
		{
			if (this.eventTable == null)
			{
				return;
			}
			Dictionary<string, Delegate> dictionary;
			Delegate source;
			if (this.eventTable.TryGetValue(handler.GetType(), out dictionary) && dictionary.TryGetValue(name, out source))
			{
				dictionary[name] = Delegate.Remove(source, handler);
			}
		}
		public void UnregisterEvent(string name, System.Action handler)
		{
			this.UnregisterEvent(name, handler);
		}
		public void UnregisterEvent<T>(string name, Action<T> handler)
		{
			this.UnregisterEvent(name, handler);
		}
		public void UnregisterEvent<T, U>(string name, Action<T, U> handler)
		{
			this.UnregisterEvent(name, handler);
		}
		public void UnregisterEvent<T, U, V>(string name, Action<T, U, V> handler)
		{
			this.UnregisterEvent(name, handler);
		}
		public void SaveResetValues()
		{
			if (this.defaultValues == null)
			{
				this.defaultValues = new List<Dictionary<string, object>>();
				this.defaultVariableValues = new Dictionary<string, object>();
				this.SaveValues();
			}
			else
			{
				this.ResetValues();
			}
		}
		private void SaveValues()
		{
			List<SharedVariable> allVariables = this.mBehaviorSource.GetAllVariables();
			for (int i = 0; i < allVariables.Count; i++)
			{
				this.defaultVariableValues.Add(allVariables[i].Name, allVariables[i].GetValue());
			}
			this.SaveValue(this.mBehaviorSource.RootTask);
		}
		private void SaveValue(Task task)
		{
			if (task == null)
			{
				return;
			}
			FieldInfo[] publicFields = TaskUtility.GetPublicFields(task.GetType());
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			int i = 0;
			while (i < publicFields.Length)
			{
				object value = publicFields[i].GetValue(task);
				if (!(value is SharedVariable))
				{
					goto IL_5A;
				}
				SharedVariable sharedVariable = value as SharedVariable;
				if (!sharedVariable.IsGlobal && !sharedVariable.IsShared)
				{
					goto IL_5A;
				}
				IL_71:
				i++;
				continue;
				IL_5A:
				dictionary.Add(publicFields[i].Name, publicFields[i].GetValue(task));
				goto IL_71;
			}
			this.defaultValues.Add(dictionary);
			if (task is ParentTask)
			{
				ParentTask parentTask = task as ParentTask;
				if (parentTask.Children != null)
				{
					for (int j = 0; j < parentTask.Children.Count; j++)
					{
						this.SaveValue(parentTask.Children[j]);
					}
				}
			}
		}
		private void ResetValues()
		{
			foreach (KeyValuePair<string, object> current in this.defaultVariableValues)
			{
				this.SetVariableValue(current.Key, current.Value);
			}
			int num = 0;
			this.ResetValue(this.mBehaviorSource.RootTask, ref num);
		}
		private void ResetValue(Task task, ref int index)
		{
			if (task == null)
			{
				return;
			}
			Dictionary<string, object> dictionary = this.defaultValues[index];
			index++;
			foreach (KeyValuePair<string, object> current in dictionary)
			{
				FieldInfo field = task.GetType().GetField(current.Key);
				if (field != null)
				{
					field.SetValue(task, current.Value);
				}
			}
			if (task is ParentTask)
			{
				ParentTask parentTask = task as ParentTask;
				if (parentTask.Children != null)
				{
					for (int i = 0; i < parentTask.Children.Count; i++)
					{
						this.ResetValue(parentTask.Children[i], ref index);
					}
				}
			}
		}
		public override string ToString()
		{
			return this.mBehaviorSource.ToString();
		}
		public static BehaviorManager CreateBehaviorManager()
		{
			if (BehaviorManager.instance == null && Application.isPlaying)
			{
				return new GameObject
				{
					name = "Behavior Manager"
				}.AddComponent<BehaviorManager>();
			}
			return null;
		}
        public virtual new int GetInstanceID()
        {
            return base.GetInstanceID();
        }
    }
}
