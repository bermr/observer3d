-- sending a basic cellular CellularSpace


cell = Cell{
    height,
    cover = "green"
}

cs = CellularSpace{
    file = filePath("cabecadeboi.shp", "gis"),
    instance = cell,
    execute = function(self)
        forEachCell (cs, function(cell)
            if (cell.height > 200) then
                cell.cover = "red"
            end
            if (cell.height <= 35) then
                cell.cover = "blue"
            end
            if (cell.height > 35 and cell.height < 120) then
                cell.cover = "green"
            end
            if (cell.height >= 120 and cell.height <= 200) then
                cell.cover = "yellow"
            end
        end)
        cs:notify()
        os.execute("sleep " .. tonumber(1.1))
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

--[[
map = Map{
    target = cs,
    select = "cover",
    value = {"red", "yellow", "green", "blue"},
    color = {"red", "yellow", "green", "blue"}
}

map2 = Map{
    target = cs,
    select = "height",
    min = 0,
    max = 1,
    slices = 10,
    color = {"white","black"}
}]]

e = Environment{
    cs
}


t = Timer{
    Event{action = cs}
}

t:run(1)