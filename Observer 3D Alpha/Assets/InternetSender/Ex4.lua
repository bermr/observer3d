    -- cabecadeboi runoff adapted example


cell = Cell{
        init = function(cell)
            if cell.height > 200 then
                cell.water = 10 * cell:area()
            else
                cell.water = 0
            end
        end,
        on_synchronize = function(cell)
            cell.water = 0
        end,
        execute = function(cell)
            local neighbors = #cell:getNeighborhood()

            if neighbors == 0 then
                cell.water = cell.water + cell.past.water
            else
                forEachNeighbor(cell, function(neigh)
                    neigh.water = neigh.water + cell.past.water / neighbors
                end)
            end
        end,
        water100000 = function(cell)
            if cell.water > 100000 then
                return 100000
            else
                return cell.water
            end
        end
}

cs = CellularSpace{
    file = filePath("cabecadeboi.shp", "gis"),
    instance = cell
}

cs:createNeighborhood{
    strategy = "mxn",
    filter = function(cell, neigh)
        return cell.height >= neigh.height
    end
}

--[[is = InternetSender{
    target = cs,
    port = 55000,
    host = "127.0.0.1",
    select = "water",
    protocol = "udp",
    compress = false,
    visible = false
}

is = InternetSender{
    target = cs,
    port = 55000,
    host = "127.0.0.1",
    select = "water",
    protocol = "udp",
    compress = false,
    visible = false
}]]

map = Map{
    target = cs,
    select = "water",
    min = 0,
    max = 100000,
    slices = 10,
    color = {"white", "blue"}
}

map2 = Map{
    target = cs,
    select = "height",
    min = 0,
    max = 250,
    slices = 10,
    color = {"white", "black"}
}

timer = Timer{
    Event{action = function()
        cs:synchronize()
        cs:execute()
        cs:notify()
        os.execute("sleep " .. tonumber(2.5))
    end},
}

timer:run(100)
