dofile('LCD.lc')
LCD.Init()
LCD.Clear()
file.open('H16.dat')
GUI.ShowCentre(3,'准备就绪')
local CacheTable={}
local data={}
local olddata
local showtable
local LCDTable
local code=""
local count=1
local check=0
local showstr=nil
local showpos=1
local showcol=0
local scanmode=0
local maxcount=100
local keydelay=100
local lastkey=0
local lastshowtime=0
uart.on("data",function(dat)
table.insert(data,dat)
if string.find(dat,'\n')~=nil then
local s=string.gsub(table.concat(data),'\r\n','')
code=s
count=CacheTable[code]~=nil and CacheTable[code] or 1
pos=0
check=(check+1)%1000
if scanmode==0 then LCD.Clear() end
scanmode=1
showtable={"","","",""}
LCDTable[1]=string.format('%16s',"获取中")
LCDTable[2]=('数量:%04d'):format(count)
LCDTable[3]='  '..s..'  '
LCDTable[4]='出库        取消'
data={}
end
end,0)

code=""
count=1
check=0
maxcount=100
LCDTable={"","","",""}
showtable={"","","",""}
olddata=""
showstr=nil
showpos=1
showcol=0
scanmode=0


function KeyEvent() 
local v=adc.read(0)
if keydelay>0 then
keydelay=keydelay-1
if v>800 then lastkey=0 end
return
end
keydelay=5
if scanmode==0 or scanmode==3 then return end
if v<20 then
scanmode=3
elseif v>250 and v<300 then
if count>maxcount then
LCDTable[2]=('库存不足:%7d'):format(maxcount)
keydelay=30
return
end
scanmode=3
count=-count
elseif v>480 and v<550 then
if count>1 then count=count-1 end
LCDTable[2]=('数量:%04d'):format(count)
keydelay=30
CacheTable[code]=count
if lastkey==1 then keydelay=2 end
lastkey=1
elseif v>600 and v<800 then
count=count+1
LCDTable[2]=('数量:%04d'):format(count)
keydelay=30
CacheTable[code]=count
if lastkey==2 then keydelay=2 end
lastkey=2
end
end

function Loop()
KeyEvent()
ShowLoop()
if showstr~=nil then
if showpos>#showstr then
showstr=nil
return
end
local c=showstr:byte(showpos)
if c>128 then
if olddata:byte(showpos)==c and olddata:byte(showpos+1)==showstr:byte(showpos+1) then showpos=showpos+2 return end
else
if olddata:byte(showpos)==c then showpos=showpos+1 return end
end
if c>=128 then
LCD.Drawdata(showpos*8-8,showcol*2-2,GUI.w*2,GUI.h,(94*(c-0xa1)+(showstr:byte(showpos+1)-0xa1))*32+1520)
showpos=showpos+2
else
LCD.Drawdata(showpos*8-8,showcol*2-2,GUI.w,GUI.h,(c-32)*GUI.s)
showpos=showpos+1
end
return
end
for i=1,#LCDTable do 
if showtable[i]~=LCDTable[i] then
olddata=showtable[i]
showtable[i]=LCDTable[i]
if #showtable[i]==0 then return end
showstr=LCDTable[i]
showpos=1
showcol=i
return
end
end 
end

function ShowLoop()
if lastshowtime~=0 then
lastshowtime=lastshowtime-1
if lastshowtime==0 then
clearInfo()
end
end
end

function clearInfo()
if scanmode~=0 then
scanmode=0
LCD.Clear()
LCDTable[1]=""
LCDTable[2]="    等待扫码"
LCDTable[3]=""
LCDTable[4]=""
end
end

function showMenu(index)
LCDTable[1]="  无线扫描枪    "
LCDTable[2]="  列表查看      "
LCDTable[3]="  发送          "
LCDTable[4]="  设置          "
LCDTable[index]='>'..LCDTable[index]:sub(2,16)
end

function showTime(msg,t)
if LCDTable[2]==msg then return end
LCD.Clear()
LCDTable[1]=""
LCDTable[2]=msg
LCDTable[3]=""
LCDTable[4]=""
lastshowtime=t
end

function setInfo(str,c,id)
if id==code and scanmode==1 then
LCDTable[1]=string.format('%16s',str)
maxcount=c
scanmode=2
end
end

function getInfo()
str='ScanInfo:'..scanmode
if scanmode==1 or scanmode==2 then
str=str..','..code..','..count
elseif scanmode==3 then
str=str..','..code..','..count..','..check
end
print(str)
end

tmr.alarm(0, 10, tmr.ALARM_AUTO, Loop)
