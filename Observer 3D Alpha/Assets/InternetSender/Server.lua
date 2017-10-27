#!/usr/bin/env terrame

local socket = require("socket")

udp = socket.udp()
udp:setsockname("*", 55000)
udp:settimeout()

data, ip, port = udp:receivefrom()
if data then
    print("Received: ", data)
    assert(data == "Data1")
    data = "Data2"
    assert(data == "Data2")
    --escrever no arquivo
end
socket.sleep(0.01)