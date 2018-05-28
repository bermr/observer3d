-- sending a basic CellularSpace

x = 5
y = 5

cell = Cell{
    cover = "green",
    height = 0.5
}

cs = CellularSpace{
    xdim = 33,
    instance = cell,
    execute = function(self)
        cs:get(x,y).cover = "blue"
        cs:get(x+4,y+4).height = 0.0
        y = y + 1
        if (y == 20) then
            cs:get(x+4,y+8).height = 1.0
            y = 5
            x = x + 1
        end
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

t:run(120)