using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
namespace BehaviorDesigner.Editor
{
	[Serializable]
	public class GraphDesigner : ScriptableObject
	{
		private NodeDesigner mEntryNode;
		private NodeDesigner mRootNode;
		private List<NodeDesigner> mDetachedNodes = new List<NodeDesigner>();
		[SerializeField]
		private List<NodeDesigner> mSelectedNodes = new List<NodeDesigner>();
		private NodeDesigner mHoverNode;
		private NodeConnection mActiveNodeConnection;
		[SerializeField]
		private List<NodeConnection> mSelectedNodeConnections = new List<NodeConnection>();
		[SerializeField]
		private int mNextTaskID;
		private List<int> mNodeSelectedID = new List<int>();
		[SerializeField]
		private int[] mPrevNodeSelectedID;
		public NodeDesigner RootNode
		{
			get
			{
				return this.mRootNode;
			}
		}
		public List<NodeDesigner> DetachedNodes
		{
			get
			{
				return this.mDetachedNodes;
			}
		}
		public List<NodeDesigner> SelectedNodes
		{
			get
			{
				return this.mSelectedNodes;
			}
		}
		public NodeDesigner HoverNode
		{
			get
			{
				return this.mHoverNode;
			}
			set
			{
				this.mHoverNode = value;
			}
		}
		public NodeConnection ActiveNodeConnection
		{
			get
			{
				return this.mActiveNodeConnection;
			}
			set
			{
				this.mActiveNodeConnection = value;
			}
		}
		public List<NodeConnection> SelectedNodeConnections
		{
			get
			{
				return this.mSelectedNodeConnections;
			}
		}
		public void OnEnable()
		{
			base.hideFlags = HideFlags.HideAndDontSave;
		}
		public NodeDesigner AddNode(BehaviorSource behaviorSource, Type type, Vector2 position)
		{
			Task task = Activator.CreateInstance(type, true) as Task;
			if (task == null)
			{
				EditorUtility.DisplayDialog("Unable to Add Task", string.Format("Unable to create task of type {0}. Is the class name the same as the file name?", type), "OK");
				return null;
			}
			return this.AddNode(behaviorSource, task, position);
		}
		private NodeDesigner AddNode(BehaviorSource behaviorSource, Task task, Vector2 position)
		{
			if (this.mEntryNode == null)
			{
				Task task2 = Activator.CreateInstance(TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.Tasks.EntryTask")) as Task;
				this.mEntryNode = ScriptableObject.CreateInstance<NodeDesigner>();
				this.mEntryNode.LoadNode(task2, behaviorSource, new Vector2(position.x, position.y - 120f), ref this.mNextTaskID);
				this.mEntryNode.MakeEntryDisplay();
			}
			NodeDesigner nodeDesigner = ScriptableObject.CreateInstance<NodeDesigner>();
			nodeDesigner.LoadNode(task, behaviorSource, position, ref this.mNextTaskID);
			TaskNameAttribute[] array;
			if ((array = (task.GetType().GetCustomAttributes(typeof(TaskNameAttribute), false) as TaskNameAttribute[])).Length > 0)
			{
				task.FriendlyName = array[0].Name;
			}
			if (this.mEntryNode.OutgoingNodeConnections.Count == 0)
			{
				this.mActiveNodeConnection = ScriptableObject.CreateInstance<NodeConnection>();
				this.mActiveNodeConnection.LoadConnection(this.mEntryNode, NodeConnectionType.Outgoing);
				this.ConnectNodes(behaviorSource, nodeDesigner);
			}
			else
			{
				this.mDetachedNodes.Add(nodeDesigner);
			}
			return nodeDesigner;
		}
		public NodeDesigner NodeAt(Vector2 point, Vector2 offset)
		{
			if (this.mEntryNode == null)
			{
				return null;
			}
			for (int i = 0; i < this.mSelectedNodes.Count; i++)
			{
				if (this.mSelectedNodes[i].Contains(point, offset, false))
				{
					return this.mSelectedNodes[i];
				}
			}
			NodeDesigner result;
			for (int j = this.mDetachedNodes.Count - 1; j > -1; j--)
			{
				if (this.mDetachedNodes[j] != null && (result = this.NodeChildrenAt(this.mDetachedNodes[j], point, offset)) != null)
				{
					return result;
				}
			}
			if (this.mRootNode != null && (result = this.NodeChildrenAt(this.mRootNode, point, offset)) != null)
			{
				return result;
			}
			if (this.mEntryNode.Contains(point, offset, true))
			{
				return this.mEntryNode;
			}
			return null;
		}
		private NodeDesigner NodeChildrenAt(NodeDesigner nodeDesigner, Vector2 point, Vector2 offset)
		{
			if (nodeDesigner.Contains(point, offset, true))
			{
				return nodeDesigner;
			}
			if (nodeDesigner.IsParent)
			{
				ParentTask parentTask = nodeDesigner.Task as ParentTask;
				if (!parentTask.NodeData.Collapsed && parentTask.Children != null)
				{
					for (int i = 0; i < parentTask.Children.Count; i++)
					{
						NodeDesigner result;
						if (parentTask.Children[i] != null && (result = this.NodeChildrenAt(parentTask.Children[i].NodeData.NodeDesigner as NodeDesigner, point, offset)) != null)
						{
							return result;
						}
					}
				}
			}
			return null;
		}
		public List<NodeDesigner> NodesAt(Rect rect, Vector2 offset)
		{
			List<NodeDesigner> list = new List<NodeDesigner>();
			if (this.mRootNode != null)
			{
				this.NodesChildrenAt(this.mRootNode, rect, offset, ref list);
			}
			for (int i = 0; i < this.mDetachedNodes.Count; i++)
			{
				this.NodesChildrenAt(this.mDetachedNodes[i], rect, offset, ref list);
			}
			return (list.Count <= 0) ? null : list;
		}
		private void NodesChildrenAt(NodeDesigner nodeDesigner, Rect rect, Vector2 offset, ref List<NodeDesigner> nodes)
		{
			if (nodeDesigner.Intersects(rect, offset))
			{
				nodes.Add(nodeDesigner);
			}
			if (nodeDesigner.IsParent)
			{
				ParentTask parentTask = nodeDesigner.Task as ParentTask;
				if (!parentTask.NodeData.Collapsed && parentTask.Children != null)
				{
					for (int i = 0; i < parentTask.Children.Count; i++)
					{
						if (parentTask.Children[i] != null)
						{
							this.NodesChildrenAt(parentTask.Children[i].NodeData.NodeDesigner as NodeDesigner, rect, offset, ref nodes);
						}
					}
				}
			}
		}
		public bool IsSelected(NodeDesigner nodeDesigner)
		{
			return this.mSelectedNodes.Contains(nodeDesigner);
		}
		public bool IsParentSelected(NodeDesigner nodeDesigner)
		{
			return nodeDesigner.ParentNodeDesigner != null && (this.IsSelected(nodeDesigner.ParentNodeDesigner) || this.IsParentSelected(nodeDesigner.ParentNodeDesigner));
		}
		public void Select(NodeDesigner nodeDesigner)
		{
			this.Select(nodeDesigner, true);
		}
		public void Select(NodeDesigner nodeDesigner, bool addHash)
		{
			if (this.mSelectedNodes.Count == 1)
			{
				this.IndicateReferencedTasks(this.mSelectedNodes[0].Task, false);
			}
			this.mSelectedNodes.Add(nodeDesigner);
			if (addHash)
			{
				this.mNodeSelectedID.Add(nodeDesigner.Task.ID);
			}
			nodeDesigner.Select();
			if (this.mSelectedNodes.Count == 1)
			{
				this.IndicateReferencedTasks(this.mSelectedNodes[0].Task, true);
			}
		}
		public void Deselect(NodeDesigner nodeDesigner)
		{
			this.mSelectedNodes.Remove(nodeDesigner);
			this.mNodeSelectedID.Remove(nodeDesigner.Task.ID);
			nodeDesigner.Deselect();
			this.IndicateReferencedTasks(nodeDesigner.Task, false);
		}
		public void DeselectAllExcept(NodeDesigner nodeDesigner)
		{
			for (int i = this.mSelectedNodes.Count - 1; i >= 0; i--)
			{
				if (!this.mSelectedNodes[i].Equals(nodeDesigner))
				{
					this.mSelectedNodes[i].Deselect();
					this.mSelectedNodes.RemoveAt(i);
					this.mNodeSelectedID.RemoveAt(i);
				}
			}
			this.IndicateReferencedTasks(nodeDesigner.Task, false);
		}
		public void ClearNodeSelection()
		{
			if (this.mSelectedNodes.Count == 1)
			{
				this.IndicateReferencedTasks(this.mSelectedNodes[0].Task, false);
			}
			for (int i = 0; i < this.mSelectedNodes.Count; i++)
			{
				this.mSelectedNodes[i].Deselect();
			}
			this.mSelectedNodes.Clear();
			this.mNodeSelectedID.Clear();
		}
		public void DeselectWithParent(NodeDesigner nodeDesigner)
		{
			for (int i = this.mSelectedNodes.Count - 1; i >= 0; i--)
			{
				if (this.mSelectedNodes[i].HasParent(nodeDesigner))
				{
					this.Deselect(this.mSelectedNodes[i]);
				}
			}
		}
		public bool ReplaceSelectedNode(BehaviorSource behaviorSource, Type taskType)
		{
			BehaviorUndo.RegisterUndo("Replace", behaviorSource.Owner.GetObject());
			Vector2 absolutePosition = this.SelectedNodes[0].GetAbsolutePosition();
			NodeDesigner parentNodeDesigner = this.SelectedNodes[0].ParentNodeDesigner;
			List<Task> list = (!this.SelectedNodes[0].IsParent) ? null : (this.SelectedNodes[0].Task as ParentTask).Children;
			this.RemoveNode(this.SelectedNodes[0]);
			this.mSelectedNodes.Clear();
			TaskReferences.CheckReferences(behaviorSource);
			NodeDesigner nodeDesigner = this.AddNode(behaviorSource, taskType, absolutePosition);
			if (nodeDesigner == null)
			{
				return false;
			}
			if (parentNodeDesigner != null)
			{
				this.ActiveNodeConnection = parentNodeDesigner.CreateNodeConnection(false);
				this.ConnectNodes(behaviorSource, nodeDesigner);
			}
			if (nodeDesigner.IsParent && list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					this.ActiveNodeConnection = nodeDesigner.CreateNodeConnection(false);
					this.ConnectNodes(behaviorSource, list[i].NodeData.NodeDesigner as NodeDesigner);
					if (i >= (nodeDesigner.Task as ParentTask).MaxChildren())
					{
						break;
					}
				}
			}
			this.Select(nodeDesigner);
			return true;
		}
		public void Hover(NodeDesigner nodeDesigner)
		{
			if (!nodeDesigner.ShowHoverBar)
			{
				nodeDesigner.ShowHoverBar = true;
				this.HoverNode = nodeDesigner;
			}
		}
		public void ClearHover()
		{
			if (this.HoverNode)
			{
				this.HoverNode.ShowHoverBar = false;
				this.HoverNode = null;
			}
		}
		private void IndicateReferencedTasks(Task task, bool indicate)
		{
			List<Task> referencedTasks = TaskInspector.GetReferencedTasks(task);
			if (referencedTasks != null && referencedTasks.Count > 0)
			{
				for (int i = 0; i < referencedTasks.Count; i++)
				{
					if (referencedTasks[i] != null && referencedTasks[i].NodeData != null)
					{
						NodeDesigner nodeDesigner = referencedTasks[i].NodeData.NodeDesigner as NodeDesigner;
						if (nodeDesigner != null)
						{
							nodeDesigner.ShowReferenceIcon = indicate;
						}
					}
				}
			}
		}
		public bool DragSelectedNodes(Vector2 delta, bool dragChildren)
		{
			if (this.mSelectedNodes.Count == 0)
			{
				return false;
			}
			bool flag = this.mSelectedNodes.Count == 1;
			for (int i = 0; i < this.mSelectedNodes.Count; i++)
			{
				this.DragNode(this.mSelectedNodes[i], delta, dragChildren);
			}
			if (flag && dragChildren && this.mSelectedNodes[0].IsEntryDisplay && this.mRootNode != null)
			{
				this.DragNode(this.mRootNode, delta, dragChildren);
			}
			return true;
		}
		private void DragNode(NodeDesigner nodeDesigner, Vector2 delta, bool dragChildren)
		{
			if (this.IsParentSelected(nodeDesigner) && dragChildren)
			{
				return;
			}
			nodeDesigner.ChangeOffset(delta);
			if (nodeDesigner.ParentNodeDesigner != null)
			{
				int num = nodeDesigner.ParentNodeDesigner.ChildIndexForTask(nodeDesigner.Task);
				if (num != -1)
				{
					int index = num - 1;
					bool flag = false;
					NodeDesigner nodeDesigner2 = nodeDesigner.ParentNodeDesigner.NodeDesignerForChildIndex(index);
					if (nodeDesigner2 != null && nodeDesigner.Task.NodeData.Offset.x < nodeDesigner2.Task.NodeData.Offset.x)
					{
						nodeDesigner.ParentNodeDesigner.MoveChildNode(num, true);
						flag = true;
					}
					if (!flag)
					{
						index = num + 1;
						nodeDesigner2 = nodeDesigner.ParentNodeDesigner.NodeDesignerForChildIndex(index);
						if (nodeDesigner2 != null && nodeDesigner.Task.NodeData.Offset.x > nodeDesigner2.Task.NodeData.Offset.x)
						{
							nodeDesigner.ParentNodeDesigner.MoveChildNode(num, false);
						}
					}
				}
			}
			if (nodeDesigner.IsParent && !dragChildren)
			{
				ParentTask parentTask = nodeDesigner.Task as ParentTask;
				if (parentTask.Children != null)
				{
					for (int i = 0; i < parentTask.Children.Count; i++)
					{
						NodeDesigner nodeDesigner3 = parentTask.Children[i].NodeData.NodeDesigner as NodeDesigner;
						nodeDesigner3.ChangeOffset(-delta);
					}
				}
			}
			this.MarkNodeDirty(nodeDesigner);
		}
		public bool DrawNodes(Vector2 mousePosition, Vector2 offset)
		{
			if (this.mEntryNode == null)
			{
				return false;
			}
			this.mEntryNode.DrawNodeConnection(offset, false);
			if (this.mRootNode != null)
			{
				this.DrawNodeConnectionChildren(this.mRootNode, offset, this.mRootNode.Task.NodeData.Disabled);
			}
			for (int i = 0; i < this.mDetachedNodes.Count; i++)
			{
				this.DrawNodeConnectionChildren(this.mDetachedNodes[i], offset, this.mDetachedNodes[i].Task.NodeData.Disabled);
			}
			for (int j = 0; j < this.mSelectedNodeConnections.Count; j++)
			{
				this.mSelectedNodeConnections[j].DrawConnection(offset, this.mSelectedNodeConnections[j].OriginatingNodeDesigner.IsDisabled());
			}
			if (mousePosition != new Vector2(-1f, -1f) && this.mActiveNodeConnection != null)
			{
				this.mActiveNodeConnection.HorizontalHeight = (this.mActiveNodeConnection.OriginatingNodeDesigner.GetConnectionPosition(offset, this.mActiveNodeConnection.NodeConnectionType).y + mousePosition.y) / 2f;
				this.mActiveNodeConnection.DrawConnection(this.mActiveNodeConnection.OriginatingNodeDesigner.GetConnectionPosition(offset, this.mActiveNodeConnection.NodeConnectionType), mousePosition, this.mActiveNodeConnection.NodeConnectionType == NodeConnectionType.Outgoing && this.mActiveNodeConnection.OriginatingNodeDesigner.IsDisabled());
			}
			this.mEntryNode.DrawNode(offset, false, false);
			bool result = false;
			if (this.mRootNode != null && this.DrawNodeChildren(this.mRootNode, offset, this.mRootNode.Task.NodeData.Disabled))
			{
				result = true;
			}
			for (int k = 0; k < this.mDetachedNodes.Count; k++)
			{
				if (this.DrawNodeChildren(this.mDetachedNodes[k], offset, this.mDetachedNodes[k].Task.NodeData.Disabled))
				{
					result = true;
				}
			}
			for (int l = 0; l < this.mSelectedNodes.Count; l++)
			{
				if (this.mSelectedNodes[l].DrawNode(offset, true, this.mSelectedNodes[l].IsDisabled()))
				{
					result = true;
				}
			}
			if (this.mRootNode != null)
			{
				this.DrawNodeCommentChildren(this.mRootNode, offset);
			}
			for (int m = 0; m < this.mDetachedNodes.Count; m++)
			{
				this.DrawNodeCommentChildren(this.mDetachedNodes[m], offset);
			}
			return result;
		}
		private bool DrawNodeChildren(NodeDesigner nodeDesigner, Vector2 offset, bool disabledNode)
		{
			if (nodeDesigner == null)
			{
				return false;
			}
			bool result = false;
			if (nodeDesigner.DrawNode(offset, false, disabledNode))
			{
				result = true;
			}
			if (nodeDesigner.IsParent)
			{
				ParentTask parentTask = nodeDesigner.Task as ParentTask;
				if (!parentTask.NodeData.Collapsed && parentTask.Children != null)
				{
					for (int i = parentTask.Children.Count - 1; i > -1; i--)
					{
						if (parentTask.Children[i] != null && this.DrawNodeChildren(parentTask.Children[i].NodeData.NodeDesigner as NodeDesigner, offset, parentTask.NodeData.Disabled || disabledNode))
						{
							result = true;
						}
					}
				}
			}
			return result;
		}
		private void DrawNodeConnectionChildren(NodeDesigner nodeDesigner, Vector2 offset, bool disabledNode)
		{
			if (nodeDesigner == null)
			{
				return;
			}
			if (!nodeDesigner.Task.NodeData.Collapsed)
			{
				nodeDesigner.DrawNodeConnection(offset, nodeDesigner.Task.NodeData.Disabled || disabledNode);
				if (nodeDesigner.IsParent)
				{
					ParentTask parentTask = nodeDesigner.Task as ParentTask;
					if (parentTask.Children != null)
					{
						for (int i = 0; i < parentTask.Children.Count; i++)
						{
							if (parentTask.Children[i] != null)
							{
								this.DrawNodeConnectionChildren(parentTask.Children[i].NodeData.NodeDesigner as NodeDesigner, offset, parentTask.NodeData.Disabled || disabledNode);
							}
						}
					}
				}
			}
		}
		private void DrawNodeCommentChildren(NodeDesigner nodeDesigner, Vector2 offset)
		{
			if (nodeDesigner == null)
			{
				return;
			}
			nodeDesigner.DrawNodeComment(offset);
			if (nodeDesigner.IsParent)
			{
				ParentTask parentTask = nodeDesigner.Task as ParentTask;
				if (!parentTask.NodeData.Collapsed && parentTask.Children != null)
				{
					for (int i = 0; i < parentTask.Children.Count; i++)
					{
						if (parentTask.Children[i] != null)
						{
							this.DrawNodeCommentChildren(parentTask.Children[i].NodeData.NodeDesigner as NodeDesigner, offset);
						}
					}
				}
			}
		}
		private void RemoveNode(NodeDesigner nodeDesigner)
		{
			if (nodeDesigner.IsEntryDisplay)
			{
				return;
			}
			if (nodeDesigner.IsParent)
			{
				for (int i = 0; i < nodeDesigner.OutgoingNodeConnections.Count; i++)
				{
					NodeDesigner destinationNodeDesigner = nodeDesigner.OutgoingNodeConnections[i].DestinationNodeDesigner;
					this.mDetachedNodes.Add(destinationNodeDesigner);
					destinationNodeDesigner.Task.NodeData.Offset = destinationNodeDesigner.GetAbsolutePosition();
					destinationNodeDesigner.ParentNodeDesigner = null;
				}
			}
			if (nodeDesigner.ParentNodeDesigner != null)
			{
				nodeDesigner.ParentNodeDesigner.RemoveChildNode(nodeDesigner);
			}
			if (this.mRootNode != null && this.mRootNode.Equals(nodeDesigner))
			{
				this.mEntryNode.RemoveChildNode(nodeDesigner);
				this.mRootNode = null;
			}
			if (this.mRootNode != null)
			{
				this.RemoveReferencedTasks(this.mRootNode, nodeDesigner.Task);
			}
			if (this.mDetachedNodes != null)
			{
				for (int j = 0; j < this.mDetachedNodes.Count; j++)
				{
					this.RemoveReferencedTasks(this.mDetachedNodes[j], nodeDesigner.Task);
				}
			}
			this.mDetachedNodes.Remove(nodeDesigner);
			BehaviorUndo.DestroyObject(nodeDesigner, false);
		}
		private void RemoveReferencedTasks(NodeDesigner nodeDesigner, Task task)
		{
			bool flag = false;
			bool flag2 = false;
			FieldInfo[] allFields = TaskUtility.GetAllFields(nodeDesigner.Task.GetType());
			for (int i = 0; i < allFields.Length; i++)
			{
				if ((!allFields[i].IsPrivate && !allFields[i].IsFamily) || BehaviorDesignerUtility.HasAttribute(allFields[i], typeof(SerializeField)))
				{
					if (typeof(IList).IsAssignableFrom(allFields[i].FieldType))
					{
						if (typeof(Task).IsAssignableFrom(allFields[i].FieldType.GetElementType()) || (allFields[i].FieldType.IsGenericType && typeof(Task).IsAssignableFrom(allFields[i].FieldType.GetGenericArguments()[0])))
						{
							Task[] array = allFields[i].GetValue(nodeDesigner.Task) as Task[];
							if (array != null)
							{
								for (int j = array.Length - 1; j > -1; j--)
								{
									if (nodeDesigner.Task.Equals(task) || array[i].Equals(task))
									{
										TaskInspector.ReferenceTasks(nodeDesigner.Task, task, allFields[i], ref flag, ref flag2, false, false);
									}
								}
							}
						}
					}
					else if (typeof(Task).IsAssignableFrom(allFields[i].FieldType))
					{
						Task task2 = allFields[i].GetValue(nodeDesigner.Task) as Task;
						if (task2 != null && (nodeDesigner.Task.Equals(task) || task2.Equals(task)))
						{
							TaskInspector.ReferenceTasks(nodeDesigner.Task, task, allFields[i], ref flag, ref flag2, false, false);
						}
					}
				}
			}
			if (nodeDesigner.IsParent)
			{
				ParentTask parentTask = nodeDesigner.Task as ParentTask;
				if (parentTask.Children != null)
				{
					for (int k = 0; k < parentTask.Children.Count; k++)
					{
						if (parentTask.Children[k] != null)
						{
							this.RemoveReferencedTasks(parentTask.Children[k].NodeData.NodeDesigner as NodeDesigner, task);
						}
					}
				}
			}
		}
		public bool NodeCanOriginateConnection(NodeDesigner nodeDesigner, NodeConnection connection)
		{
			return !nodeDesigner.IsEntryDisplay || (nodeDesigner.IsEntryDisplay && connection.NodeConnectionType == NodeConnectionType.Outgoing);
		}
		public bool NodeCanAcceptConnection(NodeDesigner nodeDesigner, NodeConnection connection)
		{
			if ((!nodeDesigner.IsEntryDisplay || connection.NodeConnectionType != NodeConnectionType.Incoming) && (nodeDesigner.IsEntryDisplay || (!nodeDesigner.IsParent && (nodeDesigner.IsParent || connection.NodeConnectionType != NodeConnectionType.Outgoing))))
			{
				return false;
			}
			if (nodeDesigner.IsEntryDisplay || connection.OriginatingNodeDesigner.IsEntryDisplay)
			{
				return true;
			}
			HashSet<NodeDesigner> hashSet = new HashSet<NodeDesigner>();
			NodeDesigner nodeDesigner2 = (connection.NodeConnectionType != NodeConnectionType.Outgoing) ? connection.OriginatingNodeDesigner : nodeDesigner;
			NodeDesigner item = (connection.NodeConnectionType != NodeConnectionType.Outgoing) ? nodeDesigner : connection.OriginatingNodeDesigner;
			return !this.CycleExists(nodeDesigner2, ref hashSet) && !hashSet.Contains(item);
		}
		private bool CycleExists(NodeDesigner nodeDesigner, ref HashSet<NodeDesigner> set)
		{
			if (set.Contains(nodeDesigner))
			{
				return true;
			}
			set.Add(nodeDesigner);
			if (nodeDesigner.IsParent)
			{
				ParentTask parentTask = nodeDesigner.Task as ParentTask;
				if (parentTask.Children != null)
				{
					for (int i = 0; i < parentTask.Children.Count; i++)
					{
						if (this.CycleExists(parentTask.Children[i].NodeData.NodeDesigner as NodeDesigner, ref set))
						{
							return true;
						}
					}
				}
			}
			return false;
		}
		public void ConnectNodes(BehaviorSource behaviorSource, NodeDesigner nodeDesigner)
		{
			NodeConnection nodeConnection = this.mActiveNodeConnection;
			this.mActiveNodeConnection = null;
			if (nodeConnection != null && !nodeConnection.OriginatingNodeDesigner.Equals(nodeDesigner))
			{
				NodeDesigner originatingNodeDesigner = nodeConnection.OriginatingNodeDesigner;
				if (nodeConnection.NodeConnectionType == NodeConnectionType.Outgoing)
				{
					this.RemoveParentConnection(nodeDesigner);
					this.CheckForLastConnectionRemoval(originatingNodeDesigner);
					originatingNodeDesigner.AddChildNode(nodeDesigner, nodeConnection, true, false);
				}
				else
				{
					this.RemoveParentConnection(originatingNodeDesigner);
					this.CheckForLastConnectionRemoval(nodeDesigner);
					nodeDesigner.AddChildNode(originatingNodeDesigner, nodeConnection, true, false);
				}
				if (nodeConnection.OriginatingNodeDesigner.IsEntryDisplay)
				{
					this.mRootNode = nodeConnection.DestinationNodeDesigner;
				}
				this.mDetachedNodes.Remove(nodeConnection.DestinationNodeDesigner);
			}
		}
		private void RemoveParentConnection(NodeDesigner nodeDesigner)
		{
			if (nodeDesigner.ParentNodeDesigner != null)
			{
				NodeDesigner parentNodeDesigner = nodeDesigner.ParentNodeDesigner;
				NodeConnection nodeConnection = null;
				for (int i = 0; i < parentNodeDesigner.OutgoingNodeConnections.Count; i++)
				{
					if (parentNodeDesigner.OutgoingNodeConnections[i].DestinationNodeDesigner.Equals(nodeDesigner))
					{
						nodeConnection = parentNodeDesigner.OutgoingNodeConnections[i];
						break;
					}
				}
				if (nodeConnection != null)
				{
					this.RemoveConnection(nodeConnection);
				}
			}
		}
		private void CheckForLastConnectionRemoval(NodeDesigner nodeDesigner)
		{
			if (nodeDesigner.IsEntryDisplay)
			{
				if (nodeDesigner.OutgoingNodeConnections.Count == 1)
				{
					this.RemoveConnection(nodeDesigner.OutgoingNodeConnections[0]);
				}
			}
			else
			{
				ParentTask parentTask = nodeDesigner.Task as ParentTask;
				if (parentTask.Children != null && parentTask.Children.Count + 1 > parentTask.MaxChildren())
				{
					NodeConnection nodeConnection = null;
					for (int i = 0; i < nodeDesigner.OutgoingNodeConnections.Count; i++)
					{
						if (nodeDesigner.OutgoingNodeConnections[i].DestinationNodeDesigner.Equals(parentTask.Children[parentTask.Children.Count - 1].NodeData.NodeDesigner as NodeDesigner))
						{
							nodeConnection = nodeDesigner.OutgoingNodeConnections[i];
							break;
						}
					}
					if (nodeConnection != null)
					{
						this.RemoveConnection(nodeConnection);
					}
				}
			}
		}
		public void NodeConnectionsAt(Vector2 point, Vector2 offset, ref List<NodeConnection> nodeConnections)
		{
			if (this.mEntryNode == null)
			{
				return;
			}
			this.NodeChildrenConnectionsAt(this.mEntryNode, point, offset, ref nodeConnections);
			if (this.mRootNode != null)
			{
				this.NodeChildrenConnectionsAt(this.mRootNode, point, offset, ref nodeConnections);
			}
			for (int i = 0; i < this.mDetachedNodes.Count; i++)
			{
				this.NodeChildrenConnectionsAt(this.mDetachedNodes[i], point, offset, ref nodeConnections);
			}
		}
		private void NodeChildrenConnectionsAt(NodeDesigner nodeDesigner, Vector2 point, Vector2 offset, ref List<NodeConnection> nodeConnections)
		{
			if (nodeDesigner.Task.NodeData.Collapsed)
			{
				return;
			}
			nodeDesigner.ConnectionContains(point, offset, ref nodeConnections);
			if (nodeDesigner.IsParent)
			{
				ParentTask parentTask = nodeDesigner.Task as ParentTask;
				if (parentTask != null && parentTask.Children != null)
				{
					for (int i = 0; i < parentTask.Children.Count; i++)
					{
						if (parentTask.Children[i] != null)
						{
							this.NodeChildrenConnectionsAt(parentTask.Children[i].NodeData.NodeDesigner as NodeDesigner, point, offset, ref nodeConnections);
						}
					}
				}
			}
		}
		public void RemoveConnection(NodeConnection nodeConnection)
		{
			nodeConnection.DestinationNodeDesigner.Task.NodeData.Offset = nodeConnection.DestinationNodeDesigner.GetAbsolutePosition();
			this.mDetachedNodes.Add(nodeConnection.DestinationNodeDesigner);
			nodeConnection.OriginatingNodeDesigner.RemoveChildNode(nodeConnection.DestinationNodeDesigner);
			if (nodeConnection.OriginatingNodeDesigner.IsEntryDisplay)
			{
				this.mRootNode = null;
			}
		}
		public bool IsSelected(NodeConnection nodeConnection)
		{
			for (int i = 0; i < this.mSelectedNodeConnections.Count; i++)
			{
				if (this.mSelectedNodeConnections[i].Equals(nodeConnection))
				{
					return true;
				}
			}
			return false;
		}
		public void Select(NodeConnection nodeConnection)
		{
			this.mSelectedNodeConnections.Add(nodeConnection);
			nodeConnection.select();
		}
		public void Deselect(NodeConnection nodeConnection)
		{
			this.mSelectedNodeConnections.Remove(nodeConnection);
			nodeConnection.deselect();
		}
		public void ClearConnectionSelection()
		{
			for (int i = 0; i < this.mSelectedNodeConnections.Count; i++)
			{
				this.mSelectedNodeConnections[i].deselect();
			}
			this.mSelectedNodeConnections.Clear();
		}
		public void GraphDirty()
		{
			if (this.mEntryNode == null)
			{
				return;
			}
			this.mEntryNode.MarkDirty();
			if (this.mRootNode != null)
			{
				this.MarkNodeDirty(this.mRootNode);
			}
			for (int i = this.mDetachedNodes.Count - 1; i > -1; i--)
			{
				this.MarkNodeDirty(this.mDetachedNodes[i]);
			}
		}
		private void MarkNodeDirty(NodeDesigner nodeDesigner)
		{
			nodeDesigner.MarkDirty();
			if (nodeDesigner.IsEntryDisplay)
			{
				if (nodeDesigner.OutgoingNodeConnections.Count > 0 && nodeDesigner.OutgoingNodeConnections[0].DestinationNodeDesigner != null)
				{
					this.MarkNodeDirty(nodeDesigner.OutgoingNodeConnections[0].DestinationNodeDesigner);
				}
			}
			else if (nodeDesigner.IsParent)
			{
				ParentTask parentTask = nodeDesigner.Task as ParentTask;
				if (parentTask.Children != null)
				{
					for (int i = 0; i < parentTask.Children.Count; i++)
					{
						if (parentTask.Children[i] != null)
						{
							this.MarkNodeDirty(parentTask.Children[i].NodeData.NodeDesigner as NodeDesigner);
						}
					}
				}
			}
		}
		public List<BehaviorSource> FindReferencedBehaviors()
		{
			List<BehaviorSource> result = new List<BehaviorSource>();
			if (this.mRootNode != null)
			{
				this.FindReferencedBehaviors(this.mRootNode, ref result);
			}
			for (int i = 0; i < this.mDetachedNodes.Count; i++)
			{
				this.FindReferencedBehaviors(this.mDetachedNodes[i], ref result);
			}
			return result;
		}
		public void FindReferencedBehaviors(NodeDesigner nodeDesigner, ref List<BehaviorSource> behaviors)
		{
			FieldInfo[] publicFields = TaskUtility.GetPublicFields(nodeDesigner.Task.GetType());
			for (int i = 0; i < publicFields.Length; i++)
			{
				Type fieldType = publicFields[i].FieldType;
				if (typeof(IList).IsAssignableFrom(fieldType))
				{
					Type type = fieldType;
					if (fieldType.IsGenericType)
					{
						while (!type.IsGenericType)
						{
							type = type.BaseType;
						}
						type = fieldType.GetGenericArguments()[0];
					}
					else
					{
						type = fieldType.GetElementType();
					}
					if (type != null)
					{
						if (typeof(ExternalBehavior).IsAssignableFrom(type) || typeof(Behavior).IsAssignableFrom(type))
						{
							IList list = publicFields[i].GetValue(nodeDesigner.Task) as IList;
							if (list != null)
							{
								for (int j = 0; j < list.Count; j++)
								{
									if (list[j] != null)
									{
										BehaviorSource behaviorSource;
										if (list[j] is ExternalBehavior)
										{
											behaviorSource = (list[j] as ExternalBehavior).BehaviorSource;
											if (behaviorSource.Owner == null)
											{
												behaviorSource.Owner = (list[j] as ExternalBehavior);
											}
										}
										else
										{
											behaviorSource = (list[j] as Behavior).GetBehaviorSource();
											if (behaviorSource.Owner == null)
											{
												behaviorSource.Owner = (list[j] as Behavior);
											}
										}
										behaviors.Add(behaviorSource);
									}
								}
							}
						}
						else if (typeof(Behavior).IsAssignableFrom(type))
						{
						}
					}
				}
				else if (typeof(ExternalBehavior).IsAssignableFrom(fieldType) || typeof(Behavior).IsAssignableFrom(fieldType))
				{
					object value = publicFields[i].GetValue(nodeDesigner.Task);
					if (value != null)
					{
						BehaviorSource behaviorSource2;
						if (value is ExternalBehavior)
						{
							behaviorSource2 = (value as ExternalBehavior).BehaviorSource;
							if (behaviorSource2.Owner == null)
							{
								behaviorSource2.Owner = (value as ExternalBehavior);
							}
							behaviors.Add(behaviorSource2);
						}
						else
						{
							behaviorSource2 = (value as Behavior).GetBehaviorSource();
							if (behaviorSource2.Owner == null)
							{
								behaviorSource2.Owner = (value as Behavior);
							}
						}
						behaviors.Add(behaviorSource2);
					}
				}
			}
			if (nodeDesigner.IsParent)
			{
				ParentTask parentTask = nodeDesigner.Task as ParentTask;
				if (parentTask.Children != null)
				{
					for (int k = 0; k < parentTask.Children.Count; k++)
					{
						if (parentTask.Children[k] != null)
						{
							this.FindReferencedBehaviors(parentTask.Children[k].NodeData.NodeDesigner as NodeDesigner, ref behaviors);
						}
					}
				}
			}
		}
		public void SelectAll()
		{
			for (int i = this.mSelectedNodes.Count - 1; i > -1; i--)
			{
				this.Deselect(this.mSelectedNodes[i]);
			}
			if (this.mRootNode != null)
			{
				this.SelectAll(this.mRootNode);
			}
			for (int j = this.mDetachedNodes.Count - 1; j > -1; j--)
			{
				this.SelectAll(this.mDetachedNodes[j]);
			}
		}
		private void SelectAll(NodeDesigner nodeDesigner)
		{
			this.Select(nodeDesigner);
			if (nodeDesigner.Task.GetType().IsSubclassOf(typeof(ParentTask)))
			{
				ParentTask parentTask = nodeDesigner.Task as ParentTask;
				if (parentTask.Children != null)
				{
					for (int i = 0; i < parentTask.Children.Count; i++)
					{
						this.SelectAll(parentTask.Children[i].NodeData.NodeDesigner as NodeDesigner);
					}
				}
			}
		}
		public void IdentifyNode(NodeDesigner nodeDesigner)
		{
			nodeDesigner.IdentifyNode();
		}
		public List<TaskSerializer> Copy(Vector2 graphOffset, float graphZoom)
		{
			List<TaskSerializer> list = new List<TaskSerializer>();
			for (int i = 0; i < this.mSelectedNodes.Count; i++)
			{
				TaskSerializer taskSerializer;
				if ((taskSerializer = TaskCopier.CopySerialized(this.mSelectedNodes[i].Task)) != null)
				{
					if (this.mSelectedNodes[i].IsParent)
					{
						ParentTask parentTask = this.mSelectedNodes[i].Task as ParentTask;
						if (parentTask.Children != null)
						{
							List<int> list2 = new List<int>();
							for (int j = 0; j < parentTask.Children.Count; j++)
							{
								int item;
								if ((item = this.mSelectedNodes.IndexOf(parentTask.Children[j].NodeData.NodeDesigner as NodeDesigner)) != -1)
								{
									list2.Add(item);
								}
							}
							taskSerializer.childrenIndex = list2;
						}
					}
					taskSerializer.offset = (taskSerializer.offset + graphOffset) * graphZoom;
					list.Add(taskSerializer);
				}
			}
			return (list.Count <= 0) ? null : list;
		}
		public bool Paste(BehaviorSource behaviorSource, List<TaskSerializer> copiedTasks, Vector2 graphOffset, float graphZoom)
		{
			if (copiedTasks == null || copiedTasks.Count == 0)
			{
				return false;
			}
			this.ClearNodeSelection();
			this.ClearConnectionSelection();
			this.RemapIDs();
			List<NodeDesigner> list = new List<NodeDesigner>();
			for (int i = 0; i < copiedTasks.Count; i++)
			{
				TaskSerializer taskSerializer = copiedTasks[i];
				Task task = TaskCopier.PasteTask(behaviorSource, taskSerializer);
				NodeDesigner nodeDesigner = ScriptableObject.CreateInstance<NodeDesigner>();
				nodeDesigner.LoadTask(task, (behaviorSource.Owner == null) ? null : (behaviorSource.Owner.GetObject() as Behavior), ref this.mNextTaskID);
				nodeDesigner.Task.NodeData.Offset = taskSerializer.offset / graphZoom - graphOffset;
				list.Add(nodeDesigner);
				this.mDetachedNodes.Add(nodeDesigner);
				this.Select(nodeDesigner);
			}
			for (int j = 0; j < copiedTasks.Count; j++)
			{
				TaskSerializer taskSerializer2 = copiedTasks[j];
				if (taskSerializer2.childrenIndex != null)
				{
					for (int k = 0; k < taskSerializer2.childrenIndex.Count; k++)
					{
						NodeDesigner nodeDesigner2 = list[j];
						NodeConnection nodeConnection = ScriptableObject.CreateInstance<NodeConnection>();
						nodeConnection.LoadConnection(nodeDesigner2, NodeConnectionType.Outgoing);
						nodeDesigner2.AddChildNode(list[taskSerializer2.childrenIndex[k]], nodeConnection, true, false);
						this.mDetachedNodes.Remove(list[taskSerializer2.childrenIndex[k]]);
					}
				}
			}
			this.Save(behaviorSource);
			return true;
		}
		public bool Delete(BehaviorSource behaviorSource)
		{
			bool flag = false;
			if (this.mSelectedNodeConnections != null)
			{
				for (int i = 0; i < this.mSelectedNodeConnections.Count; i++)
				{
					this.RemoveConnection(this.mSelectedNodeConnections[i]);
				}
				this.mSelectedNodeConnections.Clear();
				flag = true;
			}
			if (this.mSelectedNodes != null)
			{
				for (int j = 0; j < this.mSelectedNodes.Count; j++)
				{
					this.RemoveNode(this.mSelectedNodes[j]);
				}
				this.mSelectedNodes.Clear();
				flag = true;
			}
			if (flag)
			{
				BehaviorUndo.RegisterUndo("Delete", behaviorSource.Owner.GetObject());
				TaskReferences.CheckReferences(behaviorSource);
				this.Save(behaviorSource);
			}
			return flag;
		}
		public bool RemoveSharedVariableReferences(SharedVariable sharedVariable)
		{
			if (this.mEntryNode == null)
			{
				return false;
			}
			bool result = false;
			if (this.mRootNode != null && this.RemoveSharedVariableReference(this.mRootNode, sharedVariable))
			{
				result = true;
			}
			if (this.mDetachedNodes != null)
			{
				for (int i = 0; i < this.mDetachedNodes.Count; i++)
				{
					if (this.RemoveSharedVariableReference(this.mDetachedNodes[i], sharedVariable))
					{
						result = true;
					}
				}
			}
			return result;
		}
		private bool RemoveSharedVariableReference(NodeDesigner nodeDesigner, SharedVariable sharedVariable)
		{
			bool result = false;
			FieldInfo[] allFields = TaskUtility.GetAllFields(nodeDesigner.Task.GetType());
			for (int i = 0; i < allFields.Length; i++)
			{
				if (typeof(SharedVariable).IsAssignableFrom(allFields[i].FieldType))
				{
					SharedVariable sharedVariable2 = allFields[i].GetValue(nodeDesigner.Task) as SharedVariable;
					if (sharedVariable2 != null && !string.IsNullOrEmpty(sharedVariable2.Name) && sharedVariable2.IsGlobal == sharedVariable.IsGlobal && sharedVariable2.Name.Equals(sharedVariable.Name))
					{
						if (!allFields[i].FieldType.IsAbstract)
						{
							sharedVariable2 = (Activator.CreateInstance(allFields[i].FieldType) as SharedVariable);
							sharedVariable2.IsShared = true;
							allFields[i].SetValue(nodeDesigner.Task, sharedVariable2);
						}
						result = true;
					}
				}
			}
			if (nodeDesigner.IsParent)
			{
				ParentTask parentTask = nodeDesigner.Task as ParentTask;
				if (parentTask.Children != null)
				{
					for (int j = 0; j < parentTask.Children.Count; j++)
					{
						if (parentTask.Children[j] != null && this.RemoveSharedVariableReference(parentTask.Children[j].NodeData.NodeDesigner as NodeDesigner, sharedVariable))
						{
							result = true;
						}
					}
				}
			}
			return result;
		}
		private void RemapIDs()
		{
			if (this.mEntryNode == null)
			{
				return;
			}
			this.mNextTaskID = 0;
			this.mEntryNode.SetID(ref this.mNextTaskID);
			if (this.mRootNode != null)
			{
				this.mRootNode.SetID(ref this.mNextTaskID);
			}
			for (int i = 0; i < this.mDetachedNodes.Count; i++)
			{
				this.mDetachedNodes[i].SetID(ref this.mNextTaskID);
			}
			this.mNodeSelectedID.Clear();
			for (int j = 0; j < this.mSelectedNodes.Count; j++)
			{
				this.mNodeSelectedID.Add(this.mSelectedNodes[j].Task.ID);
			}
		}
		public Rect GraphSize()
		{
			if (this.mEntryNode == null)
			{
				return default(Rect);
			}
			Rect result = default(Rect);
			result.xMin = 3.40282347E+38f;
			result.xMax = -3.40282347E+38f;
			result.yMin = 3.40282347E+38f;
			result.yMax = -3.40282347E+38f;
			this.GetNodeMinMax(this.mEntryNode, ref result);
			if (this.mRootNode != null)
			{
				this.GetNodeMinMax(this.mRootNode, ref result);
			}
			for (int i = 0; i < this.mDetachedNodes.Count; i++)
			{
				this.GetNodeMinMax(this.mDetachedNodes[i], ref result);
			}
			return result;
		}
		private void GetNodeMinMax(NodeDesigner nodeDesigner, ref Rect minMaxRect)
		{
			Rect rect = nodeDesigner.Rectangle(Vector2.zero, true, true);
			if (rect.xMin < minMaxRect.xMin)
			{
				minMaxRect.xMin = rect.xMin;
			}
			if (rect.yMin < minMaxRect.yMin)
			{
				minMaxRect.yMin = rect.yMin;
			}
			if (rect.xMax > minMaxRect.xMax)
			{
				minMaxRect.xMax = rect.xMax;
			}
			if (rect.yMax > minMaxRect.yMax)
			{
				minMaxRect.yMax = rect.yMax;
			}
			if (nodeDesigner.IsParent)
			{
				ParentTask parentTask = nodeDesigner.Task as ParentTask;
				if (parentTask.Children != null)
				{
					for (int i = 0; i < parentTask.Children.Count; i++)
					{
						this.GetNodeMinMax(parentTask.Children[i].NodeData.NodeDesigner as NodeDesigner, ref minMaxRect);
					}
				}
			}
		}
		public void Save(BehaviorSource behaviorSource)
		{
			if (object.ReferenceEquals(behaviorSource.Owner.GetObject(), null))
			{
				return;
			}
			this.RemapIDs();
			List<Task> list = new List<Task>();
			for (int i = 0; i < this.mDetachedNodes.Count; i++)
			{
				list.Add(this.mDetachedNodes[i].Task);
			}
			behaviorSource.Save((!(this.mEntryNode != null)) ? null : this.mEntryNode.Task, (!(this.mRootNode != null)) ? null : this.mRootNode.Task, list);
			if (BehaviorDesignerPreferences.GetBool(BDPreferences.BinarySerialization))
			{
				BinarySerialization.Save(behaviorSource);
			}
			else
			{
				SerializeJSON.Save(behaviorSource);
			}
		}
		public bool Load(BehaviorSource behaviorSource, bool loadPrevBehavior, Vector2 nodePosition)
		{
			if (behaviorSource == null)
			{
				this.Clear(false);
				return false;
			}
			this.DestroyNodeDesigners();
			if (behaviorSource.Owner != null && behaviorSource.Owner is Behavior && (behaviorSource.Owner as Behavior).ExternalBehavior != null)
			{
				List<SharedVariable> list = null;
				bool force = !Application.isPlaying;
				if (Application.isPlaying && !(behaviorSource.Owner as Behavior).HasInheritedVariables)
				{
					behaviorSource.CheckForSerialization(true, null);
					list = behaviorSource.GetAllVariables();
					(behaviorSource.Owner as Behavior).HasInheritedVariables = true;
					force = true;
				}
				ExternalBehavior externalBehavior = (behaviorSource.Owner as Behavior).ExternalBehavior;
				externalBehavior.BehaviorSource.Owner = externalBehavior;
				externalBehavior.BehaviorSource.CheckForSerialization(force, behaviorSource);
				if (list != null)
				{
					for (int i = 0; i < list.Count; i++)
					{
						behaviorSource.SetVariable(list[i].Name, list[i]);
					}
				}
			}
			else
			{
				behaviorSource.CheckForSerialization(!Application.isPlaying, null);
			}
			if (behaviorSource.EntryTask == null && behaviorSource.RootTask == null && behaviorSource.DetachedTasks == null)
			{
				this.Clear(false);
				return false;
			}
			if (loadPrevBehavior)
			{
				this.mSelectedNodes.Clear();
				this.mSelectedNodeConnections.Clear();
				if (this.mPrevNodeSelectedID != null)
				{
					for (int j = 0; j < this.mPrevNodeSelectedID.Length; j++)
					{
						this.mNodeSelectedID.Add(this.mPrevNodeSelectedID[j]);
					}
					this.mPrevNodeSelectedID = null;
				}
			}
			else
			{
				this.Clear(false);
			}
			this.mNextTaskID = 0;
			this.mEntryNode = null;
			this.mRootNode = null;
			this.mDetachedNodes.Clear();
			Task task;
			Task task2;
			List<Task> list2;
			behaviorSource.Load(out task, out task2, out list2);
			if (BehaviorDesignerUtility.AnyNullTasks(behaviorSource) || (behaviorSource.TaskData != null && BehaviorDesignerUtility.HasRootTask(behaviorSource.TaskData.JSONSerialization) && behaviorSource.RootTask == null))
			{
				behaviorSource.CheckForSerialization(true, null);
				behaviorSource.Load(out task, out task2, out list2);
			}
			if (task == null)
			{
				if (task2 != null || (list2 != null && list2.Count > 0))
				{
					task = (behaviorSource.EntryTask = (Activator.CreateInstance(TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.Tasks.EntryTask"), true) as Task));
					this.mEntryNode = ScriptableObject.CreateInstance<NodeDesigner>();
					if (task2 != null)
					{
						this.mEntryNode.LoadNode(task, behaviorSource, new Vector2(task2.NodeData.Offset.x, task2.NodeData.Offset.y - 120f), ref this.mNextTaskID);
					}
					else
					{
						this.mEntryNode.LoadNode(task, behaviorSource, new Vector2(nodePosition.x, nodePosition.y - 120f), ref this.mNextTaskID);
					}
					this.mEntryNode.MakeEntryDisplay();
					EditorUtility.SetDirty(behaviorSource.Owner.GetObject());
				}
			}
			else
			{
				this.mEntryNode = ScriptableObject.CreateInstance<NodeDesigner>();
				this.mEntryNode.LoadTask(task, (behaviorSource.Owner == null) ? null : (behaviorSource.Owner.GetObject() as Behavior), ref this.mNextTaskID);
				this.mEntryNode.MakeEntryDisplay();
			}
			if (task2 != null)
			{
				this.mRootNode = ScriptableObject.CreateInstance<NodeDesigner>();
				this.mRootNode.LoadTask(task2, (behaviorSource.Owner == null) ? null : (behaviorSource.Owner.GetObject() as Behavior), ref this.mNextTaskID);
				NodeConnection nodeConnection = ScriptableObject.CreateInstance<NodeConnection>();
				nodeConnection.LoadConnection(this.mEntryNode, NodeConnectionType.Fixed);
				this.mEntryNode.AddChildNode(this.mRootNode, nodeConnection, false, false);
				this.LoadNodeSelection(this.mRootNode);
				if (this.mEntryNode.OutgoingNodeConnections.Count == 0)
				{
					this.mActiveNodeConnection = ScriptableObject.CreateInstance<NodeConnection>();
					this.mActiveNodeConnection.LoadConnection(this.mEntryNode, NodeConnectionType.Outgoing);
					this.ConnectNodes(behaviorSource, this.mRootNode);
				}
			}
			if (list2 != null)
			{
				for (int k = 0; k < list2.Count; k++)
				{
					if (list2[k] != null)
					{
						NodeDesigner nodeDesigner = ScriptableObject.CreateInstance<NodeDesigner>();
						nodeDesigner.LoadTask(list2[k], (behaviorSource.Owner == null) ? null : (behaviorSource.Owner.GetObject() as Behavior), ref this.mNextTaskID);
						this.mDetachedNodes.Add(nodeDesigner);
						this.LoadNodeSelection(nodeDesigner);
					}
				}
			}
			return true;
		}
		public bool HasEntryNode()
		{
			return this.mEntryNode != null && this.mEntryNode.Task != null;
		}
		public Vector2 EntryNodePosition()
		{
			return this.mEntryNode.GetAbsolutePosition();
		}
		public void SetRootNodesOffset(Vector2 offset)
		{
			Vector2 b = this.mEntryNode.Task.NodeData.Offset - offset;
			this.mEntryNode.Task.NodeData.Offset = offset;
			for (int i = 0; i < this.mDetachedNodes.Count; i++)
			{
				this.mDetachedNodes[i].Task.NodeData.Offset -= b;
			}
		}
		private void LoadNodeSelection(NodeDesigner nodeDesigner)
		{
			if (nodeDesigner == null)
			{
				return;
			}
			if (this.mNodeSelectedID != null && this.mNodeSelectedID.Contains(nodeDesigner.Task.ID))
			{
				this.Select(nodeDesigner, false);
			}
			if (nodeDesigner.IsParent)
			{
				ParentTask parentTask = nodeDesigner.Task as ParentTask;
				if (parentTask.Children != null)
				{
					for (int i = 0; i < parentTask.Children.Count; i++)
					{
						if (parentTask.Children[i] != null && parentTask.Children[i].NodeData != null)
						{
							this.LoadNodeSelection(parentTask.Children[i].NodeData.NodeDesigner as NodeDesigner);
						}
					}
				}
			}
		}
		public void Clear(bool saveSelectedNodes)
		{
			if (saveSelectedNodes)
			{
				this.mPrevNodeSelectedID = this.mNodeSelectedID.ToArray();
			}
			else
			{
				this.mPrevNodeSelectedID = null;
			}
			this.mNodeSelectedID.Clear();
			this.mSelectedNodes.Clear();
			this.mSelectedNodeConnections.Clear();
			this.DestroyNodeDesigners();
		}
		public void DestroyNodeDesigners()
		{
			if (this.mEntryNode != null)
			{
				this.Clear(this.mEntryNode);
			}
			if (this.mRootNode != null)
			{
				this.Clear(this.mRootNode);
			}
			for (int i = this.mDetachedNodes.Count - 1; i > -1; i--)
			{
				this.Clear(this.mDetachedNodes[i]);
			}
			this.mEntryNode = null;
			this.mRootNode = null;
			this.mDetachedNodes = new List<NodeDesigner>();
		}
		private void Clear(NodeDesigner nodeDesigner)
		{
			if (nodeDesigner == null)
			{
				return;
			}
			if (nodeDesigner.IsParent)
			{
				ParentTask parentTask = nodeDesigner.Task as ParentTask;
				if (parentTask != null && parentTask.Children != null)
				{
					for (int i = parentTask.Children.Count - 1; i > -1; i--)
					{
						if (parentTask.Children[i] != null)
						{
							this.Clear(parentTask.Children[i].NodeData.NodeDesigner as NodeDesigner);
						}
					}
				}
			}
			nodeDesigner.DestroyConnections();
			UnityEngine.Object.DestroyImmediate(nodeDesigner, true);
		}
	}
}
