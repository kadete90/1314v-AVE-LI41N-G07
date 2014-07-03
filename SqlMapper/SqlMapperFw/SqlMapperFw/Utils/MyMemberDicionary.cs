using System;
using System.Collections.Generic;
using System.Reflection;
using SqlMapperFw.Binder;

namespace SqlMapperFw.Utils
{
    public struct PairInfoBind
    {
        public MemberInfo MemberInfo;
        public AbstractBindMember BindMember;

        public PairInfoBind(MemberInfo memberInfo, AbstractBindMember bindMember)
        {
            MemberInfo = memberInfo;
            BindMember = bindMember;
        }
    }

    public class MyMemberDictionary : Dictionary<String, PairInfoBind>
    {
        public void Add(String key, MemberInfo memberInfo, AbstractBindMember bindMember)
        {
            Add(key, new PairInfoBind(memberInfo, bindMember));
        }
    }
}
