using RabbitMQ.Client;
using System.Text;

namespace Producer
{
    public class RabbitMqProducer
    {
        private readonly IModel _channel;

        public RabbitMqProducer(IRabbitMqConnectionService connectionService)
        {
            _channel = connectionService.CreateChannel();
        }

        public void PublishMessage(string queueName, string message)
        {
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange: "",
                                  routingKey: queueName,
                                  body: body);
        }
    }
}
