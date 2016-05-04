using System;
using UnityEditor;
using UnityEngine;
namespace BehaviorDesigner.Editor
{
	[Serializable]
	public class NodeConnection : ScriptableObject
	{
		[SerializeField]
		private NodeDesigner originatingNodeDesigner;
		[SerializeField]
		private NodeDesigner destinationNodeDesigner;
		[SerializeField]
		private NodeConnectionType nodeConnectionType;
		[SerializeField]
		private bool selected;
		[SerializeField]
		private float horizontalHeight;
		private readonly Color selectedDisabledProColor = new Color(0.1316f, 0.3212f, 0.4803f);
		private readonly Color selectedDisabledStandardColor = new Color(0.1701f, 0.3982f, 0.5873f);
		private readonly Color selectedEnabledProColor = new Color(0.188f, 0.4588f, 0.6862f);
		private readonly Color selectedEnabledStandardColor = new Color(0.243f, 0.5686f, 0.839f);
		private readonly Color taskRunningProColor = new Color(0f, 0.698f, 0.4f);
		private readonly Color taskRunningStandardColor = new Color(0f, 1f, 0.2784f);
		private bool horizontalDirty = true;
		private Vector2 startHorizontalBreak;
		private Vector2 endHorizontalBreak;
		private Vector3[] linePoints = new Vector3[4];
		public NodeDesigner OriginatingNodeDesigner
		{
			get
			{
				return this.originatingNodeDesigner;
			}
			set
			{
				this.originatingNodeDesigner = value;
			}
		}
		public NodeDesigner DestinationNodeDesigner
		{
			get
			{
				return this.destinationNodeDesigner;
			}
			set
			{
				this.destinationNodeDesigner = value;
			}
		}
		public NodeConnectionType NodeConnectionType
		{
			get
			{
				return this.nodeConnectionType;
			}
			set
			{
				this.nodeConnectionType = value;
			}
		}
		public float HorizontalHeight
		{
			set
			{
				this.horizontalHeight = value;
				this.horizontalDirty = true;
			}
		}
		public void select()
		{
			this.selected = true;
		}
		public void deselect()
		{
			this.selected = false;
		}
		public void OnEnable()
		{
			base.hideFlags = HideFlags.HideAndDontSave;
		}
		public void LoadConnection(NodeDesigner nodeDesigner, NodeConnectionType nodeConnectionType)
		{
			this.originatingNodeDesigner = nodeDesigner;
			this.nodeConnectionType = nodeConnectionType;
			this.selected = false;
		}
		public void DrawConnection(Vector2 offset, bool disabled)
		{
			this.DrawConnection(this.OriginatingNodeDesigner.GetConnectionPosition(offset, NodeConnectionType.Outgoing), this.DestinationNodeDesigner.GetConnectionPosition(offset, NodeConnectionType.Incoming), disabled);
		}
		public void DrawConnection(Vector2 source, Vector2 destination, bool disabled)
		{
			Color color = (!disabled) ? Color.white : new Color(0.7f, 0.7f, 0.7f);
			bool flag = this.destinationNodeDesigner != null && this.destinationNodeDesigner.Task != null && this.destinationNodeDesigner.Task.NodeData.PushTime != -1f && this.destinationNodeDesigner.Task.NodeData.PushTime >= this.destinationNodeDesigner.Task.NodeData.PopTime;
			float num = (!BehaviorDesignerPreferences.GetBool(BDPreferences.FadeNodes)) ? 0.01f : 0.5f;
			if (this.selected)
			{
				if (disabled)
				{
					if (EditorGUIUtility.isProSkin)
					{
						color = this.selectedDisabledProColor;
					}
					else
					{
						color = this.selectedDisabledStandardColor;
					}
				}
				else if (EditorGUIUtility.isProSkin)
				{
					color = this.selectedEnabledProColor;
				}
				else
				{
					color = this.selectedEnabledStandardColor;
				}
			}
			else if (flag)
			{
				if (EditorGUIUtility.isProSkin)
				{
					color = this.taskRunningProColor;
				}
				else
				{
					color = this.taskRunningStandardColor;
				}
			}
			else if (num != 0f && this.destinationNodeDesigner != null && this.destinationNodeDesigner.Task != null && this.destinationNodeDesigner.Task.NodeData.PopTime != -1f && Time.realtimeSinceStartup - this.destinationNodeDesigner.Task.NodeData.PopTime < num)
			{
				float t = 1f - (Time.realtimeSinceStartup - this.destinationNodeDesigner.Task.NodeData.PopTime) / num;
				Color white = Color.white;
				if (EditorGUIUtility.isProSkin)
				{
					white = this.taskRunningProColor;
				}
				else
				{
					white = this.taskRunningStandardColor;
				}
				color = Color.Lerp(Color.white, white, t);
			}
			Handles.color = color;
			if (this.horizontalDirty)
			{
				this.startHorizontalBreak = new Vector2(source.x, this.horizontalHeight);
				this.endHorizontalBreak = new Vector2(destination.x, this.horizontalHeight);
				this.horizontalDirty = false;
			}
			this.linePoints[0] = source;
			this.linePoints[1] = this.startHorizontalBreak;
			this.linePoints[2] = this.endHorizontalBreak;
			this.linePoints[3] = destination;
			Handles.DrawPolyLine(this.linePoints);
			for (int i = 0; i < this.linePoints.Length; i++)
			{
				Vector3[] expr_2C2_cp_0 = this.linePoints;
				int expr_2C2_cp_1 = i;
				expr_2C2_cp_0[expr_2C2_cp_1].x = expr_2C2_cp_0[expr_2C2_cp_1].x + 1f;
				Vector3[] expr_2E0_cp_0 = this.linePoints;
				int expr_2E0_cp_1 = i;
				expr_2E0_cp_0[expr_2E0_cp_1].y = expr_2E0_cp_0[expr_2E0_cp_1].y + 1f;
			}
			Handles.DrawPolyLine(this.linePoints);
		}
		public bool Contains(Vector2 point, Vector2 offset)
		{
			Vector2 center = this.originatingNodeDesigner.OutgoingConnectionRect(offset).center;
			Vector2 vector = new Vector2(center.x, this.horizontalHeight);
			float num = Mathf.Abs(point.x - center.x);
			if (num < 7f && ((point.y >= center.y && point.y <= vector.y) || (point.y <= center.y && point.y >= vector.y)))
			{
				return true;
			}
			Rect rect = this.destinationNodeDesigner.IncomingConnectionRect(offset);
			Vector2 vector2 = new Vector2(rect.center.x, rect.y);
			Vector2 vector3 = new Vector2(vector2.x, this.horizontalHeight);
			num = Mathf.Abs(point.y - this.horizontalHeight);
			if (num < 7f && ((point.x <= center.x && point.x >= vector3.x) || (point.x >= center.x && point.x <= vector3.x)))
			{
				return true;
			}
			num = Mathf.Abs(point.x - vector2.x);
			return num < 7f && ((point.y >= vector2.y && point.y <= vector3.y) || (point.y <= vector2.y && point.y >= vector3.y));
		}
	}
}
