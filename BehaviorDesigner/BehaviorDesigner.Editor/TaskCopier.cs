using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace BehaviorDesigner.Editor
{
	public class TaskCopier : UnityEditor.Editor
	{
		public static TaskSerializer CopySerialized(Task task)
		{
			TaskSerializer taskSerializer = new TaskSerializer();
			taskSerializer.offset = (task.NodeData.NodeDesigner as NodeDesigner).GetAbsolutePosition() + new Vector2(10f, 10f);
			taskSerializer.unityObjects = new List<UnityEngine.Object>();
			taskSerializer.serialization = MiniJSON.Serialize(SerializeJSON.SerializeTask(task, false, ref taskSerializer.unityObjects));
			return taskSerializer;
		}
		public static Task PasteTask(BehaviorSource behaviorSource, TaskSerializer serializer)
		{
			Dictionary<int, Task> dictionary = new Dictionary<int, Task>();
			return DeserializeJSON.DeserializeTask(behaviorSource, MiniJSON.Deserialize(serializer.serialization) as Dictionary<string, object>, ref dictionary, serializer.unityObjects);
		}
	}
}
