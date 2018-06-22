-- sending a basic CellularSpace

x = 20
y = 5

cell = Cell{
    cover = "green",
    height = 1.0
}

cs = CellularSpace{
    xdim = 33,
    instance = cell,
    execute = function(self)
        cs:get(x,y).cover = "blue"
        cs:get(x+1,y).cover = "blue"
        cs:get(x-1,y).cover = "blue"
        y = y + 1
        if (y == 18) then
           cs:get(x-2,y-1).cover = "blue"
           cs:get(x-2,y).cover = "blue"
           cs:get(x+2,y-1).cover = "blue"
           cs:get(x+2,y).cover = "blue"
       end
        cs:notify()
        os.execute("sleep " .. tonumber(0.1))
    end
}

cs:get(19,5).height = 1.0
cs:get(21,5).height = 1.0
cs:get(20,5).height = 1.0
cs:get(19,6).height = 1.0
cs:get(20,6).height = 1.0
cs:get(21,6).height = 1.0
cs:get(19,7).height = 1.0
cs:get(20,7).height = 1.0
cs:get(21,7).height = 1.0
cs:get(19,8).height = 0.9
cs:get(19,9).height = 0.8
cs:get(19,10).height = 0.7
cs:get(19,11).height = 0.6
cs:get(19,12).height = 0.5
cs:get(19,13).height = 0.4
cs:get(19,14).height = 0.3
cs:get(19,15).height = 0.2
cs:get(19,16).height = 0.1
cs:get(19,17).height = 0
cs:get(19,18).height = 0
cs:get(20,8).height = 0.9
cs:get(20,9).height = 0.8
cs:get(20,10).height = 0.7
cs:get(20,11).height = 0.6
cs:get(20,12).height = 0.5
cs:get(20,13).height = 0.4
cs:get(20,14).height = 0.3
cs:get(20,15).height = 0.2
cs:get(20,16).height = 0.1
cs:get(20,17).height = 0
cs:get(20,18).height = 0
cs:get(21,8).height = 0.9
cs:get(21,9).height = 0.8
cs:get(21,10).height = 0.7
cs:get(21,11).height = 0.6
cs:get(21,12).height = 0.5
cs:get(21,13).height = 0.4
cs:get(21,14).height = 0.3
cs:get(21,15).height = 0.2
cs:get(21,16).height = 0.1
cs:get(21,17).height = 0
cs:get(21,18).height = 0
cs:get(22,17).height = 0
cs:get(22,18).height = 0
cs:get(18,17).height = 0
cs:get(18,18).height = 0

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

map1 = Map{
    target = cs,
    select = "cover",
    value = {"green", "blue"},
    color = {"green", "blue"}
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

t:run(14)