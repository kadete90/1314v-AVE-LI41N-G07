using System.Collections.Generic;

namespace SqlMapperFw.DataMapper
{
    //1ªparte 2.
    public interface ISqlEnumerable<T> : IEnumerable<T>
    {
        ISqlEnumerable<T> Where(string clause);
    }
}
