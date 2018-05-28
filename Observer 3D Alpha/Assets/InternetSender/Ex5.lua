-- sending a basic CellularSpace

cell = Cell{
    cover = "green",
    --height = 0.0
}

cs = CellularSpace{
    file = filePath("cabecadeboi.shp", "gis"),
    instance = cell,
    execute = function(self)
        cs:notify()
        os.execute("sleep " .. tonumber(0.1))
    end
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

t:run(2)