# ContosoCrafts (Orleans Edition)

## Spinning up the environment

First, spin up the supporting infrastructure components

```bash
> docker-compose -f docker-compose-infra.yml up -d
```

Next, launch the application containers and sidecars.

```bash
> docker-compose up -d
```

### Requirements

- Docker
- Visual Studio Code
- .NET Core SDK

## What's in the box

### Application Components

- [Website](src/ContosoCrafts.WebSite) - The main application UI
- [Products API](src/ContosoCrafts.ProductsApi) - Orleans Silo co-hosted alongside an ASP.NET Core Web API.
- [Checkout Processor](src/ContosoCrafts.CheckoutProcessor) - Optional additional Silo that bootstrapped using [Generic Host](https://docs.microsoft.com/dotnet/core/extensions/generic-host?WT.mc_id=dotnet-github-cephilli). This is just here to demo the custom placement strategy. It also matches up with the layouts of the other projects.

### Infrastructure Components

- [Redis](https://redis.io/) - State store and clustering
- [MongoDB](https://docs.mongodb.com/) - Products data
- [Seq](https://datalust.co/seq) - Log Aggregator
