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

        private String SetHamming(String data, bool Error)
        {
            int len = (int)Math.Ceiling(Math.Log(data.Length + Math.Log(data.Length, 2), 2));
            char[][] Matr = new char[len][];
            for (int i = 0; i < len; i++ )
            {
                Matr[i] = new char[data.Length + len];
            }

            String temp = data;
            for (int i = 0; i < len; i++ )
            {
                temp = temp.Insert((int)Math.Pow(2, i) - 1, "0");
            }

            for (int i = 0; i < temp.Length; i++ )
            {
                String index = Convert.ToString(i + 1, 2);
                index = index.PadLeft(len, '0');
                for (int j = len - 1, k = 0; j >= 0; j--, k++ )
                {
                    Matr[j][i] = index[k];
                }
            }

            for (int i = 0; i < len; i++)
            {
                int code = 0;
                for (int j = 0; j < temp.Length; j++ )
                {
                    if (temp[j] == '1' && Matr[i][j] == '1')
                    {
                        code++;
                    }
                }
                code = code % 2;
                data = data.Insert((int)Math.Pow(2, i) - 1, Convert.ToString(code));
            }

            if (Error == true)
            {
                Random rnd = new Random();
                int ind = rnd.Next(temp.Length);
                char symb = data[ind];

                data = data.Remove(ind, 1);
                if (symb == '0')
                {
                    data = data.Insert(ind, "1");
                }
                else
                {
                    data = data.Insert(ind, "0");
                }
            }
            return data;
        }

        private String UnSetHamming(String data)
        {
            int len = (int)Math.Ceiling(Math.Log(data.Length, 2));
            char[][] Matr = new char[len][];
            for (int i = 0; i < len; i++)
            {
                Matr[i] = new char[data.Length];
            }

            for (int i = 0; i < data.Length; i++)
            {
                String index = Convert.ToString(i + 1, 2);
                index = index.PadLeft(len, '0');
                for (int j = len - 1, k = 0; j >= 0; j--, k++)
                {
                    Matr[j][i] = index[k];
                }
            }

            char[] CheckHamming = new char[len];

            for (int i = 0; i < len; i++)
            {
                int code = 0;
                for (int j = 0; j < data.Length; j++)
                {
                    if (data[j] == '1' && Matr[i][j] == '1')
                    {
                        code++;
                    }
                }
                code = code % 2;
                CheckHamming[len - 1 - i] = (char)(code + '0');
            }
            String temp = "";
            for (int i = 0; i < len; i++)
            {
                temp += CheckHamming[i];
            }
            int ErrorIndex = Convert.ToInt32(temp, 2);
            if (ErrorIndex != 0)
            {
                char symb = data[ErrorIndex - 1];
                data = data.Remove(ErrorIndex - 1, 1);
                if (symb == '0')
                {
                    data = data.Insert(ErrorIndex - 1, "1");
                }
                else
                {
                    data = data.Insert(ErrorIndex - 1, "0");
                }

                Info.ErrorData = true;
            }

            for (int i = len - 1; i >= 0; i-- )
            {
                data = data.Remove((int)Math.Pow(2, i) - 1, 1);
            }
            return data;
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
                if (temp1.Length < 8 && (i != data.Length - 1 || data[i] == 126))
                {
                    temp1 = temp1.PadLeft(8, '0');
                }
                temp += temp1;
            }

            int index = temp.IndexOf("01111110");
            if (index == -1)
            {
                return;
            }
            temp = temp.Substring(index + 8);
            index = temp.IndexOf("01111110");
            if (index == -1)
            {
                return;
            }
            temp = temp.Remove(index);

            temp = temp.Replace("111110", "11111");

            temp = UnSetHamming(temp);

            Info.MachineNumberFromSend = Convert.ToInt16(temp.Remove(2), 2);
            temp = temp.Substring(2);
            Info.MachineNumberToSend = Convert.ToInt16(temp.Remove(2), 2);
            temp = temp.Substring(2);
            if (Info.MachineNumber == Info.MachineNumberFromSend)
            {
                Info.ErrorSndRcv = true;
            }

            int num = (int)Math.Ceiling(((double)temp.Length / 8));
            byte[] data1 = new byte[num];
            for (int i = 0; i < data1.Length; i++ )
            {
                if (temp.Length <= 8)
                {
                    data1[i] = (byte)Convert.ToInt16(temp, 2);
                }
                else
                {
                    data1[i] = (byte)Convert.ToInt16(temp.Remove(8), 2);
                }
                if (temp.Length >= 8)
                {
                    temp = temp.Substring(8);
                }
            }

            Encoding enc = Encoding.GetEncoding(1251);
            String readBuffer = enc.GetString(data1);

            if (Info.MachineNumberToSend != Info.MachineNumber)
            {
                Info.Transfer = true;
            }
            OnRecievedEventArgs arg = new OnRecievedEventArgs(readBuffer);
            OnRecived(this, arg);
        }

        private void serialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            MessageBox.Show(e.EventType.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void serialPort_SendData(String data)
        {
            bool Error = Info.Error;
            try
            {
                Encoding enc = Encoding.GetEncoding(1251);

                String temp = "";
                String temp1 = "";
                if (Info.Transfer == false)
                {
                    temp1 = Convert.ToString(Info.MachineNumber, 2);
                } 
                else
                {
                    temp1 = Convert.ToString(Info.MachineNumberFromSend, 2);
                    Info.Transfer = false;
                }
                if (temp1.Length < 2)
                {
                    temp1 = temp1.PadLeft(2, '0');
                }
                temp += temp1;
                temp1 = Convert.ToString(Info.MachineNumberToSend, 2);
                if (temp1.Length < 2)
                {
                    temp1 = temp1.PadLeft(2, '0');
                }
                temp += temp1;

                for (int i = 0; i < data.Length; i++ )
                {
                    temp1 = "";
                    temp1 = Convert.ToString(data[i], 2);
                    if (temp1.Length < 8)
                    {
                        temp1 = temp1.PadLeft(8, '0');
                    }
                    temp += temp1;
                }

                temp = SetHamming(temp, Error);

                temp = temp.Replace("11111", "111110");
                temp = "01111110" + temp;
                temp = temp + "01111110";
               
                int num = (int)Math.Ceiling(((double)temp.Length / 8));
                byte[] package_b = new byte[num];
                //char[] data1 = new char[num];
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
                    
                    package_b[i] = (byte)temp2;
                    //data1 += (unsigned char)temp2;
                    if (temp.Length >= 8)
                    {
                        temp = temp.Substring(8);
                    }
                }

                //byte[] package_b = enc.GetBytes(data1);

                //while (port.BytesToRead != 0)
                //    Thread.Sleep(21);

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