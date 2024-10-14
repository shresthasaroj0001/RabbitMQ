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
                Console.WriteLine("Initiating RabbitMQ");
                await _consumer.StartConsumingAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Handle the cancellation gracefully
                Console.WriteLine("Operation canceled, shutting down consumer service.");
            }
            catch (Exception ex)
            {
                // Log any unexpected errors during execution
                Console.WriteLine($"Unexpected error occurred: {ex.Message}");
            }
        }
    }
}
