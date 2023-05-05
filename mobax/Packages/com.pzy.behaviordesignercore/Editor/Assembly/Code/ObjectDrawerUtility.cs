namespace BehaviorDesigner.Editor
{
    using BehaviorDesigner.Runtime.Tasks;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    internal static class ObjectDrawerUtility
    {
        private static Dictionary<Type, Type> objectDrawerTypeMap = new Dictionary<Type, Type>();
        private static Dictionary<int, ObjectDrawer> objectDrawerMap = new Dictionary<int, ObjectDrawer>();
        private static bool mapBuilt = false;

        private static void BuildObjectDrawers()
        {
            if (!mapBuilt)
            {
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly != null)
                    {
                        try
                        {
                            foreach (Type type in assembly.GetExportedTypes())
                            {
                                if (typeof(ObjectDrawer).IsAssignableFrom(type) && (type.IsClass && !type.IsAbstract))
                                {
                                    CustomObjectDrawer[] drawerArray = null;
                                    if ((drawerArray = type.GetCustomAttributes(typeof(CustomObjectDrawer), false) as CustomObjectDrawer[]).Length > 0)
                                    {
                                        objectDrawerTypeMap.Add(drawerArray[0].Type, type);
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                mapBuilt = true;
            }
        }

        public static ObjectDrawer GetObjectDrawer(BehaviorDesigner.Runtime.Tasks.Task task)
        {
            if (task == null)
            {
                return null;
            }
            ObjectDrawer objectDrawer = null;
            Type objectDrawerType = null;
            if (!ObjectDrawerForType(task.GetType(), ref objectDrawer, ref objectDrawerType, task.GetHashCode()))
            {
                return null;
            }
            if (objectDrawer == null)
            {
                objectDrawer = Activator.CreateInstance(objectDrawerType) as ObjectDrawer;
                objectDrawerMap.Add(task.GetHashCode(), objectDrawer);
            }
            objectDrawer.FieldInfo = null;
            objectDrawer.Task = task;
            return objectDrawer;
        }

        public static ObjectDrawer GetObjectDrawer(BehaviorDesigner.Runtime.Tasks.Task task, FieldInfo field)
        {
            ObjectDrawer objectDrawer = null;
            Type objectDrawerType = null;
            if (!ObjectDrawerForType(field.FieldType, ref objectDrawer, ref objectDrawerType, ((task == null) ? 0 : task.GetHashCode()) + field.GetHashCode()))
            {
                return null;
            }
            if (objectDrawer == null)
            {
                objectDrawer = Activator.CreateInstance(objectDrawerType) as ObjectDrawer;
                objectDrawerMap.Add(((task == null) ? 0 : task.GetHashCode()) + field.GetHashCode(), objectDrawer);
            }
            objectDrawer.FieldInfo = field;
            objectDrawer.Task = task;
            return objectDrawer;
        }

        public static ObjectDrawer GetObjectDrawer(BehaviorDesigner.Runtime.Tasks.Task task, FieldInfo field, BehaviorDesigner.Runtime.Tasks.ObjectDrawerAttribute attribute)
        {
            ObjectDrawer objectDrawer = null;
            Type objectDrawerType = null;
            if (!ObjectDrawerForType(attribute.GetType(), ref objectDrawer, ref objectDrawerType, (((task == null) ? 0 : task.GetHashCode()) + field.GetHashCode()) + attribute.GetHashCode()))
            {
                return null;
            }
            if (objectDrawer != null)
            {
                objectDrawer.Task = task;
                return objectDrawer;
            }
            objectDrawer = Activator.CreateInstance(objectDrawerType) as ObjectDrawer;
            objectDrawer.Attribute = attribute;
            objectDrawer.Task = task;
            objectDrawer.FieldInfo = field;
            objectDrawerMap.Add((((task == null) ? 0 : task.GetHashCode()) + field.GetHashCode()) + attribute.GetHashCode(), objectDrawer);
            return objectDrawer;
        }

        private static bool ObjectDrawerForType(Type type, ref ObjectDrawer objectDrawer, ref Type objectDrawerType, int hash)
        {
            BuildObjectDrawers();
            if (!objectDrawerTypeMap.ContainsKey(type))
            {
                return false;
            }
            objectDrawerType = objectDrawerTypeMap[type];
            if (objectDrawerMap.ContainsKey(hash))
            {
                objectDrawer = objectDrawerMap[hash];
            }
            return true;
        }
    }
}

