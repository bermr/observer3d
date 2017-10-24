-- @example A simple example with one Agent that moves randomly in space.
-- @image single-agent.png

singleFooAgent = Agent{
	value = 5,
	execute = function(self)
		self:walk()
        singleFooAgent:notify()
	end
}

is = InternetSender{
    target = singleFooAgent,
    --host = "127.0.0.1",
    port = 53474,
    select = "value",
    protocol = "udp",
    compress = false
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

t:run(10000)

