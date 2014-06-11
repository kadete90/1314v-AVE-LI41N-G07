using System.Reflection;

namespace SqlMapperFw.Reflection.Binder
{
    public class BindFields : AbstractBindMember
    {
        public override MemberInfo GetMemberInfoValid(MemberInfo mi)
        {
            return (mi.MemberType == MemberTypes.Field) ? (FieldInfo)mi : null;
        }
    }
}
