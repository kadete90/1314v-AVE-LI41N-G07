using System;

namespace SqlMapperFw.Reflection
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DBTableNameAttribute : Attribute
    {
        public string Name;

        public DBTableNameAttribute(string name)
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
