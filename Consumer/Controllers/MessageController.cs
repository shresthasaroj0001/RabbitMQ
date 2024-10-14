using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace Consumer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly RabbitMqConsumer _consumer;

        public MessageController(RabbitMqConsumer consumer)
        {
            _consumer = consumer;
        }

        [HttpGet("pause")]
        public IActionResult PauseConsuming()
        {
            if (!_consumer.IsInitialized())
            {
                return BadRequest("Consumer not initialized. Start consuming first.");
            }

            try
            {
                Task.Run(() =>
                      _consumer.PauseQueue()
                );
                return Ok("Queue pause requested");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error pausing queue. Error message:{ex.Message}");
                return StatusCode(500, $"Queue pause requested");
            }
        }

        [HttpGet("resume")]
        public IActionResult ResumeConsuming()
        {
            _consumer.ResumeQueue();
            return Ok("Resumed Queue");
        }

        [HttpGet("setschedule")]
        public IActionResult SetSchedule(DateTime? start_date, DateTime? end_date)
        {
            if (start_date.HasValue && end_date.HasValue)
            {
                if (end_date.Value < start_date.Value || start_date.Value < DateTime.Now)
                {
                    return BadRequest("Invalid schedules sent");
                }
            }
            else if (start_date.HasValue && !end_date.HasValue)
            {
                return BadRequest("Invalid schedules sent; Missing End-date");
            }
            else if (!start_date.HasValue && end_date.HasValue)
            {
                if (DateTime.Now > end_date.Value)
                {
                    return BadRequest("Invalid schedules sent; End-date not greater than current date");
                }
            }
            _consumer.SetSchedule(start_date, end_date);
            Console.WriteLine($"{MethodBase.GetCurrentMethod()?.Name} - Done");
            return Ok("Scheduled");
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            (var start, var end) = _consumer.GetSchedule();
            var responseObj = new
            {
                IsQueueActive = _consumer.GetConsumerStatus(),
                Schedule = new
                {
                    stop_datetime = start.HasValue ? start.Value.ToString("yyyy-MM-dd HH:mm") : "",
                    resume_datetime = end.HasValue ? end.Value.ToString("yyyy-MM-dd HH:mm") : ""
                }
            };
            return Ok(responseObj);
        }
    }
}
