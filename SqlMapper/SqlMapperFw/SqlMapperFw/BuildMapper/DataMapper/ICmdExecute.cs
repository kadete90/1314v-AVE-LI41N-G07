using SqlMapperFw.MySqlConnection;

namespace SqlMapperFw.BuildMapper.DataMapper
{
    interface ICmdExecute : IMySqlConnection
    {
        object Execute(string name, object o);
    }

}
