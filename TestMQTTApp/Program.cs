using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Subscribing;
using MQTTnet.Formatter;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestMQTTApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread t = new Thread(ExecuteForLoop);          // Kick off a new thread
            t.Start();

            ConnectMqtt();

            t.Join();
        }

        public static void ExecuteForLoop()
        {
            int example = 0;
            for (; ; )
            {
                example++;
            }
        }

        public static async void ConnectMqtt()
        {
            try
            {
                var client = new MqttFactory().CreateMqttClient();

                var options = new MqttClientOptionsBuilder()
                    .WithTcpServer("brokerservis", 1883)
                    .WithCredentials("admin", "password")
                    .WithProtocolVersion(MqttProtocolVersion.V311)
                    .Build();

                var auth = await client.ConnectAsync(options);

                if (auth.ResultCode != MqttClientConnectResultCode.Success)
                {
                    throw new Exception(auth.ResultCode.ToString());
                }
                else
                {
                    var result = (await client.SubscribeAsync(new TopicFilterBuilder().WithTopic("topic/#").Build())).Items[0];

                    switch (result.ResultCode)
                    {
                        case MqttClientSubscribeResultCode.GrantedQoS0:
                        case MqttClientSubscribeResultCode.GrantedQoS1:
                        case MqttClientSubscribeResultCode.GrantedQoS2:
                            client.UseApplicationMessageReceivedHandler(me =>
                            {
                                var msg = me.ApplicationMessage;
                                var data = Encoding.UTF8.GetString(msg.Payload);
                                Console.WriteLine($"{msg.Topic} {data}\n");
                            });

                            break;
                        default:
                            throw new Exception(result.ResultCode.ToString());
                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
