using System.Collections.Generic;
using SqlMapperFw.BuildMapper;

namespace SqlMapperClient.Entities
{

    [TableName("Customers")]
    public class Customer
    {
        [PropPK]
        [DBFieldNameAttribute("CustomerId")]
        public int id { set; get; }  //PK
        public string CompanyName { set; get; } 
        public string ContactName { set; get; } 
        public string ContactTitle { set; get; } 
        public string Address { set; get; } 
        public string City { set; get; } 
        public string Region { set; get; }
        [DBFieldNameAttribute("PostalCode")]
        public string Postal { set; get; } 
        public string Country { set; get; } 
        public string Phone { set; get; } 
        public string Fax { set; get; }
        public IEnumerable<Order> Orders;
    }
}
