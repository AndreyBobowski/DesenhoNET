using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using NetCore.Teste;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Services.Paint
{
    public class PaintServer : IPaintServer
    {
        private readonly IConnection connection;
        private readonly IModel channel;
        private Dictionary<string, string> clients;
        private readonly string queueName;
        private readonly IReciver reciver;
        private readonly IBasicProperties replyProps;
        private List<MessageAction> DrawCache;

        public PaintServer(IReciver reciver)
        {
            DrawCache = new List<MessageAction>();
            clients = new Dictionary<string, string>();
            this.reciver = reciver;
            queueName = Guid.NewGuid().ToString();

            var host = System.Environment.GetEnvironmentVariable("RABBITMQ_HOST");
            var factory = new ConnectionFactory() { HostName = host };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();

            channel.QueueDeclare(queueName, false, true, true, null);
            channel.BasicQos(0, 1, false);

            var consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume(queueName, false, consumer);
            replyProps = channel.CreateBasicProperties();
            consumer.Received += OnRecive;
        }
        public string GetAddress()
        {
            return queueName;
        }
        private void OnRecive(object model, BasicDeliverEventArgs e)
        {
            var body = Encoding.UTF8.GetString(e.Body.ToArray());
            var props = e.BasicProperties;
            var messageAction = JsonSerializer.Deserialize<MessageAction>(body);
            if (messageAction.action == "connect")
            {
                clients.Add(props.ReplyTo, props.CorrelationId);
                SendCacheTo(props.ReplyTo);
            }
            else if (messageAction.action == "disconnect")
            {
                clients.Remove(props.ReplyTo);
            }
            else
            {
                SaveIfDraw(body);
                reciver.Recive(body);
                SendAllExceptOne(body, props.CorrelationId);
            }
            channel.BasicAck(e.DeliveryTag, false);
        }
        private void SaveIfDraw(string body)
        {
            var messageAction = JsonSerializer.Deserialize<MessageAction>(body);
            if (messageAction.action == "desenhar")
                DrawCache.Add(messageAction);
        }
        private void SendCacheTo(string address)
        {
            var messageAction = JsonSerializer.Serialize(DrawCache);
            var cacheMessage = new MessageAction()
            {
                action = "drawcache",
                data = messageAction
            };
            var cacheMessageSerialize = JsonSerializer.Serialize(cacheMessage);
            RabbitSend(address, cacheMessageSerialize);
        }
        public void Send(string msg)
        {
            SaveIfDraw(msg);
            foreach (var client in clients)
                RabbitSend(client.Key, msg);
        }
        private void SendAllExceptOne(string msg, string idExcept)
        {
            foreach (var client in clients)
                if (client.Value != idExcept)
                    RabbitSend(client.Key, msg);
        }
        private void RabbitSend(string address, string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish("", address, replyProps, messageBytes);
        }
        public void Dispose()
        {
            connection.Close();
        }
    }

}