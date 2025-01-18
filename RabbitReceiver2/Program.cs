using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Channels;

ConnectionFactory factory = new ConnectionFactory();

factory.Uri = new Uri("amqp://guest:guest@localhost:5672");
factory.ClientProvidedName = "Rabbit Receiver1 Name";

IConnection conn = await factory.CreateConnectionAsync();
IChannel channel = await conn.CreateChannelAsync();

string exchangeName = "DemoExchange";
string routingKey = "demo-routing-key";
string queueName = "DemoQueue";

await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Direct);
await channel.QueueDeclareAsync(queueName, false, false, false, null);
await channel.QueueBindAsync(queueName, exchangeName, routingKey);


await channel.BasicQosAsync(0, 1, false);

var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += async (s, e) =>
{
    await Task.Delay(TimeSpan.FromSeconds(5));

    var body = e.Body.ToArray();
    string message = Encoding.UTF8.GetString(body);
    Console.WriteLine($"Message Received : {message}");
    await channel.BasicAckAsync(e.DeliveryTag, false);
};

string consumerTag = await channel.BasicConsumeAsync(queueName, false, consumer);
Console.ReadLine();

await channel.BasicCancelAsync(consumerTag);

await channel.CloseAsync();
await conn.CloseAsync();
await channel.DisposeAsync();
await conn.DisposeAsync();