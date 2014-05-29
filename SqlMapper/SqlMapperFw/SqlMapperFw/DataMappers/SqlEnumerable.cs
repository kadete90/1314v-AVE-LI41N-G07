//2ªparte
using System.Collections;
using System.Collections.Generic;

namespace SqlMapperFw.DataMappers
{
    public class SqlEnumerable<T> : ISqlEnumerable<T>
    {
        public ISqlEnumerable<T> Where(string clause)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class SqlEnumerable
    {

    }
}
