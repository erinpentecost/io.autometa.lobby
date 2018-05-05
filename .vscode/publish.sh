ROOTDIR=$(pwd)

cd "${ROOTDIR}"
cd ./src/Io/Autometa/Lobby/Server/
dotnet restore
dotnet lambda deploy-function FunctionHandler