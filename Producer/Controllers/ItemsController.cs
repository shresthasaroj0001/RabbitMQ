using Microsoft.AspNetCore.Mvc;

namespace Producer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly RabbitMqProducer _producer;
        public ItemsController(RabbitMqProducer rabbitMqProducer)
        {
            _producer = rabbitMqProducer;
        }

        [HttpPost("add-item")]
        public IActionResult AddItem([FromBody] string itemId)
        {
            string itemIds = $"itemId-{DateTime.Now.Hour}-{DateTime.Now.Minute}-{DateTime.Now.Second}";

            _producer.PublishMessage(itemIds);

            Console.WriteLine($"Item '{itemIds}' added to the queue.");

            return Ok($"Item '{itemIds}' added to the queue.");
        }
    }
}
