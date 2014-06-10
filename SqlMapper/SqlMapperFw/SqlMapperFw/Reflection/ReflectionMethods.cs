using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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

        public static string getTypeValues<T>(this Object instance)
        {
            Type t = instance.GetType();
            String values = "";
            foreach (MemberInfo m in t.GetMembers())
            {
                if (m.GetValidType() != null && m.GetCustomAttribute(typeof(PropPKAttribute)) == null)
                {
                    var value = m.GetValue(instance);
                    if (value is string)
                        values += "'" + value + "', ";
                    else
                        values += value + ", ";
                }
            }
            if (values != "")
                values = values.Substring(0, values.Length - 2);
            return values;
        }
        
        public static SqlDbType GetSqlDbType<T>(this MemberInfo m, Object instance)
        {
            Type t = m.GetValue(instance).GetType();
            /*switch(t)
            {
                case:
                    return;
                case:
                    return;
                default:
                    return null;
            }*/
            return 0;
            //throw new ArgumentException("MemberInfo must be if type FieldInfo, PropertyInfo or EventInfo", "member");
        }
    }
}
