using Dapper.Framework;
using HRMS.Models;
using HRMS.Models.Attendance;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApiCore.Models.Attendance;
using WebApiCore.Models.HR.Employee;
using WebApiCore.ViewModels.Attendance;
using WebApiCore.ViewModels.HR;

namespace WebApiCore.DbContext.Attendance
{
    public partial class AttendenceApplication
    {

        //Shift Setup
        public static List<ShiftSetupModel> GetAllShift()
        {
            var conn = new SqlConnection(Connection.ConnectionString());
            var dataset = conn.Query<ShiftSetupModel>("SELECT * FROM ShiftInfo").ToList();
            return dataset;
        }
        public static bool SaveOrUpdateShift(ShiftSetupModel model)
        {
            model.pOptions = model.Id > 0 ? 2 : 1;
            object paramObj = new
            {
                model.Id,
                model.ShfitName,
                model.ShiftStartHour,
                model.ShiftStartMin,
                model.ShiftEndtHour,
                model.ShiftEndMin,
                model.MinLogout,
                model.MaxLogout,
                model.MinIntime,
                model.MaxIntime,
                model.Out,
                model.NextDate,
                model.CompanyID,
                model.NextShiftID,
                model.DutyHours,
                model.DutyMinute,
                model.pOptions
            };
            using (var con = new SqlConnection(Connection.ConnectionString()))
            {
                int rowAffect = con.Execute("sp_ShiftInfo_New", param: paramObj, commandType: System.Data.CommandType.StoredProcedure);
                return rowAffect > 0;
            }
        }
        //Shift Assign
        /// <summary>
        /// Get Employee to assign shift
        /// </summary>
        /// <param name="filterModel"></param>
        /// <returns></returns>
        public static List<EmpSearchViewModel> GetEmployees(FilterModel filterModel)
        {
            var paramObj = new
            {
                department = filterModel.Department ?? "-1",
                designation = filterModel.Designation ?? "-1",
                location = filterModel.Location ?? "-1",
                Workstation = filterModel.WorkStation ?? "-1",
                GradeValue = filterModel.GradeValue ?? -1,
                CompanyID = (filterModel.CompanyID == 0) ? -1 : filterModel.CompanyID
            };
            using (var con = new SqlConnection(Connection.ConnectionString()))
            {
                var employees = con.Query<EmpSearchViewModel>("spGetEmpShiftData", param: paramObj, commandType: CommandType.StoredProcedure).ToList();
                return employees;
            }
        }
        public static List<ShiftDayInfoViewModel> GetDayOfShiftInfo(string startDate, string endDate, string shiftName)
        {
            object paramObj = new
            {
                startdate = startDate,
                enddate = endDate,
                sname = shiftName
            };
            using (var con = new SqlConnection(Connection.ConnectionString()))
            {
                var days = con.Query<ShiftDayInfoViewModel>("sp_getDayOFShiftInfo", param: paramObj, commandType: CommandType.StoredProcedure).ToList();
                return days;
            }
        }

        [Obsolete("Recommend to AssignShift()")]
        public static bool InsertShiftManagementInfo(List<ShiftManagementModel> shiftInfoList)
        {
            using (var con = new SqlConnection(Connection.ConnectionString()))
            {
                con.Open();
                using (var tran = con.BeginTransaction())
                {
                    try
                    {
                        foreach (var shiftInfo in shiftInfoList)
                        {
                            object paramObj = new
                            {
                                shiftInfo.ShiftID,
                                shiftInfo.EmpCode,
                                shiftInfo.ShiftDate,
                                shiftInfo.Note,
                                shiftInfo.DDMMYYYY,
                                shiftInfo.NextDate,
                                shiftInfo.ShiftIDRostaring,
                                shiftInfo.CompanyID
                            };
                            con.Execute("sp_ShiftmanagementInfo_Insert", param: paramObj, transaction: tran, commandType: CommandType.StoredProcedure);
                        }
                        tran.Commit();
                        return true;
                    }
                    catch
                    {
                        tran.Rollback();
                        return false;
                    }
                }
            }
        }

