using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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

        public static object GetValue(this MemberInfo member, object instance)
        {
            if (member.MemberType == MemberTypes.Property)
                return ((PropertyInfo)member).GetValue(instance, null);
            else if (member.MemberType == MemberTypes.Field)
                return ((FieldInfo)member).GetValue(instance);
            else
                throw new Exception("Property must be of type FieldInfo or PropertyInfo");
        }
        
        // Converte System.type em SqlDbType
        public static SqlDbType GetSqlDbType<T>(this MemberInfo m, Object instance)
        {
            return new SqlParameter("x", m.GetValue(instance)).SqlDbType;
        }

        public static string StringBuilder(Dictionary<string, MemberInfo>.KeyCollection keyCollection)
        {
            String s = "";
            foreach (String fieldName in keyCollection)
                    s += "@" + fieldName + ", ";
            if (s != "")
                s = s.Substring(0, s.Length - 2);
            return s;
        }
    }
}
