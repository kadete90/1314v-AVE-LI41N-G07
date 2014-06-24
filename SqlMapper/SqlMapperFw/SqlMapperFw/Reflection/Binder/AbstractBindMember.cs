using System;
using System.Reflection;

namespace SqlMapperFw.Reflection.Binder
{
    public abstract class AbstractBindMember
    {
        public bool bind<T>(T instance, MemberInfo mi, object dbvalue)
        {
            try
            {
                var info = mi as FkMemberInfo;
                if (info != null)
                {
                    MemberInfo mymi = instance.GetType().GetMember(info.ToBindInfo.Name)[0];
                    if (mymi == null || ReferenceEquals(dbvalue.ToString(), "")) return false;

                    SetValue(info.MyInstance, info.PkInfo, dbvalue);
                    SetValue(instance, mymi, info.MyInstance);
                }
                else
                {
                    MemberInfo mymi = instance.GetType().GetMember(mi.Name)[0];
                    if (mymi == null || ReferenceEquals(dbvalue.ToString(), "")) return false;

                    SetValue(instance, mymi, dbvalue);
                }
                    

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Could not bind: " + dbvalue + " on " + mi.Name + "\n"+ ex);
            }  
        }

        public abstract MemberInfo GetMemberInfo(MemberInfo mi);

        protected abstract void SetValue<T>(T instance, MemberInfo mi, Object value);
        public abstract Object GetValue<T>(T instance, MemberInfo mi);
    }
}
