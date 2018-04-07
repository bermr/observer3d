-- @example A simple example with one Agent that moves randomly in space.
-- @image single-agent.png

singleFooAgent = Agent{
    cell,
    execute = function(self)
        self:walk()
        cell = getCell
        singleFooAgent:notify()
    end
}

cs = CellularSpace{
    xdim = 10
}

cs:createNeighborhood()

e = Environment{
    cs,
    singleFooAgent
}

e:createPlacement()

is = InternetSender{
    target = singleFooAgent,
    port = 55000,
    host = "127.0.0.1",
    select = "cell",
    protocol = "udp",
    compress = false,
    visible = false
}

t = Timer{
    Event{action = singleFooAgent},
}

t:run(100)

