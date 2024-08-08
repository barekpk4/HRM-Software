using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiCore.Models.Attendance
{
    public class WeekEndSetupModel
    {
        public int ID {get;set;}
        public string EmpCode {get;set;}
        public string WeekEndDay {get;set;}
        public DateTime SysDate  { get; set; }
        public int UserID {get;set;}
        public int CompanyID {get;set;}
        public int? DepartmentID  {get;set;}
        public int? DesignationID {get;set;}
        public int? BranchID {get;set;}
        public int? LocationID {get;set;}
        public int POptions { get; set; }
        public string Day { get; set; }
        public int Grade { get; set; }
        public List<WeekEndSetupFormList> WeekEndFormArray  {get;set;}
    }

    public class WeekEndSetupFormList
    {
        public string EmpCode { get; set; }
        public string EmpName { get; set; }
        public string Designation { get; set; }
        public string Department { get; set; }
        public string WeekEndDay { get; set; }
        
    }
}
