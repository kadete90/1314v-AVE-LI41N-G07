using System;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using SqlMapperFw.Reflection.Binder;

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

        public static bool IsPrimaryKey(this MemberInfo type)
        {
            if (type is FkMemberInfo)
                return false;
            return (type.GetCustomAttribute(typeof(PKAttribute)) != null);
        }

        public static bool IsForeignKey(this MemberInfo type)
        {
            if(type is FkMemberInfo)
                return true;
            return type.GetCustomAttribute(typeof(FKAttribute)) != null;
        }

        public static string GetTableName(this Type type)
        {
            DBTableNameAttribute tableNameAttribute = (DBTableNameAttribute)type.GetCustomAttribute(typeof(DBTableNameAttribute));
            return tableNameAttribute != null ? tableNameAttribute.Name : type.Name;
        }

        public static string GetDBFieldName(this MemberInfo type)
        {
            if (type is FkMemberInfo)
                return type.Name;
            DBNameAttribute fieldNameAttribute = (DBNameAttribute)type.GetCustomAttribute(typeof(DBNameAttribute));
            return (fieldNameAttribute != null)
                ? fieldNameAttribute.Name
                : (type.MemberType != MemberTypes.Property)
                    ? type.Name
                    : type.Name.Replace("set_", "").Replace("get_", "");
        }

        public static MemberInfo GetPkMemberInfo(this Type type)
        {
            foreach (MemberInfo member in type.GetMembers())
            {
                Object[] myAttributes = member.GetCustomAttributes(typeof(PKAttribute), true);
                if (myAttributes.Length > 0)
                {
                    return member;
                }
            }
            return null;
        }

        public static Object GetEDFieldValue(this Object instance, MemberInfo mi, AbstractBindMember bm)
        {
            FkMemberInfo fk = mi as FkMemberInfo;
            if (fk == null)
                return ValidDBValue(bm.GetValue(instance, mi));

            Object entity = bm.GetValue(instance, instance.GetType().GetMember(fk.ToBindInfo.Name)[0]);
            return bm.GetValue(entity, fk.PkInfo);
        }

        private static object ValidDBValue(object Value)
        {
            if (Value == null || String.IsNullOrEmpty(Value.ToString()) || Equals(Value, default(DateTime)))
                 return null;
            return Value;
        }

        public static Object BindEDFieldValue(this Object instance, MemberInfo mi, AbstractBindMember bm, Object dbvalue)
        {
            FkMemberInfo fk = mi as FkMemberInfo;
            if (fk == null) return bm.bind(instance, mi, dbvalue);

            Object entity = bm.GetValue(instance, instance.GetType().GetMember(fk.ToBindInfo.Name)[0]);
            return bm.bind(entity, fk.PkInfo, dbvalue);
        }

        // Converte System.Type em SqlDbType
        public static SqlDbType GetSqlDbType(this MemberInfo mi, Object instance, AbstractBindMember bm)
        {
            return new SqlParameter("x", instance.GetEDFieldValue(mi, bm)).SqlDbType;
        }

        public static SqlDbType GetSqlDbType(object value)
        {
            return new SqlParameter("x", value).SqlDbType;
        }
    }
}
