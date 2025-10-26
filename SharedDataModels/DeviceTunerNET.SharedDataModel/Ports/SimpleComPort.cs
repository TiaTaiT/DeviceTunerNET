using DeviceTunerNET.SharedDataModel.Devices;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;

namespace DeviceTunerNET.SharedDataModel.Ports
{
    public class SimpleComPort : IPort
    {
        private static List<byte> _readBuffer = [];// Буфер чтения для RS485
        protected byte[] _sendData; // Данные для отправки в порт

        public SerialPort SerialPort { get; set; }
        public int MaxRepetitions { get; set; } = 15;
        public int Timeout { get; set; } = 1;

        public byte[] Send(byte[] command)
        {
            if (SerialPort == null)
                return null;

            if (SerialPort.IsOpen)
            {
                SendData(command);
            }
            else
            {
                try
                {
                    SerialPort.Open();
                    SendData(command);
                }
                catch
                {

                }
                finally
                {
                    SerialPort.DataReceived -= Sp_DataReceived;
                    SerialPort.Close();
                }
            }

            return _readBuffer.ToArray();

        }

        public void SendWithoutСonfirmation(byte[] data)
        {
            if (!SerialPort.IsOpen)
            {
                return;
            }

            SerialPort.Write(data, 0, data.Length);
        }

        private void SendData(byte[] command)
        {
            // make DataReceived event handler
            SerialPort.DataReceived += Sp_DataReceived;

            for (int i = 0; i < MaxRepetitions; i++)
            {
                _readBuffer.Clear();
                SendWithoutСonfirmation(command);

                var timeCounter = (int)IOrionNetTimeouts.Timeouts.notResponse;
                while ((timeCounter >= 0))
                {
                    Thread.Sleep(Timeout);
                    timeCounter -= Timeout;
                }

                if (IsReceivePacketComplete())
                    break;
            }
        }

        protected bool IsReceivePacketComplete()
        {
            if (_readBuffer.Count() < 1)
                return false;

            return true;
        }

        private static void Sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var sPort = (SerialPort)sender;

            var tempBuffer = new byte[sPort.BytesToRead];
            sPort.Read(tempBuffer, 0, tempBuffer.Length);

            foreach (byte b in tempBuffer)
            {
                _readBuffer.Add(b);
            }
        }

        public void Dispose()
        {
            SerialPort.Dispose();
        }
    }
}
