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
    public partial class Form3 : Form
    {

        public Form3()
        {
            InitializeComponent();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            LoadLog();
        }
        public void LoadLog()
        {

            string MyConnectionString;
            string mySQLQuery;
            OleDbCommand myCommand;
            OleDbConnection myConnection;
            OleDbDataAdapter myDataAdapter;
            DataTable myDataTable;

            try
            {

                MyConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=Database.mdb;";
                myConnection = new OleDbConnection(MyConnectionString);
                mySQLQuery = "SELECT [ID], [type], [senderrecepient], [description], [message], FORMATDATETIME([eventtime]) AS [eventtime] FROM [log] ORDER BY [eventtime] DESC";
                myCommand = new OleDbCommand(mySQLQuery, myConnection);
                myConnection.Open();
                myDataAdapter = new OleDbDataAdapter(myCommand);
                myDataAdapter.Fill(myDataTable = new DataTable());
                dataGridView4.DataSource = myDataTable;
                myConnection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataGridView_ColumnAdded(object sender, DataGridViewColumnEventArgs e)
        {
            var datagridview = sender as DataGridView;
            datagridview.Columns[e.Column.Index].SortMode = DataGridViewColumnSortMode.NotSortable;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            LoadLog();
        }

        private void Form3_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}

       

        
            
        
    

