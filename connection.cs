using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using GsmComm.GsmCommunication;

namespace WindowsFormsApplication1
{
    public partial class connection : Form
    {
        private string portName;
        private int baudRate;
        private int timeout;

        public connection()
        {
            InitializeComponent();
        }

        public void SetData(string portName, int baudRate, int timeout)
        {
            this.portName = portName;
            this.baudRate = baudRate;
            this.timeout = timeout;
        }

        public void GetData(out string portName, out int baudRate, out int timeout)
        {
            portName = this.portName;
            baudRate = this.baudRate;
            timeout = this.timeout;
        }

        private bool EnterNewSettings()
        {
            string newPortName;
            int newBaudRate;
            int newTimeout;

            try
            {
                if (cboPort.Text.Length == 0)
                    throw new FormatException();
                newPortName = cboPort.Text;
            }
            catch (Exception)
            {
                MessageBox.Show(this, "Invalid port name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                cboPort.Focus();
                return false;
            }

            try
            {
                newBaudRate = int.Parse(cboBaudRate.Text);
            }
            catch (Exception)
            {
                MessageBox.Show(this, "Invalid baud rate.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                cboBaudRate.Focus();
                return false;
            }

            try
            {
                newTimeout = int.Parse(cboTimeout.Text);
            }
            catch (Exception)
            {
                MessageBox.Show(this, "Invalid timeout value.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                cboTimeout.Focus();
                return false;
            }

            this.portName = newPortName;
            this.baudRate = newBaudRate;
            this.timeout = newTimeout;

            return true;
        }


        private void btnTest_Click(object sender, System.EventArgs e)
        {
            if (!EnterNewSettings())
                return;

            
            GsmCommMain comm = new GsmCommMain(portName, baudRate, timeout);
            try
            {
                comm.Open();
                while (!comm.IsConnected())
                {
                    
                    if (MessageBox.Show(this, "No phone connected.", "Connection setup",
                        MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation) == DialogResult.Cancel)
                    {
                        comm.Close();
                        return;
                    }
                    
                }

                comm.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Connection error: " + ex.Message, "Connection setup", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            MessageBox.Show(this, "Successfully connected to the phone.", "Connection setup", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (!EnterNewSettings())
                DialogResult = DialogResult.None;
        }
    }
}
