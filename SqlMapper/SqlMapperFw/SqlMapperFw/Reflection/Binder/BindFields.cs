using System.Reflection;

namespace SqlMapperFw.Reflection.Binder
{
    public class BindFields : AbstractBindMember
    {
        public override MemberInfo GetMemberInfoValid(MemberInfo mi)
        {
            var xpto = (mi.MemberType == MemberTypes.Field) ? (FieldInfo) mi : null;
            return xpto;
        }

        public override void SetValue<T>(T instance, MemberInfo mi, object value)
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
