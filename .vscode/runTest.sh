ROOTDIR=$(pwd)

cd "${ROOTDIR}"
cd ./test/Io/Autometa/Redis
./redis-server &

sleep 3

cd "${ROOTDIR}"
cd ./src/Io/Autometa/Lobby/WebServer/bin/Debug/netcoreapp2.0/
dotnet ./Io.Autometa.Lobby.WebServer.dll &
KESTREL=$!

sleep 3

cd "${ROOTDIR}"
cd ./test/Io/Autometa/Lobby/
dotnet xunit --fx-version 2.0.6

sleep 3

kill ${KESTREL}

cd "${ROOTDIR}"
cd ./test/Io/Autometa/Redis
./redis-cli FLUSHALL
./redis-cli SHUTDOWN NOSAVE

rm ./dump.rdb