using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.IO;
using System.Text;

namespace WebApplication1
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                txtQuery.Text = "东南大学";
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            WebRequest request = WebRequest.Create("http://localhost:8081?query=" + HttpUtility.UrlEncode(txtQuery.Text, Encoding.Default));
            WebResponse response = request.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream());
            string result = sr.ReadToEnd();
            sr.Close();
            ltlResult.Text = result;
        }
    }
}