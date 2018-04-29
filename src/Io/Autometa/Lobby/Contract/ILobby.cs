using System.Collections.Generic;

namespace Io.Autometa.Lobby.Contract
{
    /// <summary>
    /// This defines the actual interface for the lobby.
    /// HTTP path calls these methods via reflection from
    /// Server.Gateway. Each method defined here must take in
    /// one parameter, and must return a ServerResponse<>.
    /// </summary>
    public interface ILobby
    {
        /// <summary>
        /// Create a new lobby, with the calling user as the owner.
        /// </summary>
        /// <param name="newLobby">lobby creation parameters</param>
        /// <returns>the lobby that was created</returns>
        ServerResponse<GameLobby> Create(CreateGameLobby newLobby);

        /// <summary>
        /// Join an existing lobby.
        /// </summary>
        /// <param name="request">lobby to join</param>
        /// <returns>the lobby that was joined</returns>
        ServerResponse<GameLobby> Join(LobbyRequest request);

        /// <summary>
        /// Leave a lobby. If the caller is the owner,
        /// any client may be kicked. If the caller is the owner
        /// and leaves herself, the lobby will be deleted.
        /// Otherwise, clients can call Leave on themselves.
        /// </summary>
        /// <param name="request">lobby to leave</param>
        /// <returns>the lobby that was left</returns>
        ServerResponse<GameLobby> Leave(LobbyRequest request);

        /// <summary>
        /// Get most current state on a lobby given a key.
        /// This will refresh the expiration timer on the lobby.
        /// </summary>
        /// <param name="request">lobby id</param>
        /// <returns>lobby that was read</returns>
        ServerResponse<GameLobby> Read(ReadRequest request);

        /// <summary>
        /// Get a list of current lobbies.
        /// Unlike other methods, this will not refresh
        /// expiration timers. This is the most computationally
        /// expensive method.
        /// </summary>
        /// <param name="game">game to look for lobbies for</param>
        /// <returns>big ol' list o' lobbies</returns>
        ServerResponse<SearchResponse> Search(Game game);
    }
}