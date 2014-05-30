using System;

namespace SqlMapperFw.BuildMapper
{

    [AttributeUsage(AttributeTargets.Class)]
    class TableNameAttribute : Attribute
    {
        public string name;

        public TableNameAttribute(string name)
        {
            this.name = name;
        }

    }
}
