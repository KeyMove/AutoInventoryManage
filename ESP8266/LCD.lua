LCD={
SetPos=function(x,y)
gpio.write(2, 0)
spi.send(1,0xb0+y,0x10+x/16,x%16)
end
,
SetContrast=function(v)
gpio.write(2, 0)
spi.send(1,0x81,v)
end
,
Init=function()
local cmd={0xe2,0x0A,0xA2,0xA0,0xc8,0x40,0x25,0x26,0x2f,0x81,0x2,0xaf,0xb0,0x10,0x00}
gpio.mode(1, gpio.OUTPUT) gpio.mode(2, gpio.OUTPUT) gpio.mode(4, gpio.OUTPUT) gpio.mode(5, gpio.OUTPUT) gpio.mode(7, gpio.OUTPUT)
gpio.write(1, gpio.HIGH) gpio.write(2, gpio.HIGH) gpio.write(4, gpio.HIGH) gpio.write(5, gpio.HIGH) gpio.write(7, gpio.HIGH)
gpio.write(4, gpio.LOW)
tmr.delay(1000)
gpio.write(4, gpio.HIGH)
tmr.delay(1000)
spi.setup(1,1,1,0,8,16)
for i=1,#cmd do
gpio.write(1, gpio.LOW)
gpio.write(2, 0)
spi.send(1,cmd[i])
gpio.write(1, gpio.HIGH)
tmr.delay(10)
end
end
,
Clear=function()
gpio.write(1, gpio.LOW)
for y=0,7 do
LCD.SetPos(0,y)
gpio.write(2, 1)
for x=1,16 do
spi.send(1,0,0,0,0,0,0,0,0)
end
end
gpio.write(1, gpio.HIGH)
end
,
Drawdata=function(x,y,w,h,addr)
h=h+7
h=h/8
gpio.write(1, gpio.LOW)
file.seek('set',addr)
for i=1,h do
LCD.SetPos(x,y)
y=y+1
gpio.write(2, 1)
spi.send(1,file.read(w))
end
gpio.write(1, gpio.HIGH)
end
}

GUI={
w=8,
h=16,
s=16,
l=16,
SetFont=function(f,w,h)
file.open(f)
GUI.w=w
GUI.h=h
GUI.s=w*((h+7)/8)
GUI.l=128/w
end
,
ShowString=function(x,y,str)
local i=1
while i<=#str do
local c=string.byte(str,i)
if c>=128 then
i=i+1
LCD.Drawdata(x,y,GUI.w*2,GUI.h,(94*(c-0xa1)+(string.byte(str,i)-0xa1))*32+1520)
x=x+GUI.w*2
else
LCD.Drawdata(x,y,GUI.w,GUI.h,(c-32)*GUI.s)
x=x+GUI.w
end
i=i+1
end
end
,
ShowCentre=function(y,str)
local i=1
local j=0
local h=(GUI.h+7)/8
while (#str-(i-1))>=GUI.l do
while j<16 do
if string.byte(str,i+j)>128 then j=j+2 else j=j+1 end
if j>16 then j=j-2 break end
end
if j==16 then
GUI.ShowString(0,y,string.sub(str,i,i+j))
else
GUI.ShowString(4,y,string.sub(str,i,i+j))
end
i=i+j
j=0
y=y+h
end
h=#str-(i-1)
h=(128-(h*GUI.w))/2
GUI.ShowString(h,y,string.sub(str,i,#str))
end
}
