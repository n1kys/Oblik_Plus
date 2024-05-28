using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Oblik_
{
    public partial class Authorization : Form
    {
        public Authorization()
        {
            InitializeComponent();
            passwordBox.UseSystemPasswordChar = true;
        }

        private void loginButton_Click(object sender, EventArgs e)
        {
            string username = loginBox.Text;
            string password = passwordBox.Text;

            if (username == "admin" && password == "admin")
            {
                
                MainForm mainForm = new MainForm();
                mainForm.Show();
                this.Hide();
                
            }
            else
            {
                MessageBox.Show("Wrong login or password\nAccess denied!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
    }
}
