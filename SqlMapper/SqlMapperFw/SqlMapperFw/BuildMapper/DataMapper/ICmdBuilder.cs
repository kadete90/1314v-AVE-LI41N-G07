using SqlMapperFw.MySqlConnection;

namespace SqlMapperFw.BuildMapper.DataMapper
{
    interface ICmdBuilder : IMySqlConnection
    {
        object Execute(string name, object o);
    }

}
