using System;
using UnityEditor;
using UnityEngine;
namespace BehaviorDesigner.Editor
{
	public class WelcomeScreen : EditorWindow
	{
		private Texture m_WelcomeScreenImage;
		private Texture m_SamplesImage;
		private Texture m_DocImage;
		private Texture m_VideoImage;
		private Texture m_ForumImage;
		private Texture m_ContactImage;
		private Rect m_WelcomeScreenImageRect = new Rect(0f, 0f, 340f, 44f);
		private Rect m_WelcomeIntroRect = new Rect(46f, 12f, 306f, 40f);
		private Rect m_SamplesImageRect = new Rect(15f, 58f, 50f, 50f);
		private Rect m_DocImageRect = new Rect(15f, 124f, 53f, 50f);
		private Rect m_VideoImageRect = new Rect(15f, 190f, 50f, 50f);
		private Rect m_ForumImageRect = new Rect(15f, 256f, 50f, 50f);
		private Rect m_ContactImageRect = new Rect(15f, 322f, 50f, 50f);
		private Rect m_VersionRect = new Rect(5f, 385f, 125f, 20f);
		private Rect m_ToggleButtonRect = new Rect(220f, 385f, 125f, 20f);
		private Rect m_SamplesHeaderRect = new Rect(70f, 57f, 250f, 20f);
		private Rect m_DocHeaderRect = new Rect(70f, 123f, 250f, 20f);
		private Rect m_VideoHeaderRect = new Rect(70f, 189f, 250f, 20f);
		private Rect m_ForumHeaderRect = new Rect(70f, 258f, 250f, 20f);
		private Rect m_ContactHeaderRect = new Rect(70f, 324f, 250f, 20f);
		private Rect m_SamplesDescriptionRect = new Rect(70f, 77f, 250f, 30f);
		private Rect m_DocDescriptionRect = new Rect(70f, 143f, 250f, 30f);
		private Rect m_VideoDescriptionRect = new Rect(70f, 209f, 250f, 30f);
		private Rect m_ForumDescriptionRect = new Rect(70f, 278f, 250f, 30f);
		private Rect m_ContactDescriptionRect = new Rect(70f, 344f, 250f, 30f);
		[MenuItem("Tools/Behavior Designer/Welcome Screen", false, 3)]
		public static void ShowWindow()
		{
			WelcomeScreen window = EditorWindow.GetWindow<WelcomeScreen>(true, "Welcome to Behavior Designer");
			EditorWindow arg_25_0 = window;
			Vector2 vector = new Vector2(340f, 410f);
			window.maxSize = vector;
			arg_25_0.minSize = vector;
		}
		public void OnEnable()
		{
			this.m_WelcomeScreenImage = BehaviorDesignerUtility.LoadTexture("WelcomeScreenHeader.png", false, this);
			this.m_SamplesImage = BehaviorDesignerUtility.LoadIcon("WelcomeScreenSamplesIcon.png", this);
			this.m_DocImage = BehaviorDesignerUtility.LoadIcon("WelcomeScreenDocumentationIcon.png", this);
			this.m_VideoImage = BehaviorDesignerUtility.LoadIcon("WelcomeScreenVideosIcon.png", this);
			this.m_ForumImage = BehaviorDesignerUtility.LoadIcon("WelcomeScreenForumIcon.png", this);
			this.m_ContactImage = BehaviorDesignerUtility.LoadIcon("WelcomeScreenContactIcon.png", this);
		}
		public void OnGUI()
		{
			GUI.DrawTexture(this.m_WelcomeScreenImageRect, this.m_WelcomeScreenImage);
			GUI.Label(this.m_WelcomeIntroRect, "Welcome To Behavior Designer", BehaviorDesignerUtility.WelcomeScreenIntroGUIStyle);
			GUI.DrawTexture(this.m_SamplesImageRect, this.m_SamplesImage);
			GUI.Label(this.m_SamplesHeaderRect, "Samples", BehaviorDesignerUtility.WelcomeScreenTextHeaderGUIStyle);
			GUI.Label(this.m_SamplesDescriptionRect, "Download sample projects to get a feel for Behavior Designer.", BehaviorDesignerUtility.WelcomeScreenTextDescriptionGUIStyle);
			GUI.DrawTexture(this.m_DocImageRect, this.m_DocImage);
			GUI.Label(this.m_DocHeaderRect, "Documentation", BehaviorDesignerUtility.WelcomeScreenTextHeaderGUIStyle);
			GUI.Label(this.m_DocDescriptionRect, "Browser our extensive online documentation.", BehaviorDesignerUtility.WelcomeScreenTextDescriptionGUIStyle);
			GUI.DrawTexture(this.m_VideoImageRect, this.m_VideoImage);
			GUI.Label(this.m_VideoHeaderRect, "Videos", BehaviorDesignerUtility.WelcomeScreenTextHeaderGUIStyle);
			GUI.Label(this.m_VideoDescriptionRect, "Watch our tutorial videos which cover a wide variety of topics.", BehaviorDesignerUtility.WelcomeScreenTextDescriptionGUIStyle);
			GUI.DrawTexture(this.m_ForumImageRect, this.m_ForumImage);
			GUI.Label(this.m_ForumHeaderRect, "Forums", BehaviorDesignerUtility.WelcomeScreenTextHeaderGUIStyle);
			GUI.Label(this.m_ForumDescriptionRect, "Join the forums!", BehaviorDesignerUtility.WelcomeScreenTextDescriptionGUIStyle);
			GUI.DrawTexture(this.m_ContactImageRect, this.m_ContactImage);
			GUI.Label(this.m_ContactHeaderRect, "Contact", BehaviorDesignerUtility.WelcomeScreenTextHeaderGUIStyle);
			GUI.Label(this.m_ContactDescriptionRect, "We are here to help.", BehaviorDesignerUtility.WelcomeScreenTextDescriptionGUIStyle);
			GUI.Label(this.m_VersionRect, "Version 1.5.5");
			bool flag = GUI.Toggle(this.m_ToggleButtonRect, BehaviorDesignerPreferences.GetBool(BDPreferences.ShowWelcomeScreen), "Show at Startup");
			if (flag != BehaviorDesignerPreferences.GetBool(BDPreferences.ShowWelcomeScreen))
			{
				BehaviorDesignerPreferences.SetBool(BDPreferences.ShowWelcomeScreen, flag);
			}
			EditorGUIUtility.AddCursorRect(this.m_SamplesImageRect, MouseCursor.Link);
			EditorGUIUtility.AddCursorRect(this.m_SamplesHeaderRect, MouseCursor.Link);
			EditorGUIUtility.AddCursorRect(this.m_SamplesDescriptionRect, MouseCursor.Link);
			EditorGUIUtility.AddCursorRect(this.m_DocImageRect, MouseCursor.Link);
			EditorGUIUtility.AddCursorRect(this.m_DocHeaderRect, MouseCursor.Link);
			EditorGUIUtility.AddCursorRect(this.m_DocDescriptionRect, MouseCursor.Link);
			EditorGUIUtility.AddCursorRect(this.m_VideoImageRect, MouseCursor.Link);
			EditorGUIUtility.AddCursorRect(this.m_VideoHeaderRect, MouseCursor.Link);
			EditorGUIUtility.AddCursorRect(this.m_VideoDescriptionRect, MouseCursor.Link);
			EditorGUIUtility.AddCursorRect(this.m_ForumImageRect, MouseCursor.Link);
			EditorGUIUtility.AddCursorRect(this.m_ForumHeaderRect, MouseCursor.Link);
			EditorGUIUtility.AddCursorRect(this.m_ForumDescriptionRect, MouseCursor.Link);
			EditorGUIUtility.AddCursorRect(this.m_ContactImageRect, MouseCursor.Link);
			EditorGUIUtility.AddCursorRect(this.m_ContactHeaderRect, MouseCursor.Link);
			EditorGUIUtility.AddCursorRect(this.m_ContactDescriptionRect, MouseCursor.Link);
			if (Event.current.type == EventType.MouseUp)
			{
				Vector2 mousePosition = Event.current.mousePosition;
				if (this.m_SamplesImageRect.Contains(mousePosition) || this.m_SamplesHeaderRect.Contains(mousePosition) || this.m_SamplesDescriptionRect.Contains(mousePosition))
				{
					Application.OpenURL("http://www.opsive.com/assets/BehaviorDesigner/samples.php");
				}
				else if (this.m_DocImageRect.Contains(mousePosition) || this.m_DocHeaderRect.Contains(mousePosition) || this.m_DocDescriptionRect.Contains(mousePosition))
				{
					Application.OpenURL("http://www.opsive.com/assets/BehaviorDesigner/documentation.php");
				}
				else if (this.m_VideoImageRect.Contains(mousePosition) || this.m_VideoHeaderRect.Contains(mousePosition) || this.m_VideoDescriptionRect.Contains(mousePosition))
				{
					Application.OpenURL("http://www.opsive.com/assets/BehaviorDesigner/videos.php");
				}
				else if (this.m_ForumImageRect.Contains(mousePosition) || this.m_ForumHeaderRect.Contains(mousePosition) || this.m_ForumDescriptionRect.Contains(mousePosition))
				{
					Application.OpenURL("http://www.opsive.com/forum");
				}
				else if (this.m_ContactImageRect.Contains(mousePosition) || this.m_ContactHeaderRect.Contains(mousePosition) || this.m_ContactDescriptionRect.Contains(mousePosition))
				{
					Application.OpenURL("http://www.opsive.com/assets/BehaviorDesigner/documentation.php?id=12");
				}
			}
		}
	}
}
