namespace Consumer
{
    public class RabbitMqConsumerService : BackgroundService
    {
        private readonly RabbitMqConsumer _consumer;
        private readonly CancellationTokenSource _cts;

        public RabbitMqConsumerService(RabbitMqConsumer consumer)
        {
            _consumer = consumer;
            _cts = new CancellationTokenSource();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                Console.WriteLine("Start Consuming");
                await _consumer.StartConsumingAsync(_cts.Token);
            }
            catch (OperationCanceledException)
            {
                // This exception is thrown when the service is stopped
            }
        }
    }
}
