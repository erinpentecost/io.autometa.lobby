if redis.call("EXISTS",KEYS[1]) == 1 then
    redis.call("DEL",ARGV[1])
    redis.call("SET",KEYS[1],ARGV[1])
    return true
else
    return false
end