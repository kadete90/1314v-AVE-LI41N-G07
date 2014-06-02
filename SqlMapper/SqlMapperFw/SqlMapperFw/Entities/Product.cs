using SqlMapperFw.BuildMapper;

namespace SqlMapperFw.Entities
{
    [TableName("Products")]
    public class Product
    {
        [PropPK]
        public int ProductID { set; get; } //PK
        public string ProductName { set; get; } 
        public string QuantityPerUnit { set; get; } 
        public decimal UnitPrice { set; get; } 
        public short UnitsInStock { set; get; } 
        public short UnitsOnOrder { set; get; }
    }
}
