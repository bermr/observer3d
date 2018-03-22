-- sending a basic cellular space

r = Random()
cell = Cell{
    cover = Random{"green", "black", "red"}
}

cs = CellularSpace{
    xdim = 33,
    instance = cell,
    execute = function(self)
        forEachCell (cs, function(cell)
            cell.cover = Random{"green", "black", "red"}
        end)
        cs:notify()
        os.execute("sleep " .. tonumber(0.05))
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

t:run(100)