using System;

namespace SqlMapperFw.BuildMapper
{

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TableNameAttribute : Attribute
    {
        public string Name;

        public TableNameAttribute(string name)
        {
            Name = name;
        }
    }
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class DBFieldNameAttribute : Attribute
    {
        public string Name;

        public DBFieldNameAttribute(string name)
        {
            Name = name;
        }
    }
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class PropPKAttribute : Attribute
    {
    }
}
