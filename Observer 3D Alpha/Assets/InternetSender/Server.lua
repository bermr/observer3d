#!/usr/bin/env terrame

local socket = require("socket")

udp = socket.udp()
udp:setsockname("*", 55000)
udp:settimeout()

while true do
    data, ip, port = udp:receivefrom()
    if data then
        print("Received: ", data)
    end
end

socket.sleep(0.01)