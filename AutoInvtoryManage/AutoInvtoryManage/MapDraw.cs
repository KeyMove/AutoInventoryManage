using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.github.KeyMove.Tools
{
    public enum PathType : int
    {
        nil=-1,
        Forward = 0,
        Left = 1,
        Right = 2,
        Back = 3,
    }

    class FlagNode
    {
        public int id;
        public int nodeid;

        public FlagNode(int id,int nodeid)
        {
            this.id = id;
            this.nodeid = nodeid;
        }

        public override string ToString()
        {
            return string.Format("ID:0x{0:X8}", id);
        }

    }

    class PathNode
    {
        

        public PathNode[] paths=new PathNode[4];
        public int[] pathlenght = new int[4];
        public int pathCount;
        public int pathID;
        public string pathName;
        public int x, y;
        public int w;
        public object Tag;

        public bool isNew
        {
            get { return pathCount == 0; }
            private set { }
        }
        public bool isDead
        {
            get { return pathCount <= 1; }
            private set { }
        }

        public PathType this[PathNode node]
        {
            get { for (int i = 0; i < paths.Length; i++) if (paths[i] == node) return (PathType)i; return PathType.nil; }
            private set
            {
            }
        }

        public PathNode this[PathType type]
        {
            get { return paths[(int)type]; }
            set {
                paths[(int)type] = value;

                pathCount = 0;
                for (int i = 0; i < this.paths.Length; i++)if(this.paths[i]!=null) pathCount++;
            }
        }

        public void update()
        {
            pathCount = 0;
            for (int i = 0; i < this.paths.Length; i++) if (this.paths[i] != null) pathCount++;
        }

        public PathNode(int id):this(id,"节点"+id)
        {

        }

        public PathNode(int id,string name)
        {
            pathID = id;
            pathName = name;
        }

        public void addPath(PathNode node,PathType dir)
        {
            this.paths[(int)dir] = node;
            pathCount = 0;
            for (int i = 0; i < this.paths.Length; i++) pathCount++;
        }



        public List<PathNode> gotoNode(PathNode node,List<PathNode> list=null)
        {
            List<PathNode> nodes=list;
            if (nodes == null)
            {
                nodes = new List<PathNode>();
            }
            for (int i = 0; i < paths.Length; i++)
            {
                if (this.paths[i] != null)
                {
                    if (this.paths[i] == node)
                    {
                        nodes.Add(this);
                        nodes.Add(this.paths[i]);
                        return nodes;
                    }
                    if (this.paths[i].isDead) continue;
                    if (nodes.Contains(this.paths[i])) continue;
                    nodes.Add(this);
                    if (this.paths[i].gotoNode(node, nodes) != null)
                        return nodes;
                    nodes.Remove(this);
                }
            }
            return null;
        }

        public List<PathNode> getEndNode(List<PathNode> list = null, List<PathNode> point = null)
        {
            List<PathNode> nodes = list;
            if (nodes == null)
            {
                nodes = new List<PathNode>();
                point = new List<PathNode>();
                //nodes.Add(this);
            }
            for (int i = 0; i < paths.Length; i++)
            {
                if (this.paths[i] != null)
                {
                    if (this.paths[i].isDead)
                    {
                        point.Add(this.paths[i]);
                        continue;
                    }
                    if (nodes.Contains(this.paths[i])) continue;
                    nodes.Add(this);
                    this.paths[i].getEndNode(nodes, point);
                    //nodes.Remove(this);
                }
            }
            return point;
        }

        public List<PathNode> getAllNode(List<PathNode> list = null)
        {
            List<PathNode> nodes = list;
            if (nodes == null)
            {
                nodes = new List<PathNode>();
            }
            for (int i = 0; i < paths.Length; i++)
            {
                if (this.paths[i] != null)
                {
                    if (nodes.Contains(this.paths[i])) continue;
                    nodes.Add(this.paths[i]);
                    this.paths[i].getAllNode(nodes);
                }
            }
            return nodes;
        }

        public override string ToString()
        {
            return pathName + ((Tag != null) ?" - ["+ Tag.ToString()+"]" : "");
        }

    }

    class PathDir
    {
        public PathType Forward = PathType.Forward;
        public PathType Left = PathType.Left;
        public PathType Right = PathType.Right;
        public PathType Back = PathType.Back;

        public PathDir()
        {

        }

        public PathDir(PathType type)
        {
            Rotate(type);
        }

        public PathDir(PathType f, PathType l, PathType r, PathType b)
        {
            Forward = f;
            Left = l;
            Right = r;
            Back = b;
        }

        public PathType this[PathType type]
        {
            get
            {
                switch (type)
                {
                    case PathType.Forward: return Forward;
                    case PathType.Left: return Left;
                    case PathType.Right: return Right;
                    case PathType.Back: return Back;
                }
                return PathType.nil;
            }
            private set { }
        }
        public PathType getDir(PathType type)
        {
            if (Forward == type) return PathType.Forward;
            if (Left == type) return PathType.Left;
            if (Right == type) return PathType.Right;
            if (Back == type) return PathType.Back;
            return PathType.nil;
        }

        public void Reset()
        {
            Forward = PathType.Forward;
            Left = PathType.Left;
            Right = PathType.Right;
            Back = PathType.Back;
        }
        public void Rotate(PathType type)
        {
            PathType t;
            switch (type)
            {
                case PathType.Forward: break;
                case PathType.Left:
                    t = Forward;
                    Forward = Left;
                    Left = Back;
                    Back = Right;
                    Right = t;
                    break;
                case PathType.Right:
                    t = Left;
                    Left = Forward;
                    Forward = Right;
                    Right = Back;
                    Back = t;
                    break;
                case PathType.Back:
                    t = Left;
                    Left = Forward;
                    Forward = Right;
                    Right = Back;
                    Back = t;
                    t = Left;
                    Left = Forward;
                    Forward = Right;
                    Right = Back;
                    Back = t;
                    break;
            }
        }

        public PathDir clone()
        {
            return new PathDir(this.Forward,this.Left,this.Right,this.Back);
        }

    }

    class CarInfo
    {
        public int WayStatus;
        public int StartNodeID;
        public int TargetNodeID;
        public int lastNodeID;
        public int nowNodeID;
        public int lastLenght;
        public int pross;
    }

    class MapDraw
    {
        Bitmap map;
        Graphics Draw;
        Pen norpen = new Pen(Color.Gray,5);
        Brush norBrush = Brushes.Gray;
        Brush deadBrush = Brushes.Black;
        Pen newpen = new Pen(Color.SkyBlue, 3);
        Brush targetBrush = Brushes.Red;
        Pen PathPen = new Pen(Color.GreenYellow, 1);
        Pen OtherPathPen = new Pen(Color.SkyBlue, 1);
        Brush CarBrush = Brushes.Orange;
        Brush OtherCarBrush= Brushes.SkyBlue;

        List<PathNode> TargetPath = new List<PathNode>();
        List<PathType> TargetDirList = new List<PathType>();
        List<PathNode> NodeList=new List<PathNode>();
        public List<CarInfo> CarList=new List<CarInfo>();
        List<PathNode> EndPathList;
        PathDir findDir=new PathDir();
        Font strFont = new Font("宋体", 9);    
        int xpos, ypos;
        int width=10;
        public List<PathNode> allPath = new List<PathNode>();

        public double Scale=1;
        public double DrawScale
        {
            get { return Scale; }
            set
            {
                Scale = value;
                if (MapSize != null)
                {
                    xpos = (int)(map.Width  / 2 - MapSize.Width * Scale / 2 + MapSize.X * Scale);
                    ypos = (int)(map.Height  / 2 - MapSize.Height * Scale / 2 + MapSize.Y * Scale);
                }
            }
        }
        int nodescount = 0;
        PathNode nodes;

        PathNode TargetNode;
        PathNode lastNode;
        PathType lastPath;
        bool TurnBack = false;
        bool isOver = false;
        PathDir LastDir=new PathDir();

        PathDir LastPathDirValue = new PathDir();
        public PathDir LastPathDir
        {
            get { return LastPathDirValue.clone(); }
            set
            {
                if (value != null)
                    LastPathDirValue = value.clone();
            }
        }

        public int NodeCount
        {
            get { return allPath.Count; }
        }

        public PathNode this[int index]
        {
            get { if (index < allPath.Count)  return allPath[index]; return null; }
        }

        public bool WayLenght { get; set; }

        Stack<PathNode> searchStack = new Stack<PathNode>();
        private int lastlenght;
        public List<FlagNode> FlagNodeList = new List<FlagNode>();
        private Rectangle MapSize;
        public CarInfo SelectCar;

        public MapDraw(int w,int h)
        {
            map = new Bitmap(w, h);
            Draw = Graphics.FromImage(map);
            Draw.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            xpos = map.Width / 2;
            ypos = map.Height / 2;
        }

        void DrawTargetPath()
        {
            if (TargetDirList.Count == 0) return;
            PathNode node=null;
            foreach(PathNode n in TargetPath)
            {
                if(node==null)
                {
                    node = n;
                    continue;
                }
                Draw.DrawLine(PathPen, node.x, node.y, n.x, n.y);
                node = n;
            }
        }        

        //public List<PathNode> getAllNode(List<PathNode> list = null)
        //{
        //    List<PathNode> nodes = list;
        //    if (nodes == null)
        //    {
        //        nodes = new List<PathNode>();
        //    }
        //    for (int i = 0; i < paths.Length; i++)
        //    {
        //        if (this.paths[i] != null)
        //        {
        //            if (nodes.Contains(this.paths[i])) continue;
        //            nodes.Add(this.paths[i]);
        //            this.paths[i].getAllNode(nodes);
        //        }
        //    }
        //    return nodes;
        //}
        List<PathNode> DrawPathList(int x,int y,PathNode node,List<PathNode> pathlist=null, List<Point> ps=null)
        {
            List<PathNode> nodes = pathlist;
            List<Point> Points=ps;
            bool f = false;
            if (nodes == null)
            {
                f = true;
                Points = new List<Point>();
                nodes = new List<PathNode>();
            }
            int ax=0, ay=0;
            int w;
            for (int i = 0; i < node.paths.Length; i++)
            {
                if (node.paths[i] != null)
                {
                    if (nodes.Contains(node.paths[i])) continue;
                    if ((w = node.pathlenght[i]) == 0) w = width;
                    w = (int)(w * DrawScale);
                    switch ((PathType)i)
                    {
                        case PathType.Forward:
                            ax = 0;
                            ay = -w;
                            break;
                        case PathType.Left:
                            ax = -w;
                            ay = 0;
                            break;
                        case PathType.Right:
                            ax = w;
                            ay = 0;
                            break;
                        case PathType.Back:
                            ax = 0;
                            ay = w;
                            break;
                    }
                    node.paths[i].x = x + ax;
                    node.paths[i].y = y + ay;
                    if (!node.paths[i].isDead)
                    {
                        Draw.DrawLine(norpen, x, y, x + ax, y + ay);
                        Draw.FillEllipse(norBrush, new Rectangle(x + ax - 2, y + ay - 2, 4, 4));
                        //if (lastNode == node.paths[i])
                        //    Draw.DrawEllipse(targetpen, new Rectangle(x + ax - 5, y + ay - 5, 10, 10));
                    }
                    else if (node.paths[i].isNew)
                    {
                        Draw.DrawLine(newpen, x, y, x + ax, y + ay);
                        //node.paths[i].x = x + ax;
                        //node.paths[i].y = y + ay;
                    }
                    else
                    {
                        Draw.DrawLine(norpen, x, y, x + ax, y + ay);
                        //Draw.DrawEllipse(deadpen, new Rectangle(x + ax - 5, y + ay - 5, 10, 10));
                        //if (lastNode == node.paths[i])
                        //    Draw.DrawEllipse(targetpen, new Rectangle(x + ax - 5, y + ay - 5, 10, 10));
                        Draw.DrawString(node.paths[i].pathName, strFont, Brushes.Black, x + ax + 5, y + ay + 5);
                        //node.paths[i].x = x + ax;
                        //node.paths[i].y = y + ay;
                        Points.Add(new Point(node.paths[i].x, node.paths[i].y));
                    }
                    nodes.Add(node.paths[i]);
                    DrawPathList(x + ax, y + ay, node.paths[i], nodes, Points);
                }
            }
            if (f)
            {
                foreach(Point p in Points)
                {
                    Draw.FillRectangle(deadBrush,p.X-8,p.Y-8,16,16);
                }
                Draw.FillEllipse(targetBrush, new Rectangle(lastNode.x - 5, lastNode.y - 5, 10, 10));
            }
            return nodes;
        }

        public void LinkNode(PathNode snode,PathNode dnode,PathType dir)
        {
            PathDir d = new PathDir(dir);
            snode[d.Forward] = dnode;
            dnode[d.Back] = snode;
        }

        public List<PathNode> getEndPoint()
        {
            //if (EndPathList == null)
                UpdateEndPoint();
            return EndPathList;
        }

        public Bitmap Update()
        {
            Draw.Clear(Color.White);
            NodeList.Clear();
            DrawPathList(xpos, ypos, nodes);
            DrawTargetPath();
            foreach(CarInfo info in CarList)
            {
                updateCar(info);
            }
            //updateCar((int)(lastlenght*Scale));
            //DrawPath(xpos,ypos,nodes);
            //findDir.Rotate(PathType.Back);
            //DrawPath(nodes[PathType.Forward].x, nodes[PathType.Forward].y, nodes[PathType.Forward],PathType.Back);
            findDir.Reset();
            return map;
        }

        public bool MoveToPoint(PathNode node)
        {
            if (node.isNew || !node.isDead) return false;
            PathDir dir = new PathDir();
            for(int i=0;i< node.paths.Length;i++)
                if(node.paths[i]!=null)
                {
                    dir.Rotate((PathType)i);
                    break;
                }
            dir.Rotate(PathType.Back);
            LastPathDir = dir;
            lastNode = node;
            return true;
        }


        public List<PathNode> getLastTargetPath()
        {
            return TargetPath;
        }

        public bool setTargetPoint(PathNode node,PathNode lastnode=null)
        {
            if (node == null) return false;
            if (lastnode == null) lastnode = lastNode;
            TargetDirList.Clear();
            if (lastNode == null) return false;
            if (node.isNew || !node.isDead) return false;
            if (lastNode == node) return false;
            TargetPath = lastnode.gotoNode(node);
            if (TargetPath == null) return false;
            PathDir dir = LastPathDir;
            PathNode nt = null;
            foreach (PathNode n in TargetPath)
            {
                PathType type;
                if (nt == null)
                {
                    nt = n;
                    continue;
                }
                type = dir.getDir(nt[n]);
                dir.Rotate(type);
                TargetDirList.Add(type);
                nt = n;
            }
            return true;
        }

        public void clearTargetPathList()
        {
            TargetDirList.Clear();
        }

        public byte[] getTargetPointList(PathNode TargetNode, PathNode StartNode)
        {
            if (StartNode == null) return null;
            if (TargetNode.isNew || !TargetNode.isDead) return null;
            byte[] array = new byte[allPath.Count];
            if (StartNode == TargetNode)
            {
                array[StartNode.pathID]++;
                return array;
            }
            List<PathNode> nodes = StartNode.gotoNode(TargetNode);
            if (nodes == null) return null;
            
            foreach (PathNode node in nodes)
                array[node.pathID]++;
            return array;
        }

        public List<PathType> getTargetPath()
        {
            return TargetDirList;
        }

        public PathNode getNewPathNode()
        {
            PathNode node = new PathNode(nodescount++);
            allPath.Add(node);
            return node;
        }

        void ClearAllPath()
        {
            allPath.Clear();
            nodescount = 0;
        }

        public void SearchInit()
        {
            ClearAllPath();
            isOver = false;
            TargetPath.Clear();
            TargetNode = nodes = getNewPathNode();
            searchStack.Clear();
            searchStack.Push(nodes);
            nodes[PathType.Forward] = getNewPathNode();
            nodes[PathType.Forward][PathType.Back] = nodes;
            lastNode = nodes[PathType.Forward];
            LastDir.Reset();
            LastPathDirValue.Reset();
            setCarNode(nodes);
            TurnBack = false;
            WayLenght = true;
        }


        public void setCarNode(PathNode d,PathNode s=null)
        {

            if (d == s)
                s = null;
            if (s != null)
                TargetNode = s;
            else
                TargetNode = lastNode;
            lastNode = d;
            LastDir.Reset();
            PathType pt = TargetNode[d];
            //LastDir.Rotate(PathType.Back);
            LastDir.Rotate(pt);
        }

        void updateCar(CarInfo info,Pen drawpen=null)
        {            
            int angle=0;
            int ax = 0, ay = 0;
            int w = (int)(info.lastLenght * Scale * info.pross / 100);
            if (drawpen == null) drawpen = OtherPathPen;
            PathDir pdir = new PathDir();
            if (info.lastNodeID >= allPath.Count || info.nowNodeID >= allPath.Count) return;
            PathNode node = allPath[info.lastNodeID];
            pdir.Rotate(node[allPath[info.nowNodeID]]);
            switch (pdir.Forward)
            {
                case PathType.Forward:
                    ax = 0;
                    ay = -w;
                    angle = 0;
                    break;
                case PathType.Left:
                    ax = -w;
                    ay = 0;
                    angle = -90;
                    break;
                case PathType.Right:
                    ax = w;
                    ay = 0;
                    angle = 90;
                    break;
                case PathType.Back:
                    ax = 0;
                    ay = w;
                    angle = 180;
                    break;
            }
            drawCar(node.x+ax, node.y+ay, angle,(info==SelectCar?CarBrush:OtherCarBrush));
        }

        void drawCar(int x,int y,int angle,Brush b)
        {
            Draw.FillEllipse(b, new Rectangle(x - 8, y - 8, 16, 16));
            Point[] ps = new Point[4];
            double dy = Math.Sin((angle - 180) * Math.PI / 180) * 8 + y;
            double dx = Math.Cos((angle - 180) * Math.PI / 180) * 8 + x;
            ps[0].X = (int)dx;
            ps[0].Y = (int)dy;
            dy = Math.Sin((angle - 90) * Math.PI / 180) * 14 + y;
            dx = Math.Cos((angle - 90) * Math.PI / 180) * 14 + x;
            ps[1].X = (int)dx;
            ps[1].Y = (int)dy;
            dy = Math.Sin((angle) * Math.PI / 180) * 8 + y;
            dx = Math.Cos((angle) * Math.PI / 180) * 8 + x;
            ps[2].X = (int)dx;
            ps[2].Y = (int)dy;

            ps[3].X = x;
            ps[3].Y = y;
            Draw.FillPolygon(b, ps);
            Draw.FillEllipse(Brushes.White, new Rectangle(x - 3, y - 3, 6, 6));
        }

        public Rectangle getMapSize()
        {
            if (nodes == null) return new Rectangle(0,0,0,0);
            Point size = new Point(0,0);
            Point s = new Point();
            Stack<PathNode> backstack = new Stack<PathNode>();
            List<PathNode> nodeslist = new List<PathNode>();
            Stack<Point> sizelist = new Stack<Point>();
            int minx=0, miny=0, maxx=0, maxy=0;
            int x=0, y=0;
            int ax=0, ay=0;
            int w;
            nodeslist.Add(nodes);
            PathNode node = nodes;
            for (int i = 0; i < 4; i++)
            {
                if (nodeslist.Contains(node.paths[i])|| node.paths[i] == null)
                {
                    if (i == 3)
                        if (backstack.Count != 0)
                        {
                            i = 0;
                            node = backstack.Pop();
                            size = sizelist.Pop();
                            x = size.X;
                            y = size.Y;
                            continue;
                        }
                        else
                        {
                            minx = -minx;
                            miny = -miny;
                            size.X = maxx + minx;
                            size.Y = maxy + miny;
                            s.X = minx;
                            s.Y = miny;
                        }
                    continue;
                }
                if ((w = node.pathlenght[i]) == 0) w = width;
                switch ((PathType)i)
                {
                    case PathType.Forward:
                        ax = 0;
                        ay = -w;
                        break;
                    case PathType.Left:
                        ax = -w;
                        ay = 0;
                        break;
                    case PathType.Right:
                        ax = w;
                        ay = 0;
                        break;
                    case PathType.Back:
                        ax = 0;
                        ay = w;
                        break;
                }
                if (maxx < (x + ax)) maxx = x + ax;
                if (maxy < (y + ay)) maxy = y + ay;
                if (minx > (x + ax)) minx = x + ax;
                if (miny > (y + ay)) miny = y + ay;
                nodeslist.Add(node.paths[i]);
                sizelist.Push(new Point(x,y));
                backstack.Push(node);
                node = node.paths[i];
                x += ax;
                y += ay;
                i = -1;
            }
            return new Rectangle(s.X,s.Y,size.X,size.Y);
        }

        public void AutoOffset()
        {
            MapSize = getMapSize();
            xpos = (int)(map.Width*Scale / 2 - MapSize.Width / 2 + MapSize.X);
            ypos = (int)(map.Height*Scale / 2 - MapSize.Height / 2+ MapSize.Y);
        }

        public void SetLastPathLenght(int lenght)
        {
            if(WayLenght)
            { lastNode.pathlenght[(int)LastDir.Back]=TargetNode.pathlenght[(int)LastDir.Forward] = lenght; }
            lastlenght = lenght> lastNode.pathlenght[(int)LastDir.Back]? lastNode.pathlenght[(int)LastDir.Back]:lenght;
        }

        public void SetCarPathPos(PathNode node, int pross, PathType dir)
        {
            if (pross > 100) pross = 100;
            if (pross < 0) pross = 0;
            lastlenght = node.pathlenght[(int)dir] * pross / 100;
        }

        public int GetLastPathLenght()
        {
            return lastNode.pathlenght[(int)LastDir.Back];
        }

        public int getlastTargetLenght()
        {
            return TargetNode.pathlenght[(int)TargetNode[lastNode]];
        }

        public void SetLastPathTag(object obj)
        {
            lastNode.Tag = obj;
        }

        public int SearchCheck(int lastPathSelect)
        {
            int PathOut = -1;
            WayLenght = false;
            if (!TurnBack)
            {
                lastPath = (PathType)PathOut;
                if ((lastPathSelect & 2) != 0)
                {
                    lastNode[LastDir.Left] = getNewPathNode();
                    PathOut = (int)PathType.Left;
                }
                if ((lastPathSelect & 1) != 0)
                {
                    lastNode[LastDir.Forward] = getNewPathNode();
                    PathOut = (int)PathType.Forward;
                }
                if ((lastPathSelect & 4) != 0)
                {
                    lastNode[LastDir.Right] = getNewPathNode();
                    PathOut = (int)PathType.Right;
                }
                if (PathOut < 0)
                {
                    TargetNode = lastNode;
                    lastNode = searchStack.Pop();
                    PathOut = (int)PathType.Back;
                    LastDir.Rotate(PathType.Back);
                    TurnBack = true;
                }
                else
                {
                    WayLenght = true;
                    searchStack.Push(lastNode);
                    lastPath = LastDir[(PathType)PathOut];
                    LastDir.Rotate((PathType)PathOut);
                    lastNode[lastPath][LastDir.Back] = lastNode;
                    TargetNode = lastNode;
                    lastNode = lastNode[lastPath];
                }
            }
            else
            {
                PathOut = -1;
                if ((lastPathSelect & 2) != 0)
                {
                    if (lastNode[LastDir.Left] != null)
                        if (lastNode[LastDir.Left].isNew)
                            PathOut = (int)PathType.Left;
                }
                if ((lastPathSelect & 1) != 0)
                {
                    if (lastNode[LastDir.Forward] != null)
                        if (lastNode[LastDir.Forward].isNew)
                            PathOut = (int)PathType.Forward;
                }
                if ((lastPathSelect & 4) != 0)
                {
                    if (lastNode[LastDir.Right] != null)
                        if (lastNode[LastDir.Right].isNew)
                            PathOut = (int)PathType.Right;
                }
                if (PathOut < 0)
                {
                    if (searchStack.Count > 1)
                    {
                        TargetNode = lastNode;
                        lastPath = lastNode[searchStack.Pop()];
                        PathOut = (int)LastDir.getDir(lastPath);
                        lastNode = lastNode[lastPath];
                        LastDir.Rotate((PathType)PathOut);
                    }
                    else
                    {
                        if (!isOver)
                        {
                            isOver = true;
                        }
                        else
                        {
                            UpdateEndPoint();
                            lastlenght = 0;
                            TargetNode = nodes[PathType.Forward];
                            return -1;
                        }
                        if (lastNode[PathType.Back].isDead)
                        {
                            searchStack.Push(lastNode);
                            PathOut = (int)LastDir.getDir(lastNode[lastNode[PathType.Back]]);
                            LastDir.Rotate((PathType)PathOut);
                            TargetNode = lastNode;
                            lastNode = lastNode[PathType.Back];
                            TurnBack = false;
                        }
                        
                    }
                }
                else
                {
                    TargetNode = lastNode;
                    searchStack.Push(lastNode);
                    lastPath = LastDir[(PathType)PathOut];
                    LastDir.Rotate((PathType)PathOut);
                    lastNode[lastPath][LastDir.Back] = lastNode;
                    lastNode = lastNode[lastPath];
                    TurnBack = false;
                    WayLenght = true;
                }
            }
            return PathOut;
        }

        public bool isNull()
        {
            return nodes == null;
        }

        public void UpdateEndPoint()
        {
            EndPathList = nodes.getEndNode();
        }

        public void setNode(PathNode node)
        {
            nodes = node;
            UpdateEndPoint();
            lastNode=node;
            TargetNode = lastNode;
            LastDir.Reset();
        }
        public Stream toBin(Stream s)
        {
            allPath = nodes.getAllNode();
            int type = allPath.Count;
            int index;
            allPath.Sort((PathNode a, PathNode b) => { return a.pathID > b.pathID ? 1 : a.pathID == b.pathID ? 0 : -1; });
            s.WriteByte((byte)FlagNodeList.Count);
            s.WriteByte((byte)type);
            foreach (PathNode node in allPath)
            {
                index = node.pathID;
                //s.WriteByte((byte)index);
                for (int i = 0; i < node.paths.Length; i++)
                {
                    index = node.paths[i] == null ? 0xff : node.paths[i].pathID;
                    s.WriteByte((byte)index);
                    index = node.pathlenght[i];
                    s.WriteByte((byte)index);
                }
            }
            foreach(FlagNode flag in FlagNodeList)
            {
                s.WriteByte((byte)flag.nodeid);
                s.WriteByte((byte)(flag.id >> 24));
                s.WriteByte((byte)(flag.id >> 16));
                s.WriteByte((byte)(flag.id >> 8));
                s.WriteByte((byte)(flag.id));
            }
            return s;
        }

        public PathNode toNode(Stream s)
        {
            PathNode node;
            allPath.Clear();
            FlagNodeList.Clear();
            int index;
            int flaglen = s.ReadByte();
            int len = s.ReadByte();
            for (int i = 0; i < len; i++)
            {
                allPath.Add(new PathNode(i));
            }
            for (int i = 0; i < len; i++)
            {
                node = allPath[i];
                //node.pathID = (s.ReadByte());
                node.pathID = i;
                for (int j = 0; j < node.paths.Length; j++)
                {
                    index = (s.ReadByte());
                    if (index != (-1 & 0xff))
                        node[(PathType)j] = allPath[index];
                    node.pathlenght[j] = (s.ReadByte());
                }
            }
            for(int i = 0; i < flaglen; i++)
            {
                index = s.ReadByte();
                int id = (byte)s.ReadByte(); id <<= 8;
                id |= (byte)s.ReadByte(); id <<= 8;
                id |= (byte)s.ReadByte(); id <<= 8;
                id |= (byte)s.ReadByte();
                FlagNode flag = new FlagNode(id, index);
                FlagNodeList.Add(flag);
                allPath[index].Tag = flag;
            }
            return allPath[0];
        }
    }
}
