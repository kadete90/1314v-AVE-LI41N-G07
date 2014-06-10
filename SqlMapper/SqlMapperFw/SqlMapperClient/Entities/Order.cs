using System;
using SqlMapperFw;

namespace SqlMapperClient.Entities
{

    [DBTableName("Orders")]
    public class Order
    {
        [PropPK]
        public int OrderId { set; get; } //PK
        //public Customer Customer { set; get; } //FK
        public String CustomerId;
        //public Employee Employee { set; get; } //FK
        public int EmployeeId;
        public DateTime OrderDate { set; get; }
        public DateTime RequiredDate { set; get; }
        public DateTime ShippedDate { set; get; }
        public int ShipVia { set; get; } //FK
        public Decimal Freight { set; get; } //db -> money
        public string ShipName { set; get; }
        public string ShipAddress { set; get; }
        public string ShipCity { set; get; }
        public string ShipRegion { set; get; }
        public string ShipPostalCode { set; get; }
        public string ShipCountry { set; get; } 
    }
}
