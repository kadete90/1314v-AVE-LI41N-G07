using System;
using System.Collections.Generic;
using SqlMapperFw.BuildMapper;

namespace SqlMapperFw.Entities
{
    [TableName("Employees")]
    public class Employee
    {
        public int EmployeeId { set; get; } //PK
        public string FirstName { set; get; }
        public string LastName { set; get; }
        public string Title { set; get; }
        public string TitleOfCourtesy { set; get; }
        public DateTime BirthDate { set; get; }
        public DateTime HireDate { set; get; }
        public string Address { set; get; }
        public string City { set; get; }
        public string Region { set; get; }
        public string PostalCode { set; get; }
        public string Country { set; get; }
        public string HomePhone { set; get; }
        public string Extension { set; get; }
        //public string Photo { set; get; } //bd -> image
        public string PhotoPath { set; get; } 
        public string Notes { set; get; }
        public Employee ReportsTo { set; get; }
        public IEnumerable<Order> Orders;

    }
}
