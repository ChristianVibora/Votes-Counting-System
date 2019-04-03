using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        Form4 form4;
        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            LogIn();
        }

        public void LogIn()
        {
            string MyConnectionString;
            string mySQLQuery;
            OleDbCommand myCommand;
            OleDbDataReader myDataReader;
            OleDbConnection myConnection;

            try
            {
                MyConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=Database.mdb;";
                myConnection = new OleDbConnection(MyConnectionString);
                mySQLQuery = "SELECT * FROM [user] WHERE [username] = @username AND [password] = @password";
                myCommand = new OleDbCommand(mySQLQuery, myConnection);
                myCommand.Parameters.Add(new OleDbParameter("@username", (textBox1.Text)));
                myCommand.Parameters.Add(new OleDbParameter("@password", (textBox2.Text)));
                myConnection.Open();
                myDataReader = myCommand.ExecuteReader();

                bool userFound = false;
                string username = "";
                string password = "";
               
                while (myDataReader.Read())
                {
                    username = myDataReader["username"].ToString();
                    password = myDataReader["password"].ToString();
                    userFound = true;
                }

                if (userFound == true)
                {
                    MessageBox.Show("Log In Successful. Welcome " + username + "!", "Log In Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    if (form4 == null)
                    {
                        form4 = new Form4();
                    }
                    form4.Show(this);
                    Hide();
                }
                else
                    MessageBox.Show("Log in Failed. Username and Password Does Not Matched! Please Try Again.", "Log In Failed", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                textBox1.Clear();
                textBox2.Clear();
                textBox1.Focus();
                myConnection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
