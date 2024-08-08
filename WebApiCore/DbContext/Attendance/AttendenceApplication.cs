using Dapper.Framework;
using HRMS.Models.SalarySetup;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using WebApiCore.Models.Attendance;
using WebApiCore.ViewModels;
using WebApiCore.ViewModels.Attendance;
using WebApiCore.ViewModels.Attendence;

namespace WebApiCore.DbContext.Attendance
{
    public partial class AttendenceApplication
    {
        public static bool SaveOrUpdateAttendence(int pOptions, AttendenceApplicationModel attendence)
        {


            var conn = new SqlConnection(Connection.ConnectionString());
            var obj = new
            {
                attendence.ID,
                attendence.EmpCode,
                attendence.Reason,
                attendence.AttnDate,
                attendence.InTime,
                attendence.OutTime,
                attendence.ApproveType,
                attendence.UserID,
                attendence.CompanyID,
                attendence.ReqTo,
                attendence.Msg,
                pOptions

            };
            int rowAffected = conn.Execute("sp_AttendanceApplication_New", param: obj, commandType: CommandType.StoredProcedure);
            return rowAffected > 0;
        }

        public static List<AttendenceApplicationModel> GetAttendenceByEmpCodeAndComId(string empCode, int comId, int pOption)
        {
            var conn = new SqlConnection(Connection.ConnectionString());
            var obj = new
            {
                EmpCode = empCode,
                CompanyID = comId,
                pOptions = pOption
            };
            List<AttendenceApplicationModel> resutl = conn.Query<AttendenceApplicationModel>("sp_AttendanceApplication_New", param: obj, commandType: CommandType.StoredProcedure).ToList();
            return resutl;
        }

        public static AttendenceApplicationModel GetByIdAttendenceApplication(int id)
        {
            var conn = new SqlConnection(Connection.ConnectionString());
            string quire = $"SELECT * FROM AttendanceApplication WHERE ID={id}";
            AttendenceApplicationModel result = conn.QuerySingle<AttendenceApplicationModel>(quire);
            return result;
        }

        //============================================ WeekEnd Setup ================================================================

        public static List<WeekEndSetupFormList> GetAllWeekEndSetupList(WeekEndSetupModel weekEndSetup, int poption)
        {

            var conn = new SqlConnection(Connection.ConnectionString());
            var obj = new
            {
                weekEndSetup.EmpCode,
                DesignationID = weekEndSetup.DesignationID ?? -1,
                Location = weekEndSetup.LocationID ?? -1,
                DepartmentID = weekEndSetup.DepartmentID ?? -1,
                BranchID = weekEndSetup.BranchID ?? -1,
                GradeValue = weekEndSetup.Grade,
                weekEndSetup.CompanyID,
                WeekEndDay = weekEndSetup.Day,
                pOptions = poption
            };
            List<WeekEndSetupFormList> resutl = conn.Query<WeekEndSetupFormList>("sp_WeekEndSetup_New", param: obj, commandType: CommandType.StoredProcedure).ToList();
            return resutl;
        }

