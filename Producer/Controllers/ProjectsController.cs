using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Threading.Channels;

namespace Producer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly RabbitMqProducer _producer;
        public ProjectsController(RabbitMqProducer rabbitMqProducer)
        {
            _producer = rabbitMqProducer;
        }

        [HttpPost("add-project")]
        public IActionResult AddProject([FromBody] string projectId)
        {
            var messageBody = Encoding.UTF8.GetBytes(projectId);

            _producer.PublishMessage("local_bill_rate", projectId);

            Console.WriteLine($"Project '{projectId}' added to the queue.");

            return Ok($"Project '{projectId}' added to the queue.");
        }
    }
}
