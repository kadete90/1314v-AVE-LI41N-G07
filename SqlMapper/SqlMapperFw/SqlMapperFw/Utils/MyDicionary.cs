using System;
using System.Collections.Generic;
using System.Reflection;
using SqlMapperFw.Binder;

namespace SqlMapperFw.Utils
{
    //public class MyMapperDictionary : Dictionary<String, PairDataConnection>
    //{
    //    public void Add(String key, IDataMapper dataMapper, AbstractSqlConnection mySqlConnection)
    //    {
    //        Add(key, new PairDataConnection(dataMapper, mySqlConnection));
    //    }
    //}

    //public struct PairDataConnection
    //{
    //    public IDataMapper DataMapper;
    //    public AbstractSqlConnection MySqlConnection;

    //    public PairDataConnection(IDataMapper dataMapper, AbstractSqlConnection mySqlConnection)
    //    {
    //        DataMapper = dataMapper;
    //        MySqlConnection = mySqlConnection;
    //    }
    //}

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
