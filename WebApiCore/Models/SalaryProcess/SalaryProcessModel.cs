using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiCore.Models.SalaryProcess
{
    public class SalaryProcessModel
    {
        public int? ID { get; set; }
        public string EmpCode { get; set; }
        public int? StructureID { get; set; }
        public int? PeriodID { get; set; }
        public int UserTypeID { get; set; }
        public int Grade { get; set; }
        public int CompanyID { get; set; }
        public int DepartmentID { get; set; }
        public string PeriodName { get; set; }
        public int TaxYearID { get; set; }
        public int YearID { get; set; }
        public int SalaryTypeID { get; set; }
        public string Block { get; set; }
        public string EmpName { get; set; }
        public string EmailID { get; set; }
    }


    public class MobileBillSetup
    {
        public int Id { get; set; }
        public string EmpCode { get; set; }
        public int CompanyID { get; set; }
        public int UserID { get; set; }
        public string EmpName { get; set; }
        public string Designation { get; set; }
        public string Department { get; set; }
        public decimal Amount { get; set; }
        public string MobileNo { get; set; }
    }

    public class MonthlyEmpMobileBillModel
    {
        public int Id { get; set; }
        public int CompanyID { get; set; }
        public int UserID { get; set; }
        public int? PeriodID { get; set; }
        public int? YearID { get; set; }
        public int? DepartmentID { get; set; }
        public int? ProjectID { get; set; }
        public string EmpCode { get; set; }
        public string EmpName { get; set; }
        public string Designation { get; set; }
        public string Department { get; set; }
        public decimal Amount { get; set; }
        public decimal MonthlyBill { get; set; }
        public string MobileNo { get; set; }
    }
}
