using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace BehaviorDesigner.Editor
{
	public class AssetCreator : EditorWindow
	{
		public enum AssetClassType
		{
			Action,
			Conditional,
			SharedVariable
		}
		private bool m_CSharp = true;
		private AssetCreator.AssetClassType m_classType;
		private string m_AssetName;
		private bool CSharp
		{
			set
			{
				this.m_CSharp = value;
			}
		}
		private AssetCreator.AssetClassType ClassType
		{
			set
			{
				this.m_classType = value;
				switch (this.m_classType)
				{
				case AssetCreator.AssetClassType.Action:
					this.m_AssetName = "NewAction";
					break;
				case AssetCreator.AssetClassType.Conditional:
					this.m_AssetName = "NewConditional";
					break;
				case AssetCreator.AssetClassType.SharedVariable:
					this.m_AssetName = "SharedNewVariable";
					break;
				}
			}
		}
		public static void ShowWindow(AssetCreator.AssetClassType classType, bool cSharp)
		{
			AssetCreator window = EditorWindow.GetWindow<AssetCreator>(true, "Asset Name");
			EditorWindow arg_25_0 = window;
			Vector2 vector = new Vector2(300f, 55f);
			window.maxSize = vector;
			arg_25_0.minSize = vector;
			window.ClassType = classType;
			window.CSharp = cSharp;
		}
		private void OnGUI()
		{
			this.m_AssetName = EditorGUILayout.TextField("Name", this.m_AssetName, new GUILayoutOption[0]);
			EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
			if (GUILayout.Button("OK", new GUILayoutOption[0]))
			{
				AssetCreator.CreateScript(this.m_AssetName, this.m_classType, this.m_CSharp);
				base.Close();
			}
			if (GUILayout.Button("Cancel", new GUILayoutOption[0]))
			{
				base.Close();
			}
			EditorGUILayout.EndHorizontal();
		}
		public static void CreateAsset(Type type, string name)
		{
			ScriptableObject asset = ScriptableObject.CreateInstance(type);
			string text = AssetDatabase.GetAssetPath(Selection.activeObject);
			if (text == string.Empty)
			{
				text = "Assets";
			}
			else if (Path.GetExtension(text) != string.Empty)
			{
				text = text.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), string.Empty);
			}
			string path = AssetDatabase.GenerateUniqueAssetPath(text + "/" + name + ".asset");
			AssetDatabase.CreateAsset(asset, path);
			AssetDatabase.SaveAssets();
		}
		private static void CreateScript(string name, AssetCreator.AssetClassType classType, bool cSharp)
		{
			string text = AssetDatabase.GetAssetPath(Selection.activeObject);
			if (text == string.Empty)
			{
				text = "Assets";
			}
			else if (Path.GetExtension(text) != string.Empty)
			{
				text = text.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), string.Empty);
			}
			string path = AssetDatabase.GenerateUniqueAssetPath(text + "/" + name + ((!cSharp) ? ".js" : ".cs"));
			StreamWriter streamWriter = new StreamWriter(path, false);
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
			string value = string.Empty;
			switch (classType)
			{
			case AssetCreator.AssetClassType.Action:
				value = AssetCreator.ActionTaskContents(fileNameWithoutExtension, cSharp);
				break;
			case AssetCreator.AssetClassType.Conditional:
				value = AssetCreator.ConditionalTaskContents(fileNameWithoutExtension, cSharp);
				break;
			case AssetCreator.AssetClassType.SharedVariable:
				value = AssetCreator.SharedVariableContents(fileNameWithoutExtension);
				break;
			}
			streamWriter.Write(value);
			streamWriter.Close();
			AssetDatabase.Refresh();
		}
		private static string ActionTaskContents(string name, bool cSharp)
		{
			if (cSharp)
			{
				return "using UnityEngine;\nusing BehaviorDesigner.Runtime;\nusing BehaviorDesigner.Runtime.Tasks;\n\npublic class " + name + " : Action\n{\n\tpublic override void OnStart()\n\t{\n\t\t\n\t}\n\n\tpublic override TaskStatus OnUpdate()\n\t{\n\t\treturn TaskStatus.Success;\n\t}\n}";
			}
			return "#pragma strict\n\nclass " + name + " extends BehaviorDesigner.Runtime.Tasks.Action\n{\n\tfunction OnStart()\n\t{\n\t\t\n\t}\n\n\tfunction OnUpdate()\n\t{\n\t\treturn BehaviorDesigner.Runtime.Tasks.TaskStatus.Success;\n\t}\n}";
		}
		private static string ConditionalTaskContents(string name, bool cSharp)
		{
			if (cSharp)
			{
				return "using UnityEngine;\nusing BehaviorDesigner.Runtime;\nusing BehaviorDesigner.Runtime.Tasks;\n\npublic class " + name + " : Conditional\n{\n\tpublic override TaskStatus OnUpdate()\n\t{\n\t\treturn TaskStatus.Success;\n\t}\n}";
			}
			return "#pragma strict\n\nclass " + name + " extends BehaviorDesigner.Runtime.Tasks.Conditional\n{\n\tfunction OnUpdate()\n\t{\n\t\treturn BehaviorDesigner.Runtime.Tasks.TaskStatus.Success;\n\t}\n}";
		}
		private static string SharedVariableContents(string name)
		{
			string text = name.Remove(0, 6);
			return string.Concat(new string[]
			{
				"using UnityEngine;\nusing BehaviorDesigner.Runtime;\n\n[System.Serializable]\npublic class ",
				text,
				"\n{\n\n}\n\n[System.Serializable]\npublic class ",
				name,
				" : SharedVariable<",
				text,
				">\n{\n\tpublic override string ToString() { return mValue == null ? \"null\" : mValue.ToString(); }\n\tpublic static implicit operator ",
				name,
				"(",
				text,
				" value) { return new ",
				name,
				" { mValue = value }; }\n}"
			});
		}
	}
}
