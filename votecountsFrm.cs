using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

using GsmComm.PduConverter;
using GsmComm.PduConverter.SmartMessaging;
using GsmComm.GsmCommunication;
using GsmComm.Interfaces;
using GsmComm.Server;
using System.Collections.Generic;
using System.Data.OleDb;
using GSMCommDemo;
using System.Text.RegularExpressions;

namespace WindowsFormsApplication1
{
    public partial class votecountsFrm : Form
    {
        Form2 form2;

        private GsmCommMain comm;
        private bool registerMessageReceived;
        private SmsServer smsServer;
        private SecuritySettings remotingSecurity;
        private delegate void SetTextCallback(string text);
        string varmessage;

        public votecountsFrm()
        {
            InitializeComponent();

            this.comm = null;
            this.registerMessageReceived = false;
            this.smsServer = null;
            this.remotingSecurity = new SecuritySettings();
        }

        private delegate void ConnectedHandler(bool connected);
        private void OnPhoneConnectionChange(bool connected)
        {
            lblNotConnected.Visible = !connected;
        }

        private void comm_PhoneConnected(object sender, EventArgs e)
        {
            this.Invoke(new ConnectedHandler(OnPhoneConnectionChange), new object[] { true });
        }

        private void comm_PhoneDisconnected(object sender, EventArgs e)
        {
            this.Invoke(new ConnectedHandler(OnPhoneConnectionChange), new object[] { false });
        }

