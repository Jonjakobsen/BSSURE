using Bssure.CortriumDevice;
using Bssure.DTO;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace Bssure.Services
{

    public interface IRawDataService
    {
        void PublishRawData(EKGSampleDTO ekgSample);
    }

    public class RawDataService : IRawDataService
    {

        private readonly IMQTTService mqttService;
        public RawDataService(IMQTTService MQTTManager)
        {
            mqttService = MQTTManager;
        }
        public void PublishRawData(EKGSampleDTO ekgSample)
        {
            mqttService.Publish_RawData(ekgSample);
        }

    }

    public interface IMQTTService
    {
        void OpenConncetion();
        void CloseConncetion();
        void Publish_RawData(EKGSampleDTO data);

        void PublishMetaData(UserDataDTO data);

        void StartSending(string userId);
        void StopSending();

    }

    class MqttService : IMQTTService
    {

        private readonly MqttClient client;
        private readonly string clientId;
        public MqttClient Client => client;

        private bool started;

        public bool Started
        {
            get { return started; }
            set { started = value; }
        }


        public MqttService()
        {
            Started = false;
            client = new MqttClient("172.20.10.3");
            clientId = Guid.NewGuid().ToString();
            OpenConncetion();
            //Minor change
        }


        public void Publish(string topic, byte[] data)
        {
            if (Client.IsConnected)
            {
                client.Publish(topic, data, MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, false);
            }
        }

        //This code runs when a message is received
        void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            string topic = e.Topic;

            //Switch case to handle messages different topics
            switch (topic)
            {
                case Topics.Topic_Status_CSSURE:
                    //TODO Handle status from CSSURE
                    break;
                default:
                    Debug.WriteLine("Received message from unhandled topic: " + e.Topic + " Message: " + e.Message);
                    break;
            }
        }

        //This code runs when the client has subscribed to a topic
        static void client_MqttMsqSubsribed(object senser, MqttMsgSubscribedEventArgs e)
        {
            Debug.WriteLine("Subscribed to topic: " + e.MessageId);
        }

        public void OpenConncetion()
        {
            try
            {

                if (!Client.IsConnected)
                {
                    client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

                    client.MqttMsgSubscribed += client_MqttMsqSubsribed;

                    //client.Connect(clientId);
                    client.Connect(
                        clientId: clientId,
                        username: "",
                        password: "",
                        cleanSession: false,
                        keepAlivePeriod: 60,
                        willFlag: true,
                        willTopic: Topics.Topic_Status_BSSURE,
                        willMessage: "Offine",
                        willRetain: true,
                        willQosLevel: MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE
                        );

                    client.Subscribe(new string[] { Topics.Topic_Series_TempToBSSURE }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
                    Publish(Topics.Topic_Status_BSSURE, Encoding.UTF8.GetBytes("Online"));
                }
            }
            catch (Exception)
            {

            }
        }

        public void CloseConncetion()
        {
            if (Client.IsConnected)
            {
                Client.Disconnect();
            }
        }

        public void Publish_RawData(EKGSampleDTO data)
        {
            if (Started)
            {
                data.PatientId = UserId;
                var serialData = JsonSerializer.Serialize<EKGSampleDTO>(data);
                client.Publish(Topics.Topic_Series_FromBSSURE, Encoding.UTF8.GetBytes(serialData));
            }
        }

        public void PublishMetaData(UserDataDTO data)
        {
            if (Client.IsConnected)
            {
                var serialData = JsonSerializer.Serialize<UserDataDTO>(data);
                client.Publish(Topics.Topic_UserData+"/"+data.UserId, Encoding.UTF8.GetBytes(serialData), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, false);
            }
        }

        private string UserId = "Unknown";
        public void StartSending(string userId)
        {
            UserId = userId;
            Started = true;
        }

        public void StopSending()
        {
            Started = false;
        }

        
    }


    public static class Topics
    {
        public const string

            Topic_Status_CSSURE = "ECG/Status/CSSURE",
            Topic_Status_BSSURE = "ECG/Status/BSSURE",


            Topic_UserData = "ECG/Userdata",
            Topic_Series_FromBSSURE = "ECG/Series/BSSURE2CSSURE",
            Topic_Series_TempToBSSURE = "ECG/Temp/ToBSSURE";
    }
}
