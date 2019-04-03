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
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            LoadPerBarangay();
            ShowResults();
            timer1.Start();
            timer2.Enabled = true;
            timer2.Start();
        }

        public void LoadPerBarangay()
        {
            string MyConnectionString;
            string mySQLQuery;
            OleDbCommand myCommand;
            OleDbConnection myConnection;
            OleDbDataAdapter myDataAdapter = new OleDbDataAdapter();
            DataTable myDataTable = new DataTable();

            try
            {
                MyConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=Database.mdb;";
                myConnection = new OleDbConnection(MyConnectionString);
                mySQLQuery = "SELECT [barangays].[barangay] AS [Barangay], SUM([table].[status]) AS [Entries Left], SUM([table].[Meer]) AS [Meer], SUM([table].[Sanchez]) AS [Sanchez], SUM([table].[Marasigan]) AS [Marasigan], SUM([table].[Meer]) + SUM([table].[Sanchez]) + SUM([table].[Marasigan]) AS [Total] FROM [barangays] INNER JOIN [table] ON [barangays].[ID] = [table].[barangayID] GROUP BY [barangays].[barangay]";
                myCommand = new OleDbCommand(mySQLQuery, myConnection);
                myConnection.Open();
                myDataAdapter.SelectCommand = myCommand;
                myDataAdapter.Fill(myDataTable);
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

        public void datagridview_SelectionChanged(Object sender, EventArgs e)
        {
            dataGridView4.ClearSelection();
            dataGridView2.ClearSelection();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label6.Text = DateTime.Now.ToString();
            LoadPerBarangay();
            ShowResults();
        }

        public void ShowResults()
        {
            int totalentries = 0;
            int sum = 0;
            int sum1 = 0;
            int sum2 = 0;
            int sum3 = 0; 
            string row1name = "";
            string row2name = "";
            string row3name = "";
            string row1votes = "";
            string row2votes = "";
            string row3votes = "";

            dataGridView2.Rows.Clear();

            for (int i = 0; i < dataGridView4.Rows.Count; ++i)
            {
                totalentries += Convert.ToInt32(dataGridView4.Rows[i].Cells[1].Value);
                sum += Convert.ToInt32(dataGridView4.Rows[i].Cells[5].Value);
                sum1 += Convert.ToInt32(dataGridView4.Rows[i].Cells[2].Value);
                sum2 += Convert.ToInt32(dataGridView4.Rows[i].Cells[3].Value);
                sum3 += Convert.ToInt32(dataGridView4.Rows[i].Cells[4].Value);
            }

            totalvotesLbl.Text = sum.ToString();
            meerLbl.Text = sum1.ToString();
            sanchezLbl.Text = sum2.ToString();
            marasiganLbl.Text = sum3.ToString();

            if (sum1 >= sum2 && sum1 >= sum3)
            {
                row1name = "Meer";
                row1votes = sum1.ToString();

                if (sum2 > sum3)
                {
                    row2name = "Sanchez";
                    row2votes = sum2.ToString();
                    row3name = "Marasigan";
                    row3votes = sum3.ToString();
                }
                else
                {
                    row3name = "Sanchez";
                    row3votes = sum2.ToString();
                    row2name = "Marasigan";
                    row2votes = sum3.ToString();
                }
            }
            else if (sum2 >= sum1 && sum2 >= sum3)
            {
                row1name = "Sanchez";
                row1votes = sum2.ToString();

                if (sum1 > sum3)
                {
                    row2name = "Meer";
                    row2votes = sum1.ToString();
                    row3name = "Marasigan";
                    row3votes = sum3.ToString();
                }
                else
                {
                    row3name = "Meer";
                    row3votes = sum1.ToString();
                    row2name = "Marasigan";
                    row2votes = sum3.ToString();
                }
            }
            else if (sum3 >= sum1 && sum3 >= sum2)
            {
                row1name = "Marasigan";
                row1votes = sum3.ToString();

                if (sum1 > sum2)
                {
                    row2name = "Meer";
                    row2votes = sum1.ToString();
                    row3name = "Sanchez";
                    row3votes = sum2.ToString();
                }
                else
                {
                    row3name = "Meer";
                    row3votes = sum1.ToString();
                    row2name = "Sanchez";
                    row2votes = sum2.ToString();
                }
            }
            else
            {
                row1name = "Meer";
                row2name = "Sanchez";
                row3name = "Marasigan";
                row1votes = sum1.ToString();
                row2votes = sum2.ToString();
                row3votes = sum3.ToString();
            }

            dataGridView2.ColumnCount = 3;
            dataGridView2.Columns[0].Name = "Rank";
            dataGridView2.Columns[1].Name = "Name";
            dataGridView2.Columns[2].Name = "Votes";

            string[] row1 = new string[] { "1", row1name, row1votes };
            string[] row2 = new string[] { "2", row2name, row2votes };
            string[] row3 = new string[] { "3", row3name, row3votes };
            string[] row4 = new string[] { "4", "Total", sum.ToString() };
            object[] rows = new object[] { row1, row2, row3, row4 };

            foreach (string[] rowArray in rows)
            {
                dataGridView2.Rows.Add(rowArray);
            }

            foreach (DataGridViewRow row in dataGridView2.Rows)
            {
                string rowname = row.Cells[1].Value.ToString();

                if (rowname == "Meer")
                {
                    row.DefaultCellStyle.ForeColor = Color.Red;
                }
                else if (rowname == "Sanchez")
                {
                    row.DefaultCellStyle.ForeColor = Color.HotPink;
                }
                else if (rowname == "Marasigan")
                {
                    row.DefaultCellStyle.ForeColor = Color.DeepSkyBlue;
                }
                else if (rowname == "Total")
                {
                    row.DefaultCellStyle.ForeColor = Color.Black;
                }
            }

            entriesLbl.Text = (125 - totalentries).ToString() + "/125";
        }

        private void dataGridView4_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dataGridView4.Rows[0].Selected = false;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            Close();
        }
    }
}

