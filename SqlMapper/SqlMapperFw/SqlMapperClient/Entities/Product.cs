using System;
using SqlMapperFw.Utils;

namespace SqlMapperClient.Entities
{
    [DBTableName("Products")]
    public class Product
    {
        [PK("ProductId")]
        public Int32 ID
        { set; get; } //PK
        public String ProductName { set; get; }
        [FK]
        public Supplier Supplier { get; set; }
        public String QuantityPerUnit { set; get; } 
        public Decimal UnitPrice { set; get; } 
        public short UnitsInStock { set; get; } 
        public short UnitsOnOrder { set; get; }
        public short ReorderLevel;
        public Boolean Discontinued;
    }
}
