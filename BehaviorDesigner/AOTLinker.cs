using BehaviorDesigner.Runtime;
using System;
using UnityEngine;
public class AOTLinker : MonoBehaviour
{
	public void Linker()
	{
		BehaviorManager.BehaviorTree behaviorTree = new BehaviorManager.BehaviorTree();
		BehaviorManager.BehaviorTree.ConditionalReevaluate conditionalReevaluate = new BehaviorManager.BehaviorTree.ConditionalReevaluate();
		BehaviorManager.TaskAddData taskAddData = new BehaviorManager.TaskAddData();
		BehaviorManager.TaskAddData.OverrideFieldValue overrideFieldValue = new BehaviorManager.TaskAddData.OverrideFieldValue();
	}
}
