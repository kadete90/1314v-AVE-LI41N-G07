﻿using System;
using System.Reflection;

namespace SqlMapperFw.Binder
{
    public class BindProperties : AbstractBindMember
    {
        public override MemberInfo GetMemberInfo(MemberInfo mi)
        {
            return (mi.MemberType == MemberTypes.Property) ? mi : null;
        }

        public override Type GetMemberType(MemberInfo mi)
        {
            return ((PropertyInfo)mi).PropertyType;
        }

        protected override void SetValue<T>(T instance, MemberInfo mi, object value)
        {
            if(mi.MemberType == MemberTypes.Property)
                ((PropertyInfo)mi).
                    SetValue(instance, value);
        }

        public override object GetValue<T>(T instance, MemberInfo mi)
        {
            return (mi.MemberType == MemberTypes.Property)?((PropertyInfo)mi).GetValue(instance, null): null;
        }
    }
}
