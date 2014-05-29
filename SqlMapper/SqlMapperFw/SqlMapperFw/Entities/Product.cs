namespace SqlMapperFw.Entities
{
    public class Product
    {
        public int ProductID { set; get; } //PK
        public string ProductName { set; get; } 
        public string QuantityPerUnit { set; get; } 
        public decimal UnitPrice { set; get; } 
        public short UnitsInStock { set; get; } 
        public short UnitsOnOrder { set; get; }
    }
}
