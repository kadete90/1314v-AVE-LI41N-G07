﻿using System;
using SqlMapperFw.Reflection;

namespace SqlMapperClient.Entities
{
    [DBTableName("Products")]
    public class Product
    {
        [PropPK]
        [DBFieldName("ProductId")]
        public int id { set; get; } //PK
        public String ProductName { set; get; }
        public String QuantityPerUnit { set; get; } 
        public Decimal UnitPrice { set; get; } 
        public short UnitsInStock { set; get; } 
        public short UnitsOnOrder { set; get; }
        public short ReorderLevel;
        public Boolean Discontinued;

    }
}
