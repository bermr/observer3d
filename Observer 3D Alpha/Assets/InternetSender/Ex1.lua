-- sending a basic cellular CellularSpace

r = Random()
cell = Cell{
    height = Random{min = 0, max = 1},
    cover = "green"
}

cs = CellularSpace{
    xdim = 33,
    instance = cell,
    execute = function(self)
        forEachCell (cs, function(cell)
            cell.height = (cell.x+15*r:number())/33
            if (cell.height > 0.95) then
                cell.cover = "red"
            end
            if (cell.height <= 0.2) then
                cell.cover = "blue"
            end
            if (cell.height > 0.2 and cell.height < 0.6) then
                cell.cover = "green"
            end
            if (cell.height >= 0.6 and cell.height <= 0.95) then
                cell.cover = "yellow"
            end
            if (cell.x == 32) then
                cell.height = 0
            end
            if (cell.y == 32) then
                cell.height = 0
            end
            if (cell.y == 0) then
                cell.height = 0
            end
            --print(cell.x, cell.height)
        end)
        cs:notify()
        os.execute("sleep " .. tonumber(0.1))
    end
}

--[[is = InternetSender{
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
}]]

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
}

e = Environment{
    cs
}


t = Timer{
    Event{action = cs}
}

t:run(100)