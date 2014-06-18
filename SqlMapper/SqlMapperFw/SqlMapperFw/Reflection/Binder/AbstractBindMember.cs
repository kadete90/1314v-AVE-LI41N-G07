using System;
using System.Reflection;

namespace SqlMapperFw.Reflection.Binder
{
    public abstract class AbstractBindMember
    {
        public bool bind<T>(T instance, MemberInfo mi, object dbvalue)
        {
            MemberInfo mymi = instance.GetType().GetMember(mi.Name)[0];
            if (mymi == null) { return false; }

            if (ReferenceEquals(dbvalue.ToString(), "")) { return false; }

            SetValue(instance, mymi, dbvalue);
            return true;
        }

        public abstract MemberInfo GetMemberInfoValid(MemberInfo mi);

        protected abstract void SetValue<T>(T instance, MemberInfo mi, Object value);
        public abstract Object GetValue<T>(T instance, MemberInfo mi);
    }
}
