-- sending a basic CellularSpace

x = 0
y = 15

r = Random()
cell = Cell{
    height = 0,
    cover = "green"
}

cs = CellularSpace{
    xdim = 33,
    instance = cell,
    execute = function(self)
        cs:get(x,y).cover = "red"
        cs:get(x,y).height = 0.1
        --y = y + 1
        x = x + 1
        cs:notify()
        os.execute("sleep " .. tonumber(0.1))
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

t:run(33)