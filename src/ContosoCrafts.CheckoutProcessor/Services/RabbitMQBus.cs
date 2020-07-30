using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading;
using ContosoCrafts.CheckoutProcessor.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ContosoCrafts.CheckoutProcessor.Services
{
    public class RabbitMQBus
    {
        private const string CHECKOUT_QUEUE_NAME = "contoso_steeltoe_checkout";
        private const string CHECKOUT_EXCHANGE_NAME = "contoso_steeltoe_cart";
        private const string CHECKOUT_ROUTING_KEY = "cart_checkout";
        private readonly ObjectPool<IModel> _rabbitBuilderPool;
        private readonly ILogger<RabbitMQBus> _logger;

        public RabbitMQBus(ObjectPool<IModel> rabbitBuilderPool, ILogger<RabbitMQBus> logger)
        {
            _logger = logger;
            _rabbitBuilderPool = rabbitBuilderPool;
            InitializeQueue();
        }

        protected virtual void InitializeQueue()
        {
            _logger.LogInformation($"Initializing queue:{CHECKOUT_QUEUE_NAME} and exchange:{CHECKOUT_EXCHANGE_NAME}");

            IModel channel = _rabbitBuilderPool.Get();
            channel.QueueDeclare(queue: CHECKOUT_QUEUE_NAME, durable: true,
                                 exclusive: false, autoDelete: false, arguments: null);

            channel.ExchangeDeclare(CHECKOUT_EXCHANGE_NAME, ExchangeType.Direct);
            channel.QueueBind(CHECKOUT_QUEUE_NAME, CHECKOUT_EXCHANGE_NAME, CHECKOUT_ROUTING_KEY, null);

            _rabbitBuilderPool.Return(channel);
        }

        public virtual void Publish<T>(T payload)
        {
            var json_payload = JsonSerializer.Serialize(payload);

            IModel channel = _rabbitBuilderPool.Get();

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = MediaTypeNames.Application.Json;
            properties.MessageId = Guid.NewGuid().ToString("N");

            channel.BasicPublish(exchange: CHECKOUT_EXCHANGE_NAME,
                                 routingKey: CHECKOUT_ROUTING_KEY,
                                 mandatory: false, //TODO: huh??
                                 basicProperties: properties,
                                 body: Encoding.UTF8.GetBytes(json_payload));

            _rabbitBuilderPool.Return(channel);
        }

        public virtual void Consume(CancellationToken cancellation)
        {
            IModel channel = _rabbitBuilderPool.Get();

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, args) =>
           {
               _logger.LogInformation("Message Received");

               var json_payload = Encoding.UTF8.GetString(args.Body.ToArray());
               var cartItems = JsonSerializer.Deserialize<IEnumerable<CartItem>>(json_payload);

               _logger.LogInformation($"Received {cartItems.Count()} items");

           };

            channel.BasicConsume(CHECKOUT_QUEUE_NAME, true, consumer);

            _logger.LogInformation("Shutting down consumer");

            cancellation.WaitHandle.WaitOne();
            _rabbitBuilderPool.Return(channel);
        }
    }
}