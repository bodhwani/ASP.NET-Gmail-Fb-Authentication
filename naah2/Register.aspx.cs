using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Http;

namespace naah2
{
    [Authorize]
    public partial class Register : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["RegisterConnectionString"].ConnectionString);

                conn.Open();
                string checkuser = "SELECT COUNT(*) FROM UserData where id='" + TextBox1.Text + "' ";
                SqlCommand com = new SqlCommand(checkuser, conn);
                int temp = Convert.ToInt32(com.ExecuteScalar().ToString());
                if (temp == 1)
                {
                    Response.Write("User already exisis");
                }
                conn.Close();

            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["RegisterConnectionString"].ConnectionString);

                conn.Open();
                string insertQuery = "INSERT INTO UserData(id,email) VALUES(@ID,@EMAIL)";

                SqlCommand com = new SqlCommand(insertQuery, conn);
                com.Parameters.AddWithValue("@ID", TextBox1.Text);
                com.Parameters.AddWithValue("@EMAIL", TextBox2.Text);
                com.ExecuteNonQuery();
                Response.Redirect("Manager.aspx");
                Response.Write("Success");


                conn.Close();

            }
            catch (Exception ex)
            {
                Response.Write("Error :-- " + ex.ToString());
            }

        }

        protected void Button2_Click(object sender, EventArgs e)
        {

        }
    }
}