using Consumer;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

string sHostName = "localhost";
string sUserName = "guest";
string sPassword = "guest";

string sQueueName = "feature_queue";
string sExchangeName = "feature_exchange";
string sRoutingKey = "feature_key";

builder.Services.AddSingleton<IConnectionFactory>(sp =>
{
    return new ConnectionFactory
    {
        HostName = sHostName,
        UserName = sUserName,
        Password = sPassword
    };
});
builder.Services.AddSingleton<RabbitMqConfig>(sp =>
{
    return new RabbitMqConfig
    {
        ExchangeName = sExchangeName,
        RoutingKey = sRoutingKey,
        QueueName = sQueueName
    };
});

builder.Services.AddSingleton<IRabbitMqConnectionService, RabbitMqConnectionService>();
builder.Services.AddSingleton<RabbitMqConsumer>();
builder.Services.AddHostedService<RabbitMqConsumerService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
