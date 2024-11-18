using ClientDotNet.Configurations;
using MQTTnet.Client;
using MQTTnet;
using ClientDotNet.MQTT;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<MqttBrokerSettings>(builder.Configuration.GetSection("MqttBrokerSettings"));
builder.Services.Configure<MqttClientSettings>(builder.Configuration.GetSection("MqttClientSettings"));

builder.Services.AddSingleton<IMqttClient>(provider => new MqttFactory().CreateMqttClient());

builder.Services.AddSingleton<MqttPublisher>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
