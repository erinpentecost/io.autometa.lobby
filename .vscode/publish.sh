ROOTDIR=$(pwd)

cd "${ROOTDIR}"
cd ./src/Io/Autometa/Lobby/Server/
dotnet restore
dotnet build

cd "${ROOTDIR}"
cd ./src/Io/Autometa/Lobby/WebServer/
dotnet restore
dotnet build

dotnet lambda deploy-function FunctionHandler