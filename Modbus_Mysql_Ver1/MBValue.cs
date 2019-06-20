using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus_Mysql_Ver1
{
    public class MBValue
    {
        private int deviceId;
        private string ipadress;
        private int registerId;
        private string register;
        private int modbusIndex;
        private byte slaveId;
        private double scaleValue;
        private double value;
        private string timeStamp;
        public MBValue(int deviceid, string ipadress, int registerId, string register, int modbusIndex, byte slaveId, double scaleValue, string timeStamp)
        {
            this.DeviceId = deviceid;
            this.Ipadress = ipadress;
            this.RegisterId = registerId;
            this.Register = register;
            this.ModbusIndex = modbusIndex;
            this.SlaveId = slaveId;
            this.ScaleValue = scaleValue;
            this.TimeStamp = timeStamp;

        }

        public string Register
        {
            get
            {
                return register;
            }

            set
            {
                register = value;
            }
        }

        public int RegisterId
        {
            get
            {
                return registerId;
            }

            set
            {
                registerId = value;
            }
        }

        public int DeviceId
        {
            get
            {
                return deviceId;
            }

            set
            {
                deviceId = value;
            }
        }

        public string Ipadress
        {
            get
            {
                return ipadress;
            }

            set
            {
                ipadress = value;
            }
        }

        public int ModbusIndex
        {
            get
            {
                return modbusIndex;
            }

            set
            {
                modbusIndex = value;
            }
        }

        public double Value
        {
            get
            {
                return value;
            }

            set
            {
                this.value = value;
            }
        }

        public byte SlaveId
        {
            get
            {
                return slaveId;
            }

            set
            {
                slaveId = value;
            }
        }

        public double ScaleValue
        {
            get
            {
                return scaleValue;
            }

            set
            {
                scaleValue = value;
            }
        }

        public string TimeStamp
        {
            get
            {
                return timeStamp;
            }

            set
            {
                timeStamp = value;
            }
        }
    }
}
