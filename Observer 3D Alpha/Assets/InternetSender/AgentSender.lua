-- singleagent sample

singleFooAgent = Agent{
    value = 1,
    execute = function(self)
        self:walk()
        singleFooAgent:notify()
    end
}

is = InternetSender{
    target = singleFooAgent,
    port = 55000,
    host = "127.0.0.1",
    select = "value",
    protocol = "udp",
    compress = false,
    visible = false
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

t = Timer{
    Event{action = singleFooAgent}
}

t:run(1)