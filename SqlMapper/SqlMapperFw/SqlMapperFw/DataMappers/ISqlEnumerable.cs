using System.Collections.Generic;

namespace SqlMapperFw.DataMappers
{
    //1ªparte 2.
    public interface ISqlEnumerable<out T> : IEnumerable<T>
    {
        ISqlEnumerable<T> Where(string clause);
    }
}
