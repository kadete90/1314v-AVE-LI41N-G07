using System;

namespace SqlMapperFw.Utils
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class DBNameAttribute : Attribute
    {
        public string Name;

        public DBNameAttribute(string name)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DBTableNameAttribute : DBNameAttribute
    {
        public DBTableNameAttribute(string name) : base(name){}
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class PKAttribute : DBNameAttribute
    {
        public PKAttribute(string name) : base(name){}
    }

    // TODO AttributeTargets.All
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)] 
    public class FKAttribute : Attribute
    {
    }
}
