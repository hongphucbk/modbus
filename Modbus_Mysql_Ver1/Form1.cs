using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EasyModbus;
using MySql;
using MySql.Data.MySqlClient;
using Tutorial.SqlConn;
using System.Collections;
using Quobject.SocketIoClientDotNet.Client;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace Modbus_Mysql_Ver1
{
    public partial class Form1 : Form
    {
        private string ip = "";
        private int port = 502;
        private ushort startaddress = 1;
        private ushort lengh = 10;
        public  ModbusClient[] modbustcp = new ModbusClient[10];
        public int[] Bvalue = new int[20];//saving data from res
        public string[] IDDevices = new string[10];
        public string[] IPAddress = new string[10];
        public string[] Resgisters = new string[100];

        public string[] BResgisters = new string[20];   //register
        public byte[] BSlaveID = new byte[255];         //slave ID
        public string[] BIDDevice = new string[20];
        public string[] BIPadress = new string[20];     //IPaddress/
        public string[] BIDRegister = new string[20];
        public int[] BIndexDevice = new int[20];        //List of Client
        public float[] BScaleValue = new float[100];

        public double errorCount = 0;

        public Queue DataQueue = new Queue();

        public Queue<SendData> sendData = new Queue<SendData>();

        public Socket socket;
        //New
        ArrayList arrMBValue = new ArrayList();

        ArrayList arrOldData = new ArrayList();

        public Form1()
        {
            InitializeComponent();            
        }


        MySqlConnection connection = DBUtils.GetDBConnection();

        private void button1_Click(object sender, EventArgs e)
        {
            //timer1.Enabled = true;
            connection.Open();
            ReadALLdevice();
            CreateClient();
        }
        private void CreateClient()
        {
            for(int i=0;i<IPAddress.Length;i++)
            {
                if (IPAddress[i]!=null)
                {
                    modbustcp[i] = new ModbusClient(IPAddress[i], 502);
                    modbustcp[i].Connect();
                    // modbustcp = ModbusClient(ip, port);
                }
            }
            
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            ReadALLdevice();
            ReadAllParameter();
            readDataFromDeviceNew();

            textBox2.Clear();
            txb_errorCount.Text = errorCount.ToString();

            string dataSend;
            
            foreach (MBValue item in arrMBValue)
            {
                
                string today = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                lbToday.Text = today;

                textBox2.Text += today + ": ID Device: " +item.DeviceId + " -- IP: " + item.Ipadress + " -- Value: " + item.Value + "\r\n";
                //data = today + ": Parameter id = " + item.RegisterId + ", Value = " +  item.Value;

                dataSend = @"{ ""device_id"": " + item.DeviceId + @",""ipaddress"": """ + item.Ipadress.ToString()
                         + @""", ""parameter_id"": " + item.RegisterId + @", ""value"": " + item.Value
                         + @", ""created_at"": """ + item.TimeStamp + @" ""}";

                

                DataQueue.Enqueue(dataSend);
                if (DataQueue.Count > 1)
                {
                    txtStatus.Clear();
                    txtStatus.Text += DataQueue.Peek().ToString() + "\r\n ";
                }
                SendData dataSendToSQL = new SendData(item.RegisterId, item.Value, item.ScaleValue, today);
                sendData.Enqueue(dataSendToSQL);
            }
        }
        /// <summary>
        /// FALSE = TRUNG;
        /// TRUE = KHONG TRUNG
        /// </summary>
        /// <param name="Current"></param>
        /// <returns></returns>
        public bool is_doublecheck(SendData Current) 
        {
            int count = 0;
            foreach (SendData item in arrOldData)
            {
                if (item.Id == Current.Id)
                {
                    if (item.Value == Current.Value)
                    {
                        count++;
                        return false;
                    }
                    else
                    {
                        item.Value = Current.Value;
                        return true;
                    }
                }
                 
            }
            if (count != 0)
            {
                return false;
            }
            else
            {
                return true;
            }

        }

        
        public void SendDataToSQLDatabase()
        {
            
            SendData Temp;
            while (true)
            {
                if (sendData.Count > 0)
                {
                    Thread.Sleep(100);
                    Temp = sendData.Dequeue();
                    if (is_doublecheck(Temp))
                    {
                        insertToMysql(Temp.Id, Temp.Value, Temp.TimeStamp);
                        Thread.Sleep(500);
                    }
                    
                }
            }
            
        }

        public void SendDataToServer()
        {
            string FirstData;
            int DataCount;
            socket = IO.Socket("http://localhost:6001");
            socket.On(Socket.EVENT_CONNECT, () =>
            {
                socket.Emit("notification", "Sensor Đã kết nối");
            });
            while (true)
            {
                DataCount = DataQueue.Count;
                if (DataCount > 0)
                {
                    try
                    {
                        FirstData = DataQueue.Dequeue().ToString();
                        socket.Emit("modbustcp", FirstData);
                        Thread.Sleep(50);
                    }
                    catch (Exception ex)
                    {
                        // Xử lý ngoại lệ trong phần này …
                        MessageBox.Show(ex.ToString());
                    }
                }
                Thread.Sleep(50);
            }
            //socket.Disconnect();
            //socket.Close();
        }
        
        private void insertToMysql(int parameter_id, double value, string timestamp)
        {
            try
            {
                string sql = "Insert into ins_modbustcp_value(parameter_id, value, created_at) "
                        + " values (" + parameter_id + "," + value + ",'" +  timestamp + "')";

                MySqlCommand cmd = connection.CreateCommand();
                cmd.CommandText = sql;

                int rowCount = cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {

                connection.Close();
                connection.Dispose();
                connection.Open();
            }
            
        }
        private void ReadALLdevice()
        {
            //conection 

            string sql = "Select * from ins_modbustcp_device ";

           
            MySqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            MySqlDataReader reader = cmd.ExecuteReader();//read data from table my sql;
            int index = 0;
            while (reader.Read())
            {
                IDDevices[index] = reader.GetString("id");
                IPAddress[index] = reader.GetString("IPaddress");
                index = index + 1;
               
            }

            data2txt.Text = IDDevices[1];
            datatxt.Text = IPAddress[1];
         //   Resgister.Text= reader.GetString("IPaddress");
            reader.Close();
            reader.Dispose();

        }

        //read parameter
        private void ReadAllParameter()
        {
            int index = 0;

            //Delete all variable
            arrMBValue.Clear();

            //conection 
            for (int i = 0; i < 10; i++)
            {
                if(IDDevices[i] != null)
                {
                    string sql = "Select * from ins_modbustcp_parameter where device_id =" + IDDevices[i];

                    MySqlCommand cmd = connection.CreateCommand();
                    cmd.CommandText = sql;
                    MySqlDataReader reader = cmd.ExecuteReader();//read data from table my sql;

                    while (reader.Read())
                    {
                        BIndexDevice[index] = i;
                        BIDDevice[index] = IDDevices[i];
                        BIPadress[index] = IPAddress[i];
                        BResgisters[index] = reader.GetString("register");  //get register                    
                        BIDRegister[index] = reader.GetString("id");        //ID register
                        //BSlaveID[index] = reader.GetInt32("slaveid");        

                        int deviceid = Int32.Parse(IDDevices[i]);
                        int registerid = Int32.Parse(reader.GetString("id"));
                        byte slaveid = reader.GetByte("slaveid");
                        double scalevl = Math.Round(reader.GetFloat("scalevalue"), 4);
                        string today = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                        arrMBValue.Add(new MBValue(deviceid, IPAddress[i], registerid, reader.GetString("register"), i, slaveid, scalevl, today));
                        //arrOldData de check trung du lieu
                        //arrOldData.Add(new SendData(registerid, 0.0, 0.0,""));
                        index = index + 1;
                    
                    }
                  
                    reader.Close();
                    reader.Dispose();
                }              
                
            }
            
        }
        /// read data with parameter from db
        /// 
        private void readdatafromdevice()
        {
            //modbustcp = new ModbusClient(BIPadress[0], port);
            for (int i = 0; i < 20; i++)
            {
                if (BIDDevice[i] != null)
                {
                    switch (BResgisters[i].Substring(0, 1))
                    {
                        case "3":
                            {
                                //readregisterInput
                                break;
                            }
                        case "4":
                            {                              
                                
                                string Address = BResgisters[i].Substring(1, 4);
                                int IAddress = Int32.Parse(Address);
                                //int Bindex = Int32.Parse(BIDDevice[i]) - 1;
                                int[] data = modbustcp[BIndexDevice[i]].ReadHoldingRegisters(IAddress, 1);
                            
                                Bvalue[i] = data[0];
                              

                                break;
                            }
                    }
                }
            }
            
        }

        void readDataFromDeviceNew()
        {
            foreach (MBValue item in arrMBValue)
            {
                switch (item.Register.Substring(0, 1))
                {
                    case "0":
                        {
                            //readregisterInput
                            string Address = item.Register.Substring(1, 4);
                            int IAddress = Int32.Parse(Address);
                            modbustcp[item.ModbusIndex].UnitIdentifier = item.SlaveId;
                            bool[] BoolData = modbustcp[item.ModbusIndex].ReadCoils(IAddress, 1);
                            if (BoolData[0])
                            {
                                item.Value = 1;
                            }
                            else
                            {
                                item.Value = 0;
                            }

                            break;
                        }
                    case "1":
                        {
                            //read Digital Input
                            string Address = item.Register.Substring(1, 4);
                            int IAddress = Int32.Parse(Address);
                            modbustcp[item.ModbusIndex].UnitIdentifier = item.SlaveId;
                            bool[] BoolData = modbustcp[item.ModbusIndex].ReadDiscreteInputs(IAddress,1);
                            if (BoolData[0])
                            {
                                item.Value = 1;
                            }
                            else
                            {
                                item.Value = 0;
                            }
                            
                            break;
                        }
                    case "3":
                        {
                            //readregisterInput
                            string Address = item.Register.Substring(1, 4);
                            int IAddress = Int32.Parse(Address);
                            modbustcp[item.ModbusIndex].UnitIdentifier = item.SlaveId;
                            int[] data = modbustcp[item.ModbusIndex].ReadInputRegisters(IAddress, 1);
                            item.Value = data[0];
                            break;
                        }
                    case "4":
                        {

                            string Address = item.Register.Substring(1, 4);
                            int IAddress = Int32.Parse(Address) - 1;
                            //int Bindex = Int32.Parse(BIDDevice[i]) - 1;
                            try
                            {
                                modbustcp[item.ModbusIndex].UnitIdentifier = item.SlaveId;
                                modbustcp[item.ModbusIndex].ConnectionTimeout = 3000;
                                if (modbustcp[item.ModbusIndex].Connected)
                                {
                                    int[] data = modbustcp[item.ModbusIndex].ReadHoldingRegisters(IAddress, 1);
                                    Thread.Sleep(200);
                                    item.Value = data[0]* item.ScaleValue;
                                }
                            }
                            catch (IOException ioe)
                            {
                                errorCount++;
                                Thread.Sleep(10000);
                            }
                            
                            
                            break;
                        }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox2.Clear(); //Clear textbox
            ReadALLdevice();
            ReadAllParameter();

            readDataFromDeviceNew();

            timer1.Start();

            ThreadStart ts = new ThreadStart(SendDataToServer);
            Thread newThread = new Thread(ts);
            newThread.Start();

            ThreadStart ts2 = new ThreadStart(SendDataToSQLDatabase);
            Thread newThread2 = new Thread(ts2);
            newThread2.Start();
            

        }

        
        
private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
