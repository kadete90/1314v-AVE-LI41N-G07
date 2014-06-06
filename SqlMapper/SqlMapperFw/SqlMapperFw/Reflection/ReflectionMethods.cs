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
                    //throw new ArgumentException("MemberInfo must be if type FieldInfo, PropertyInfo or EventInfo", "member");
            }
        }

        public static void SetValue(this MemberInfo member, object property, object value)
        {
            if (member.MemberType == MemberTypes.Property)
                ((PropertyInfo)member).SetValue(property, value, null);
            else if (member.MemberType == MemberTypes.Field)
                ((FieldInfo)member).SetValue(property, value);
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
