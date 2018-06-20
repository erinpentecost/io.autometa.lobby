# Io.Autometa.Redis

This package is a low-level wrapper around the Redis protocol.

Other, better wrappers out there, but this one targets dotnet core and initializes fast.

Fast initialization is necessary if the server is going to start and stop all the time, which is the case for Amazon Lambda.