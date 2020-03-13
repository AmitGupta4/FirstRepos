using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using BusinessAccessLayer;
using RusticiSoftware.HostedEngine.Client;
using System.Text;

namespace TalentFirst.TalentFirstV3.Admin
{
    public partial class ApprovalProcess : System.Web.UI.Page
    {
        #region Local Variable
        TrainingAndDevelopmentBAL objTrainingAndDevelopmentBAL = null;
        private const string PAGE_TITLE = "Approval Process";
        #endregion

        #region Properties

        /// <summary>
        /// Gets the value for the Member Org Id.
        /// </summary>
        public int MemberOrgId
        {
            get
            {
                int intMemOrgID = 0;

                if (Session["MemberOrgId"] != null)
                    intMemOrgID = Convert.ToInt32(Session["MemberOrgId"]);

                return intMemOrgID;
            }
        }

        /// <summary>
        /// Gets and set the Employee id for the logged in user
        /// </summary>
        public int LoggedInEmpId
        {
            get
            {
                int id = 0;
                if (Session["EmpId"] != null)
                {
                    Int32.TryParse(Session["EmpId"].ToString(), out id);
                }

                return id;
            }
        }

        #endregion


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                objTrainingAndDevelopmentBAL = new TrainingAndDevelopmentBAL();
                DataSet dsData = objTrainingAndDevelopmentBAL.TrainingApprovalProcess(this.MemberOrgId);
                ViewState["TrainingApprovalData"] = dsData;
                if (ddlFirstLevel.Items.Count > 0)
                    ddlFirstLevel.Items.Clear();
                ddlFirstLevel.DataTextField = "ApprovalBy";
                ddlFirstLevel.DataValueField = "Id";
                ddlFirstLevel.DataSource = dsData.Tables[0];
                ddlFirstLevel.DataBind();
                ddlFirstLevel.Items.Insert(0, new ListItem("--Select--", "-1"));
                if (dsData.Tables[1] != null && dsData.Tables[1].Rows.Count>0)
                {
                    txtHighestEmployee.Text = Convert.ToString(dsData.Tables[1].Rows[0]["OtherEmpName"]);
                    hdnHighestEmpId.Value = Convert.ToString(dsData.Tables[1].Rows[0]["OtherEmpId"]);
                    ddlFirstLevel.SelectedValue = Convert.ToString(dsData.Tables[1].Rows[0]["FirstLevel"]);
                    ddlFirstLevel_SelectedIndexChange(null, null);
                }
            }
        }
        protected void btnSave_Click(object sender, EventArgs e)
        {
            int result = 0;

            try
            {
                if (Valid())
                {
                    objTrainingAndDevelopmentBAL = new TrainingAndDevelopmentBAL();
                    objTrainingAndDevelopmentBAL.TrainingApprovalSave(this.MemberOrgId,ddlFirstLevel.SelectedValue ,ddlSecondLevel.SelectedValue,ddlThirdLevel.SelectedValue, hdnHighestEmpId.Value, LoggedInEmpId, out result);
                    if (result > 0)
                    {
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "Training & Development", "showSuccessToast('" + ResourceReader.GetMessage("TrainingAndDevelopment_044") + "');", true);
                    }
                    else
                    {
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "Training & Development", "showErrorToast('" + ResourceReader.GetMessage("TrainingAndDevelopment_045") + "');", true);
                    }

                }
            }
            catch(Exception ex)
            {
                Comman.LogException(ex, PAGE_TITLE, Comman.MemberOrgId, true);
            }
        }
        protected Boolean Valid()
        {
            if (ddlFirstLevel.SelectedValue == "3" || ddlSecondLevel.SelectedValue == "3" || ddlThirdLevel.SelectedValue == "3")
            {
                if (string.IsNullOrEmpty(txtHighestEmployee.Text))
                {
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "Training & Development", "showErrorToast('" + ResourceReader.GetMessage("TrainingAndDevelopment_046") + "');", true);
                    return false;
                }
                return true;
            }
            return true;
        }
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("CompanyAdminMenu.aspx");
        }
        
        protected void ddlFirstLevel_SelectedIndexChange(object sender, EventArgs e)
        {
            DataSet dsData;
            dsData = ViewState["TrainingApprovalData"] as DataSet;
            DataTable dt = dsData.Tables[0];

            if (ddlFirstLevel.SelectedValue == "3")
            {
                dvOrganizational.Style.Add("display", "block");
            }
            int intFirstLevel = Convert.ToInt32(ddlFirstLevel.SelectedValue);
            if (ddlSecondLevel.Items.Count > 0)
                ddlSecondLevel.Items.Clear();
            DataView dvData = new DataView(dt);
            dvData.RowFilter = string.Format("Id<>{0}", intFirstLevel);
            ddlSecondLevel.DataTextField = "ApprovalBy";
            ddlSecondLevel.DataValueField = "Id";
            ddlSecondLevel.DataSource = dvData;
            ddlSecondLevel.DataBind();
            ddlSecondLevel.Items.Insert(0, new ListItem("--Select--", "-1"));
            if (!Page.IsPostBack)
            {
                if (dsData.Tables[1].Rows[0]["SecondLevel"] != null)
                {
                    ddlSecondLevel.SelectedValue = Convert.ToString(dsData.Tables[1].Rows[0]["SecondLevel"]);
                    ddlSecondLevel_SelectedIndexChange(null, null);
                }
            }
        }
        protected void ddlSecondLevel_SelectedIndexChange(object sender, EventArgs e)
        {
            DataSet dsData;
            dsData = ViewState["TrainingApprovalData"] as DataSet;
            DataTable dt = dsData.Tables[0];

            if (ddlSecondLevel.SelectedValue == "3")
            {
                dvOrganizational.Style.Add("display", "block");
            }
            int intSecondLevel = Convert.ToInt32(ddlSecondLevel.SelectedValue);
            int intFirstLevel = Convert.ToInt32(ddlFirstLevel.SelectedValue);
            if (ddlThirdLevel.Items.Count > 0)
                ddlThirdLevel.Items.Clear();
            DataView dvData = new DataView(dt);
            dvData.RowFilter = string.Format("Id<>{0} AND Id<>{1}", intSecondLevel, intFirstLevel);

            ddlThirdLevel.DataTextField = "ApprovalBy";
            ddlThirdLevel.DataValueField = "Id";
            ddlThirdLevel.DataSource = dvData;
            ddlThirdLevel.DataBind();
            ddlThirdLevel.Items.Insert(0, new ListItem("--Select--", "-1"));
            if (!Page.IsPostBack)
            {
                if (dsData.Tables[1].Rows[0]["ThirdLevel"] != null)
                {
                    ddlThirdLevel.SelectedValue = Convert.ToString(dsData.Tables[1].Rows[0]["ThirdLevel"]);
                    ddlThirdLevel_SelectedIndexChange(null, null);
                }
            }
        }
        protected void ddlThirdLevel_SelectedIndexChange(object sender, EventArgs e)
        {
            DataSet dsData;
            dsData = ViewState["TrainingApprovalData"] as DataSet;
            if (ddlThirdLevel.SelectedValue == "3")
            {
                dvOrganizational.Style.Add("display", "block");
            }
        }
    }
}