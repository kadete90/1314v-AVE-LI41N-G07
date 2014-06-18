using System;
using SqlMapperFw.Reflection;

namespace SqlMapperClient.Entities
{

    [DBTableName("Orders")]
    public class Order
    {
        [PropPK]
        public int OrderId { set; get; } //PK
        public String CustomerId { set; get; } //FK
        public int EmployeeId { set; get; }//FK
        //public DateTime OrderDate { set; get; }
        //public DateTime RequiredDate { set; get; }
        //public DateTime ShippedDate { set; get; }
        public int ShipVia { set; get; } //FK
        public Decimal Freight { set; get; } //db -> money
        public String ShipName { set; get; }
        public String ShipAddress { set; get; }
        //public String ShipCity { set; get; }
        //public String ShipRegion;
        public String ShipPostalCode { set; get; }
        //public String ShipCountry { set; get; } 
    }
}
