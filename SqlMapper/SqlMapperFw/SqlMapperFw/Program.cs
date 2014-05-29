using System.Collections.Generic;
using SqlMapperFw.BuildMapper;
using SqlMapperFw.DataMappers;
using SqlMapperFw.Entities;

namespace SqlMapperFw
{
    class Program
    {
        static void Main()
        {

            Builder b = new Builder(..., ...);
            
            IDataMapper<Order> orderMapper = b.Build<Order>(); //1ªparte 1.
            IDataMapper<Customer> custMapper = b.Build<Customer>(); //1ªparte 1.
            IDataMapper<Employee> empMapper = b.Build<Employee>(); //1ªparte 1.


            IDataMapper<Product> prodMapper = b.Build<Product>(); //1ªparte 1.
            IEnumerable<Product> prods = prodMapper.GetAll();
            //prods.Where("CategoryID = 7").Where("UnitsinStock > 30"); //1ªparte 2.
        }
    }
}
