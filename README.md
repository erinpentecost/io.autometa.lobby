# Io.Autometa.Lobby

## API
[See Contract documentation.](./src/Io/Autometa/Lobby/Contract/README.md)

## Configuration
This application is only intended to run on AWS Lambda, and it's been created in such a way as to make it pretty cheap to run. The application is created and torn down for each method call, so anything that has internal caching or expensive factory creation methods should be avoided. To that end, there is no ASP.NET framework and I created my own thin client to talk to Redis.
1. Create a Redis ElastiCache server.
2. Set up an AWS Lambda function targetting *Io.Autometa.Lobby.Server::Io.Autometa.Lobby.Server.Gateway::FunctionHandler*. Set the *ElasticacheConnectionString* environment variable on it to point to the Redis server. Give it permissions to access the Redis server.
3. Set up an API Gateway to point to the Lambda function. It should be set up in proxy mode, since the same function will handle all paths.
4. Set up an external DNS in AWS and point it to the API Gateway.
