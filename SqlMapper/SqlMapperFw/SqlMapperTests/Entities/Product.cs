using SqlMapperFw;

namespace SqlMapperTests.Entities
{
    [DBTableName("Products")]
    public class Product
    {
        [PropPK]
        [DBFieldName("ProductId")]
        public int id { set; get; } //PK
        public string ProductName { set; get; } 
        public string QuantityPerUnit { set; get; } 
        public decimal UnitPrice { set; get; } 
        public short UnitsInStock { set; get; } 
        public short UnitsOnOrder { set; get; }

        public Product(int _id, string _ProductName, string _QuantityPerUnit, 
                decimal _UnitPrice,short _UnitsInStock, short _UnitsOnOrder)
        {
            id = _id;
            ProductName = _ProductName;
            QuantityPerUnit = _QuantityPerUnit;
            UnitPrice = _UnitPrice;
            UnitsInStock = _UnitsInStock;
            UnitsOnOrder = _UnitsOnOrder;
        }
    }
}
