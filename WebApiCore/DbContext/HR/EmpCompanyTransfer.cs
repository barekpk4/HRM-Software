using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper.Framework;
using WebApiCore.Models.HR;
using WebApiCore.ViewModels.HR;

namespace WebApiCore.DbContext.HR
{
    public class EmpCompanyTransfer
    {
        public static EmpCompanyTransferModel GetCompanytransfer(string empCode, int companyID,int ptType)
        {
            var conn = new SqlConnection(Connection.ConnectionString());
            var dateset = conn.QuerySingle<EmpCompanyTransferModel>("SELECT  Top 1 * FROM EmpTransfer WHERE TPType='"+ptType+"' AND EmpCode='" + empCode + "' AND CompanyID=" + companyID+" Order By ID DESC");
            return dateset;
        }

        public static bool SaveUpdate(EmpCompanyTransferModel companyTransfer)
        {
            var conn=new SqlConnection(Connection.ConnectionString());
            var varperam = new
            {
                companyTransfer.EmpCode,
                companyTransfer.PreCompanyID,
                companyTransfer.PreDepartmentID,
                companyTransfer.PreProjectID,
                companyTransfer.PreDesignationID,
                companyTransfer.PreDivisionID,
                companyTransfer.PreBranchID,
                companyTransfer.PreUnit,
                companyTransfer.PreLocation,
                companyTransfer.PreGrade,
                companyTransfer.PrePayscaleGrade,
                companyTransfer.PasCompanyID,
                companyTransfer.PasDepartmentID,
                companyTransfer.PasProjectID,
                companyTransfer.PasDesignationID,
                companyTransfer.PasDivisionID,
                companyTransfer.PasBranchID,
                companyTransfer.PasUnit,
                companyTransfer.PasLocation,
                companyTransfer.PasGrade,
                companyTransfer.PasPayscaleGrade,
                companyTransfer.Note,
                companyTransfer.TransferDate,
                companyTransfer.TPType,
                companyTransfer.CompanyID,
                companyTransfer.jobresponsibilities
            };
            int rowAffect = conn.Execute("INSertTransferCompany", param:varperam,commandType:CommandType.StoredProcedure);
            return rowAffect > 0;
        }
      public static  List<TransferViweModel> View(string EmpCode,int TPType,int CompanyID)
        {
            var conn = new SqlConnection(Connection.ConnectionString());
            var peram = new
            {
                EmpCode,
                TPType,
                CompanyID 
            };
            var dataset=conn.Query<TransferViweModel>("spGetPromotionTransferList",param:peram,commandType:CommandType.StoredProcedure).ToList();
            return dataset;
        }
        public static EmpCompanyTransferModel getEmploymentSalaryDetails(string EmpCode, int CompanyID)
        {
            var conn = new SqlConnection(Connection.ConnectionString());
            var peram = new
            {
                CompanyID,
                EmpCode
            };
            var dataset = conn.QuerySingle<EmpCompanyTransferModel>("sp_EmpTransfer_List", param: peram, commandType: CommandType.StoredProcedure);
            return dataset;
        }
       
    }
}