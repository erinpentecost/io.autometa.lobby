# Io.Autometa.Lobby

https://lobby.autometa.io/


## /Create
```CSharp
ServerResponse<GameLobby> Create(CreateGameLobbyRequest newLobby);
```

```bash
curl -H "Content-Type: application/json" -X POST -d '{"owner":{"ip":"localhost","port":6969,"game":{"id":"LocalRedisTest","api":1},"nickName":"nickName"},"hidden":false}' https://lobby.autometa.io/Create
```
```CSharp
[ServerResponse<GameLobby>](../src/Io/Autometa/Lobby/Contract/Schema/ServerResponse(GameLobby).json) Create([CreateGameLobbyRequest](../src/Io/Autometa/Lobby/Contract/Schema/CreateGameLobbyRequest.json) newLobby);
```

[ServerResponse<GameLobby>](../src/Io/Autometa/Lobby/Contract/Schema/ServerResponse(GameLobby).json) Create([CreateGameLobbyRequest](../src/Io/Autometa/Lobby/Contract/Schema/CreateGameLobbyRequest.json) newLobby);

## Create(CreateGameLobby) -> ServerResponse(GameLobby)
CreateGameLobby
```json
{"owner":{"ip":"localhost","port":6969,"game":{"id":"LocalRedisTest","api":1},"nickName":"nickName"},"hidden":false}
```
ServerResponse(GameLobby)
```json
{"response":{"lobbyID":"LocalRedisTestv1-279c656425","game":{"id":"LocalRedisTest","api":1},"host":{"ip":"localhost","port":6969,"game":{"id":"LocalRedisTest","api":1},"nickName":"nickName"},"clients":[{"ip":"localhost","port":6960,"game":{"id":"LocalRedisTest","api":1},"nickName":"nonowner"}],"creationTime":"0001-01-01T00:00:00","locked":false,"hidden":false},"valid":{"result":true,"reason":[]}}
```

```bash
curl -H "Content-Type: application/json" -X POST -d '{"owner":{"ip":"localhost","port":6969,"game":{"id":"LocalRedisTest","api":1},"nickName":"nickName"},"hidden":false}' https://xp92sqtki2.execute-api.us-west-2.amazonaws.com/deploy/Create
```

## Join(LobbyRequest) -> ServerResponse(GameLobby)
LobbyRequest
```json
{"lobbyId":"LocalRedisTestv1-279c656425","client":{"ip":"localhost","port":6960,"game":{"id":"LocalRedisTest","api":1},"nickName":"nonowner"}}
```
## Lock(LobbyRequest) -> ServerResponse(GameLobby)
## Read(LobbyRequest) -> ServerResponse(GameLobby)
## Search(Game) -> ServerResponse(SearchResponse)
Game

ServerResponse(SearchResponse)
```json
{"response":{"lobbyID":["LocalRedisTestv1-279c656425"]},"valid":{"result":true,"reason":[]}}
```

```bash
curl -H "Content-Type: application/json" -X POST -d '{"id":"LocalRedisTest","api":1}' https://xp92sqtki2.execute-api.us-west-2.amazonaws.com/deploy/Search
```