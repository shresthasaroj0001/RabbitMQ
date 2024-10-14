using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using System.Diagnostics;
using System.Reflection;

namespace Consumer
{
    public class RabbitMqConsumer
    {
        public readonly IModel _channel;
        private readonly string _queueName;
        private readonly string _exchangeName;
        private readonly string _routingKey;
        public string _consumerTag;
        private readonly TimeSpan stopTimeSpan;

        private bool _pauseProcessing = false; //safeguard for shutting down queue
        private bool _processingMessage = false; //safeguard for shutting down queue

        private bool _isConsumerActive = true; //safe guard for resume/pause queue
        private bool _IsPauseQueueProcessing = false;
        private bool _isResumeQueueProcessing = false;

        private DateTime? _stop_date;
        private DateTime? _resume_date;
        private System.Timers.Timer _timer;

        public RabbitMqConsumer(IRabbitMqConnectionService connectionService, RabbitMqConfig mqConfig)
        {
            _channel = connectionService.CreateChannel();
            _queueName = mqConfig.QueueName;
            _exchangeName = mqConfig.ExchangeName;
            _routingKey = mqConfig.RoutingKey;

            DateTime dt = DateTime.Now.AddMinutes(1);
            stopTimeSpan = new TimeSpan(dt.Hour, dt.Minute, 0);
            Console.WriteLine($"Time now: {DateTime.Now}");
        }

        public bool IsInitialized() => _channel != null && _consumerTag != null;

        public async Task StartConsumingAsync(CancellationToken token)
        {
            Console.WriteLine("Queue Declaration");
            // Main Queue Configuration
            _channel.ExchangeDeclare(_exchangeName, ExchangeType.Direct);
            _channel.QueueDeclare(_queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object>
                {
                    {"x-dead-letter-exchange", $"{_exchangeName}_DLX" },
                    {"x-dead-letter-routing-key", $"{_routingKey}_DLX" }
                });
            _channel.QueueBind(_queueName, _exchangeName, _routingKey);

            // Dead-Letter Queue Configuration
            _channel.ExchangeDeclare($"{_exchangeName}_DLX", ExchangeType.Direct);
            _channel.QueueDeclare($"{_queueName}_DLX",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object>
                {
                    {"x-dead-letter-exchange", _exchangeName },
                    {"x-dead-letter-routing-key", _routingKey },
                    {"x-message-ttl", 11000 } // 10 second
                });
            _channel.QueueBind($"{_queueName}_DLX", $"{_exchangeName}_DLX", $"{_routingKey}_DLX");
            _channel.BasicQos(0, 1, false);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += ConsumerReceived;

            Console.WriteLine("Starting Event Consumption");
            _consumerTag = _channel.BasicConsume(_queueName, false, consumer); // Save the consumer tag to manage pause/resume

            try
            {
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(1000); // Periodically check if shutdown is requested
                }
                _pauseProcessing = true;
                while (_processingMessage)
                {
                    Console.WriteLine("StartConsumingAsync - Waiting for task to be finished");
                    await Task.Delay(1000); // Periodically check if shutdown is requested
                }
            }
            catch (OperationCanceledException)
            {
                // Shutdown has been requested, stop consuming messages
                Console.WriteLine("Consumer stopped due to shutdown request.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unknown error: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("Initiating graceful shutdown...");
                
                // Wait for current message to be processed
                Console.WriteLine($"Is there current task processing ? {_processingMessage}");

                while (_processingMessage) // safety check to see if any message is in processing
                {
                    Console.WriteLine("StartConsumingAsync - finally - Waiting for task to be finished");
                    await Task.Delay(1000); // Wait for processing to finish
                }

                Console.WriteLine("Graceful shutdown completed.");
            }

        }

        private void ConsumerReceived(object sender, BasicDeliverEventArgs e)
        {
            if (_pauseProcessing)
            {
                Console.WriteLine("Queue consumption override. Paused Processing message.");
                _channel.BasicReject(e.DeliveryTag, false);
                return;
            }

            _processingMessage = true;

            var body = e.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($"##### Received message: {message} #####");

            // Simulating business logic check (e.g., item readiness)
            bool isReady = true;
            {
                // Get current time
                DateTime now = DateTime.Now;

                // Extract time component, removing seconds
                TimeSpan currentTime = new TimeSpan(now.Hour, now.Minute, 0);

                // Compare time components, ignoring seconds
                isReady = currentTime > stopTimeSpan;
                Console.WriteLine($"Current time: {now} ----- {currentTime.ToString()} > StopTime Time {stopTimeSpan.ToString()}");
            }

            Console.WriteLine("Checking if it can be use...");
            Thread.Sleep(10000); // Simulate processing delay

            if (!isReady)
            {
                Console.WriteLine("Message is not ready for processing, sending back to queue");
                // Reject message without requeueing (send to Dead Letter Exchange for retry)
                _channel.BasicReject(deliveryTag: e.DeliveryTag, false);
            }
            else
            {
                // Simulate processing
                Thread.Sleep(5000); // Simulate processing delay

                // Acknowledge the message once processed
                _channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);

                Console.WriteLine("Message Process Complete. Removed from the queue...");
            }
            _processingMessage = false; // Mark processing as finished
        }

