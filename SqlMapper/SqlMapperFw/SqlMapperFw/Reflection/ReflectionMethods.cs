using System;
using System.Reflection;

namespace SqlMapperFw.Reflection
{
    public static class ReflectionMethods
    {
        public static bool ImplementsInterface(this Type t, Type tIntf)
        {
            if (t == null)
                throw new ArgumentNullException("t");
            if (tIntf == null)
                throw new ArgumentNullException("tIntf");
            return tIntf.IsInterface && t.IsClass && tIntf.IsAssignableFrom(t);
        }

        public static string getTableName(this Type type)
        {
            DBTableNameAttribute tableNameAttribute = (DBTableNameAttribute)type.GetCustomAttribute(typeof(DBTableNameAttribute));
            return tableNameAttribute != null ? tableNameAttribute.Name : type.Name;
        }

        public static bool isPrimaryKey(this MemberInfo type)
        {
            PropPKAttribute pkAttribute = (PropPKAttribute)type.GetCustomAttribute(typeof(PropPKAttribute));
            return (pkAttribute != null);
        }

        public static string getDBFieldName(this MemberInfo type)
        {
            DBFieldNameAttribute fieldNameAttribute =
                (DBFieldNameAttribute)type.GetCustomAttribute(typeof(DBFieldNameAttribute));
            return (fieldNameAttribute != null)
                ? fieldNameAttribute.Name
                : (type.MemberType != MemberTypes.Property)
                    ? type.Name
                    : type.Name.Replace("set_", "").Replace("get_", "");
        }

        public static MemberInfo GetValidType(this MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    return (FieldInfo)member;
                case MemberTypes.Property:
                    return (PropertyInfo)member;
                case MemberTypes.Event:
                    return (EventInfo)member;
                default:
                    return null;
            }
        }

        public static void SetValue(this MemberInfo member, object instance, object value)
        {
            if (member.MemberType == MemberTypes.Property)
                ((PropertyInfo)member).SetValue(instance, value, null);
            else if (member.MemberType == MemberTypes.Field)
                ((FieldInfo)member).SetValue(instance, value);
            else
                throw new Exception("Property must be of type FieldInfo or PropertyInfo");
        }

        public static object GetValue(this MemberInfo member, object property)
        {
            if (member.MemberType == MemberTypes.Property)
                return ((PropertyInfo)member).GetValue(property, null);
            else if (member.MemberType == MemberTypes.Field)
                return ((FieldInfo)member).GetValue(property);
            else
                throw new Exception("Property must be of type FieldInfo or PropertyInfo");
        }
    }
}
