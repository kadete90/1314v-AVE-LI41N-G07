using System;
using SqlMapperFw.Utils;

namespace SqlMapperClient.Entities
{
    [DBTableName("Suppliers")]
    public class Supplier
    {
        public String CompanyName { set; get; }

        [PK("SupplierID")]
        public Int32 ID { set; get; }
        
        //public IEnumerable<Employee> Employees { set; get; }
        //...
    }
}
