using System;

namespace SqlMapperFw.BuildMapper
{

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    class TableNameAttribute : Attribute
    {
        public string name;

        public TableNameAttribute(string name)
        {
            this.name = name;
        }
    }
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    class PropNameAttribute : Attribute
    {
        public string name;

        public PropNameAttribute(string name)
        {
            this.name = name;
        }
    }
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    class PropPKAttribute : Attribute
    {
        public bool pk = true;
    }
}
