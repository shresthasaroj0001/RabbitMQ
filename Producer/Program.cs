using Producer;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

string sHostName = "localhost";
string sUserName = "guest";
string sPassword = "guest";

string sRoutingKey = "feature_key";
string sExchangeName = "feature_exchange";

builder.Services.AddSingleton<IConnectionFactory>(sp =>
{
    return new ConnectionFactory
    {
        HostName = sHostName,
        UserName = sUserName,
        Password = sPassword
    };
});
//to pass routing key and exchange name
builder.Services.AddSingleton<RabbitMqConfig>(sp =>
{
    return new RabbitMqConfig
    {
        RoutingKey = sRoutingKey,
        ExchangeName = sExchangeName
    };
});
builder.Services.AddSingleton<IRabbitMqConnectionService, RabbitMqConnectionService>();
builder.Services.AddScoped<RabbitMqProducer>();

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
