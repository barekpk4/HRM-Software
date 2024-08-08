using Dapper.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using WebApiCore.Models.SalaryProcess;
using WebApiCore.ViewModels.SalaryProcess;
using DocumentFormat.OpenXml.Bibliography;
using WebApiCore.Models.Attendance;
using WebApiCore.ViewModels;

namespace WebApiCore.DbContext.SalaryProcess
{
    public class SalaryProcessDB
    {
        public static int Process(SalaryProcessModel model)
        {
            var salaryProcessInfo = GetSalaryProcessInfo(model);
            if (salaryProcessInfo.Count==0)
            {
                int employees = 0;
                if (model.SalaryTypeID == 1)
                    { employees = CountEmpPayscale(model); }
                else
                    { employees = CountEmp(model); }

                if (employees > 0)
                {
                    using (var conn = new SqlConnection(Connection.ConnectionString()))
                    {
                        conn.Open();
                        using (var tran = conn.BeginTransaction())
                        {
                            try
                            {
                                DeleteExistingSalary(model, conn, tran);
                                ProcessEmpSalary(model, conn, tran);
                                ProcessEmpAdvanceSalary(model, conn, tran);
                                ProcessEmpLWP(model, conn, tran);
                                tran.Commit();
                                return employees;
                            }
                            catch(Exception err)
                            {
                                tran.Rollback();
                                throw new Exception(err.Message);
                            }
                        }
                    }
                }
                else { throw new Exception("Employee not found for process!"); }
            }
            else
            {
                throw new Exception($"Salary already processed for { model.PeriodName}");
            }

        }
        public static List<SalaryProcessModel> GetSalaryProcessInfo(SalaryProcessModel processModel)
        {
            var conn = new SqlConnection(Connection.ConnectionString());
            var obj = new
            {
                Period = processModel.PeriodID ?? -1,
                Grade = (processModel.UserTypeID != 1 && processModel.UserTypeID != 4) ?
                processModel.Grade : -1,
                processModel.CompanyID
            };
            List<SalaryProcessModel> result = conn.Query<SalaryProcessModel>("Sp_GetSalaryProcessInfo",commandTimeout:20000,param: obj, commandType: CommandType.StoredProcedure).ToList();
            return result;

        }


        public static List<MobileBillSetup> GetEmpForMobileBillSetup(int departmentID, int companyID)
        {
            using (var conn = new SqlConnection(Connection.ConnectionString()))
            {
                var obj = new
                {
                    DepartmentID = departmentID,
                    CompanyID = companyID
                };

                List<MobileBillSetup> getEmp = conn.Query<MobileBillSetup>("sp_GetEmpForMobileBillSetup", param: obj, commandType: CommandType.StoredProcedure).ToList();
                return getEmp;
            }
        }

