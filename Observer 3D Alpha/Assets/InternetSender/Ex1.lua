-- sending a basic cellular CellularSpace

r = Random()
cell = Cell{
    height = Random{min = 0, max = 1},
    cover = Random{"green", "black", "red"}
}

cs = CellularSpace{
    xdim = 80,
    instance = cell,
    execute = function(self)
        forEachCell (cs, function(cell)
            cell.height = (cell.x+15*r:number())/50
            --print(cell.x, cell.height)
        end)
        cs:notify()
        os.execute("sleep " .. tonumber(0.03))
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

t:run(100)