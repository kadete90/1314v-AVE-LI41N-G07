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
        internal readonly SqlCommand _sqlCommand;
        internal readonly List<AbstractBindMember> _bindMembers;
        internal readonly Dictionary<string, MemberInfo>.ValueCollection _values;

        public SqlEnumerable(SqlCommand cmd, List<AbstractBindMember> bindMembers, Dictionary<string, MemberInfo>.ValueCollection values)
        {
            _sqlCommand = cmd;
            _bindMembers = bindMembers;
            _values = values;
        }

        public ISqlEnumerable<T> Where(string clause)
        {
            if (clause == null)
                throw new ArgumentNullException("clause");
            _sqlCommand.CommandText += ((!_sqlCommand.CommandText.Contains("WHERE")) ? " WHERE " : " AND ") + clause;
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
        private readonly SqlEnumerable<T> _mySqlEnumerable;
        private readonly SqlDataReader _sqlDataReader;

        private bool gotCurrent;
        private T current;

        public SqlEnumerator(SqlEnumerable<T> mySqlEnumerable)
        {
            _mySqlEnumerable = mySqlEnumerable;
            _sqlDataReader = _mySqlEnumerable._sqlCommand.ExecuteReader();
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
                Dictionary<string, MemberInfo>.ValueCollection.Enumerator MemberInfos = _mySqlEnumerable._values.GetEnumerator();
                foreach (object value in DBRowValues)
                {
                    if (!MemberInfos.MoveNext())
                        break;
                    foreach (AbstractBindMember bm in _mySqlEnumerable._bindMembers)
                        if (bm.bind(newInstance, MemberInfos.Current, value))
                            break;
                    
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
