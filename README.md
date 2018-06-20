# Io.Autometa.Lobby

## API

Swagger exposes the API; rely on it instead of documentation which may be out of date.

## Configuration for AWS

1. Create a Redis ElastiCache server.
2. Set up an AWS Lambda function targetting *Io.Autometa.Lobby.Server::Io.Autometa.Lobby.Server.Gateway::FunctionHandler*. Set the *ElasticacheConnectionString* environment variable on it to point to the Redis server. Give it permissions to access the Redis server.
3. Set up an API Gateway to point to the Lambda function. It should be set up in proxy mode, since the same function will handle all paths.
4. Set up an external DNS in AWS and point it to the API Gateway.

## Configuration for Local Execution

See the [integration test] (./.vscode/runTest.sh) for an example.