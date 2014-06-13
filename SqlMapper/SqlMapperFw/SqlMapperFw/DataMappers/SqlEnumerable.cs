//1.2ªparte

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;
using SqlMapperFw.BuildMapper;
using SqlMapperFw.MySqlConnection;
using SqlMapperFw.Reflection.Binder;

namespace SqlMapperFw.DataMappers
{
    public sealed class SqlEnumerable<T> : ISqlEnumerable<T>
    {
        internal readonly SqlCommand SqlCommand;
        internal readonly List<AbstractBindMember> BindMembers;
        internal readonly List<MemberInfo> MemberInfos;
        internal readonly MemberInfo PkMemberInfo;
        internal readonly AbstractMapperSqlConnection<T> _mapperSqlConnection;

        public SqlEnumerable(SqlCommand cmd, List<AbstractBindMember> bindMembers,
            List<MemberInfo> memberInfos, MemberInfo pkMemberInfo, AbstractMapperSqlConnection<T> mapperSqlConnection)
        {
            SqlCommand = cmd;
            BindMembers = bindMembers;
            MemberInfos = memberInfos;
            PkMemberInfo = pkMemberInfo;
            _mapperSqlConnection = mapperSqlConnection;
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
        readonly AbstractMapperSqlConnection<T> _mapperSqlConnection;

        private bool gotCurrent;
        private bool gotDisposed;
        private T current;

        public SqlEnumerator(SqlEnumerable<T> mySqlEnumerable)
        {
            _mySqlEnumerable = mySqlEnumerable;
            _mapperSqlConnection = _mySqlEnumerable._mapperSqlConnection;
            _sqlDataReader = _mapperSqlConnection.ReadTransaction(_mySqlEnumerable.SqlCommand);
            gotDisposed = false;
        }

        public void Dispose()
        {
            if (gotDisposed) return;
            gotDisposed = true;
            _sqlDataReader.Close();
            if (_mapperSqlConnection.autoCommit)
                _mapperSqlConnection.Commit();
        }

        public bool MoveNext()
        {
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
            if(!gotDisposed)
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