        public static bool AssignEmpMobileBill(List<MobileBillSetup> setupList)
        {
            using (var con = new SqlConnection(Connection.ConnectionString()))
            {
                con.Open();
                using (var tran = con.BeginTransaction())
                {
                    try
                    {
                        //var vonne = new
                        //{
                        //    setupList[0].EmpCode
                        //};
                        //int test = con.Execute("DeleteWeakendShift", param: vonne, transaction: tran, commandType: CommandType.StoredProcedure);


                        foreach (var setupInfo in setupList)
                        {
                            

                            var param = new
                            {
                                setupInfo.Id,
                                setupInfo.EmpCode,
                                setupInfo.Amount,
                                setupInfo.CompanyID,
                                setupInfo.UserID
                            };
                            int result = con.Execute("INSertupdateMobileBill", param: param, transaction: tran, commandType: CommandType.StoredProcedure);
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


        public static List<MonthlyEmpMobileBillModel> GetMonthlyEmpMobileBill(MonthlyEmpMobileBillModel monthlyEmpMobileBill)
        {
            var conn = new SqlConnection(Connection.ConnectionString());
            var obj = new
            {
                monthlyEmpMobileBill.CompanyID,
                departmentID = monthlyEmpMobileBill.DepartmentID ?? -1,
                PeriodID = monthlyEmpMobileBill.PeriodID ?? -1,
                ProjectID = monthlyEmpMobileBill.ProjectID ?? -1
            };
            List<MonthlyEmpMobileBillModel> result = conn.Query<MonthlyEmpMobileBillModel>("spGetMonthlyMobileBillData", param: obj, commandType: CommandType.StoredProcedure).ToList();
            return result;
        }



        public static bool AssignEmpMonthlyBill(List<MonthlyEmpMobileBillModel> monthlyBillList)
        {
            using (var con = new SqlConnection(Connection.ConnectionString()))
            {
                con.Open();
                using (var tran = con.BeginTransaction())
                {
                    try
                    {

                        foreach (var monthBillInfo in monthlyBillList)
                        {


                            var param = new
                            {
                                monthBillInfo.Id,
                                monthBillInfo.EmpCode,
                                monthBillInfo.Amount,
                                monthBillInfo.MonthlyBill,
                                monthBillInfo.CompanyID,
                                monthBillInfo.UserID,
                                monthBillInfo.YearID,
                                monthBillInfo.PeriodID,
                            };
                            int result = con.Execute("INSertupdateMonthlyMobileBill", param: param, transaction: tran, commandType: CommandType.StoredProcedure);
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

        public static int CountEmpPayscale(SalaryProcessModel model)
        {
            var conn = new SqlConnection(Connection.ConnectionString());
            int countEmpPayScale;
            var obj = new
            {
                EmpCode = model.EmpCode??"",
                model.CompanyID,
                Grade = (model.UserTypeID != 1 && model.UserTypeID != 4) ? model.Grade : -1
            };
            object countResult = conn.ExecuteScalar("spGetCountEmployeesPayscale", param: obj, commandType: CommandType.StoredProcedure);
            int.TryParse(countResult.ToString(), out countEmpPayScale);
            return countEmpPayScale;
        }
        public static int CountEmp(SalaryProcessModel model)
        {
            var conn = new SqlConnection(Connection.ConnectionString());
            int countEmloyee;
            var obj = new
            {
                model.StructureID,
                EmpCode = model.EmpCode??"",
                model.CompanyID,
                Grade = (model.UserTypeID != 1 && model.UserTypeID != 4) ? model.Grade : -1
            };
            object countEmp = conn.ExecuteScalar("spGetCountEmployees", param: obj, commandType: CommandType.StoredProcedure);
            int.TryParse(countEmp.ToString(), out countEmloyee);
            return countEmloyee;
        }
        public static void DeleteExistingSalary(SalaryProcessModel model, SqlConnection conn, SqlTransaction tran)
        {
            var obj = new
            {
                model.PeriodID,
                model.StructureID,
                model.TaxYearID,
                model.YearID,
                EmpCode = model.EmpCode??"",
                model.CompanyID,
                Grade = (model.UserTypeID != 1 && model.UserTypeID != 4) ? model.Grade : -1,
                Salarttype = model.SalaryTypeID
            };
            conn.Execute("spDeleteExistingSalary", commandTimeout:20000, param: obj, transaction: tran, commandType: CommandType.StoredProcedure);
        }
        public static void ProcessEmpSalary(SalaryProcessModel model, SqlConnection conn, SqlTransaction tran)
        {
            var obj = new
            {
                model.PeriodID,
                model.StructureID,
                model.TaxYearID,
                model.PeriodName,
                model.YearID,
                EmployeeCode = model.EmpCode??"",
                model.CompanyID,
                Grade = (model.UserTypeID != 1 && model.UserTypeID != 4) ? model.Grade : -1,
            };
            if (model.SalaryTypeID == 1)
            {
                int rowAffect = conn.Execute("spProcessEmpSalaryPayScale", commandTimeout: 20000, transaction: tran, param: obj, commandType: CommandType.StoredProcedure);
            }
            else
            {
                int rowAfect = conn.Execute("spProcessEmpSalary",commandTimeout:20000, transaction: tran, param: obj, commandType: CommandType.StoredProcedure);
            }
        }
        public static void ProcessEmpAdvanceSalary(SalaryProcessModel model, SqlConnection conn, SqlTransaction tran)
        {
            var obj = new
            {
                model.PeriodID,
                model.PeriodName,
                model.StructureID,
                model.TaxYearID,
                model.YearID,
                EmployeeCode = model.EmpCode??"",
                model.CompanyID,
                Grade = (model.UserTypeID != 1 && model.UserTypeID != 4) ? model.Grade : -1,
                Block = "No"
            };
            int rowAffect = conn.Execute("spProcessEmpLoanPayscale", commandTimeout:20000,param: obj, transaction: tran, commandType: CommandType.StoredProcedure);

        }
        public static void ProcessEmpLWP(SalaryProcessModel model, SqlConnection conn, SqlTransaction tran)
        {
            var obj = new
            {
                model.PeriodID,
                model.PeriodName,
                structureID = model.StructureID,
                model.TaxYearID,
                model.YearID,
                EmployeeCode = model.EmpCode??"",
                model.CompanyID,
                Grade = (model.UserTypeID != 1 && model.UserTypeID != 4) ? model.Grade : -1,
                Block = "No"
            };
            int rowAffect = conn.Execute("spProcessEmpSalaryLWPPayscale", commandTimeout: 20000, param: obj, transaction: tran, commandType: CommandType.StoredProcedure);
        }


        public static bool DeleteExistingPayslip(SalaryProcessModel payslip)
        {
            var conn = new SqlConnection(Connection.ConnectionString());
            //int grade;
            //if (payslip.UserTypeID != 1 && payslip.UserTypeID != 4)
            //{
            //    grade = payslip.Grade;
            //}

            //else
            //{
            //    grade = -1;
            //}
            try
            {
                var rowAffect = conn.Execute("DELETE PayslipToEmail WHERE PeriodID=" + payslip.PeriodID + " AND EmpCode='" + payslip.EmpCode + "' AND CompanyID=" + payslip.CompanyID);
                return rowAffect > 0;
            }
            catch (Exception err)
            {
                throw new Exception(err.Message);

            }

        }
        public static List<SalaryProcessModel> GetGeneralinfoForPayslip(string empCode, int companyID, int gradeValue)
        {
            var conn = new SqlConnection(Connection.ConnectionString());
            var obj = new
            {
                EmpCode = empCode,
                CompanyID = companyID,
                GradeValue = gradeValue
            };

            List<SalaryProcessModel> empInfo = conn.Query<SalaryProcessModel>("spGetGeneralinfoForPayslip", param: obj, commandType: CommandType.StoredProcedure).ToList();
            return empInfo;
        }
    }
}
