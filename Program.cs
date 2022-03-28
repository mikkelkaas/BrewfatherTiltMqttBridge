using System.Text.Json;
using System.Text.Json.Serialization;
using MQTTnet;
using MQTTnet.Client;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.MapPost("/", async (TiltInput input) =>
{
    Console.Write(input);

    var mqttServerAddress = Environment.GetEnvironmentVariable("MqttServerAddress");
    var mqttServerPort = Environment.GetEnvironmentVariable("MqttServerPort");
    var mqttServerUsername = Environment.GetEnvironmentVariable("MqttServerUser");
    var mqttServerPassword = Environment.GetEnvironmentVariable("MqttServerPassword");

    var options = new MqttClientOptionsBuilder()
        .WithTcpServer(mqttServerAddress, int.Parse(mqttServerPort))
        .WithCredentials(mqttServerUsername, mqttServerPassword)
        .Build();

    var factory = new MqttFactory();
    var mqttClient = factory.CreateMqttClient();
    var res = await mqttClient.ConnectAsync(options, CancellationToken.None);
    var message = new MqttApplicationMessageBuilder()
        .WithTopic("BrewTilt")
        .WithPayload(JsonSerializer.Serialize(input))
        .WithRetainFlag()
        .Build();
    await mqttClient.PublishAsync(message, CancellationToken.None);
    mqttClient.Dispose();
    return Results.Ok();
});

app.Run();

internal record TiltInput
{
    [JsonPropertyName("id")] public string Id { get; set; }

    [JsonPropertyName("type")] public string Type { get; set; }

    [JsonPropertyName("time")] public long Time { get; set; }

    [JsonPropertyName("timepoint")] public long Timepoint { get; set; }

    [JsonPropertyName("temp")] public decimal Temp { get; set; }

    [JsonPropertyName("sg")] public decimal Sg { get; set; }

    [JsonPropertyName("comment")] public string Comment { get; set; }
}