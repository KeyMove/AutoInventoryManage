using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace com.github.KeyMove.Tools
{

    enum CheckStatus : int
    {
        CheckWay = 0,
        ConfirmWay = 1,
        ReturnWay = 2,
        PreTurn = 3,
        TurnWay = 4,
        TurnDone = 5,
        Stop = 6,
    };

    enum ReturnStatus : int
    {
        RunStatus=0,
        PathFind=1,
        MapSend=2,
        MapRecv=3,
        PreRunScan=4,
        ScanIDCard=5,
    }


    enum SendCode : int
    {
        PID = 0,
        SetSensorInfo = 1,
        PathRun = 2,
        MotorControl = 3,
        Stop = 4,
        RecvMap = 5,
        SendMap = 6,
        AutoFindPath = 7,
        SetTargetNode = 8,
        SetSensorTH = 9,
        SensorSetZero = 10,
    }

    class CarNetData:NetInvoke
    {
        public UserInfo info;
        public List<int> cmdUse = new List<int>();
        public List<int> cmdID = new List<int>();
        public List<byte[]> cmdData = new List<byte[]>();
        public byte SendID = 0;
        public byte lastRecvID = 0;

        byte[] buildPacket(int id, byte[] data)
        {
            MemoryStream ms = new MemoryStream();
            byte sum = 0;
            ms.WriteByte(0xAA);            
            ms.WriteByte((byte)((data.Length >> 8) & 0xff));
            ms.WriteByte((byte)((data.Length) & 0xff));
            ms.WriteByte((byte)id);
            ms.Write(data, 0, data.Length);
            foreach (byte v in data)
                sum += v;
            ms.WriteByte(sum);
            ms.WriteByte(0x55);
            return ms.ToArray();
        }
        public void SendData(int id,byte[] list)
        {

            int pos = cmdUse.IndexOf(id);
            byte[] senddata = new byte[list.Length + 2];
            senddata[1] = (byte)id;
            for (int i = 0; i < list.Length; i++)
                senddata[i + 2] = list[i];
            if (pos == -1)
            {
                senddata[0] = SendID;
                cmdUse.Add(id);
                cmdID.Add(SendID);
                cmdData.Add(buildPacket(2, senddata));
                SendID++;
            }
            else
            {
                senddata[0] = (byte)(cmdID[pos] = SendID);
                cmdData[pos] = buildPacket(2, senddata);
                SendID++;
            }
        }

        public void RecvData(byte[] data)
        {
            throw new NotImplementedException();
        }

        public void SendLoop()
        {
            throw new NotImplementedException();
        }
    }

    class CarData:CarInfo
    {
        public MapData mapID;
        public IPAddress ip;
        public int ID;

        public CheckStatus status;
        public CarNetData net;
        
        public CarData(IPAddress ip, int id)
        {
            this.ip = ip;
            this.ID = id;
            net = new CarNetData();
        }

        public override string ToString()
        {
            return string.Format("ID:{0}[{1}]", ID, ip.ToString());
        }
    }

    class MapData
    {
        public int mapID;
        public byte[] mapdata;
        public List<int> IDCard;
        public MapDraw Handle;
        public List<CarData> Cars;
        public MapData(int id)
        {
            mapID = id;
            IDCard = new List<int>();
            Cars = new List<CarData>();
        }
        
        public bool CheckPath(CarData car)
        {
            if (Handle == null) return false;
            if (car.TargetNodeID == 255) car.TargetNodeID = car.nowNodeID;
            byte[] usenodesid = Handle.getTargetPointList(Handle[car.TargetNodeID], Handle[car.nowNodeID]);
            if (usenodesid==null) return false;
            foreach(CarData c in Cars)
            {
                if (car == c) continue;
                if (c.TargetNodeID == 255) c.TargetNodeID = c.nowNodeID;
                byte[] pathuse = Handle.getTargetPointList(Handle[c.TargetNodeID], Handle[c.nowNodeID]);
                if (pathuse == null) return false;
                for (int i = 0; i < usenodesid.Length; i++)
                    if ((usenodesid[i] + pathuse[i]) >= 2)
                        return false;
            }
            return true;
        }

        public override string ToString()
        {
            return "地图"+mapID;
        }

    }

    class CarControl
    {

        List<MapData> mapList = new List<MapData>();
        List<CarData> CarList = new List<CarData>();
        public List<CarData> inMapCar = new List<CarData>();
        private int MapCount;

        public void addCar(CarData car)
        {
            CarList.Add(car);
        }
        public void setCarMap(CarData car, MapData map)
        {
            car.mapID = map;
            map.Cars.Add(car);
            inMapCar.Add(car);
        }

        public MapData getMapFormID(int IDCard)
        {
            MapData targetmap = null;
            foreach (MapData map in mapList)
            {
                if (map.IDCard.Contains(IDCard))
                {
                    targetmap = map;
                    break;
                }
            }
            return targetmap;
        }

        public bool setCarMap(int id,int IDCard)
        {
            MapData targetmap=null;
            foreach (MapData map in mapList)
            {
                if (map.IDCard.Contains(IDCard))
                {
                    targetmap = map;
                    break;
                }
            }
            if (targetmap == null) return false;
            foreach(CarData car in CarList)
            {
                if (car.ID == id)
                {
                    if (targetmap.Cars.Contains(car)) return true;
                    car.mapID = targetmap;
                    targetmap.Cars.Add(car);
                    inMapCar.Add(car);
                    return true;
                }
            }
            return false;
        }

        public void removeCar(int id)
        {

        }

        public CarData getCarFormIP(IPAddress targetip)
        {
            foreach(CarData c in CarList)
            {
                if (c.ip.Address == targetip.Address)
                    return c;
            }
            return null;
        }

        public bool CheckPath(CarData car)
        {
            try
            {
                if (car.mapID == null) return false;
                return car.mapID.CheckPath(car);
            }
            catch { return false; }
        }

        public MapData CheckMap(byte[] data)
        {
            foreach(MapData m in mapList)
            {
                int i;
                if (m.mapdata == null) continue;
                if (data.Length<m.mapdata.Length) continue;
                for(i = 0; i < m.mapdata.Length; i++)
                {
                    if (m.mapdata[i] != data[i])
                    {
                        i = 0;
                        break;
                    }
                }
                if (i != 0) return m;
            }
            return null;
        }

        public MapData addMap(MapDraw draw)
        {
            MapData m = new MapData(MapCount++);
            m.Handle = draw;
            m.IDCard.Clear();
            foreach(FlagNode node in draw.FlagNodeList)
            {
                m.IDCard.Add(node.id);
            }
            mapList.Add(m);
            return m;
        }




    }
}
