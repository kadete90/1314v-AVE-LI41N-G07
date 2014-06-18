using System;
using System.Collections.Generic;
using SqlMapperFw.Reflection;

namespace SqlMapperClient.EntitiesWA
{
    [DBTableName("Employees")]
    public class Employee
    {
        [PropPK]
        public Int32 EmployeeId { set; get; } //PK
        public String FirstName { set; get; }
        public String LastName { set; get; }
        public String Title { set; get; }
        public String TitleOfCourtesy { set; get; }
        public DateTime BirthDate { set; get; }
        public DateTime HireDate { set; get; }
        public String Address { set; get; }
        public String City { set; get; }
        public String Region { set; get; }
        public String PostalCode { set; get; }
        public String Country { set; get; }
        public String HomePhone { set; get; }
        public String Extension { set; get; }
        public Byte[] Photo { set; get; } //bd -> image
        public String Notes { set; get; }
        public Employee ReportsTo { set; get; } // FK
        public String PhotoPath { set; get; } 
    }
}
