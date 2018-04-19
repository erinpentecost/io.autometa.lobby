ROOTDIR=$(pwd)

cd "${ROOTDIR}"
cd ./src/Io/Autometa/Lobby/
dotnet restore
dotnet lambda deploy-function FunctionHandler

cd "${ROOTDIR}"
cd ./src/Io/Autometa/
dotnet ./Schema/bin/Debug/netcoreapp2.0/Io.Autometa.Schema.dll -d "./Schema/bin/Debug/netcoreapp2.0/Io.Autometa.Lobby.dll" -t "Io.Autometa.Lobby.Message.*" -o "./Lobby/Schema/"
dotnet ./Schema/bin/Debug/netcoreapp2.0/Io.Autometa.Schema.dll -d "./Schema/bin/Debug/netcoreapp2.0/Io.Autometa.Lobby.dll" -t "ILobby" -o "./Lobby/Schema/"