ROOTDIR=$(pwd)

cd "${ROOTDIR}"
cd ./test/Io/Autometa/Redis
./redis-server &
sleep 3

cd "${ROOTDIR}"
cd ./test/Io/Autometa/Lobby/
dotnet xunit --fx-version 2.0.6

cd "${ROOTDIR}"
cd ./test/Io/Autometa/Redis
./redis-cli FLUSHALL
./redis-cli SHUTDOWN NOSAVE

rm ./dump.rdb