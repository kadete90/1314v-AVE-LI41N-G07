﻿using System;
using SqlMapperFw.Reflection;

namespace SqlMapperClient.EntitiesWA
{

    [DBTableName("Orders")]
    public class Order
    {
        [PropPK]
        public Int32? OrderId { set; get; } //PK
        public Customer CustomerId; //FK
        public Employee EmployeeId;//FK
        public DateTime OrderDate { set; get; }
        public DateTime RequiredDate { set; get; }
        public DateTime ShippedDate { set; get; }
        //public Shipper ShipVia { set; get; } //FK
        public Int32? ShipVia { set; get; } //FK
        public Decimal Freight { set; get; } //db -> money
        public String ShipName { set; get; }
        public String ShipAddress { set; get; }
        public String ShipCity { set; get; }
        public String ShipRegion;
        public String ShipPostalCode { set; get; }
        public String ShipCountry { set; get; } 
    }
}