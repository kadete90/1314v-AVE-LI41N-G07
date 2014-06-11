using System.Reflection;

namespace SqlMapperFw.Reflection.Binder
{
    public abstract class AbstractBindMember
    {
        public bool bind(object instance, MemberInfo memberInfo, object dbvalue)
        {
            if (GetMemberInfoValid(memberInfo) == null) { return false; }

            MemberInfo mymi = instance.GetType().GetMember(memberInfo.Name)[0];
            if (mymi == null) { return false; }

            if (ReferenceEquals(dbvalue.ToString(), "")) { return false; }

            mymi.SetValue(instance, dbvalue);
            return true;
        }

        public abstract MemberInfo GetMemberInfoValid(MemberInfo mi);
    }
}
