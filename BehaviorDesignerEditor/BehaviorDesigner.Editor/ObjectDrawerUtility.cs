using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections.Generic;
using System.Reflection;
namespace BehaviorDesigner.Editor
{
	internal static class ObjectDrawerUtility
	{
		private static Dictionary<Type, Type> objectDrawerTypeMap = new Dictionary<Type, Type>();
		private static Dictionary<int, ObjectDrawer> objectDrawerMap = new Dictionary<int, ObjectDrawer>();
		private static bool mapBuilt = false;
		private static void BuildObjectDrawers()
		{
			if (ObjectDrawerUtility.mapBuilt)
			{
				return;
			}
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			for (int i = 0; i < assemblies.Length; i++)
			{
				Assembly assembly = assemblies[i];
				if (assembly != null)
				{
					try
					{
						Type[] exportedTypes = assembly.GetExportedTypes();
						for (int j = 0; j < exportedTypes.Length; j++)
						{
							Type type = exportedTypes[j];
							CustomObjectDrawer[] array;
							if (typeof(ObjectDrawer).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract && (array = (type.GetCustomAttributes(typeof(CustomObjectDrawer), false) as CustomObjectDrawer[])).Length > 0)
							{
								ObjectDrawerUtility.objectDrawerTypeMap.Add(array[0].Type, type);
							}
						}
					}
					catch (Exception)
					{
					}
				}
			}
			ObjectDrawerUtility.mapBuilt = true;
		}
		private static bool ObjectDrawerForType(Type type, ref ObjectDrawer objectDrawer, ref Type objectDrawerType, int hash)
		{
			ObjectDrawerUtility.BuildObjectDrawers();
			if (!ObjectDrawerUtility.objectDrawerTypeMap.ContainsKey(type))
			{
				return false;
			}
			objectDrawerType = ObjectDrawerUtility.objectDrawerTypeMap[type];
			if (ObjectDrawerUtility.objectDrawerMap.ContainsKey(hash))
			{
				objectDrawer = ObjectDrawerUtility.objectDrawerMap[hash];
			}
			return true;
		}
		public static ObjectDrawer GetObjectDrawer(Task task, FieldInfo field)
		{
			ObjectDrawer objectDrawer = null;
			Type type = null;
			if (!ObjectDrawerUtility.ObjectDrawerForType(field.FieldType, ref objectDrawer, ref type, field.GetHashCode()))
			{
				return null;
			}
			if (objectDrawer != null)
			{
				return objectDrawer;
			}
			objectDrawer = (Activator.CreateInstance(type) as ObjectDrawer);
			objectDrawer.FieldInfo = field;
			objectDrawer.Task = task;
			ObjectDrawerUtility.objectDrawerMap.Add(field.GetHashCode(), objectDrawer);
			return objectDrawer;
		}
		public static ObjectDrawer GetObjectDrawer(Task task, ObjectDrawerAttribute attribute)
		{
			ObjectDrawer objectDrawer = null;
			Type type = null;
			if (!ObjectDrawerUtility.ObjectDrawerForType(attribute.GetType(), ref objectDrawer, ref type, attribute.GetHashCode()))
			{
				return null;
			}
			if (objectDrawer != null)
			{
				return objectDrawer;
			}
			objectDrawer = (Activator.CreateInstance(type) as ObjectDrawer);
			objectDrawer.Attribute = attribute;
			objectDrawer.Task = task;
			ObjectDrawerUtility.objectDrawerMap.Add(attribute.GetHashCode(), objectDrawer);
			return objectDrawer;
		}
	}
}
