# ContosoCrafts (Tye Edition)

## Spinning up the environment

Navigate to the `src/` folder and run the following command in your terminal:

```bash
> tye run
```


### Requirements
- [Tye](https://github.com/dotnet/tye)
- Docker
- Visual Studio Code
- .NET Core SDK


### Application Components

- [Contoso Website](src/ContosoCrafts.WebSite)
- [Products API](src/ContosoCrafts.ProductsApi)
- [Checkout Processor](src/ContosoCrafts.CheckoutProcessor)

### Infrastructure Components

- Eureka - Service Discovery
- [Redis](https://redis.io/) - State store
- [RabbitMQ](https://www.rabbitmq.com/) - Message Broker
- [Zipkin](https://zipkin.io/) - Distributed tracing
- [MongoDB](https://docs.mongodb.com/) - Products data
- [Seq](https://datalust.co/seq) - Log Aggregator