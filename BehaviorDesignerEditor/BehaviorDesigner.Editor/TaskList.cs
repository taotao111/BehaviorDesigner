using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
namespace BehaviorDesigner.Editor
{
	[Serializable]
	public class TaskList : ScriptableObject
	{
		public enum TaskTypes
		{
			Action,
			Composite,
			Conditional,
			Decorator,
			Last
		}
		private class SearchableType
		{
			private Type mType;
			private bool mVisible = true;
			private string mName;
			public Type Type
			{
				get
				{
					return this.mType;
				}
			}
			public bool Visible
			{
				get
				{
					return this.mVisible;
				}
				set
				{
					this.mVisible = value;
				}
			}
			public string Name
			{
				get
				{
					return this.mName;
				}
			}
			public SearchableType(Type type)
			{
				this.mType = type;
				this.mName = BehaviorDesignerUtility.SplitCamelCase(this.mType.Name);
			}
		}
		private class CategoryList
		{
			private string mName = string.Empty;
			private string mFullpath = string.Empty;
			private List<TaskList.CategoryList> mSubcategories;
			private List<TaskList.SearchableType> mTasks;
			private bool mExpanded = true;
			private bool mVisible = true;
			private int mID;
			public string Name
			{
				get
				{
					return this.mName;
				}
			}
			public string Fullpath
			{
				get
				{
					return this.mFullpath;
				}
			}
			public List<TaskList.CategoryList> Subcategories
			{
				get
				{
					return this.mSubcategories;
				}
			}
			public List<TaskList.SearchableType> Tasks
			{
				get
				{
					return this.mTasks;
				}
			}
			public bool Expanded
			{
				get
				{
					return this.mExpanded;
				}
				set
				{
					this.mExpanded = value;
				}
			}
			public bool Visible
			{
				get
				{
					return this.mVisible;
				}
				set
				{
					this.mVisible = value;
				}
			}
			public int ID
			{
				get
				{
					return this.mID;
				}
			}
			public CategoryList(string name, string fullpath, bool expanded, int id)
			{
				this.mName = name;
				this.mFullpath = fullpath;
				this.mExpanded = expanded;
				this.mID = id;
			}
			public void addSubcategory(TaskList.CategoryList category)
			{
				if (this.mSubcategories == null)
				{
					this.mSubcategories = new List<TaskList.CategoryList>();
				}
				this.mSubcategories.Add(category);
			}
			public void addTask(Type taskType)
			{
				if (this.mTasks == null)
				{
					this.mTasks = new List<TaskList.SearchableType>();
				}
				this.mTasks.Add(new TaskList.SearchableType(taskType));
			}
		}
		private List<TaskList.CategoryList> mCategoryList;
		private Vector2 mScrollPosition = Vector2.zero;
		private string mSearchString = string.Empty;
		private bool mFocusSearch;
		public void OnEnable()
		{
			base.hideFlags = HideFlags.HideAndDontSave;
		}
		public void Init()
		{
			this.mCategoryList = new List<TaskList.CategoryList>();
			List<Type> list = new List<Type>();
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			for (int i = 0; i < assemblies.Length; i++)
			{
				Type[] types = assemblies[i].GetTypes();
				for (int j = 0; j < types.Length; j++)
				{
					if (!types[j].Equals(typeof(BehaviorReference)) && !types[j].IsAbstract)
					{
						if (types[j].IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.Action)) || types[j].IsSubclassOf(typeof(Composite)) || types[j].IsSubclassOf(typeof(Conditional)) || types[j].IsSubclassOf(typeof(Decorator)))
						{
							list.Add(types[j]);
						}
					}
				}
			}
			list.Sort(new AlphanumComparator<Type>());
			Dictionary<string, TaskList.CategoryList> dictionary = new Dictionary<string, TaskList.CategoryList>();
			string text = string.Empty;
			int id = 0;
			for (int k = 0; k < list.Count; k++)
			{
				if (list[k].IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.Action)))
				{
					text = "Actions";
				}
				else if (list[k].IsSubclassOf(typeof(Composite)))
				{
					text = "Composites";
				}
				else if (list[k].IsSubclassOf(typeof(Conditional)))
				{
					text = "Conditionals";
				}
				else
				{
					text = "Decorators";
				}
				TaskCategoryAttribute[] array;
				if ((array = (list[k].GetCustomAttributes(typeof(TaskCategoryAttribute), false) as TaskCategoryAttribute[])).Length > 0)
				{
					text = text + "/" + array[0].Category;
				}
				string text2 = string.Empty;
				string[] array2 = text.Split(new char[]
				{
					'/'
				});
				TaskList.CategoryList categoryList = null;
				TaskList.CategoryList categoryList2;
				for (int l = 0; l < array2.Length; l++)
				{
					if (l > 0)
					{
						text2 += "/";
					}
					text2 += array2[l];
					if (!dictionary.ContainsKey(text2))
					{
						categoryList2 = new TaskList.CategoryList(array2[l], text2, this.PreviouslyExpanded(id), id++);
						if (categoryList == null)
						{
							this.mCategoryList.Add(categoryList2);
						}
						else
						{
							categoryList.addSubcategory(categoryList2);
						}
						dictionary.Add(text2, categoryList2);
					}
					else
					{
						categoryList2 = dictionary[text2];
					}
					categoryList = categoryList2;
				}
				categoryList2 = dictionary[text2];
				categoryList2.addTask(list[k]);
			}
			this.Search(BehaviorDesignerUtility.SplitCamelCase(this.mSearchString).ToLower().Replace(" ", string.Empty), this.mCategoryList);
		}
		public void AddTasksToMenu(ref GenericMenu genericMenu, Type selectedTaskType, string parentName, GenericMenu.MenuFunction2 menuFunction)
		{
			this.AddCategoryTasksToMenu(ref genericMenu, this.mCategoryList, selectedTaskType, parentName, menuFunction);
		}
		public void AddConditionalTasksToMenu(ref GenericMenu genericMenu, Type selectedTaskType, string parentName, GenericMenu.MenuFunction2 menuFunction)
		{
			if (this.mCategoryList[2].Tasks != null)
			{
				for (int i = 0; i < this.mCategoryList[2].Tasks.Count; i++)
				{
					if (parentName.Equals(string.Empty))
					{
						genericMenu.AddItem(new GUIContent(string.Format("{0}/{1}", this.mCategoryList[2].Fullpath, this.mCategoryList[2].Tasks[i].Name.ToString())), this.mCategoryList[2].Tasks[i].Type.Equals(selectedTaskType), menuFunction, this.mCategoryList[2].Tasks[i].Type);
					}
					else
					{
						genericMenu.AddItem(new GUIContent(string.Format("{0}/{1}/{2}", parentName, this.mCategoryList[22].Fullpath, this.mCategoryList[2].Tasks[i].Name.ToString())), this.mCategoryList[2].Tasks[i].Type.Equals(selectedTaskType), menuFunction, this.mCategoryList[2].Tasks[i].Type);
					}
				}
			}
			this.AddCategoryTasksToMenu(ref genericMenu, this.mCategoryList[2].Subcategories, selectedTaskType, parentName, menuFunction);
		}
		private void AddCategoryTasksToMenu(ref GenericMenu genericMenu, List<TaskList.CategoryList> categoryList, Type selectedTaskType, string parentName, GenericMenu.MenuFunction2 menuFunction)
		{
			for (int i = 0; i < categoryList.Count; i++)
			{
				if (categoryList[i].Subcategories != null)
				{
					this.AddCategoryTasksToMenu(ref genericMenu, categoryList[i].Subcategories, selectedTaskType, parentName, menuFunction);
				}
				if (categoryList[i].Tasks != null)
				{
					for (int j = 0; j < categoryList[i].Tasks.Count; j++)
					{
						if (parentName.Equals(string.Empty))
						{
							genericMenu.AddItem(new GUIContent(string.Format("{0}/{1}", categoryList[i].Fullpath, categoryList[i].Tasks[j].Name.ToString())), categoryList[i].Tasks[j].Type.Equals(selectedTaskType), menuFunction, categoryList[i].Tasks[j].Type);
						}
						else
						{
							genericMenu.AddItem(new GUIContent(string.Format("{0}/{1}/{2}", parentName, categoryList[i].Fullpath, categoryList[i].Tasks[j].Name.ToString())), categoryList[i].Tasks[j].Type.Equals(selectedTaskType), menuFunction, categoryList[i].Tasks[j].Type);
						}
					}
				}
			}
		}
		public void FocusSearchField()
		{
			this.mFocusSearch = true;
		}
		public void DrawTaskList(BehaviorDesignerWindow window, bool enabled)
		{
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			GUI.SetNextControlName("Search");
			string value = GUILayout.TextField(this.mSearchString, GUI.skin.FindStyle("ToolbarSeachTextField"), new GUILayoutOption[0]);
			if (this.mFocusSearch)
			{
				GUI.FocusControl("Search");
				this.mFocusSearch = false;
			}
			if (!this.mSearchString.Equals(value))
			{
				this.mSearchString = value;
				this.Search(BehaviorDesignerUtility.SplitCamelCase(this.mSearchString).ToLower().Replace(" ", string.Empty), this.mCategoryList);
			}
			if (GUILayout.Button(string.Empty, (!this.mSearchString.Equals(string.Empty)) ? GUI.skin.FindStyle("ToolbarSeachCancelButton") : GUI.skin.FindStyle("ToolbarSeachCancelButtonEmpty"), new GUILayoutOption[0]))
			{
				this.mSearchString = string.Empty;
				this.Search(string.Empty, this.mCategoryList);
				GUI.FocusControl(null);
			}
			GUILayout.EndHorizontal();
			BehaviorDesignerUtility.DrawContentSeperator(2);
			GUILayout.Space(4f);
			this.mScrollPosition = GUILayout.BeginScrollView(this.mScrollPosition, new GUILayoutOption[0]);
			GUI.enabled = enabled;
			if (this.mCategoryList.Count > 1)
			{
				this.DrawCategory(window, this.mCategoryList[1]);
			}
			if (this.mCategoryList.Count > 3)
			{
				this.DrawCategory(window, this.mCategoryList[3]);
			}
			if (this.mCategoryList.Count > 0)
			{
				this.DrawCategory(window, this.mCategoryList[0]);
			}
			if (this.mCategoryList.Count > 2)
			{
				this.DrawCategory(window, this.mCategoryList[2]);
			}
			GUI.enabled = true;
			GUILayout.EndScrollView();
		}
		private void DrawCategory(BehaviorDesignerWindow window, TaskList.CategoryList category)
		{
			if (category.Visible)
			{
				category.Expanded = EditorGUILayout.Foldout(category.Expanded, category.Name, BehaviorDesignerUtility.TaskFoldoutGUIStyle);
				this.SetExpanded(category.ID, category.Expanded);
				if (category.Expanded)
				{
					EditorGUI.indentLevel++;
					if (category.Tasks != null)
					{
						for (int i = 0; i < category.Tasks.Count; i++)
						{
							if (category.Tasks[i].Visible)
							{
								GUILayout.BeginHorizontal(new GUILayoutOption[0]);
								GUILayout.Space((float)(EditorGUI.indentLevel * 16));
								TaskNameAttribute[] array;
								string name;
								if ((array = (category.Tasks[i].Type.GetCustomAttributes(typeof(TaskNameAttribute), false) as TaskNameAttribute[])).Length > 0)
								{
									name = array[0].Name;
								}
								else
								{
									name = category.Tasks[i].Name;
								}
								if (GUILayout.Button(name, EditorStyles.toolbarButton, new GUILayoutOption[]
								{
									GUILayout.MaxWidth((float)(300 - EditorGUI.indentLevel * 16 - 24))
								}))
								{
									window.AddTask(category.Tasks[i].Type, false);
								}
								GUILayout.Space(3f);
								GUILayout.EndHorizontal();
							}
						}
					}
					if (category.Subcategories != null)
					{
						this.DrawCategoryTaskList(window, category.Subcategories);
					}
					EditorGUI.indentLevel--;
				}
			}
		}
		private void DrawCategoryTaskList(BehaviorDesignerWindow window, List<TaskList.CategoryList> categoryList)
		{
			for (int i = 0; i < categoryList.Count; i++)
			{
				this.DrawCategory(window, categoryList[i]);
			}
		}
		private bool Search(string searchString, List<TaskList.CategoryList> categoryList)
		{
			bool result = searchString.Equals(string.Empty);
			for (int i = 0; i < categoryList.Count; i++)
			{
				bool flag = false;
				categoryList[i].Visible = false;
				if (categoryList[i].Subcategories != null && this.Search(searchString, categoryList[i].Subcategories))
				{
					categoryList[i].Visible = true;
					result = true;
				}
				if (BehaviorDesignerUtility.SplitCamelCase(categoryList[i].Name).ToLower().Replace(" ", string.Empty).Contains(searchString))
				{
					result = true;
					flag = true;
					categoryList[i].Visible = true;
					if (categoryList[i].Subcategories != null)
					{
						this.MarkVisible(categoryList[i].Subcategories);
					}
				}
				if (categoryList[i].Tasks != null)
				{
					for (int j = 0; j < categoryList[i].Tasks.Count; j++)
					{
						categoryList[i].Tasks[j].Visible = searchString.Equals(string.Empty);
						if (flag || categoryList[i].Tasks[j].Name.ToLower().Replace(" ", string.Empty).Contains(searchString))
						{
							categoryList[i].Tasks[j].Visible = true;
							result = true;
							categoryList[i].Visible = true;
						}
					}
				}
			}
			return result;
		}
		private void MarkVisible(List<TaskList.CategoryList> categoryList)
		{
			for (int i = 0; i < categoryList.Count; i++)
			{
				categoryList[i].Visible = true;
				if (categoryList[i].Subcategories != null)
				{
					this.MarkVisible(categoryList[i].Subcategories);
				}
				if (categoryList[i].Tasks != null)
				{
					for (int j = 0; j < categoryList[i].Tasks.Count; j++)
					{
						categoryList[i].Tasks[j].Visible = true;
					}
				}
			}
		}
		private bool PreviouslyExpanded(int id)
		{
			return EditorPrefs.GetBool("BehaviorDesignerTaskList" + id, true);
		}
		private void SetExpanded(int id, bool visible)
		{
			EditorPrefs.SetBool("BehaviorDesignerTaskList" + id, visible);
		}
	}
}
