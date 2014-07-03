using System;
using System.Reflection;

namespace SqlMapperFw.Binder
{
    public class BindFields : AbstractBindMember
    {
        public override MemberInfo GetMemberInfo(MemberInfo mi)
        {
            return (mi.MemberType == MemberTypes.Field) ? mi : null;
        }

        public override Type GetMemberType(MemberInfo mi)
        {
            return ((FieldInfo)mi).FieldType;
        }

        protected override void SetValue<T>(T instance, MemberInfo mi, object value)
        {
            if(mi.MemberType == MemberTypes.Field)
                ((FieldInfo) mi).SetValue(instance, value);
        }

        public override object GetValue<T>(T instance, MemberInfo mi)
        {
            return (mi.MemberType == MemberTypes.Field)?((FieldInfo)mi).GetValue(instance): null;
        }
    }
}
