using System;
using SqlMapperFw.Reflection;

namespace SqlMapperClient.Entities
{
    [DBTableName("Employees")]
    public class Employee
    {
        [PK("EmployeeID")]
        public Int32 ID { set; get; } //PK
        public String FirstName { set; get; }
        public String LastName { set; get; }
        public String Title { set; get; }
        //public String TitleOfCourtesy { set; get; }
        public DateTime BirthDate { set; get; }
        //public DateTime HireDate { set; get; }
        public String Address { set; get; }
        public String City { set; get; }
        //public String Region { set; get; }
        //public String PostalCode { set; get; }
        public String Country { set; get; }
        //public String HomePhone { set; get; }
        //public String Extension { set; get; }
        //public Byte[] Photo { set; get; } //bd -> image
        //public String Notes { set; get; }
        [FK]
        public Employee ReportsTo { set; get; } // FK
        //public String PhotoPath { set; get; } 
    }
}
