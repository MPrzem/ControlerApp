using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace Iotpublisher
{
    public enum cmd
    {
        SendData = 49,
        GetControlVal = 50
    }
    public class HiLevComm
    {
        ManualResetEvent dataRecived = new ManualResetEvent(false);
        IControler controler;
        MqttClient client;//ref to inited client
        System.Timers.Timer oTimer;
        string ctrTopic = "ctrTopic";
        string dataTopic = "DataTopic";
        public readonly object SyncObject = new object();
        Sensors sensors;
        public void StartTimer(Action<object, ElapsedEventArgs> onTimeEvent, int interval_ms)
        {
            oTimer = new System.Timers.Timer();
            oTimer.Elapsed += new ElapsedEventHandler(onTimeEvent);
            oTimer.Interval = interval_ms;
            oTimer.Enabled = true;
        }
        public void StopTimer()
        {
            oTimer.Stop();
        }
        public SensData decodeData(string message)
        {
            var data = JsonSerializer.Deserialize<SensData>(message);
            return data;

        }
        private void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            sensors.MakeFromJson(Encoding.UTF8.GetString(e.Message));
            dataRecived.Set();
        }
        private byte[] MakeCtrlFrame(float output)
        {
            byte[] bytes = new byte[4];
            output = (float)Math.Round(output, 1);
            UInt16 output_dec = (UInt16)(output * 10);
            byte[] output_bytes = BitConverter.GetBytes(output_dec);
            bytes[0] = (byte)cmd.GetControlVal;
            bytes[1] = output_bytes[0];
            bytes[2] = output_bytes[1];
            bytes[3] = 0;
            return bytes;
        }
        private void OnTimeEvent(object oSource, ElapsedEventArgs oElapsedEventArgs)
        {
            byte[] cmd_ = new byte[4];
            cmd_[0] = (byte)cmd.SendData;
            cmd_[1] = 0;
            client.Publish(ctrTopic, cmd_);
            dataRecived.WaitOne();
            dataRecived.Reset();
            float output;
            lock (SyncObject)
            {
                controler.getSensData(sensors.data);
                output = controler.setOutput();
            }
            var bytes = MakeCtrlFrame(output);
            client.Publish(ctrTopic, bytes, 1, false);
        }

        public HiLevComm(MqttClient _client, IControler _controler,SensData sensData_)
        {
            sensors = new Sensors(sensData_);
            controler = _controler;
            client = _client;
            StartTimer(OnTimeEvent, 10000);
            client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;
            client.Subscribe(new string[] { dataTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
            OnTimeEvent(null, null);


        }


    }
}
