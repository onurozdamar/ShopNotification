using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ShopNotification.Common;

Console.WriteLine("Hello, World!");

Rabbit rabbit = new Rabbit();

rabbit.Connect();

Console.ReadLine();

class Rabbit
{
    private IConnection connection;
    private readonly string connectionString = "amqp://admin:123456@localhost:5672";

    private readonly string createNotificationQ = "create_notification_queue";
    private readonly string notificationCreatedQ = "notification_created_queue";
    private readonly string notificationCreatedExchange = "notification_create_exchange";

    private IModel? _channel;
    private IModel channel => _channel ?? (_channel = GetChannel());

    public void Connect()
    {
        connection = GetConnection();

        channel.ExchangeDeclare(notificationCreatedExchange, "direct");

        channel.QueueDeclare(createNotificationQ, false, false, false);
        channel.QueueBind(createNotificationQ, notificationCreatedExchange, createNotificationQ);

        channel.QueueDeclare(notificationCreatedQ, false, false, false);
        channel.QueueBind(notificationCreatedQ, notificationCreatedExchange, notificationCreatedQ);

        var consumerEvent = new EventingBasicConsumer(channel);

        consumerEvent.Received += (chnl, eventArgs) =>
        {
            var modelJson = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
            var modelReceived = JsonConvert
                .DeserializeObject<CreateNotificationModel>(modelJson);

            Console.WriteLine("Received " + modelJson);

            // Create Notification
            Task.Delay(5000).Wait();

            modelReceived.Message = "Hello from other side";

            WriteToQueue(notificationCreatedQ, modelReceived);
        };

        channel.BasicConsume(createNotificationQ, true, consumerEvent);

        Console.WriteLine(notificationCreatedExchange + " listening");
    }

    private void WriteToQueue(string queName, CreateNotificationModel model)
    {
        var messageArr = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model));

        channel.BasicPublish(notificationCreatedExchange, queName, null, messageArr);
        Console.WriteLine("message published");
    }

    private IModel GetChannel()
    {
        return connection.CreateModel();
    }

    private IConnection GetConnection()
    {
        var connectionFactory = new ConnectionFactory()
        {
            Uri = new Uri(connectionString)
        };

        return connectionFactory.CreateConnection();
    }
}