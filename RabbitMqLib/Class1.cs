using RabbitMQ.Client;
using System.Text;

namespace RabbitMqLib;

public class RabbitMqPublisher
{
    private readonly string _hostname;
    private readonly string _queueName;
    private readonly string _username;
    private readonly string _password;

    public RabbitMqPublisher(string hostname, string queueName, string username = "guest", string password = "guest")
    {
        _hostname = hostname;
        _queueName = queueName;
        _username = username;
        _password = password;
    }

    public void Publish(string message)
    {
        var factory = new ConnectionFactory() { HostName = _hostname, UserName = _username, Password = _password };        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            var body = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchange: "", routingKey: _queueName, basicProperties: null, body: body);
        }
    }
}
