using System.Reflection;

namespace SqlMapperFw.Reflection.Binder
{
    public class BindProperties : AbstractBindMember
    {
        public override MemberInfo GetMemberInfoValid(MemberInfo mi)
        {
            return (mi.MemberType == MemberTypes.Property) ? (PropertyInfo)mi : null;
        }

        public override void SetValue<T>(T instance, MemberInfo mi, object value)
        {
            if(mi.MemberType == MemberTypes.Property)
                ((PropertyInfo)mi).SetValue(instance, value);
        }

        public override object GetValue<T>(T instance, MemberInfo mi)
        {
            return (mi.MemberType == MemberTypes.Property)?((PropertyInfo)mi).GetValue(instance, null): null;
        }
    }
}
