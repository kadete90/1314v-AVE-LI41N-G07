using SqlMapperFw.Reflection;

namespace SqlMapperClient.Entities
{

    [DBTableName("Customers")]
    public class Customer
    {
        [PropPK]
        [DBFieldName("CustomerId")]
        public string id { set; get; }  //PK
        public string CompanyName { set; get; } 
        public string ContactName { set; get; } 
        public string ContactTitle { set; get; } 
        public string Address { set; get; } 
        public string City { set; get; } 
        public string Region { set; get; }
        [DBFieldName("PostalCode")]
        public string Postal { set; get; } 
        public string Country { set; get; } 
        public string Phone { set; get; } 
        public string Fax { set; get; }
        //public IEnumerable<Order> Orders;
    }
}
