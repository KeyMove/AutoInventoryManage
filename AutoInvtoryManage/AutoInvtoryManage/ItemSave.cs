using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;

namespace AutoInvtoryManage
{
    class ItemSave
    {
        string 物料代码;
        bool 先进先出;
        List<ItemInfo> 物料信息;
        Dictionary<string, ItemIO> 物料链接;
        public ItemSave(string 物料代码, string 物料名称, string 规格型号, string 单位, int 数量, string 储存位置, string 时间)
        {
            this.物料代码 = 物料代码;
            this.物料信息 = new List<ItemInfo>();
            ItemInfo info = new ItemInfo(物料名称, 规格型号, 单位, 数量, 储存位置, 时间);
            info.P = this;
            this.物料信息.Add(info);
            物料链接 = new Dictionary<string, ItemIO>();
        }

        public void AddInfo(string 物料名称, string 规格型号, string 单位, int 数量, string 储存位置, string 时间)
        {

            if (物料链接.ContainsKey(物料名称))
            {

            }
            else
            {
                ItemInfo info = new ItemInfo(物料名称, 规格型号, 单位, 数量, 储存位置, 时间);
                info.P = this;
                this.物料信息.Add(info);
            }
        }

        public void In()
        {

        }
        public void Out()
        {

        }

        class ItemInfo
        {
            public ItemInfo(string 物料名称, string 规格型号, string 单位, int 数量, string 储存位置, string 时间)
            {
                this.物料名称 = 物料名称;
                this.规格型号 = 规格型号;
                this.单位 = 单位;
                this.总数量 = 数量;
                this.储存信息 = new List<ItemIO>();
                ItemIO item = new ItemIO(储存位置, 数量, 时间);
                item.P = this;
                this.储存信息.Add(item);
            }
            public ItemSave P { get; set; }
            string 物料名称;
            string 规格型号;
            string 单位;
            int 总数量;
            List<ItemIO> 储存信息;
        }
        class ItemIO
        {
            public ItemIO(string 储存位置,int 数量,string 时间)
            {
                this.储存位置 = 储存位置;
                this.数量 = 数量;
                this.时间 = 时间;
            }
            public ItemInfo P { get; set; }
            string 储存位置;
            int 数量;
            string 时间;
        }

        public void toStream(Stream data)
        {
            XmlSerializer bf = new XmlSerializer(typeof(ItemSave));
            bf.Serialize(data, this);
        }

        public static ItemSave toItem(Stream data)
        {
            XmlSerializer bf = new XmlSerializer(typeof(ItemSave));
            ItemSave item = (ItemSave)bf.Deserialize(data);
            return item;
        }
    }
}
