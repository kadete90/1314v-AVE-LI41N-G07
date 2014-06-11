using System.Reflection;

namespace SqlMapperFw.Reflection.Binder
{
    public class BindProperties : AbstractBindMember
    {
        public override MemberInfo GetMemberInfoValid(MemberInfo mi)
        {
            return (mi.MemberType == MemberTypes.Property) ? (PropertyInfo)mi : null;
        }
    }
}
