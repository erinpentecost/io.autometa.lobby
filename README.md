# Io.Autometa.Lobby

Lightweight RESTful lobby service that supports different games simultaneously by abusing Redis. It functions similarly to the [JackBox game lobby](https://jackbox.tv/).

## API

Swagger exposes the API; rely on it instead of documentation which may be out of date.

### Create

Create a lobby for the "gitgame" game:

```bash
curl -H "Content-Type: application/json" -X POST -d '{"port":6969,"name":"host nickname","hidden":false,"meta":{}}' https://lobby.autometa.io/api/gitgame
```

This can be extended with metadata.

### Search

Look for lobbies for the "gitgame" game:

```bash
curl -H "Content-Type: application/json" https://lobby.autometa.io/api/gitgame
```

Search results can be further filtered by metadata.

### Refresh/Read

Lobbies that are not called for a time are dropped. Call this method to refresh the TTL.
This is for a lobby with an ID of "2KQPG" for the "gitgame" game.

```bash
curl -H "Content-Type: application/json" https://lobby.autometa.io/api/gitgame/2KQPG
```

### Join Lobby

Join a lobby as a player named "player1", declaring a desire to use port 7000. This isn't technically necessary to use the service.

```bash
curl -H "Content-Type: application/json" -X POST https://lobby.autometa.io/api/gitgame/2KQPG/player1?port=7000
```

### Leave/Kick/Destroy Lobby

This method is overloaded. If the caller is the host, they can kick any player. If they kick themself, the lobby is destroyed. If the caller is not the host, they may only kick themself. This isn't technically necessary to use the service.

```bash
curl -H "Content-Type: application/json" -X DELETE https://lobby.autometa.io/api/gitgame/2KQPG/player1
```

## Configuration for AWS

1. Create a Redis ElastiCache server.
2. Set up an AWS Lambda function targetting *Io.Autometa.Lobby.Server::Io.Autometa.Lobby.Server.Gateway::FunctionHandler*. Set the *ElasticacheConnectionString* environment variable on it to point to the Redis server. Give it permissions to access the Redis server.
3. Set up an API Gateway to point to the Lambda function. It should be set up in proxy mode, since the same function will handle all paths.
4. Set up an external DNS in AWS and point it to the API Gateway.

TODO: Make this all automatic on publish.

## Configuration for Local Execution

See the [integration test](./.vscode/runTest.sh) for an example.

## Attribution

1. [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis)
2. [https://github.com/neuecc/RespClient](https://github.com/neuecc/RespClient/blob/master/RespClient/Cmdlet/Cmdlets.cs)
