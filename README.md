# Io.Autometa.Lobby

https://lobby.autometa.io/


## https://lobby.autometa.io/Create
Call *Create* to create a new lobby. The caller will become the owner of the lobby.

[ServerResponse(GameLobby)](./src/Io/Autometa/Lobby/Contract/Schema/ServerResponse(GameLobby).json) Create([CreateGameLobbyRequest](./src/Io/Autometa/Lobby/Contract/Schema/CreateGameLobbyRequest.json) newLobby)

```bash
curl -H "Content-Type: application/json" -X POST -d '{"owner":{"ip":"localhost","port":6969,"nickName":"some server"},"gameType":{"id":"LocalRedisTest","api":1},"hidden":false}' https://lobby.autometa.io/Create
```

## https://lobby.autometa.io/Join
Call *Join* to join an existing lobby. The caller's information will be added to the list of lobby clients.

[ServerResponse(GameLobby)](./src/Io/Autometa/Lobby/Contract/Schema/ServerResponse(GameLobby).json) Join([LobbyRequest](./src/Io/Autometa/Lobby/Contract/Schema/LobbyRequest.json) request)

```bash
curl -H "Content-Type: application/json" -X POST -d '{"lobbyId":"LocalRedisTestv1-7Y5FBA","client":{"ip":"localhost","port":6969,"nickName":"some client"}}' https://lobby.autometa.io/Join
```

## https://lobby.autometa.io/Leave
Call *Leave* to exit an existing lobby. If the caller is the host, the host may use this function to remove any client in the lobby. If the host removes themselves, the lobby is instantly deleted.

[ServerResponse(GameLobby)](./src/Io/Autometa/Lobby/Contract/Schema/ServerResponse(GameLobby).json) Leave([LobbyRequest](./src/Io/Autometa/Lobby/Contract/Schema/LobbyRequest.json) request)

## https://lobby.autometa.io/Read
Call *Read* to get the most current state of the lobby. This will reset the expiration timer, too.

[ServerResponse(GameLobby)](./src/Io/Autometa/Lobby/Contract/Schema/ServerResponse(GameLobby).json) Read([LobbyRequest](./src/Io/Autometa/Lobby/Contract/Schema/ReadRequest.json) request)

## https://lobby.autometa.io/Search
Call *Search* to get a list of all currently active lobbies for a game type. The number of results may be limited, but this can be mitigated if hosts play by the rules and call *Leave* once a game starts.

[ServerResponse(SearchResponse)](./src/Io/Autometa/Lobby/Contract/Schema/ServerResponse(SearchResponse).json) Search([Game](./src/Io/Autometa/Lobby/Contract/Schema/Game.json) game)

```bash
curl -H "Content-Type: application/json" -X POST -d '{"id":"LocalRedisTest","api":1}' https://lobby.autometa.io/Search
```

## Expiration
Lobbies will expire 5 minutes after the last call to *Create*, *Join*, *Leave*, or *Read*. Expiration can be forced if a lobby owner calls *Leave* on themselves. Expiration can be post-poned if the host periodically calls *Read* to get the most recent state.

## Lazy/Asynchronous/Promiscuous Mode
You don't have to use the lobby as intended if your game does not require a simultaneous start (that is, you want to allow people to drop in and out and it's no problem). In that case, you can design matchmaking like the following:
1. Call *Search* to find any already-existing games. Randomly select one and just start communicating to the host's IP and Port directly.
2. If there are no already-existing games, call Create to start a new game. Every 5 or more minutes, call Create again to make a new lobby.
This uses the lobby system just as a lighweight discovery address book, and that's ok.

## Configuration
This application is only intended to run on AWS Lambda, and it's been created in such a way as to make it pretty cheap to run. The application is created and torn down for each method call, so anything that has internal caching or expensive factory creation methods should be avoided. To that end, there is no ASP.NET framework and I created my own thin client to talk to Redis.
1. Create a Redis ElastiCache server.
2. Set up an AWS Lambda function targetting *Io.Autometa.Lobby.Server::Io.Autometa.Lobby.Server.Gateway::FunctionHandler*. Set the *ElasticacheConnectionString* environment variable on it to point to the Redis server. Give it permissions to access the Redis server.
3. Set up an API Gateway to point to the Lambda function. It should be set up in proxy mode, since the same function will handle all paths.
4. Set up an external DNS in AWS and point it to the API Gateway.
