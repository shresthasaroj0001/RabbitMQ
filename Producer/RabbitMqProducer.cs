using RabbitMQ.Client;
using System.Text;

namespace Producer
{
    public class RabbitMqProducer
    {
        private readonly IModel _channel;
        private readonly string exchangeName;
        private readonly string routingkey;

        public RabbitMqProducer(IRabbitMqConnectionService connectionService, RabbitMqConfig config)
        {
            _channel = connectionService.CreateChannel();
            exchangeName = config.ExchangeName;
            routingkey = config.RoutingKey;
        }

        public void PublishMessage(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange: exchangeName,
                                  routingKey: routingkey,
                                  body: body);
        }
    }
}
