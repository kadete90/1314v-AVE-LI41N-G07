using System.Collections.Generic;

namespace SqlMapperFw.BuildMapper.DataMapper
{
    public interface ISqlEnumerable<out T> : IEnumerable<T>
    {
        ISqlEnumerable<T> Where(string clause);
        int Count();
    }
}