        private void comm_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            try
            {
                IMessageIndicationObject obj = e.IndicationObject;
                if (obj is ShortMessage)
                {
                    ShortMessage msg = (ShortMessage)obj;
                    SmsPdu pdu = comm.DecodeReceivedMessage(msg);
                    ReceiveMessage(pdu);
                    return;
                }
                MessageBox.Show("Error: Unknown notification object!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OutputCPnumber(string text)
        {
            if (this.cellphonenumberTxtbx.InvokeRequired)
            {
                SetTextCallback stc = new SetTextCallback(OutputCPnumber);
                this.Invoke(stc, new object[] { text });
            }
            else
            {
                cellphonenumberTxtbx.Text = text;
            }
        }

        private void OutputMeerVotes(string text)
        {
            if (this.meer_votesTxtbx.InvokeRequired)
            {
                SetTextCallback stc = new SetTextCallback(OutputMeerVotes);
                this.Invoke(stc, new object[] { text });
            }
            else
            {
                meer_votesTxtbx.Text = text;
            }
        }

        private void OutputSanchezVotes(string text)
        {
            if (this.sanchez_votesTxtbx.InvokeRequired)
            {
                SetTextCallback stc = new SetTextCallback(OutputSanchezVotes);
                this.Invoke(stc, new object[] { text });
            }
            else
            {
                sanchez_votesTxtbx.Text = text;
            }
        }

        private void OutputMarasiganVotes(string text)
        {
            if (this.marasigan_votesTxtbx.InvokeRequired)
            {
                SetTextCallback stc = new SetTextCallback(OutputMarasiganVotes);
                this.Invoke(stc, new object[] { text });
            }
            else
            {
                marasigan_votesTxtbx.Text = text;
            }
        }

        private void ReceiveMessage(SmsPdu pdu)
        {
            try
            {
                if (pdu is SmsDeliverPdu)
                {
                    // Received message
                    SmsDeliverPdu data = (SmsDeliverPdu)pdu;
                    varmessage = data.UserDataText;
                    string[] message = varmessage.Split(' ');
                    if (message.Length == 3)
                    {
                        for (int i = 0; i < message.Length; i++)
                        {
                            string[] tmp_msg = message[i].Split('/');

                            if (tmp_msg[0].ToUpper() == "RM")
                            {
                                OutputMeerVotes(tmp_msg[1]);
                            }
                            else if (tmp_msg[0].ToUpper() == "ES")
                            {
                                OutputSanchezVotes(tmp_msg[1]);
                            }
                            else if (tmp_msg[0].ToUpper() == "AM")
                            {
                                OutputMarasiganVotes(tmp_msg[1]);
                            }
                        }
                        OutputCPnumber(data.OriginatingAddress);
                        return;
                    }
                    else if (message.Length == 1)
                    {
                        if (message[0].ToUpper() == "VERIFY")
                        {
                            OutputMeerVotes("VERIFY");
                            OutputSanchezVotes("VERIFY");
                            OutputMarasiganVotes("VERIFY");
                            OutputCPnumber(data.OriginatingAddress);
                            return;
                        }
                    }
                    else
                    {
                        OutputMeerVotes("0");
                        OutputSanchezVotes("0");
                        OutputMarasiganVotes("0");
                        OutputCPnumber(data.OriginatingAddress);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void votecountsFrm_Load(object sender, EventArgs e)
        {
            LoadConnection();
            LoadForm();
        }

        public void LoadConnection()
        {
            // Prompt user for connection settings
            string portName = GsmCommMain.DefaultPortName;
            int baudRate = GsmCommMain.DefaultBaudRate;
            int timeout = GsmCommMain.DefaultTimeout;
            connection dlg = new connection();
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.SetData(portName, baudRate, timeout);
            if (dlg.ShowDialog(this) == DialogResult.OK)
                dlg.GetData(out portName, out baudRate, out timeout);
            else
            {
                Environment.Exit(0);
                return;
            }

            
            comm = new GsmCommMain(portName, baudRate, timeout);
            
            comm.PhoneConnected += new EventHandler(comm_PhoneConnected);
            comm.PhoneDisconnected += new EventHandler(comm_PhoneDisconnected);

            bool retry;
            do
            {
                retry = false;
                try
                {
                    
                    comm.Open();
                    
                }
                catch (Exception)
                {
                    
                    if (MessageBox.Show(this, "Unable to open the port.", "Error",
                        MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning) == DialogResult.Retry)
                        retry = true;
                    else
                    {
                        Close();
                        return;
                    }
                }
            }
            while (retry);

            // Add custom commands
            ProtocolCommand[] commands = new ProtocolCommand[]
			{
				new ProtocolCommand("Send", true, false, false), // NeedsData
				new ProtocolCommand("Receive", false, false, false),
				new ProtocolCommand("ExecCommand", true, false, false), // NeedsData
				new ProtocolCommand("ExecCommand2", true, false, true), // NeedsData, NeedsError
				new ProtocolCommand("ExecAndReceiveMultiple", true, false, false), // NeedsData
				new ProtocolCommand("ExecAndReceiveAnything", true, true, false), // NeedsData, NeedsPattern
				new ProtocolCommand("ReceiveMultiple", false, false, false),
				new ProtocolCommand("ReceiveAnyhing", false, true, false) // NeedsPattern
			};
        }

       

       public void LoadForm()
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
                mySQLQuery = "SELECT [clusternumber] AS [Cluster #], [barangay] AS [Barangay], [Meer], [Sanchez], [Marasigan], [Meer] + [Sanchez] + [Marasigan] AS [Total] FROM [table] WHERE [Status] = 0 ORDER BY [receivetime] DESC";
                myCommand = new OleDbCommand(mySQLQuery, myConnection);
                myConnection.Open();
                myDataAdapter.SelectCommand = myCommand;
                myDataAdapter.Fill(myDataTable);
                dataGridView1.DataSource = myDataTable;
                myConnection.Close();

                Details();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

       public void Details()
       {
           int sum = 0;
           int sum1 = 0;
           int sum2 = 0;
           int sum3 = 0;
           string row1name = "";
           string row2name = "";
           string row3name = "";
           int row1votes = 0;
           int row2votes = 0;
           int row3votes = 0;

           dataGridView2.Rows.Clear();

           for (int i = 0; i < dataGridView1.Rows.Count; ++i)
           {
               sum += Convert.ToInt32(dataGridView1.Rows[i].Cells[5].Value);
               sum1 += Convert.ToInt32(dataGridView1.Rows[i].Cells[2].Value);
               sum2 += Convert.ToInt32(dataGridView1.Rows[i].Cells[3].Value);
               sum3 += Convert.ToInt32(dataGridView1.Rows[i].Cells[4].Value);
           }

           totalvotesLbl.Text = sum.ToString();
           meerLbl.Text = sum1.ToString();
           sanchezLbl.Text = sum2.ToString();
           marasiganLbl.Text = sum3.ToString();

           if (sum1 >= sum2 && sum1 >= sum3) {
               row1name = "Meer";
               row1votes = sum1;

               if (sum2 > sum3)
               {
                   row2name = "Sanchez";
                   row2votes = sum2;
                   row3name = "Marasigan";
                   row3votes = sum3;
               }
               else
               {
                   row3name = "Sanchez";
                   row3votes = sum2;
                   row2name = "Marasigan";
                   row2votes = sum3;
               }
           }
           else if (sum2 >= sum1 && sum2 >= sum3) {
               row1name = "Sanchez";
               row1votes = sum2;

               if (sum1 > sum3)
               {
                   row2name = "Meer";
                   row2votes = sum1;
                   row3name = "Marasigan";
                   row3votes = sum3;
               }
               else
               {
                   row3name = "Meer";
                   row3votes = sum1;
                   row2name = "Marasigan";
                   row2votes = sum3;
               }
           }
           else if (sum3 >= sum1 && sum3 >= sum2)
           {
               row1name = "Marasigan";
               row1votes = sum3;

               if (sum1 > sum2)
               {
                   row2name = "Meer";
                   row2votes = sum1;
                   row3name = "Sanchez";
                   row3votes = sum2;
               }
               else
               {
                   row3name = "Meer";
                   row3votes = sum1;
                   row2name = "Sanchez";
                   row2votes = sum2;
               }
           }
           else
           {
               row1name = "Meer";
               row2name = "Sanchez";
               row3name = "Marasigan";
               row1votes = sum1;
               row2votes = sum2;
               row3votes = sum3;
           }

           dataGridView2.ColumnCount = 3;
           dataGridView2.Columns[0].Name = "Rank";
           dataGridView2.Columns[1].Name = "Name";
           dataGridView2.Columns[2].Name = "Votes";

           string[] row1 = new string[] { "1", row1name, String.Format("{0:n0}", row1votes) };
           string[] row2 = new string[] { "2", row2name, String.Format("{0:n0}", row2votes) };
           string[] row3 = new string[] { "3", row3name, String.Format("{0:n0}", row3votes) };
           string[] row4 = new string[] { "4", "Total", String.Format("{0:n0}", sum) };
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

           int totalentries = 0;
           totalentries = dataGridView1.Rows.Count;
           entriesLbl.Text = totalentries.ToString() + "/125";

           if (totalentries == 125)
           {
               if (Convert.ToInt32(meerLbl.Text) > Convert.ToInt32(sanchezLbl.Text) && Convert.ToInt32(meerLbl.Text) > Convert.ToInt32(marasiganLbl.Text))
               {
                   MessageBox.Show("Vote Counting Finish. Rey Meer Won!", "Meer", MessageBoxButtons.OK, MessageBoxIcon.Information);
               }
               else if (Convert.ToInt32(sanchezLbl.Text) > Convert.ToInt32(meerLbl.Text) && Convert.ToInt32(meerLbl.Text) > Convert.ToInt32(marasiganLbl.Text))
               {
                   MessageBox.Show("Vote Counting Finish. Sanchez Won!", "Sanchez", MessageBoxButtons.OK, MessageBoxIcon.Information);
               }
               else if (Convert.ToInt32(marasiganLbl.Text) > Convert.ToInt32(meerLbl.Text) && Convert.ToInt32(marasiganLbl.Text) > Convert.ToInt32(sanchezLbl.Text))
               {
                   MessageBox.Show("Vote Counting Finish. Sanchez Won!", "Sanchez", MessageBoxButtons.OK, MessageBoxIcon.Information);
               }
           }
       }

       public void datagridview_SelectionChanged(Object sender, EventArgs e)
       {
           dataGridView1.ClearSelection();
           dataGridView2.ClearSelection();
       }

       public void UpdateVotes()
       {
           int result = 0;
           string MyConnectionString;
           string mySQLQuery;
           OleDbCommand myCommand;
           OleDbConnection myConnection;

           try
           {
               MyConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=Database.mdb;";
               myConnection = new OleDbConnection(MyConnectionString);
               mySQLQuery = "UPDATE [table] SET [Meer] = @meer, [Sanchez] = @sanchez, [Marasigan] = @marasigan, [receivetime] = @receivetime WHERE [Status] = 1 AND [cellphonenumber] = @cellphonenumber OR [alternatecellphonenumber] = @cellphonenumber";
               myCommand = new OleDbCommand(mySQLQuery, myConnection);
               myCommand.Parameters.Add(new OleDbParameter("@meer", (meer_votesTxtbx.Text)));
               myCommand.Parameters.Add(new OleDbParameter("@sanchez", (sanchez_votesTxtbx.Text)));
               myCommand.Parameters.Add(new OleDbParameter("@marasigan", (marasigan_votesTxtbx.Text)));
               myCommand.Parameters.Add(new OleDbParameter("@receivetime", String.Format("{0:G}", DateTime.Now)));
               myCommand.Parameters.Add(new OleDbParameter("@cellphonenumber", (cellphonenumberTxtbx.Text)));
               myConnection.Open();
               result = myCommand.ExecuteNonQuery();
               myCommand.Dispose();
               myConnection.Close();

               if (result == 1)
               {
                   UpdateStatus();
                   SendMessage("Your Votes Have Been Registered! Thank You.", cellphonenumberTxtbx.Text);
                   OutputCPnumber("");
               }
               else
               {
                   UpdateLog("Received", "Invalid Phone Number", cellphonenumberTxtbx.Text, varmessage);
               }

               LoadForm();
           }
           catch (Exception ex)
           {
               MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
           }
       }

       public void UpdateStatus()
       {
           int contactfound = 0;
           string MyConnectionString;
           string mySQLQuery;
           OleDbCommand myCommand;
           OleDbConnection myConnection;

           try
           {
               MyConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=Database.mdb;";
               myConnection = new OleDbConnection(MyConnectionString);
               mySQLQuery = "UPDATE [table] SET [status] = 0 WHERE NOT [Meer] = 0 AND NOT [Sanchez] = 0  AND NOT [Marasigan] = 0 AND [cellphonenumber] = @cellphonenumber OR [alternatecellphonenumber] = @cellphonenumber";
               myCommand = new OleDbCommand(mySQLQuery, myConnection);
               myCommand.Parameters.Add(new OleDbParameter("@cellphonenumber", (cellphonenumberTxtbx.Text)));
               myConnection.Open();
               contactfound = myCommand.ExecuteNonQuery();
               myCommand.Dispose();
               myConnection.Close();
           }
           catch (Exception ex)
           {
               MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
           }
       }

       private void resetBttn_Click(object sender, EventArgs e)
       {
           Reset();
       }

       public void Reset()
       {
           string MyConnectionString;
           string mySQLQuery;
           OleDbCommand myCommand;
           OleDbConnection myConnection;

           try
           {
               MyConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=Database.mdb;";
               myConnection = new OleDbConnection(MyConnectionString);
               mySQLQuery = "UPDATE [table] SET [status] = 1, [Meer] = 0, [Sanchez] = 0, [Marasigan] = 0, [receivetime] = @receivetime";
               myCommand = new OleDbCommand(mySQLQuery, myConnection);
               myCommand.Parameters.Add(new OleDbParameter("@receivetime", String.Format("{0:G}", DateTime.Now)));
               myConnection.Open();
               myCommand.ExecuteNonQuery();
               myCommand.Dispose();
               myConnection.Close();

               LoadForm();
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

        private void timer1_Tick(object sender, EventArgs e)
        {
            label6.Text = DateTime.Now.ToString();
        }

        private void votecountsFrm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Kill SMS server if running
            DestroySmsServer();

            // Clean up comm object
            if (comm != null)
            {
                // Unregister events
               
                comm.PhoneConnected -= new EventHandler(comm_PhoneConnected);
                comm.PhoneDisconnected -= new EventHandler(comm_PhoneDisconnected);
                if (registerMessageReceived)
                {
                    comm.MessageReceived -= new MessageReceivedEventHandler(comm_MessageReceived);
                    registerMessageReceived = false;
                }

                // Close connection to phone
                if (comm != null && comm.IsOpen())
                    comm.Close();

                comm = null;
                Environment.Exit(0);
            }
        }

        private void DestroySmsServer()
        {
            if (smsServer != null)
            {
                if (smsServer.IsRunning())
                    smsServer.Stop();

                // Unregister events

                smsServer = null;
            }
        }

        private void btnMsgRoutingOn_Click(object sender, System.EventArgs e)
        {
            btnMsgRoutingOn.Enabled = false;
            btnMsgRoutingOn.BackColor = Color.Red;
            btnMsgRoutingOff.Enabled = true;
            btnMsgRoutingOff.BackColor = Color.White;

            try
            {
                // Enable direct message routing to the application
                if (!registerMessageReceived)
                {
                    comm.MessageReceived += new MessageReceivedEventHandler(comm_MessageReceived);
                    registerMessageReceived = true;
                }
                comm.EnableMessageRouting();
                MessageBox.Show("Message receiving activated.", "Message Receiving", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void btnMsgRoutingOff_Click(object sender, System.EventArgs e)
        {
            btnMsgRoutingOff.Enabled = false;
            btnMsgRoutingOff.BackColor = Color.Red;
            btnMsgRoutingOn.Enabled = true;
            btnMsgRoutingOn.BackColor = Color.White;

            try
            {
                // Disable message routing
                comm.DisableMessageRouting();
                if (registerMessageReceived)
                {
                    comm.MessageReceived -= new MessageReceivedEventHandler(comm_MessageReceived);
                    registerMessageReceived = false;
                }
                MessageBox.Show("Message receiving deactivated.", "Message Receiving", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void SendMessage(string message, string cellphonenumber)
        {
            try
            {
                // Send an SMS message
                SmsSubmitPdu pdu;
                // Send message in the default format
                pdu = new SmsSubmitPdu(message, cellphonenumber);
                comm.SendMessage(pdu);
                UpdateLog("Sent", "Dashboard Messages", cellphonenumberTxtbx.Text, message);
            }
            catch (Exception)
            {
                UpdateLog("Failed", "Sending Failed", cellphonenumberTxtbx.Text, message);
            }
        }

        public void UpdateLog(string type, string description, string cpnumber, string message)
        {
            string MyConnectionString;
            string mySQLQuery;
            OleDbCommand myCommand;
            OleDbConnection myConnection;
            try
            {
                MyConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=Database.mdb;";
                myConnection = new OleDbConnection(MyConnectionString);
                mySQLQuery = "INSERT INTO [log] ([type], [senderrecepient], [description], [message], [eventtime]) VALUES (@type, @senderrecepient, @description, @message, @eventtime)";
                myCommand = new OleDbCommand(mySQLQuery, myConnection);
                myCommand.Parameters.Add(new OleDbParameter("@type", (type)));
                myCommand.Parameters.Add(new OleDbParameter("@senderrecepient", (cpnumber)));
                myCommand.Parameters.Add(new OleDbParameter("@description", (description)));
                myCommand.Parameters.Add(new OleDbParameter("@message", (message)));
                myCommand.Parameters.Add(new OleDbParameter("@eventtime", (String.Format("{0:G}", DateTime.Now))));
                myConnection.Open();
                myCommand.ExecuteNonQuery();
                myCommand.Dispose();
                myConnection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            form2 = new Form2();
            form2.FormClosed += form2_FormClosed;
            timer2.Stop();
            form2.Show(this);
            Hide();
        }

        private void form2_FormClosed(object sender, EventArgs e)
        {
            Show();
            timer2.Start();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                timer2.Enabled = true;
                timer2.Start();
            }
            else if (checkBox1.Checked == false)
            {
                timer2.Stop();
                timer2.Enabled = false;
            }
        }

        private void votecountsFrm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void cellphonenumberTxtbx_TextChanged(object sender, EventArgs e)
        {
            if (meer_votesTxtbx.Text != "" & sanchez_votesTxtbx.Text != "" & marasigan_votesTxtbx.Text != "" & Regex.IsMatch(meer_votesTxtbx.Text, @"^\d+$") == true & Regex.IsMatch(sanchez_votesTxtbx.Text, @"^\d+$") == true & Regex.IsMatch(marasigan_votesTxtbx.Text, @"^\d+$") == true)
            {
                UpdateLog("Received", "Votes Received", cellphonenumberTxtbx.Text, varmessage);
                UpdateVotes();
            }
            else if (meer_votesTxtbx.Text == "VERIFY" & sanchez_votesTxtbx.Text == "VERIFY" & marasigan_votesTxtbx.Text == "VERIFY")
            {
                UpdateLog("Received", "Verification Request", cellphonenumberTxtbx.Text, varmessage);
                Verify();
            }
            else
            {
                UpdateLog("Received", "Invalid Message", cellphonenumberTxtbx.Text, varmessage);
            }
        }

        public void Verify()
        {
            string MyConnectionString;
            string mySQLQuery;
            OleDbCommand myCommand;
            OleDbDataReader myDataReader;
            OleDbConnection myConnection;
            string cluster = "";
            string barangay = "";
            bool found = false;
            try
            {
                MyConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=Database.mdb;";
                myConnection = new OleDbConnection(MyConnectionString);
                mySQLQuery = "SELECT * FROM [table] WHERE [cellphonenumber] = @cellphonenumber OR [alternatecellphonenumber] = @cellphonenumber";
                myCommand = new OleDbCommand(mySQLQuery, myConnection);
                myCommand.Parameters.Add(new OleDbParameter("@cellphonenumber", (cellphonenumberTxtbx.Text)));
                myConnection.Open();
                myDataReader = myCommand.ExecuteReader();

                while (myDataReader.Read())
                {
                    cluster = myDataReader["clusternumber"].ToString();
                    barangay = myDataReader["barangay"].ToString();
                    found = true;
                }

                if (found == true)
                {
                    SendMessage("Your Mobile Number is Authorized to Send Votes for Barangay: " + barangay + ", Cluster Number: " + cluster + ". Thank You!", cellphonenumberTxtbx.Text);
                }
                else
                {
                    SendMessage("Sorry, Your Mobile Number is NOT Authorized to Send Text Messages to this Phone Number. Thank You!", cellphonenumberTxtbx.Text);
                }
               
                myConnection.Close();


            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
      
    }
}
