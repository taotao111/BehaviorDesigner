using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace BehaviorDesigner.Runtime
{
	[Serializable]
	public class BehaviorSource : IVariableSource
	{
		public string behaviorName = "Behavior";
		public string behaviorDescription = string.Empty;
		private int behaviorID = -1;
		private Task mEntryTask;
		private Task mRootTask;
		private List<Task> mDetachedTasks;
		private List<SharedVariable> mVariables;
		private Dictionary<string, int> mSharedVariableIndex;
		[NonSerialized]
		private bool mHasSerialized;
		[SerializeField]
		private TaskSerializationData mTaskData;
		[SerializeField]
		private IBehavior mOwner;
		public int BehaviorID
		{
			get
			{
				return this.behaviorID;
			}
			set
			{
				this.behaviorID = value;
			}
		}
		public Task EntryTask
		{
			get
			{
				return this.mEntryTask;
			}
			set
			{
				this.mEntryTask = value;
			}
		}
		public Task RootTask
		{
			get
			{
				return this.mRootTask;
			}
			set
			{
				this.mRootTask = value;
			}
		}
		public List<Task> DetachedTasks
		{
			get
			{
				return this.mDetachedTasks;
			}
			set
			{
				this.mDetachedTasks = value;
			}
		}
		public List<SharedVariable> Variables
		{
			get
			{
				this.CheckForSerialization(false, null);
				return this.mVariables;
			}
			set
			{
				this.mVariables = value;
				this.UpdateVariablesIndex();
			}
		}
		public bool HasSerialized
		{
			get
			{
				return this.mHasSerialized;
			}
			set
			{
				this.mHasSerialized = value;
			}
		}
		public TaskSerializationData TaskData
		{
			get
			{
				return this.mTaskData;
			}
			set
			{
				this.mTaskData = value;
			}
		}
		public IBehavior Owner
		{
			get
			{
				return this.mOwner;
			}
			set
			{
				this.mOwner = value;
			}
		}
		public BehaviorSource()
		{
		}
		public BehaviorSource(IBehavior owner)
		{
			this.Initialize(owner);
		}
		public void Initialize(IBehavior owner)
		{
			this.mOwner = owner;
		}
		public void Save(Task entryTask, Task rootTask, List<Task> detachedTasks)
		{
			this.mEntryTask = entryTask;
			this.mRootTask = rootTask;
			this.mDetachedTasks = detachedTasks;
		}
		public void Load(out Task entryTask, out Task rootTask, out List<Task> detachedTasks)
		{
			entryTask = this.mEntryTask;
			rootTask = this.mRootTask;
			detachedTasks = this.mDetachedTasks;
		}
		public bool CheckForSerialization(bool force, BehaviorSource behaviorSource = null)
		{
			bool flag = (behaviorSource == null) ? this.HasSerialized : behaviorSource.HasSerialized;
			if (!flag || force)
			{
				if (behaviorSource != null)
				{
					behaviorSource.HasSerialized = true;
				}
				else
				{
					this.HasSerialized = true;
				}
				if (this.mTaskData != null && !string.IsNullOrEmpty(this.mTaskData.JSONSerialization))
				{
					DeserializeJSON.Load(this.mTaskData, (behaviorSource != null) ? behaviorSource : this);
				}
				else
				{
					BinaryDeserialization.Load(this.mTaskData, (behaviorSource != null) ? behaviorSource : this);
				}
				return true;
			}
			return false;
		}
		public SharedVariable GetVariable(string name)
		{
			if (name == null)
			{
				return null;
			}
			if (this.mVariables != null)
			{
				if (this.mSharedVariableIndex == null || this.mSharedVariableIndex.Count != this.mVariables.Count)
				{
					this.UpdateVariablesIndex();
				}
				int index;
				if (this.mSharedVariableIndex.TryGetValue(name, out index))
				{
					return this.mVariables[index];
				}
			}
			return null;
		}
		public List<SharedVariable> GetAllVariables()
		{
			this.CheckForSerialization(false, null);
			return this.mVariables;
		}
		public void SetVariable(string name, SharedVariable sharedVariable)
		{
			if (this.mVariables == null)
			{
				this.mVariables = new List<SharedVariable>();
			}
			else
			{
				if (this.mSharedVariableIndex == null)
				{
					this.UpdateVariablesIndex();
				}
			}
			sharedVariable.Name = name;
			int index;
			if (this.mSharedVariableIndex != null && this.mSharedVariableIndex.TryGetValue(name, out index))
			{
				SharedVariable sharedVariable2 = this.mVariables[index];
				if (!sharedVariable2.GetType().Equals(typeof(SharedVariable)) && !sharedVariable2.GetType().Equals(sharedVariable.GetType()))
				{
					Debug.LogError(string.Format("Error: Unable to set SharedVariable {0} - the variable type {1} does not match the existing type {2}", name, sharedVariable2.GetType(), sharedVariable.GetType()));
				}
				else
				{
					sharedVariable2.SetValue(sharedVariable.GetValue());
				}
			}
			else
			{
				this.mVariables.Add(sharedVariable);
				this.UpdateVariablesIndex();
			}
		}
		public void UpdateVariableName(SharedVariable sharedVariable, string name)
		{
			this.CheckForSerialization(false, null);
			sharedVariable.Name = name;
			this.UpdateVariablesIndex();
		}
		public void SetAllVariables(List<SharedVariable> variables)
		{
			this.mVariables = variables;
			this.UpdateVariablesIndex();
		}
		private void UpdateVariablesIndex()
		{
			if (this.mVariables == null)
			{
				if (this.mSharedVariableIndex != null)
				{
					this.mSharedVariableIndex = null;
				}
				return;
			}
			if (this.mSharedVariableIndex == null)
			{
				this.mSharedVariableIndex = new Dictionary<string, int>(this.mVariables.Count);
			}
			else
			{
				this.mSharedVariableIndex.Clear();
			}
			for (int i = 0; i < this.mVariables.Count; i++)
			{
				if (this.mVariables[i] != null)
				{
					this.mSharedVariableIndex.Add(this.mVariables[i].Name, i);
				}
			}
		}
		public override string ToString()
		{
			if (this.mOwner == null || this.mOwner.GetObject() == null)
			{
				return this.behaviorName;
			}
			return string.Format("{0} - {1}", this.Owner.GetOwnerName(), this.behaviorName);
		}
	}
}
