using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Services.Paint
{
    public class PaintClient : IPaintClient
    {
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly string queueName;
        private readonly IReciver reciver;
        private readonly IBasicProperties props;

        public PaintClient(string queueName, IReciver reciver)
        {
            this.reciver = reciver;
            this.queueName = queueName;
            var host = System.Environment.GetEnvironmentVariable("RABBITMQ_HOST");
            var factory = new ConnectionFactory() { HostName = host };
            connection = factory.CreateConnection();

            CheckIfQueueExistis(queueName);

            channel = connection.CreateModel();
            var replyQueueName = channel.QueueDeclare("", false, true, true, null).QueueName;
            channel.BasicQos(0, 1, false);

            var consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume(replyQueueName, false, consumer);

            props = channel.CreateBasicProperties();
            props.ReplyTo = replyQueueName;
            props.CorrelationId = Guid.NewGuid().ToString();

            consumer.Received += OnRecive;
        }

        private void CheckIfQueueExistis(string queueName)
        {
            using (var model = connection.CreateModel())
            {
                try
                {
                    model.QueueDeclarePassive(queueName);
                }
                catch (OperationInterruptedException e)
                {
                    if (e.ShutdownReason.ReplyCode == 404)
                    {
                        Dispose();
                        throw new Exception("Servidor não encontrado");
                    }
                }
            }
        }
        private void OnRecive(object model, BasicDeliverEventArgs e)
        {
            var body = Encoding.UTF8.GetString(e.Body.ToArray());
            channel.BasicAck(e.DeliveryTag, false);
            reciver.Recive(body);
        }
        public void Send(string msg)
        {
            var message = Encoding.UTF8.GetBytes(msg);
            //props.CorrelationId = Guid.NewGuid().ToString();
            channel.BasicPublish("", queueName, props, message);
        }
        public void Dispose()
        {
            connection.Close();
        }
    }

}