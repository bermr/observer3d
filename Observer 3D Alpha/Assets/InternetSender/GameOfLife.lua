-- game of life adapted example

PROBABILITY = 0.15
TURNS = 20

cell = Cell{
    cover = Random{alive = PROBABILITY, dead = 1 - PROBABILITY},
    countAlive = function(self)
        local count = 0
        forEachNeighbor(self, function(neigh)
            if neigh.past.cover == "alive" then
                count = count + 1
            end
        end)

        return count
    end,
    execute = function(self)
        local n = self:countAlive()
        if self.cover == "alive" and (n > 3 or n < 2) then
            self.cover = "dead"
        elseif self.cover == "dead" and n == 3 then
            self.cover = "alive"
        else
            self.cover = self.past.cover
        end
        cs:notify()
        os.execute("sleep " .. tonumber(0.1))
    end
}

cs = CellularSpace{
    xdim = 30,
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
    end},
}

timer:run(TURNS)

