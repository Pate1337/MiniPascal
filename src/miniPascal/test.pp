program helloprogram;

procedure hello();
begin
    writeln("Hello");
end;

function printHello(var count : integer, times : integer) : integer;
begin
    hello();
    if count = times then return 0
    else
    begin
        count := count + 1;
        return printHello(count, times)
    end
end;

begin
var count : integer;
count := 1;
printHello(count, 10)
end .
