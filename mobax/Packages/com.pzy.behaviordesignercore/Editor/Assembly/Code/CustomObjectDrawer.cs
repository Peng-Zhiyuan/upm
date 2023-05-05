namespace BehaviorDesigner.Editor
{
    using System;

    [AttributeUsage(AttributeTargets.Class, Inherited=false, AllowMultiple=true)]
    public sealed class CustomObjectDrawer : Attribute
    {
        private System.Type type;

        public CustomObjectDrawer(System.Type type)
        {
            this.type = type;
        }

        public System.Type Type
        {
            get
            {
                return this.type;
            }
        }
    }
}

