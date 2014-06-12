//1.2ªparte

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;
using SqlMapperFw.BuildMapper;
using SqlMapperFw.Reflection.Binder;

namespace SqlMapperFw.DataMappers
{
    public sealed class SqlEnumerable<T> : ISqlEnumerable<T>
    {
        internal readonly SqlCommand SqlCommand;
        internal readonly List<AbstractBindMember> BindMembers;
        internal readonly List<MemberInfo> MemberInfos;
        internal readonly MemberInfo PkMemberInfo;

        public SqlEnumerable(SqlCommand cmd, List<AbstractBindMember> bindMembers, 
            List<MemberInfo> memberInfos, MemberInfo pkMemberInfo)
        {
            SqlCommand = cmd;
            BindMembers = bindMembers;
            MemberInfos = memberInfos;
            PkMemberInfo = pkMemberInfo;
        }

        public ISqlEnumerable<T> Where(string clause)
        {
            if (clause == null)
                throw new ArgumentNullException("clause");
            SqlCommand.CommandText += ((!SqlCommand.CommandText.Contains("WHERE")) ? " WHERE " : " AND ") + clause;
            return this;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new SqlEnumerator<T>(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public sealed class SqlEnumerator<T> : IEnumerator<T>
    {
        readonly SqlEnumerable<T> _mySqlEnumerable;
        readonly SqlDataReader _sqlDataReader;

        private bool gotCurrent;
        private T current;

        public SqlEnumerator(SqlEnumerable<T> mySqlEnumerable)
        {
            _mySqlEnumerable = mySqlEnumerable;
            _sqlDataReader = _mySqlEnumerable.SqlCommand.ExecuteReader();
        }

        public void Dispose()
        {
            _sqlDataReader.Close();
        }

        public bool MoveNext()
        {
            if (_sqlDataReader.IsClosed)
                return false;

            foreach (var DBRowValues in _sqlDataReader.AsEnumerable())
            {
                T newInstance = (T)Activator.CreateInstance(typeof(T));
                
                foreach (AbstractBindMember bm in _mySqlEnumerable.BindMembers)
                    if (bm.bind(newInstance, _mySqlEnumerable.PkMemberInfo, DBRowValues[0]))
                        break;

                int idx = 1;
                List<MemberInfo> MemberInfos = _mySqlEnumerable.MemberInfos;
                foreach (MemberInfo mi in MemberInfos)
                {
                     foreach (AbstractBindMember bm in _mySqlEnumerable.BindMembers)
                        if (bm.bind(newInstance, mi, DBRowValues[idx]))
                            break;
                    idx++;
                }
                gotCurrent = true;
                current = newInstance;
                return true;
            }
            Dispose();
            return false;
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        public T Current
        {
            get
            {
                if (!gotCurrent)
                {
                    throw new InvalidOperationException();
                }
                return current;
            }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }
    }
}
