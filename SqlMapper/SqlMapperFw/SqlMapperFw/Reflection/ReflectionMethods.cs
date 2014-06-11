using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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

        public static void SetValue(this MemberInfo member, object instance, object value)
        {
            switch (member.MemberType){
                case MemberTypes.Property:
                    ((PropertyInfo) member).SetValue(instance, value, null);
                    break;
                case (MemberTypes.Field):
                    ((FieldInfo) member).SetValue(instance, value);
                    break;
                default:
                    throw new Exception("member must be of type FieldInfo or PropertyInfo");
            }
        }

        public static object GetValue(this MemberInfo member, object instance)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Property:
                    return ((PropertyInfo)member).GetValue(instance, null);
                case (MemberTypes.Field):
                    return ((FieldInfo)member).GetValue(instance);
                default:
                    throw new Exception("member must be of type FieldInfo or PropertyInfo");
            }
        }

        // Converte System.type em SqlDbType
        public static SqlDbType GetSqlDbType(this MemberInfo m, Object instance)
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
