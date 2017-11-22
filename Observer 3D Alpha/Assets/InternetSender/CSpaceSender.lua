-- sending a basic cellular space

cell = Cell{
    height = Random{0,1},
    cover = Random{"green", "grey"}
}

cs = CellularSpace{
    xdim = 33,
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