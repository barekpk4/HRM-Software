using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApiCore.Models.Attendance;

namespace WebApiCore.ViewModels
{
    public class AttendenceSummaryViewModel: AttendenceSummeryModel
    {
        public string EmpName { get; set; }
        public string Designation { get; set; }
        public string Department { get; set; }
        public int TotalDay { get; set; }
        public int Holiday { get; set; }
        public int Absent { get; set; }
        public int LeaveWithPay { get; set; }
        public int AttendenceDay { get; set; }
        public int LeavewithOutPay { get; set; }
        public string Remarks { get; set; }
    }
}
