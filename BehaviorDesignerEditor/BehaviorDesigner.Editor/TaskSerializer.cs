using System;
using System.Collections.Generic;
using UnityEngine;
namespace BehaviorDesigner.Editor
{
	[Serializable]
	public class TaskSerializer
	{
		public string serialization;
		public Vector2 offset;
		public List<UnityEngine.Object> unityObjects;
		public List<int> childrenIndex;
	}
}
