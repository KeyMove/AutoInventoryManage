dofile('LCD.lc')
LCD.Init()
LCD.Clear()
file.open('H16.dat')
GUI.ShowCentre(3,'准备就绪')
local data={}
uart.on("data",function(dat)
table.insert(data,dat)
if string.find(dat,'\n')~=nil then
local s=string.gsub(table.concat(data),'\r\n','')
s=decode(s)
LCD.Clear()
GUI.ShowCentre(3,s)
data={}
end
end,0)

function decode(s)
local hz=0
local l=0
local v=0
local str={}
for i=1,#s do
local t=string.byte(s,i)
t=(t*2)%256
for j=1,7 do
if hz~=0 then
v=v*2
if l==7 then v=v*2 end
if t>=128 then v=v+1 end
l=l-1
if l==0 then
if hz==1 then 
v=v+0xa1a1 
table.insert(str,string.char(v/256)) table.insert(str,string.char(v%256))
else 
table.insert(str,string.char(v)) 
end
hz=0
end
else
v=0
hz=2
l=7
if t>=128 then
hz=1
l=14
end
end
t=(t*2)%256
end
end
return table.concat(str)
end
