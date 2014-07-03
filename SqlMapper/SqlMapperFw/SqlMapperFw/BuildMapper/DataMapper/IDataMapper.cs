using System;

namespace SqlMapperFw.BuildMapper.DataMapper
{
    public interface IDataMapper<T> : IDataMapper
    {
        new ISqlEnumerable<T> GetAll();
        T GetById(Object id);
        void Insert(T val);
        void Update(T val);
        void Delete(T val);
    }

    public interface IDataMapper
    {
        SqlEnumerable GetAll();
        void Insert(object val);
        void Update(object val);
        void Delete(object val);
    }
}
