-- sending a basic cellular space

r = Random()
cell = Cell{
    height = Random{min = 0, max = 1},
    cover = Random{"green", "black", "red"}
}

cs = CellularSpace{
    xdim = 33,
    instance = cell,
    execute = function(self)
        cs:notify()
        os.execute("sleep " .. tonumber(2))
    end
}

is = InternetSender{
    target = cs,
    port = 55000,
    host = "127.0.0.1",
    select = "cover",
    protocol = "udp",
    compress = false,
    visible = false
}

e = Environment{
    cs
}

t = Timer{
    Event{action = cs}
}

t:run(10)