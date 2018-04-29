# Io.Autometa.Lobby

https://lobby.autometa.io/


## https://lobby.autometa.io/Create
[ServerResponse(GameLobby)](./src/Io/Autometa/Lobby/Contract/Schema/ServerResponse(GameLobby).json) Create([CreateGameLobbyRequest](./src/Io/Autometa/Lobby/Contract/Schema/CreateGameLobbyRequest.json) newLobby)

```bash
curl -H "Content-Type: application/json" -X POST -d '{"owner":{"ip":"localhost","port":6969,"nickName":"some server"},"gameType":{"id":"LocalRedisTest","api":1},"hidden":false}' https://lobby.autometa.io/Create
```

## https://lobby.autometa.io/Join
[ServerResponse(GameLobby)](./src/Io/Autometa/Lobby/Contract/Schema/ServerResponse(GameLobby).json) Join([LobbyRequest](./src/Io/Autometa/Lobby/Contract/Schema/LobbyRequest.json) request)

```bash
curl -H "Content-Type: application/json" -X POST -d '{"lobbyId":"LocalRedisTestv1-7Y5FBA","client":{"ip":"localhost","port":6969,"nickName":"some client"}}' https://lobby.autometa.io/Join
```

## https://lobby.autometa.io/Leave
[ServerResponse(GameLobby)](./src/Io/Autometa/Lobby/Contract/Schema/ServerResponse(GameLobby).json) Leave([LobbyRequest](./src/Io/Autometa/Lobby/Contract/Schema/LobbyRequest.json) request)

## https://lobby.autometa.io/Read
[ServerResponse(GameLobby)](./src/Io/Autometa/Lobby/Contract/Schema/ServerResponse(GameLobby).json) Read([LobbyRequest](./src/Io/Autometa/Lobby/Contract/Schema/ReadRequest.json) request)

## https://lobby.autometa.io/Search
[ServerResponse(SearchResponse)](./src/Io/Autometa/Lobby/Contract/Schema/ServerResponse(SearchResponse).json) Search([Game](./src/Io/Autometa/Lobby/Contract/Schema/Game.json) game)