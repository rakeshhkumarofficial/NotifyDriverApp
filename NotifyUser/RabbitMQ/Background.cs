using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using Microsoft.AspNetCore.SignalR;
using NotifyUser.Hubs;
using System.Security.Claims;
using Microsoft.AspNet.SignalR.Client.Http;

namespace NotifyUser.RabbitMQ
{
    public class Background : IHostedService
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        //private readonly IServiceProvider _serviceProvider;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<NotifyHub> _hubContext;

        public Background(IServiceScopeFactory scopeFactory, IHubContext<NotifyHub> hubContext)
        {
            //_serviceProvider = serviceProvider;
            _scopeFactory = scopeFactory;
            _hubContext = hubContext;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(() => Consumer("notifyDriver"));

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();
            await Task.CompletedTask;
        }
        public void Consumer(string queueName)
        {
            var scope = _scopeFactory.CreateScope();
            // Rabbit MQ Server
            var factory = new ConnectionFactory
            {
                HostName = "192.180.3.63",
                Port = Protocols.DefaultProtocol.DefaultPort,
                UserName = "s3",
                Password = "guest",
                VirtualHost = "/",
                ContinuationTimeout = new TimeSpan(10, 0, 0, 0)
                /*Uri
                = new Uri("amqp://s2:guest@192.180.3.63:5672")*/
            };
            //RabbitMQ connection using connection factory
            var connection = factory.CreateConnection();
            //channel with session and model
            using
            var channel = connection.CreateModel();
            //declare the queue 
            channel.QueueDeclare("notifyDriver",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
            //Set Event object which listen message from chanel which is sent by producer
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, eventArgs) =>
            {
                var body = eventArgs.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"message received: {message}");
                NotifyDriver notify = System.Text.Json.JsonSerializer.Deserialize<NotifyDriver>(message)!;
                
                Guid Id = new Guid("FEAD53CD-A365-4A70-A84D-E0FC71D05FDD");
                if (notify.driverId == Id)
                {
                  
                    channel.BasicAck(deliveryTag: eventArgs.DeliveryTag, multiple: false);
                    _hubContext.Clients.Clients(ConnectionUser.conn).SendAsync("Shipments", notify.shipmentId);
                   
                }
                else
                {
                    channel.BasicNack(deliveryTag: eventArgs.DeliveryTag, multiple: false, requeue: true);
                }
            };
            //read the message
            channel.BasicConsume(queue: "notifyDriver", autoAck: false, consumer: consumer);
            Console.ReadLine();

        }
    }

    public static class ConnectionUser
    {
        public static string conn { get; set; }
    }
}