using RabbitMQ.Client;

namespace Producer
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

}
