namespace BehaviorDesigner.Editor
{
    using BehaviorDesigner.Runtime;
    using BehaviorDesigner.Runtime.Tasks;
    using System;
    using System.Collections.Generic;

    public class AlphanumComparator<T> : IComparer<T>
    {
        public int Compare(T x, T y)
        {
            string str = string.Empty;
            if (x.GetType().IsSubclassOf(typeof(Type)))
            {
                Type t = x as Type;
                str = this.TypePrefix(t) + "/";
                BehaviorDesigner.Runtime.Tasks.TaskCategoryAttribute[] attributeArray = null;
                if ((attributeArray = t.GetCustomAttributes(typeof(BehaviorDesigner.Runtime.Tasks.TaskCategoryAttribute), true) as BehaviorDesigner.Runtime.Tasks.TaskCategoryAttribute[]).Length > 0)
                {
                    str = str + attributeArray[0].Category.TrimEnd(BehaviorDesigner.Runtime.TaskUtility.TrimCharacters) + "/";
                }
                BehaviorDesigner.Runtime.Tasks.TaskNameAttribute[] attributeArray2 = null;
                str = ((attributeArray2 = t.GetCustomAttributes(typeof(BehaviorDesigner.Runtime.Tasks.TaskNameAttribute), false) as BehaviorDesigner.Runtime.Tasks.TaskNameAttribute[]).Length <= 0) ? (str + BehaviorDesignerUtility.SplitCamelCase(t.Name.ToString())) : (str + attributeArray2[0].Name);
            }
            else if (!x.GetType().IsSubclassOf(typeof(BehaviorDesigner.Runtime.SharedVariable)))
            {
                str = BehaviorDesignerUtility.SplitCamelCase(x.ToString());
            }
            else
            {
                string name = x.GetType().Name;
                if ((name.Length > 6) && name.Substring(0, 6).Equals("Shared"))
                {
                    name = name.Substring(6, name.Length - 6);
                }
                str = BehaviorDesignerUtility.SplitCamelCase(name);
            }
            if (str == null)
            {
                return 0;
            }
            string str4 = string.Empty;
            if (y.GetType().IsSubclassOf(typeof(Type)))
            {
                Type t = y as Type;
                str4 = this.TypePrefix(t) + "/";
                BehaviorDesigner.Runtime.Tasks.TaskCategoryAttribute[] attributeArray3 = null;
                if ((attributeArray3 = t.GetCustomAttributes(typeof(BehaviorDesigner.Runtime.Tasks.TaskCategoryAttribute), true) as BehaviorDesigner.Runtime.Tasks.TaskCategoryAttribute[]).Length > 0)
                {
                    str4 = str4 + attributeArray3[0].Category.TrimEnd(BehaviorDesigner.Runtime.TaskUtility.TrimCharacters) + "/";
                }
                BehaviorDesigner.Runtime.Tasks.TaskNameAttribute[] attributeArray4 = null;
                str4 = ((attributeArray4 = t.GetCustomAttributes(typeof(BehaviorDesigner.Runtime.Tasks.TaskNameAttribute), false) as BehaviorDesigner.Runtime.Tasks.TaskNameAttribute[]).Length <= 0) ? (str4 + BehaviorDesignerUtility.SplitCamelCase(t.Name.ToString())) : (str4 + attributeArray4[0].Name);
            }
            else if (!y.GetType().IsSubclassOf(typeof(BehaviorDesigner.Runtime.SharedVariable)))
            {
                str4 = BehaviorDesignerUtility.SplitCamelCase(y.ToString());
            }
            else
            {
                string name = y.GetType().Name;
                if ((name.Length > 6) && name.Substring(0, 6).Equals("Shared"))
                {
                    name = name.Substring(6, name.Length - 6);
                }
                str4 = BehaviorDesignerUtility.SplitCamelCase(name);
            }
            if (str4 == null)
            {
                return 0;
            }
            int length = str.Length;
            int num2 = str4.Length;
            int num3 = 0;
            for (int i = 0; (num3 < length) && (i < num2); i++)
            {
                int num5 = 0;
                if (!char.IsDigit(str[num3]) || !char.IsDigit(str[i]))
                {
                    num5 = str[num3].CompareTo(str4[i]);
                }
                else
                {
                    string s = string.Empty;
                    while (true)
                    {
                        if ((num3 >= length) || !char.IsDigit(str[num3]))
                        {
                            string str8 = string.Empty;
                            while (true)
                            {
                                if ((i >= num2) || !char.IsDigit(str4[i]))
                                {
                                    int result = 0;
                                    int.TryParse(s, out result);
                                    int num7 = 0;
                                    int.TryParse(str8, out num7);
                                    num5 = result.CompareTo(num7);
                                    break;
                                }
                                str8 = str8 + str4[i];
                                i++;
                            }
                            break;
                        }
                        s = s + str[num3];
                        num3++;
                    }
                }
                if (num5 != 0)
                {
                    return num5;
                }
                num3++;
            }
            return (length - num2);
        }

        private string TypePrefix(Type t)
        {
            return (!t.IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.Action)) ? (!t.IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.Composite)) ? (!t.IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.Conditional)) ? "Decorator" : "Conditional") : "Composite") : "Action");
        }
    }
}

