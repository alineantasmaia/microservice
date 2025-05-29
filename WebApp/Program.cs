using Grpc.Net.Client;
using RabbitMqLib;
using static GrpcService.Greeter;
using GrpcService;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/grpc-example/{name}", async (string name, IConfiguration config) =>
{
    var grpcUrl = config["GrpcServiceUrl"] ?? "https://localhost:5001";
    using var channel = GrpcChannel.ForAddress(grpcUrl);
    var client = new GreeterClient(channel);
    var reply = await client.SayHelloAsync(new HelloRequest { Name = name });
    return Results.Ok(reply.Message);
});

app.MapGet("/", () => "Hello World!");

app.MapPost("/rabbitmq/publish", (string message) =>
{
    var publisher = new RabbitMqPublisher("localhost", "webapp-queue");
    publisher.Publish(message);
    return Results.Ok($"Mensagem publicada: {message}");
});

app.MapGet("/rabbitmq/consume", async () =>
{
    var messages = new List<string>();
    var consumer = new RabbitMqConsumer("localhost", "webapp-queue");
    var tcs = new TaskCompletionSource<List<string>>();
    int count = 0;
    consumer.StartConsuming(msg => {
        messages.Add(msg);
        count++;
        if (count >= 5) // Consome até 5 mensagens para exemplo
            tcs.TrySetResult(messages);
    });
    // Aguarda até 5 mensagens ou timeout de 3 segundos
    var completed = await Task.WhenAny(tcs.Task, Task.Delay(3000));
    return Results.Ok(messages);
});

// Exemplo extra: publicar e consumir em sequência
app.MapPost("/rabbitmq/publish-and-consume", async (string message) =>
{
    var publisher = new RabbitMqPublisher("localhost", "webapp-queue");
    publisher.Publish(message);
    var messages = new List<string>();
    var consumer = new RabbitMqConsumer("localhost", "webapp-queue");
    var tcs = new TaskCompletionSource<List<string>>();
    consumer.StartConsuming(msg => {
        messages.Add(msg);
        tcs.TrySetResult(messages);
    });
    var completed = await Task.WhenAny(tcs.Task, Task.Delay(2000));
    return Results.Ok(messages);
});

app.Run();
