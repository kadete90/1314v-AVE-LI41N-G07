using System;
using SqlMapperFw.Reflection;

namespace SqlMapperClient.EntitiesWA
{
    [DBTableName("Customers")]
    public class Customer
    {
        [PropPK]
        [DBFieldName("CustomerId")] 
        public String id { set; get; }  //PK
        public String CompanyName { set; get; }
        public String ContactName { set; get; }
        public String ContactTitle { set; get; }
        public String Address { set; get; }
        public String City { set; get; }
        public String Region { set; get; }
        [DBFieldName("PostalCode")]
        public String Postal { set; get; }
        public String Country { set; get; }
        public String Phone { set; get; }
        public String Fax { set; get; }
    }
}