        public static List<object> GetAssignedShift(AssignedShiftFilterModel filterModel)
        {
            List<string> dateColumns = new List<string>();
            for (DateTime date = filterModel.FromDate; date <= filterModel.ToDate; date = date.AddDays(1))
            {
                var strDate = date.ToString("[MM\\/dd\\/yyyy]");
                dateColumns.Add(strDate);
            }

            using (var con = new SqlConnection(Connection.ConnectionString()))
            {
                var paramObj = new
                {
                    FromDate = filterModel.FromDate.ToString("yyyyMMdd"),
                    ToDate = filterModel.ToDate.ToString("yyyyMMdd"),
                    DateColumns = string.Join(',', dateColumns),
                    ShiftID = filterModel.ShiftID??-1,
                    filterModel.ReportTo,
                    filterModel.CompanyID,
                    DepartmentID = filterModel.DepartmentID??-1,
                    DesignationID = filterModel.DesignationID??-1,
                    LocationID=filterModel.LocationID??-1,
                    EmpCode = filterModel.EmpCode??"-1"
                };
                var shifts = con.Query<object>("sp_shift_edit", param: paramObj, commandType: CommandType.StoredProcedure).ToList();
                return shifts;
            }
        }

        public static bool AssignShift(List<ShiftManagementModel> shiftInfoList)
        {
            using (var con = new SqlConnection(Connection.ConnectionString()))
            {
                con.Open();
                using (var tran = con.BeginTransaction())
                {
                    try
                    {
                        foreach (var shiftInfo in shiftInfoList)
                        {
                            if (!(!IsAssigned(shiftInfo.EmpCode, shiftInfo.ShiftDate, shiftInfo.CompanyID, con, tran) && shiftInfo.ShiftID == 0))
                            {
                                object paramObj = new
                                {
                                    shiftInfo.ShiftID,
                                    shiftInfo.EmpCode,
                                    shiftInfo.ShiftDate,
                                    shiftInfo.Note,
                                    DDMMYYYY = shiftInfo.ShiftDate.ToString("yyyyMMdd"),
                                    NextDate = shiftInfo.ShiftDate.AddDays(1).ToString("yyyyMMdd"),
                                    ShiftIDRostaring = shiftInfo.ShiftID,
                                    shiftInfo.CompanyID
                                };
                                con.Execute("sp_ShiftmanagementInfo_Insert", param: paramObj, transaction: tran, commandType: CommandType.StoredProcedure);
                            }
                        }
                        tran.Commit();
                        return true;
                    }
                    catch (Exception)
                    {
                        tran.Rollback();
                        return false;
                    }
                }
            }
        }

        private static bool IsAssigned(string empCode, DateTime date, int companyID, SqlConnection con, SqlTransaction tran)
        {
            //using (var con = new SqlConnection(Connection.ConnectionString()))
            //{
                string sql = $"SELECT * FROM ShiftManagemetinfo WHERE EmpCode = '{empCode}' AND DDMMYYYY='{date.ToString("yyyyMMdd")}' AND CompanyID={companyID}";
                var rows = con.Query<ShiftManagementModel>(sql, transaction:tran).ToList();
                return rows.Count > 0;
            //}
        }
        public static List<JoiningInfoModel> GetByReportTo(string EmpCode,int CompanyID)
        {
            var con = new SqlConnection(Connection.ConnectionString());
            var empCodes = con.Query<JoiningInfoModel>($"SELECT EmpCode FROM EmpEmploymentInfo WHERE ReportTo= '{EmpCode}' AND CompanyID={CompanyID}").ToList();
            return empCodes;
        }
        
        /// <summary>
        /// Get Assigned Shift Info by empcode and shiftDate
        /// </summary>
        /// <param name="empCode"></param>
        /// <param name="shiftDate">Date format should be yyyyMMdd</param>
        /// <returns></returns>
        public static List<AssignedShiftInfoViewModel> GetAssignedShiftInfo(int companyId,string empCode="-1", string shiftDate = "-1")
        {
            string selectSql = $@"SELECT EmpCode, ShiftDate, ShiftID, ShiftStartHour,ShiftStartMin, ShiftEndtHour, ShiftEndMin, smi.CompanyID FROM ShiftManagemetinfo smi
JOIN ShiftInfo si ON si.Id = smi.ShiftID
WHERE (EmpCode = CASE WHEN {empCode}<>'-1' THEN {empCode} ELSE EmpCode END) AND (CONVERT(VARCHAR,ShiftDate,112) = CASE WHEN {shiftDate}<>'-1' THEN {shiftDate} ELSE CONVERT(VARCHAR,ShiftDate,112) END) AND smi.CompanyID={companyId}";
            using(var con = new SqlConnection(Connection.ConnectionString()))
            {
                List<AssignedShiftInfoViewModel> assignedShiftIno = con.Query<AssignedShiftInfoViewModel>(selectSql).ToList();
                return assignedShiftIno;
            }
        }
    }
}
