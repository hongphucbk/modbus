using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus_Mysql_Ver1
{
    public class SendData
    {
        private int id;
        private double value;
        private double scaleValue;
        private string timeStamp;

        public SendData(int id, double value, double scaleValue, string timeStamp)
        {
            this.Id = id;
            this.Value = value;
            this.ScaleValue = scaleValue;
            this.TimeStamp = timeStamp;
        }

        public int Id
        {
            get
            {
                return id;
            }

            set
            {
                id = value;
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
