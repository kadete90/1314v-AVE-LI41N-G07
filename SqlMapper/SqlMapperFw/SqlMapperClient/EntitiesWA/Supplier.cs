using System;
using System.Collections.Generic;
using SqlMapperFw.Reflection;

namespace SqlMapperClient.EntitiesWA
{
    [DBTableName("Suppliers")]
    public class Supplier
    {
        [PropPK]
        public Int32 SupplierID { set; get; }
        public String CompanyName { set; get; }
        public IEnumerable<Product> products { set; get; }
        //...
    }
}
