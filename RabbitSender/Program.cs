using RabbitMQ.Client;
using System.Text;

ConnectionFactory factory = new ConnectionFactory();

factory.Uri = new Uri("amqp://guest:guest@localhost:5672");
factory.ClientProvidedName = "Rabbit Sender Name";

IConnection conn =  await factory.CreateConnectionAsync();
IChannel channel = await conn.CreateChannelAsync();

string exchangeName = "DemoExchange";
string routingKey = "demo-routing-key";
string queueName = "DemoQueue";

await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Direct);
await channel.QueueDeclareAsync(queueName,false, false, false,null);
await channel.QueueBindAsync(queueName,exchangeName,routingKey);

for (int i = 0; i < 60; i++)
{
    Console.WriteLine($"Sending Message #{i}");
    byte[] Message = Encoding.UTF8.GetBytes($"Sending Message #{i}");
    await channel.BasicPublishAsync(exchangeName, routingKey, Message);
    Thread.Sleep(1000);

}
await channel.CloseAsync();
await conn.CloseAsync();
await channel.DisposeAsync();
await conn.DisposeAsync();