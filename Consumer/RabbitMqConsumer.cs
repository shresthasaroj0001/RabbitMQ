using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;

namespace Consumer
{
    public class RabbitMqConsumer
    {
        private readonly IModel _channel;
        private readonly string _queueName;

        public RabbitMqConsumer(IRabbitMqConnectionService connectionService, IConfiguration configuration)
        {
            _channel = connectionService.CreateChannel();
            _queueName = "local_bill_rate";
        }

        public async Task StartConsumingAsync(CancellationToken token)
        {
            Console.WriteLine("Inside StartConsuming Async");
            _channel.QueueDeclare(queue: _queueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: new Dictionary<string, object>
                                 {
                                 {"x-message-ttl", 5 * 1000},
                                 { "x-dead-letter-exchange", "" },
                                 { "x-dead-letter-routing-key", $"{_queueName}_DLX" }
                                 });

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += ConsumerReceived;
            _channel.BasicConsume(queue: _queueName, consumer: consumer);

            while (!token.IsCancellationRequested)
            {
                await Task.Delay(100); // Check for cancellation every 100ms
            }
        }

        private void ConsumerReceived(object sender, BasicDeliverEventArgs e)
        {
            var body = e.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            Console.WriteLine($"Received message: {message}");

            // Simulate processing time
            Thread.Sleep(2000);

            // Acknowledge the message
            _channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
        }
    }
}
