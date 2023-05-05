namespace BehaviorDesigner.Editor
{
    using BehaviorDesigner.Runtime.Tasks;
    using System;
    using System.Collections.Generic;

    public class TypeNameComparer : IComparer<Type>
    {
        public int Compare(Type x, Type y)
        {
            string name = x.Name;
            string str2 = y.Name;
            int length = name.Length;
            int num2 = str2.Length;
            int num3 = 0;
            for (int i = 0; (num3 < length) && (i < num2); i++)
            {
                int num5 = 0;
                if (!char.IsDigit(name[num3]) || !char.IsDigit(name[i]))
                {
                    num5 = name[num3].CompareTo(str2[i]);
                }
                else
                {
                    string s = string.Empty;
                    while (true)
                    {
                        if ((num3 >= length) || !char.IsDigit(name[num3]))
                        {
                            string str4 = string.Empty;
                            while (true)
                            {
                                if ((i >= num2) || !char.IsDigit(str2[i]))
                                {
                                    int result = 0;
                                    int.TryParse(s, out result);
                                    int num7 = 0;
                                    int.TryParse(str4, out num7);
                                    num5 = result.CompareTo(num7);
                                    break;
                                }
                                str4 = str4 + str2[i];
                                i++;
                            }
                            break;
                        }
                        s = s + name[num3];
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

