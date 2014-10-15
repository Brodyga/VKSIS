using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VKSIS1
{
    public class OnRecievedEventArgs : EventArgs
    {
        private String data;

        public OnRecievedEventArgs(String data)
        {
            this.data = data;
        }

        public String Data
        {
            get
            {
                return data;
            }
        }
    }

    public delegate void OnRecievedHandler(object sender, OnRecievedEventArgs e);


    class ComPort
    {
        public event OnRecievedHandler OnRecived;

        private SerialPort port;
        public String Name { get; private set; }

        public ComPort(String name, int speed)
        {
            this.Name = name;

            try
            {
                // without parity bit, one stop bit
                port = new SerialPort(name, speed, Parity.None, 8, StopBits.One);
                port.ReadBufferSize = 2048;//1024;
                port.WriteBufferSize = 2048;//1024;
                port.Open();
            }
            catch (Exception e)
            {
                throw;
            }

            // - 
            port.ErrorReceived += new SerialErrorReceivedEventHandler(serialPort_ErrorReceived);
            port.DataReceived += new SerialDataReceivedEventHandler(serialPort_DataReceived);
        }

        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] data = new byte[port.BytesToRead];
            port.Read(data, 0, data.Length);

            String temp = "";
            for (int i = 0; i < data.Length; i++ )
            {
                String temp1 = "";
                temp1 = Convert.ToString(data[i], 2);
                if (temp1.Length < 8)
                {
                    temp1 = temp1.PadLeft(8, '0');
                }
                temp += temp1;
            }
            int index = temp.IndexOf("01111110");
            temp = temp.Substring(index + 8);
            index = temp.IndexOf("01111110");
            temp = temp.Remove(index);

            temp = temp.Replace("111110", "11111");

            byte[] data1 = new byte[(int)Math.Ceiling((double)(temp.Length / 8))];
            for (int i = 0; i < data1.Length; i++ )
            {
                if (temp.Length <= 8)
                {
                    data1[i] = Convert.ToByte(temp, 2);
                }
                else
                {
                    data1[i] = Convert.ToByte(temp.Remove(8), 2);
                }
                if (temp.Length >= 8)
                {
                    temp = temp.Substring(8);
                }
            }

            Encoding enc = Encoding.GetEncoding(1251);
            String readBuffer = enc.GetString(data1);

            OnRecievedEventArgs arg = new OnRecievedEventArgs(readBuffer);
            OnRecived(this, arg);
        }

        private void serialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            MessageBox.Show(e.EventType.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void serialPort_SendData(String data)
        {
            try
            {
                Encoding enc = Encoding.GetEncoding(1251);

                String temp = "";
                for (int i = 0; i < data.Length; i++ )
                {
                    String temp1 = "";
                    temp1 = Convert.ToString(data[i], 2);
                    if (temp1.Length < 8)
                    {
                        temp1 = temp1.PadLeft(8, '0');
                    }
                    temp += temp1;
                }

                temp = temp.Replace("11111", "111110");
                temp = "01111110" + temp;
                temp = temp + "01111110";

                String data1 = "";
                int num = (int)Math.Ceiling((double)(temp.Length / 8));
                for (int i = 0; i < num; i++ )
                {
                    int temp2;
                    if (temp.Length <= 8)
                    {
                        temp2 = Convert.ToInt16(temp, 2);
                    }
                    else
                    {
                        temp2 = Convert.ToInt16(temp.Remove(8), 2);
                    }
                    
                    data1 += (char)temp2;
                    if (temp.Length >= 8)
                    {
                        temp = temp.Substring(8);
                    }
                }

                byte[] package_b = enc.GetBytes(data1);

                while (port.BytesToRead != 0)
                    Thread.Sleep(21);

                // line Request To Send  is available
                port.RtsEnable = true;
                port.Write(package_b, 0, package_b.Length);

                Thread.Sleep(101);      // пауза для корректного завершения работы передатчика
                port.RtsEnable = false;
            }
            catch (Exception ex)
            {
                port.RtsEnable = false;
                throw ex;
            }
        }

        public void close()
        {
            while (port.BytesToRead != 0)
            {
                Thread.Sleep(50);
            }
            port.Close();
        }

        public bool isConnected()
        {
            return (port == null) ? false : port.IsOpen;
        }

    }
}