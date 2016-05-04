using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections.Generic;
namespace BehaviorDesigner.Editor
{
	public class AlphanumComparator<T> : IComparer<T>
	{
		public int Compare(T x, T y)
		{
			string text = string.Empty;
			if (x.GetType().IsSubclassOf(typeof(Type)))
			{
				Type type = x as Type;
				text = this.TypePrefix(type) + "/";
				TaskCategoryAttribute[] array;
				if ((array = (type.GetCustomAttributes(typeof(TaskCategoryAttribute), false) as TaskCategoryAttribute[])).Length > 0)
				{
					text = text + array[0].Category + "/";
				}
				TaskNameAttribute[] array2;
				if ((array2 = (type.GetCustomAttributes(typeof(TaskNameAttribute), false) as TaskNameAttribute[])).Length > 0)
				{
					text += array2[0].Name;
				}
				else
				{
					text += BehaviorDesignerUtility.SplitCamelCase(type.Name.ToString());
				}
			}
			else if (x.GetType().IsSubclassOf(typeof(SharedVariable)))
			{
				string text2 = x.GetType().Name;
				if (text2.Length > 6 && text2.Substring(0, 6).Equals("Shared"))
				{
					text2 = text2.Substring(6, text2.Length - 6);
				}
				text = BehaviorDesignerUtility.SplitCamelCase(text2);
			}
			else
			{
				text = BehaviorDesignerUtility.SplitCamelCase(x.ToString());
			}
			if (text == null)
			{
				return 0;
			}
			string text3 = string.Empty;
			if (y.GetType().IsSubclassOf(typeof(Type)))
			{
				Type type2 = y as Type;
				text3 = this.TypePrefix(type2) + "/";
				TaskCategoryAttribute[] array3;
				if ((array3 = (type2.GetCustomAttributes(typeof(TaskCategoryAttribute), false) as TaskCategoryAttribute[])).Length > 0)
				{
					text3 = text3 + array3[0].Category + "/";
				}
				TaskNameAttribute[] array4;
				if ((array4 = (type2.GetCustomAttributes(typeof(TaskNameAttribute), false) as TaskNameAttribute[])).Length > 0)
				{
					text3 += array4[0].Name;
				}
				else
				{
					text3 += BehaviorDesignerUtility.SplitCamelCase(type2.Name.ToString());
				}
			}
			else if (y.GetType().IsSubclassOf(typeof(SharedVariable)))
			{
				string text4 = y.GetType().Name;
				if (text4.Length > 6 && text4.Substring(0, 6).Equals("Shared"))
				{
					text4 = text4.Substring(6, text4.Length - 6);
				}
				text3 = BehaviorDesignerUtility.SplitCamelCase(text4);
			}
			else
			{
				text3 = BehaviorDesignerUtility.SplitCamelCase(y.ToString());
			}
			if (text3 == null)
			{
				return 0;
			}
			int length = text.Length;
			int length2 = text3.Length;
			int num = 0;
			int num2 = 0;
			while (num < length && num2 < length2)
			{
				int num4;
				if (char.IsDigit(text[num]) && char.IsDigit(text[num2]))
				{
					string text5 = string.Empty;
					while (num < length && char.IsDigit(text[num]))
					{
						text5 += text[num];
						num++;
					}
					string text6 = string.Empty;
					while (num2 < length2 && char.IsDigit(text3[num2]))
					{
						text6 += text3[num2];
						num2++;
					}
					int num3 = 0;
					int.TryParse(text5, out num3);
					int value = 0;
					int.TryParse(text6, out value);
					num4 = num3.CompareTo(value);
				}
				else
				{
					num4 = text[num].CompareTo(text3[num2]);
				}
				if (num4 != 0)
				{
					return num4;
				}
				num++;
				num2++;
			}
			return length - length2;
		}
		private string TypePrefix(Type t)
		{
			if (t.IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.Action)))
			{
				return "Action";
			}
			if (t.IsSubclassOf(typeof(Composite)))
			{
				return "Composite";
			}
			if (t.IsSubclassOf(typeof(Conditional)))
			{
				return "Conditional";
			}
			return "Decorator";
		}
	}
}
