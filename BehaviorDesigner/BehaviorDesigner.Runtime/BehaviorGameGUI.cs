using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace BehaviorDesigner.Runtime
{
	[AddComponentMenu("Behavior Designer/Behavior Game GUI")]
	public class BehaviorGameGUI : MonoBehaviour
	{
		private BehaviorManager behaviorManager;
		private Camera mainCamera;
		public void Start()
		{
			this.mainCamera = Camera.main;
		}
		public void OnGUI()
		{
			if (this.behaviorManager == null)
			{
				this.behaviorManager = BehaviorManager.instance;
			}
			if (this.behaviorManager == null || this.mainCamera == null)
			{
				return;
			}
			List<BehaviorManager.BehaviorTree> behaviorTrees = this.behaviorManager.BehaviorTrees;
			for (int i = 0; i < behaviorTrees.Count; i++)
			{
				BehaviorManager.BehaviorTree behaviorTree = behaviorTrees[i];
				string text = string.Empty;
				for (int j = 0; j < behaviorTree.activeStack.Count; j++)
				{
					Task task = behaviorTree.taskList[behaviorTree.activeStack[j].Peek()];
					if (task is BehaviorDesigner.Runtime.Tasks.Action)
					{
						text = text + behaviorTree.taskList[behaviorTree.activeStack[j].Peek()].FriendlyName + ((j >= behaviorTree.activeStack.Count - 1) ? string.Empty : "\n");
					}
				}
				Transform transform = behaviorTree.behavior.transform;
				Vector3 v = Camera.main.WorldToScreenPoint(transform.position);
				Vector2 vector = GUIUtility.ScreenToGUIPoint(v);
				GUIContent content = new GUIContent(text);
				Vector2 vector2 = GUI.skin.label.CalcSize(content);
				vector2.x += 14f;
				vector2.y += 5f;
				GUI.Box(new Rect(vector.x - vector2.x / 2f, vector.y + vector2.y / 2f, vector2.x, vector2.y), content);
			}
		}
	}
}
