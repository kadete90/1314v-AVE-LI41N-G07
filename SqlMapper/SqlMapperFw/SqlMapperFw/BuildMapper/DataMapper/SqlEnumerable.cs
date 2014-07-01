using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using SqlMapperFw.Utils;

namespace SqlMapperFw.BuildMapper.DataMapper
{

    public class SqlEnumerable
    {
        public SqlEnumerable()
        {
            throw new NotImplementedException();
        }
        public SqlEnumerable Where(string clause)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class SqlEnumerable<T> : ISqlEnumerable<T>
    {
        internal readonly SqlCommand MySqlCommand;
        private readonly string _tableName;
        internal readonly Dictionary<string, PairInfoBind>.ValueCollection MembersInfoBind;
        internal readonly PairInfoBind PkMemberInfoBind;
        internal readonly CloseConnection CloseSqlConnection;  //depends on type of connection
        internal List<String> WhereClauses;

        public delegate void CloseConnection();

        //TODO problema: depois de fazer um where num sqlEnumerable todos os outros sqlEnumerables desse Builder terão a clausula where
        public SqlEnumerable(SqlCommand cmd,
            String tableName,
            Dictionary<string, PairInfoBind>.ValueCollection membersInfoBind, 
            PairInfoBind pkMemberInfoBind,
            CloseConnection closeSqlConnection) //depends on type of connection
        {
            MySqlCommand = cmd;
            _tableName = tableName;
            MembersInfoBind = membersInfoBind;
            PkMemberInfoBind = pkMemberInfoBind;
            CloseSqlConnection = closeSqlConnection;
            WhereClauses = new List<string>();
        }

        public SqlEnumerable(SqlCommand cmd,
            String tableName,
            Dictionary<string, PairInfoBind>.ValueCollection membersInfoBind,
            PairInfoBind pkMemberInfoBind,
            CloseConnection closeSqlConnection, //depends on type of connection
            List<string> whereClauses)
        {
            MySqlCommand = cmd;
            _tableName = tableName;
            MembersInfoBind = membersInfoBind;
            PkMemberInfoBind = pkMemberInfoBind;
            CloseSqlConnection = closeSqlConnection;
            WhereClauses = whereClauses;
        }

        public ISqlEnumerable<T> Where(string clause)
        {
            if (clause == null)
                throw new ArgumentException("clause invalid");
            WhereClauses.Add(clause);
            return new SqlEnumerable<T>(MySqlCommand, _tableName, MembersInfoBind, PkMemberInfoBind, CloseSqlConnection, WhereClauses);
        }

        //public int Count()
        //{
        //    String aux = MySqlCommand.CommandText;
        //    MySqlCommand.CommandText += " SELECT COUNT(*) FROM " + _tableName;
        //    int count = MySqlCommand.ExecuteReader().GetFieldValue<int>(0);
        //    MySqlCommand.CommandText = aux;//repor query
        //    CloseSqlConnection();
        //    return count;
        //}

        public IEnumerator<T> GetEnumerator()
        {
            return new SqlEnumerator<T>(
                new SqlEnumerable<T>(MySqlCommand, _tableName, MembersInfoBind, PkMemberInfoBind, CloseSqlConnection, WhereClauses));
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
        private bool gotDisposed;
        private T current;

        public SqlEnumerator(SqlEnumerable<T> mySqlEnumerable)
        {
            _mySqlEnumerable = mySqlEnumerable;

            String whereClauses = "";
            foreach (String clause in _mySqlEnumerable.WhereClauses)
                whereClauses += ((ReferenceEquals(whereClauses, "")) ? " WHERE " : " AND ") + clause;

            SqlCommand cmd = _mySqlEnumerable.MySqlCommand;
            String aux = cmd.CommandText;

            cmd.CommandText += whereClauses;
            _sqlDataReader = _mySqlEnumerable.MySqlCommand.ExecuteReader();
            
            cmd.CommandText = aux;//repor query sem clausulas where
            gotDisposed = false;
        }

        public void Dispose()
        {
            if (gotDisposed) return;
            gotDisposed = true;
            _sqlDataReader.Close();
            _mySqlEnumerable.CloseSqlConnection();
            _mySqlEnumerable.WhereClauses.Clear();
        }

        public bool MoveNext()
        {
            if (_sqlDataReader == null)
                return false;
            while (_sqlDataReader.Read())
            {
                T newInstance = (T)Activator.CreateInstance(typeof(T));
                int idx = 0;

                Object[] DBRowValues = new Object[_sqlDataReader.FieldCount];

                if (_sqlDataReader.GetValues(DBRowValues) == 0)
                    continue;

                //pk
                PairInfoBind PairPkInfoBind = _mySqlEnumerable.PkMemberInfoBind;
                PairPkInfoBind.BindMember.bind(newInstance, PairPkInfoBind.MemberInfo, DBRowValues[idx++]);
                //fields
                foreach (PairInfoBind pairInfoBind in _mySqlEnumerable.MembersInfoBind)
                    pairInfoBind.BindMember.bind(newInstance, pairInfoBind.MemberInfo, DBRowValues[idx++]);

                gotCurrent = true;
                current = newInstance;
                return true;
            }
            if (!gotDisposed)
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