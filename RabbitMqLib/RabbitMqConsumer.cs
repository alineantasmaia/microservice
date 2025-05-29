using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace RabbitMqLib;

public class RabbitMqConsumer
{
    private readonly string _hostname;
    private readonly string _queueName;
    private readonly string _username;
    private readonly string _password;

    public RabbitMqConsumer(string hostname, string queueName, string username = "guest", string password = "guest")
    {
        _hostname = hostname;
        _queueName = queueName;
        _username = username;
        _password = password;
    }

    public void StartConsuming(Action<string> onMessageReceived)
    {
        var factory = new ConnectionFactory() { HostName = _hostname, UserName = _username, Password = _password };
        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();
        channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            onMessageReceived(message);
        };
        channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
    }
}
