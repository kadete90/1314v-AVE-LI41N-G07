using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using SqlMapperFw.BuildMapper;

namespace SqlMapperFw.DataMapper
{

    public class SqlEnumerable
    {
        //private readonly SqlCommand _cmd;
        //private readonly Dictionary<string, PairInfoBind>.ValueCollection _values;
        //private readonly PairInfoBind _value;
        //private readonly IDataMapper _dataMapper;

        public SqlEnumerable(SqlCommand cmd, Dictionary<string, PairInfoBind>.ValueCollection values, PairInfoBind value, IDataMapper dataMapper)
        {
            throw new NotImplementedException();
            //_cmd = cmd;
            //_values = values;
            //_value = value;
            //_dataMapper = dataMapper;
            //_dataMapper = (IDataMapper)Activator.CreateInstance(dataMapper.GetType());


        }
        public SqlEnumerable Where(string clause)
        {
            throw new NotImplementedException();
            //if (clause == null)
            //    throw new ArgumentNullException("clause");
            //_cmd.CommandText += ((!_cmd.CommandText.Contains("WHERE")) ? " WHERE " : " AND ") + clause;
            //return new SqlEnumerable(_cmd, _values, _value, _dataMapper);
        }
    }

    public sealed class SqlEnumerable<T> : ISqlEnumerable<T>
    {
        internal readonly SqlCommand SqlCommand;
        internal readonly Dictionary<string, PairInfoBind>.ValueCollection MembersInfoBind;
        internal readonly PairInfoBind PkMemberInfoBind;
        internal readonly CloseConnection CloseSqlConnection;

        public delegate void CloseConnection();

        //TODO problema: depois de fazer um where num sqlEnumerable todos os outros sqlEnumerables desse Builder terão a clausula where
        public SqlEnumerable(SqlCommand cmd, 
            Dictionary<string, PairInfoBind>.ValueCollection membersInfoBind, 
            PairInfoBind pkMemberInfoBind,
            CloseConnection CloseSqlConnection)
        {
            SqlCommand = cmd;
            MembersInfoBind = membersInfoBind;
            PkMemberInfoBind = pkMemberInfoBind;
            this.CloseSqlConnection = CloseSqlConnection;
        }

        public ISqlEnumerable<T> Where(string clause)
        {
            if (clause == null)
                throw new ArgumentNullException("clause");
            SqlCommand sqlCommand = SqlCommand;
            sqlCommand.CommandText += ((!SqlCommand.CommandText.Contains("WHERE")) ? " WHERE " : " AND ") + clause;
            return new SqlEnumerable<T>(sqlCommand, MembersInfoBind, PkMemberInfoBind, CloseSqlConnection);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new SqlEnumerator<T>(
                new SqlEnumerable<T>(SqlCommand, MembersInfoBind, PkMemberInfoBind, CloseSqlConnection));
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
            _sqlDataReader = _mySqlEnumerable.SqlCommand.ExecuteReader();
            gotDisposed = false;
        }

        public void Dispose()
        {
            if (gotDisposed) return;
            gotDisposed = true;
            _sqlDataReader.Close();
            _mySqlEnumerable.CloseSqlConnection();
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