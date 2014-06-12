using System.Collections.Generic;
using System.Data.SqlClient;

namespace SqlMapperFw.DataMappers
{
    //1ªparte
    public interface IDataMapper<T>
    {
        // Devolve todos os elementos da tabela correspondente
        //IEnumerable<T> GetAll();        //1.
        ISqlEnumerable<T> GetAll();   //2.
        void Insert(T val); // Insere uma nova linha com os valores de val e actualiza val com a PK devolvida
        void Update(T val); // Actualiza a linha que tem PK igual à propriedade PK de val (ler cap. Requisitos)
        void Delete(T val); // Apaga a linha com PK igual à propriedade PK de val

    }
    //2ªparte
    //public interface IDataMapper<T> : IDataMapper
    //{
    //    new SqlEnumerable<T> GetAll();
    //    void Insert(T val);
    //    void Update(T val);
    //    void Delete(T val);

    //}

    //public interface IDataMapper
    //{
    //    SqlEnumerable GetAll();
    //    void Insert(object val);
    //    void Update(object val); 
    //    void Delete(object val); 
    //}
}
