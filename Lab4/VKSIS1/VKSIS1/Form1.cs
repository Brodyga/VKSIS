using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VKSIS1
{
    public partial class Form1 : Form
    {
        ComPort portToSend;
        ComPort portToRecieve;


        public Form1()
        {
            InitializeComponent();
            LoadSpeedValues();
            comboBox1.DataSource = System.IO.Ports.SerialPort.GetPortNames();
            comboBox3.DataSource = System.IO.Ports.SerialPort.GetPortNames();
            comboBox4.Items.Add(1);
            comboBox4.Items.Add(2);
            comboBox4.Items.Add(3);
            comboBox4.SelectedItem = 1;
            comboBox5.Enabled = false;

            checkBox1.Enabled = false;
            checkBox2.Enabled = false;
        }


        private void LoadSpeedValues()
        {
            int[] values = new int[] { 110, 150, 300, 600, 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200 };
            comboBox2.DataSource = values;
            comboBox2.SelectedItem = 9600;
        }

        // Приём данных
        private void port_Received(object sender, OnRecievedEventArgs e)
        {
            if (Info.ErrorSndRcv == true || Info.ErrorData == true)
            {
                if (Info.ErrorSndRcv == true)
                {
                    textBox2.Text = "Machine Shut Down";
                }
                if (Info.ErrorData == true)
                {
                    textBox2.Text = "Data Damaged";
                }
                Info.ErrorSndRcv = false;
                Info.Transfer = false;
                Info.ErrorData = false;
            } 
            else
            {
                textBox2.Text = "";
                if (checkBox2.Checked == true)
                {
                    Info.Data = e.Data;
                    Info.Transfer = true;
                    sendButton_Click(sender, e);
                }
                else
                {
                    if (textBox1.InvokeRequired)
                    {
                        if (Info.Transfer == false)
                        {
                            textBox1.Invoke(new Action<string>((s) => textBox1.AppendText(s)), e.Data);
                        }
                        else
                        {
                            Info.Data = e.Data;
                            sendButton_Click(sender, e);
                        }
                    }
                    //textBox1.Invoke(new Action<string>((s) => textBox1.AppendText(s)), e.Data);
                    else
                    {
                        if (Info.Transfer == false)
                        {
                            textBox1.AppendText(e.Data);
                        }
                        else
                        {
                            Info.Data = e.Data;
                            sendButton_Click(sender, e);
                        }
                    }
                }
            }
        }

        private void initializeButton_Click(object sender, EventArgs e)
        {
            try
            {
                portToSend = new ComPort(comboBox3.SelectedItem.ToString(), int.Parse(comboBox2.SelectedItem.ToString()));
                portToRecieve = new ComPort(comboBox1.SelectedItem.ToString(), int.Parse(comboBox2.SelectedItem.ToString()));
                portToRecieve.OnRecived += new OnRecievedHandler(port_Received);

                initializeButton.Enabled = false;
                stopButton.Enabled = true;
                if (checkBox2.Checked == false)
                {
                    sendButton.Enabled = true;
                }

                comboBox1.Enabled = false;
                comboBox2.Enabled = false;
                comboBox3.Enabled = false;
                comboBox4.Enabled = false;

                comboBox5.Enabled = true;
                comboBox5.Items.Add(1);
                comboBox5.Items.Add(2);
                comboBox5.Items.Add(3);
                comboBox5.Items.Remove(comboBox4.SelectedItem);
                comboBox5.SelectedIndex = 0;

                checkBox1.Enabled = true;
                checkBox2.Enabled = true;

                Info.MachineNumber = Convert.ToInt32(comboBox4.SelectedItem);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (Info.ErrorSndRcv == true || Info.ErrorData == true)
                {
                    textBox2.Text = "";
                    Info.ErrorSndRcv = false;
                    Info.ErrorData = false;
                }
                Info.Error = checkBox1.Checked;
                if (Info.Transfer == false)
                {
                    Info.MachineNumberToSend = Convert.ToInt32(comboBox5.SelectedItem);
                    portToSend.serialPort_SendData(textBox1.Text);
                }
                else
                {
                    portToSend.serialPort_SendData(Info.Data);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            stopButton.Enabled = false;
            sendButton.Enabled = false;
            initializeButton.Enabled = true;
            comboBox1.Enabled = true;
            comboBox2.Enabled = true;
            comboBox3.Enabled = true;
            comboBox4.Enabled = true;
            comboBox5.Enabled = false;
            comboBox5.Items.Remove(1);
            comboBox5.Items.Remove(2);
            comboBox5.Items.Remove(3);
            checkBox1.Enabled = false;
            checkBox2.Enabled = false;

            if (portToSend != null && portToSend.isConnected())
                portToSend.close();
            if (portToRecieve != null && portToRecieve.isConnected())
                portToRecieve.close();
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked == true)
            {
                sendButton.Enabled = false;
            }
            else
            {
                sendButton.Enabled = true;
            }
        }

    }
}