        // Pause the queue processing
        public async Task PauseQueue()
        {
            if (_IsPauseQueueProcessing)
                return;
            _IsPauseQueueProcessing = true;
            
            Console.WriteLine($"Queue pause requested.");
            if (_channel == null)
            {
                throw new InvalidOperationException("Channel is not initialized. Call StartConsumingAsync first.");
            }

            if (_consumerTag == null)
            {
                throw new InvalidOperationException("Consumer tag is not set. StartConsumingAsync must be called first.");
            }

            if (_consumerTag != null && _isConsumerActive)
            {
                _pauseProcessing = true;
                if (_processingMessage)
                {
                    while (_processingMessage) // safety check to see if any message is in processing
                    {
                        Console.WriteLine("PauseQueue - Waiting for task to be finished");
                        await Task.Delay(1000); // Wait for processing to finish
                    }
                    Console.WriteLine("PauseQueue - Task Completed");
                }

                var sto = new Stopwatch();
                sto.Start();
                _isConsumerActive = false;
                _channel.BasicCancel(_consumerTag);

                sto.Stop();
                _pauseProcessing = false;
                Console.WriteLine($"Queue paused action completed. Took {sto.Elapsed.TotalSeconds:F2}");
            }
            else
            {
                if(!_isConsumerActive)
                    Console.WriteLine($"Queue already paused");

                if (_consumerTag == null)
                    Console.WriteLine($"Queue not ready yet");
            }
            _IsPauseQueueProcessing = false;
        }

        // Resume the queue processing
        public void ResumeQueue()
        {
            if(_isResumeQueueProcessing) return;
            _isResumeQueueProcessing = true;
            if (!_isConsumerActive)
            {
                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += ConsumerReceived;

                _consumerTag = _channel.BasicConsume(_queueName, false, consumer);
                _isConsumerActive = true;
                Console.WriteLine("Queue resumed.");
            }
            else
            {
                Console.WriteLine("Consumer is already active.");
            }
            _isResumeQueueProcessing = false;
        }

        public void SetSchedule(DateTime? startDate, DateTime? endDate)
        {
            _stop_date = startDate.HasValue ? new DateTime(startDate.Value.Year, startDate.Value.Month, startDate.Value.Day, startDate.Value.Hour, startDate.Value.Minute, 0) : null;
            _resume_date = endDate.HasValue ? new DateTime(endDate.Value.Year, endDate.Value.Month, endDate.Value.Day, endDate.Value.Hour, endDate.Value.Minute, 0) : null;

            if (_timer == null)
            {
                _timer = new System.Timers.Timer(5000);
                _timer.Elapsed += ValidateSchedule;
                _timer.Start();
            } else if (!_timer.Enabled)
            {
                _timer.Start();
                Console.WriteLine($"Timer Start");
            }
            else
            {
                Console.WriteLine($"Timer already enabled");
            }
        }

        private void ValidateSchedule(object? sender, System.Timers.ElapsedEventArgs e)
        {
            Console.WriteLine($"{MethodBase.GetCurrentMethod()?.Name} - Schedule checked @ {DateTime.Now} -- Stop {(_stop_date.HasValue?_stop_date.Value.ToShortTimeString():"") } - Resume {(_resume_date.HasValue?_resume_date.Value.ToShortTimeString():"")}");
            if (_stop_date.HasValue && _stop_date.Value <= DateTime.Now)
            {
                PauseQueue().GetAwaiter().GetResult();
                _stop_date = null;
                Console.WriteLine($"{MethodBase.GetCurrentMethod()?.Name} - Called pause queue");
                Console.WriteLine($"------------");
            }
            else if (_resume_date.HasValue && _resume_date.Value <= DateTime.Now)
            {
                ResumeQueue();
                _resume_date = null;
                Console.WriteLine($"{MethodBase.GetCurrentMethod()?.Name} - Called resume queue");
                Console.WriteLine($"------------");
            }

            if (!_stop_date.HasValue && !_resume_date.HasValue)
            {
                Console.WriteLine($"{MethodBase.GetCurrentMethod()?.Name} - Stopping timer - dates are null");
                _timer?.Stop();
                if (!_isConsumerActive)
                {
                    ResumeQueue();
                }
                Console.WriteLine($"------------");
            }
        }

        public (DateTime?, DateTime?) GetSchedule()
        {
            return (_stop_date, _resume_date);
        }

        public bool GetConsumerStatus()
        {
            return _isConsumerActive;
        }
    }
}
