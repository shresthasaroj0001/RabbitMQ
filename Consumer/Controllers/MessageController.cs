using Microsoft.AspNetCore.Mvc;

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

        [HttpGet("consume")]
        public IActionResult StartConsuming()
        {
            // Start consuming immediately
            //_consumer.StartConsuming();
            return Ok("Started consuming messages");
        }
    }
}
