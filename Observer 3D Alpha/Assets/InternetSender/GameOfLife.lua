-- game of life adapted example

PROBABILITY = 0.15
TURNS = 1000

cell = Cell{
    cover = Random{white = PROBABILITY, black = 1 - PROBABILITY},
    countAlive = function(self)
        local count = 0
        forEachNeighbor(self, function(neigh)
            if neigh.past.cover == "white" then
                count = count + 1
            end
        end)
        return count
    end,
    execute = function(self)
        local n = self:countAlive()
        if self.cover == "white" and (n > 3 or n < 2) then
            self.cover = "black"
        elseif self.cover == "black" and n == 3 then
            self.cover = "white"
        else
            self.cover = self.past.cover
        end
    end
}

cs = CellularSpace{
    xdim = 60,
    instance = cell
}

cs:createNeighborhood()

is = InternetSender{
    target = cs,
    port = 55000,
    host = "127.0.0.1",
    select = "cover",
    protocol = "udp",
    compress = false,
    visible = false
}

timer = Timer{
    Event{action = function()
        cs:synchronize()
        cs:execute()
        cs:notify()
    end},
}

timer:run(TURNS)

