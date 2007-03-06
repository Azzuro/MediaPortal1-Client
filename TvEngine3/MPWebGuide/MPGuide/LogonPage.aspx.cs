using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

public partial class LogonPage : System.Web.UI.Page
{
  protected void Page_Load(object sender, EventArgs e)
  {
    if (!IsPostBack)
    {
      textBoxLogin.Focus();
    }
  }
  protected void buttonSignIn_Click(object sender, EventArgs e)
  {
    if (FormsAuthentication.Authenticate(textBoxLogin.Text,textBox1.Text))
    {
      FormsAuthentication.RedirectFromLoginPage(textBoxLogin.Text,true);
    }
    else
    {
      textBox1.Text = "";
      
    }
  }
}
