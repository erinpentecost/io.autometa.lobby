# Io.Autometa.Lobby

https://lobby.autometa.io/


## /Create
[ServerResponse<GameLobby>](./src/Io/Autometa/Lobby/Contract/Schema/ServerResponse(GameLobby).json) Create([CreateGameLobbyRequest](./src/Io/Autometa/Lobby/Contract/Schema/CreateGameLobbyRequest.json) newLobby)

```bash
curl -H "Content-Type: application/json" -X POST -d '{"owner":{"ip":"localhost","port":6969,"game":{"id":"LocalRedisTest","api":1},"nickName":"nickName"},"hidden":false}' https://lobby.autometa.io/Create
```

## /Join
[ServerResponse<GameLobby>](./src/Io/Autometa/Lobby/Contract/Schema/ServerResponse(GameLobby).json) Join([LobbyRequest](./src/Io/Autometa/Lobby/Contract/Schema/LobbyRequest.json) request)


## /Leave
[ServerResponse<GameLobby>](./src/Io/Autometa/Lobby/Contract/Schema/ServerResponse(GameLobby).json) Leave([LobbyRequest](./src/Io/Autometa/Lobby/Contract/Schema/LobbyRequest.json) request)

## /Read
[ServerResponse<GameLobby>](./src/Io/Autometa/Lobby/Contract/Schema/ServerResponse(GameLobby).json) Read([LobbyRequest](./src/Io/Autometa/Lobby/Contract/Schema/ReadRequest.json) request)

## /Search
[ServerResponse<SearchResponse>](./src/Io/Autometa/Lobby/Contract/Schema/ServerResponse(SearchResponse).json) Search([Game](./src/Io/Autometa/Lobby/Contract/Schema/Game.json) game)