using System.Collections.Generic;
using SqlMapperFw.BuildMapper;

namespace SqlMapperFw.Entities
{

    [TableName("Customers")]
    public class Customer
    {
        public int CustomerId { set; get; }  //PK
        public string CompanyName { set; get; } 
        public string ContactName { set; get; } 
        public string ContactTitle { set; get; } 
        public string Address { set; get; } 
        public string City { set; get; } 
        public string Region { set; get; } 
        public string PostalCode { set; get; } 
        public string Country { set; get; } 
        public string Phone { set; get; } 
        public string Fax { set; get; }
        public IEnumerable<Order> Orders;
    }
}
