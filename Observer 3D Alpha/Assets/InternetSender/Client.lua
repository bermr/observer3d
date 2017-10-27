#!/usr/bin/env lua5.1

local socket = require("socket")

udp = socket.udp()
udp:setpeername("127.0.0.1", 55000)
udp:settimeout()

data = "Data1"
assert(data == "Data1")

udp:send(data)
data = udp:receive()
if data then
    --print("Received: ", data)
    assert(data, "Data2")
end