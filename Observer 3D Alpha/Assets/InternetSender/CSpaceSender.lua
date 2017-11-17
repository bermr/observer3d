-- sending a basic cellular space

cell = Cell{
    height = Random{min = 0, max = 100},
    cover = Random{"green", "brown"}
}

cs = CellularSpace{
    xdim = 10,
    instance = cell,
    execute = function(self)
        cs:notify()
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

is = InternetSender{
    target = cs,
    port = 55000,
    host = "127.0.0.1",
    select = "height",
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

t:run(1)