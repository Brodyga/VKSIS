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
        ComPort port;


        public Form1()
        {
            InitializeComponent();
            LoadSpeedValues();
            comboBox1.DataSource = System.IO.Ports.SerialPort.GetPortNames();
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
            if (textBox1.InvokeRequired)
                textBox1.Invoke(new Action<string>((s) => textBox1.AppendText(s)), e.Data);
            else textBox1.AppendText(e.Data);
        }

        private void initializeButton_Click(object sender, EventArgs e)
        {
            try
            {
                port = new ComPort(comboBox1.SelectedItem.ToString(), int.Parse(comboBox2.SelectedItem.ToString()));
                port.OnRecived += new OnRecievedHandler(port_Received);

                initializeButton.Enabled = false;
                stopButton.Enabled = true;
                sendButton.Enabled = true;

                comboBox1.Enabled = false;
                comboBox2.Enabled = false;
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
                port.serialPort_SendData(textBox1.Text);
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

            if (port != null && port.isConnected())
                port.close();
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
        }

    }
}