-- @example Implementation of a simple runoff model. It uses a
-- cellular data created from a tiff file (cabecadeboi.shp).
-- The Neighborhood of a Cell is composed by its Moore neighbors that
-- have lower height.
-- There is an initial rain of 10mm in the highest cells.
-- Each cell then sends its water equally to its neighbors.

local init = function(model)
    model.cell = Cell{
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

    model.cs = CellularSpace{
        file = filePath("cabecadeboi.shp", "gis"),
        instance = model.cell
    }

    model.cs:createNeighborhood{
        strategy = "mxn",
        filter = function(cell, neigh)
            return cell.height >= neigh.height
        end
    }

    model.is = InternetSender{
        target = model.cs,
        port = 55000,
        host = "127.0.0.1",
        select = "water",
        protocol = "udp",
        compress = false,
        visible = false
    }

    model.timer = Timer{
        Event{action = model.cs}
    }

    model.cs:notify()
end

Runoff = Model{
    finalTime = 50,
    init = init
}

Runoff:run()