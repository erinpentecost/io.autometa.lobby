ROOTDIR=$(pwd)

cd "${ROOTDIR}"
cd ./src/Io/Autometa/Lobby/Server/
dotnet restore
dotnet lambda deploy-function FunctionHandler

cd "${ROOTDIR}"
cd ./src/Io/Autometa/
dotnet ./Schema/bin/Debug/netcoreapp2.0/Io.Autometa.Schema.dll -d "./Lobby/Contract/bin/Release/netstandard2.0/Io.Autometa.Lobby.Contract.dll" -t "ILobby" -o "./Lobby/Contract/Schema/"
