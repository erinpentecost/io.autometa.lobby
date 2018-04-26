ROOTDIR=$(pwd)

cd "${ROOTDIR}"
cd ./src/Io/Autometa/Lobby/Server/
dotnet restore
dotnet lambda deploy-function FunctionHandler

cd "${ROOTDIR}"
cd ./src/Io/Autometa/
dotnet ./Schema/bin/Debug/netcoreapp2.0/Io.Autometa.Schema.dll -d "./Schema/bin/Debug/netcoreapp2.0/Io.Autometa.Lobby.dll" -t "Io.Autometa.Lobby.Contract*" -o "./Lobby/Contract/Schema/"
dotnet ./Schema/bin/Debug/netcoreapp2.0/Io.Autometa.Schema.dll -d "./Schema/bin/Debug/netcoreapp2.0/Io.Autometa.Lobby.dll" -t "ILobby" -o "./Lobby/Contract/Schema/"