        public static bool SaveWeekEndSetup(WeekEndSetupModel weekEndSetup, int poption)
        {
            var con = new SqlConnection(Connection.ConnectionString());
            con.Open();
            using (var tran = con.BeginTransaction())
            {
                try
                {
                    foreach (var item in weekEndSetup.WeekEndFormArray)
                    {
                        using (var cmd = new SqlCommand())
                        {
                            cmd.CommandText = "sp_WeekEndSetup_New";
                            cmd.Connection = con;
                            cmd.Transaction = tran;
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@EmpCode", item.EmpCode);
                            cmd.Parameters.AddWithValue("@WeekEndDay", item.WeekEndDay);
                            cmd.Parameters.AddWithValue("@UserID", weekEndSetup.UserID);
                            cmd.Parameters.AddWithValue("@CompanyID", weekEndSetup.CompanyID);
                            cmd.Parameters.AddWithValue("@pOptions", poption);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    tran.Commit();
                    return true;
                }
                catch (Exception err)
                {
                    tran.Rollback();
                    throw new Exception(err.Message);
                }
            }
        }

        //======================================================= Manual Attendence ========================================================

        public static bool SaveOrUpdateManualAttendence(ManualAttendenceModel attnModel)
        {
            //var attnDate = attnModel.AttnDate.Split("/");
            //string yyyymmdd = attnDate[2].PadLeft(4, '0') + attnDate[0].PadLeft(2, '0') + attnDate[1].PadLeft(2, '0') ;
            var time = attnModel.AttnTime.Split(":");
            var hour = time[0].PadLeft(2, '0');
            var minite = time[1].PadLeft(2, '0');
            var second = time[2].PadLeft(2, '0');
            var AttnTime = $"{hour}{minite}{second}";
            var conn = new SqlConnection(Connection.ConnectionString());
            string quire = $"INSERT INTO AttendencDetails(PerID,EmpCod,AttnDate,AttnTime,TYPEE,Hourr,Minutee,Secondd,DDMMYYYY,MachineName,CompanyID) VALUES" +
                $"({attnModel.PerID},{attnModel.EmpCod},'{attnModel.AttnDate}','{attnModel.AttnTime = AttnTime}'," +
                $"{attnModel.TYPEE},{attnModel.Hourr = decimal.Parse(hour)},{attnModel.Minutee = decimal.Parse(minite)},{attnModel.Secondd = decimal.Parse(second)},'{attnModel.DDMMYYYY = attnModel.AttnDate}',{attnModel.MachineName = "'Success'"},{attnModel.CompanyID})";
            int rowAffected = conn.Execute(quire);
            return rowAffected > 0;
        }


        public static List<ManualAttendenceModel> GetManualAttnByDDYYMM(string ddyymm)
        {
            //var attnDate = ddyymm.Split("/");
            //string yyyymmdd = attnDate[2].PadLeft(4, '0') + attnDate[0].PadLeft(2, '0') + attnDate[1].PadLeft(2, '0');
            string quireAll = $"SELECT * FROM AttendencDetails";
            string quire = $"SELECT * FROM AttendencDetails WHERE DDMMYYYY='{ddyymm}'";
            var conn = new SqlConnection(Connection.ConnectionString());

            switch (ddyymm)
            {
                case "":
                    string all = quireAll;
                    break;
                default:
                    string byid = quire;
                    break;
            }

            List<ManualAttendenceModel> result = conn.Query<ManualAttendenceModel>(quire).ToList();
            return result;
        }

        public static ManualAttendenceModel GetManualAttendenceById(int id)
        {
            var conn = new SqlConnection(Connection.ConnectionString());
            string quire = $"SELECT *FROM AttendencDetails WHERE ID={id}";
            ManualAttendenceModel result = conn.QuerySingle<ManualAttendenceModel>(quire);
            return result;
        }

        public static bool UpdateManualAttendence(ManualAttendenceModel attnModel)
        {
            //var attnDate = attnModel.AttnDate.Split("/");
            //string yyyymmdd = attnDate[2].PadLeft(4, '0') + attnDate[0].PadLeft(2, '0') + attnDate[1].PadLeft(2, '0');
            var time = attnModel.AttnTime.Split(":");
            var hour = time[0].PadLeft(2, '0');
            var minite = time[1].PadLeft(2, '0');
            var second = time[2].PadLeft(2, '0');
            var AttnTime = $"{hour}{minite}{second}";
            var conn = new SqlConnection(Connection.ConnectionString());
            string quire = $"UPDATE AttendencDetails set PerID={attnModel.PerID}, EmpCod='{attnModel.EmpCod}',AttnDate='{attnModel.AttnDate}'," +
                $" AttnTime='{attnModel.AttnTime = AttnTime}',TYPEE={attnModel.TYPEE},Hourr={attnModel.Hourr = decimal.Parse(hour)}," +
                $"Minutee={attnModel.Minutee = decimal.Parse(minite)},Secondd={attnModel.Secondd = decimal.Parse(second)},DDMMYYYY='{attnModel.DDMMYYYY = attnModel.AttnDate}'," +
                $"MachineName={attnModel.MachineName = "'Success'"},CompanyID={attnModel.CompanyID} WHERE ID={attnModel.ID}";
            int rowAffected = conn.Execute(quire);
            return rowAffected > 0;
        }

        public static bool DeleteManualAttendence(int id)
        {
            var conn = new SqlConnection(Connection.ConnectionString());
            string quire = $"DELETE AttendencDetails WHERE ID={id}";
            int rowAffected = conn.Execute(quire);
            return rowAffected > 0;
        }

        //============================================== Monthly Attendence ==========================================================

        // get Salary Period By Year ID
        public static List<SalaryPeriodModel> GetAllPeriodNameByYearID(int id)
        {
            var con = new SqlConnection(Connection.ConnectionString());
            List<SalaryPeriodModel> result = con.Query<SalaryPeriodModel>($"SELECT * FROM SalaryPeriod where YearID={id} Order BY Id Desc").ToList();
            return result;
        }

        // Show Attendence Data
        public static List<AttendenceSummaryViewModel> GetAttendenceData(AttendenceSummeryModel summeryModel)
        {
            var conn = new SqlConnection(Connection.ConnectionString());
            var obj = new
            {
                StrDate = summeryModel.StartDate,
                summeryModel.EndDate,
                summeryModel.CompanyID,
                summeryModel.Grade,
                Depertment = summeryModel.Depertment ?? -1,
                Branch = summeryModel.Branch ?? -1,
                Project = summeryModel.Project ?? -1,
                EmpCode = string.IsNullOrEmpty(summeryModel.EmpCode) ? "-1" : summeryModel.EmpCode
            };
            List<AttendenceSummaryViewModel> result = conn.Query<AttendenceSummaryViewModel>("spGetAttendenceSummary", param: obj, commandType: CommandType.StoredProcedure).ToList();
            return result;
        }

        // ForEdit Monthly Attendence
        public static List<AttendenceSummaryViewModel> ForEditMonthlyAttendence(AttendenceSummeryModel summeryModel)
        {
            var conn = new SqlConnection(Connection.ConnectionString());
            var obj = new
            {
                summeryModel.CompanyID,
                department = summeryModel.Depertment ?? -1,
                PeriodID = summeryModel.PeriodID ?? -1,
                project = summeryModel.Project ?? -1,
                EmpCode=string.IsNullOrEmpty(summeryModel.EmpCode) ? "-1" :summeryModel.EmpCode
            };
            List<AttendenceSummaryViewModel> result = conn.Query<AttendenceSummaryViewModel>("spGetAttendenceSummaryforEdit", param: obj, commandType: CommandType.StoredProcedure).ToList();
            return result;
        }

        //========================= Save Monthly Attendence =============================================================

        public static bool SaveMonthlyAttendence(AttendenceSummeryModel summeryModel)
        {
            var conn = new SqlConnection(Connection.ConnectionString());
            conn.Open();
            using (var tran = conn.BeginTransaction())
            {
                try
                {
                    foreach (var item in summeryModel.AttendenceSummaryView)
                    {
                        using (var cmd = new SqlCommand())
                        {
                            cmd.CommandText = "sp_Insert_AttendenceSummery";
                            cmd.Connection = conn;
                            cmd.Transaction = tran;
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@ID", summeryModel.ID);
                            cmd.Parameters.AddWithValue("@EmpCode", item.EmpCode);
                            cmd.Parameters.AddWithValue("@AttendenceDay", item.AttendenceDay);
                            cmd.Parameters.AddWithValue("@LeaveWithPay", item.LeaveWithPay);
                            cmd.Parameters.AddWithValue("@LeaveWithoutPay", item.LeavewithOutPay);
                            cmd.Parameters.AddWithValue("@Holiday", item.Holiday);
                            cmd.Parameters.AddWithValue("@Absent", item.Absent);
                            cmd.Parameters.AddWithValue("@TotalDay", item.TotalDay);
                            cmd.Parameters.AddWithValue("@PeriodID", summeryModel.PeriodID);
                            cmd.Parameters.AddWithValue("@StartDate", summeryModel.StartDate);
                            cmd.Parameters.AddWithValue("@EndDate", summeryModel.EndDate);
                            cmd.Parameters.AddWithValue("@CompanyID", summeryModel.CompanyID);
                            cmd.Parameters.AddWithValue("@UserID", summeryModel.UserID);
                            cmd.Parameters.AddWithValue("@Remarks", item.Remarks = "");
                            cmd.ExecuteNonQuery();
                        }
                    }
                    tran.Commit();
                    return true;
                }
                catch (Exception err)
                {
                    tran.Rollback();

                    throw new Exception(err.Message);
                }
            }

        }

        //========================================================== Process Attendence Data  ================================================

        public static bool ProcessAttendenceData(string date, int comid, int branch)
        {
            var con = new SqlConnection(Connection.ConnectionString());
            var obj = new
            {
                Date = date,
                CompanyID = comid,
                Branch = branch
            };

            int rowAffected = con.Execute("spProcessAttandanceMatchineData", param: obj, commandType: CommandType.StoredProcedure);
            return rowAffected > 0;
        }
        public static bool ProcessAttendanceSummery(string strDate,string endDate,int periodId,int compId)
        {
            var con = new SqlConnection(Connection.ConnectionString());
            var obj = new
            {
                StartDate=strDate,
                EndDate=endDate,
                PeriodID= periodId,
                CompanyID=compId
            };
            int rowAffected = con.Execute("spRptAttendenceSummaryProcess", param: obj, commandType: CommandType.StoredProcedure);
            return rowAffected > 0;
        }
        public static bool DeleteExestingAttandence(int peridId,int compId)
        {
            var con = new SqlConnection(Connection.ConnectionString());
            var obj = new
            {
                PeriodID = peridId,
                CompanyID = compId
            };
            int rowAffected = con.Execute("spDeleteExistingAttendence", param: obj, commandType: CommandType.StoredProcedure);
            return rowAffected > 0;
        }
        public static bool ProcessLateComerReport(string sDate,string eDate,int gradeVal,int compId)
        {
            var con = new SqlConnection(Connection.ConnectionString());
            var obj = new
            {
                StrDate=sDate,
                EndDate=eDate,
                GradeValue=gradeVal,
                CompanyID=compId
            };
            int rowAffect = con.Execute("spRpLetComerReportFired", param: obj, commandType: CommandType.StoredProcedure);
            return rowAffect > 0;
        }

        //============================================================== Import Employee Attendence Sheet ===============================================================

        public static bool ImportEmployeeAttendence(ImportEmployeeAttendence model, List<dynamic> details)
        {
            //bool isDelete = DeleteExistingBonus();
            using (var con = new SqlConnection(Connection.ConnectionString()))
            {
                con.Open();
                using (var tran = con.BeginTransaction())
                {
                    try
                    {
                        foreach (var item in details)
                        {
                            string quire = $"INSERT INTO AttendenceMachinedata(Date,Time,EmpCode,TerminalID)VALUES ('{model.EmpCode}','{model.TerminalID}')";
                            con.Execute(quire, transaction: tran);
                        }
                        tran.Commit();
                        return true;
                    }
                    catch (Exception err)
                    {
                        tran.Rollback();
                        throw new Exception(err.Message);
                    }
                }
            }
        }



        //=========================================== Apporve Application =============================================================


        public static List<AttendenceApproveViewModel> GetApplicationAttendenceListForApporve(int comid, string reqTo, int pOption)
        {
            var conn = new SqlConnection(Connection.ConnectionString());
            var obj = new
            {
                CompanyID = comid,
                ReqTo = reqTo,
                pOptions = pOption
            };

            List<AttendenceApproveViewModel> result = conn.Query<AttendenceApproveViewModel>("sp_AttendanceApplication_New", param: obj, commandType: CommandType.StoredProcedure).ToList();
            return result;
        }

        public static bool ApproveAttendenceApplication(AttendenceApproveViewModel attendenceApproveModel)
        {
            var con = new SqlConnection(Connection.ConnectionString());
            var obj = new
            {
                attendenceApproveModel.ID,
                attendenceApproveModel.CompanyID,
                attendenceApproveModel.ReqFrom,
                ReqTo = attendenceApproveModel.ForwordTo,
                attendenceApproveModel.UserID,
                pOptions = attendenceApproveModel.POption
            };

            int rowAffected = con.Execute("sp_AttendanceApplication_New", param: obj, commandType: CommandType.StoredProcedure);
            return rowAffected > 0;
        }

        // Recommmend For Attendence Application 
        public static bool RecommendApplayAttendanceInfo(AttendenceApproveViewModel recommendModel)
        {
            var con = new SqlConnection(Connection.ConnectionString());
            var obj = new
            {
                recommendModel.ID,
                recommendModel.CompanyID,
                recommendModel.ReqFrom,
                ReqTo = recommendModel.ForwordTo,
                recommendModel.UserID,
                pOptions = recommendModel.POption
            };

            int rowAffected = con.Execute("sp_AttendanceApplication_New", param: obj, commandType: CommandType.StoredProcedure);
            return rowAffected > 0;
        }

        // Cancel Attendence Application

        public static bool CancelApplicationAttendanceInfo(AttendenceApproveViewModel cancelModel)
        {
            var con = new SqlConnection(Connection.ConnectionString());
            var obj = new
            {
                cancelModel.ID,
                cancelModel.CompanyID,
                cancelModel.ReqFrom,
                ReqTo = cancelModel.ForwordTo,
                cancelModel.UserID,
                pOptions = cancelModel.POption
            };

            int rowAffected = con.Execute("sp_AttendanceApplication_New", param: obj, commandType: CommandType.StoredProcedure);
            return rowAffected > 0;
        }

        /// <summary>
        /// Get Attendance In Out Time
        /// </summary>
        /// <param name="startDate">StartDate format should be yyyyMMdd</param>
        /// <param name="endDate">EndDate format should be yyyyMMdd</param>
        /// <param name="companyId"></param>
        /// <param name="empCode"></param>
        /// <param name="branchId"></param>
        /// <param name="departmentId"></param>
        /// <param name="gradeVal"></param>
        /// <param name="locationId"></param>
        /// <param name="projectId"></param>
        /// <returns>AttLogViewModel | EmpCod, AttnDate, Login, Logout, CompanyID</returns>
        public static List<AttLogViewModel> GetAttLog(string startDate, string endDate, int companyId, string empCode="-1", int branchId=-1, int departmentId=-1, int gradeVal=-1, int locationId=-1, int projectId = -1)
        {
            object paramObj = new
            {
                StrDate = startDate,
                EndDate = endDate,
                CompanyID = companyId,
                EmpCod = empCode,
                BranchID = branchId,
                DepartmentID = departmentId,
                GradeValue = gradeVal,
                Location = locationId,
                ProjectID = projectId
            };
            using(var con = new SqlConnection(Connection.ConnectionString()))
            {
                List<AttLogViewModel> attLogs = con.Query<AttLogViewModel>("spRptAttandanceInAndOut", param: paramObj, commandType: CommandType.StoredProcedure).ToList();
                return attLogs;
            }
        }
        /// <summary>
        /// Get Employee Total Worked hour by date wise
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="empCode"></param>
        /// <param name="attDate">Date should be yyyyMMdd format</param>
        /// <returns></returns>
        public static double GetEmpWorkedHour(int companyId, string empCode, string attDate)
        {
            var attLog = GetAttLog(attDate, attDate, companyId, empCode).FirstOrDefault();
            var shiftInfo = GetAssignedShiftInfo(companyId,empCode,attDate).FirstOrDefault();
            if(attLog==null || shiftInfo == null) { return 0; }
            int.TryParse(attLog?.Login?.Split(':')[0], out int attInHour);
            int.TryParse(attLog?.Login?.Split(':')[1], out int attInMin);
            int.TryParse(attLog?.Logout?.Split(':')[0], out int attOutHour);
            int.TryParse(attLog?.Logout?.Split(':')[1], out int attOutMin);
            TimeSpan dutyStart;
            TimeSpan dutyEnd = new TimeSpan(attOutHour, attOutMin,0);
            if (attInHour>shiftInfo.ShiftStartHour || (attInHour==shiftInfo.ShiftStartHour && attInMin > shiftInfo.ShiftStartMin))
            {
                dutyStart = new TimeSpan(attInHour, attInMin, 0);
            }else {
                dutyStart = new TimeSpan(shiftInfo.ShiftStartHour, shiftInfo.ShiftStartMin, 0);
            }
            return (dutyEnd - dutyStart).TotalHours;

        }
    }

}

