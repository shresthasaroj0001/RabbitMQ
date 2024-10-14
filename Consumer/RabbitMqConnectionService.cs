using RabbitMQ.Client;

namespace Consumer
{
    public interface IRabbitMqConnectionService
    {
        IModel CreateChannel();
    }

    public class RabbitMqConnectionService : IRabbitMqConnectionService
    {
        private readonly IConnectionFactory _connectionFactory;

        public RabbitMqConnectionService(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public IModel CreateChannel()
        {
            return _connectionFactory.CreateConnection().CreateModel();
        }
    }

    public class RabbitMqConfig
    {
        public required string RoutingKey { get; set; }
        public required string ExchangeName { get; set; }
        public required string QueueName { get; set; }
    }
}
