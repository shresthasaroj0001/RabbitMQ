using Castle.Core.Configuration;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Consumer.Tests
{
    public static class MockDependencies
    {
        public static IRabbitMqConnectionService CreateMockConnectionService()
        {
            var mockConnectionService = new Mock<IRabbitMqConnectionService>();
            var model = new Mock<IModel>();

            mockConnectionService.Setup(x => x.CreateChannel()).Returns(model.Object);

            return mockConnectionService.Object;
        }

        //public static IConfiguration CreateMockConfig()
        //{
        //    var config = new Mock<IConfiguration>();

        //    // You can add default values here if needed
        //    config.Setup(x => x["RabbitMq:HostName"]).Returns("localhost");
        //    config.Setup(x => x["RabbitMq:UserName"]).Returns("guest");
        //    config.Setup(x => x["RabbitMq:Password"]).Returns("guest");

        //    return config.Object;
        //}

        public static CancellationToken CreateMockCancellationToken()
        {
            return new CancellationToken();
        }

        public static List<EventingBasicConsumer> CreateMockEventingBasicConsumers(int count)
        {
            return Enumerable.Range(0, count).Select(i =>
                new Mock<EventingBasicConsumer>().Object).ToList();
        }
    }
}
