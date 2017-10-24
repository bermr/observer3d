-- @example Simulation of a Society with 30 moving Agents.
-- @image single-agents-society.png

singleFooAgent = Agent{
	execute = function(self)
		local cell = self:getCell():getNeighborhood():sample()
		if cell:isEmpty() then
			self:move(cell)
		end
	end
}

soc = Society{
	instance = singleFooAgent,
	quantity = 1,
	value = 5
}

is = InternetSender{
    target = soc,
    --host = "127.0.0.1",
    select = "value",
    port = 55000,
    protocol = "udp",
    compress = false
}

cs = CellularSpace{
	xdim = 10
}

cs:createNeighborhood()

e = Environment{
	cs,
	soc
}

e:createPlacement{max = 5}

t = Timer{
	Event{action = soc}
}

t:run(10000)

