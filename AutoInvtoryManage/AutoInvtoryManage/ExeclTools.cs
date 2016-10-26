using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.github.KeyMove.Tools
{
    class ExeclTools : IDisposable
    {
        public Dictionary<string, List<string>> Sheets = new Dictionary<string, List<string>>();
        public Dictionary<string, DataTable> SheetData = new Dictionary<string, DataTable>();
        public Dictionary<string, OleDbDataAdapter> SheetAdapter = new Dictionary<string, OleDbDataAdapter>();
        OleDbConnection connection;
        public ExeclTools(string path)
        {
            var tabs = new OleDbConnection(@"Provider=Microsoft.Jet.OleDb.4.0;Data Source=" + path + ";" +
                    "Extended Properties='Excel 8.0;HDR=YES;IMEX=2';");
            tabs.Open();
            connection = tabs;
            //OleDbCommand cmd = tabs.CreateCommand();
            //cmd.CommandText = "insert into [Sheet3$](名称,数量,库存)values('A1',10,10)";
            //cmd.ExecuteNonQuery();
            var tab = tabs.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { });
            if (tab.Rows.Count < 2) return;
            List<string> a = new List<string>();
            for (int i = 0; i < tab.Rows.Count; i++)
            {
                string v = tab.Rows[i]["TABLE_NAME"].ToString();
                if (!v.EndsWith("_"))
                {
                    Sheets.Add(v, new List<string>());
                    SheetData.Add(v, new DataTable());
                }
            }
            foreach (string s in Sheets.Keys)
            {
                OleDbDataAdapter adapter;
                SheetAdapter.Add(s, adapter = new OleDbDataAdapter(new OleDbCommand("select * from [" + s + "] ", tabs)));
                adapter.Fill(SheetData[s]);
                foreach (DataColumn c in SheetData[s].Columns)
                {
                    Sheets[s].Add(c.ToString());
                }
            }
            GC.Collect();
            connection.Close();
        }

        public int insert(string sheet,string[] data)
        {
            if (Sheets.ContainsKey(sheet))
            {
                List<string> list = Sheets[sheet];
                DataTable dt = SheetData[sheet];
                DataRow dr = dt.NewRow();
                for (int i = 0; i < list.Count; i++)
                    dr[list[i]] = data[i];
                dt.Rows.Add(dr);
                OleDbDataAdapter adapter = SheetAdapter[sheet];
                OleDbCommandBuilder odcb = new OleDbCommandBuilder(adapter);
                odcb.QuotePrefix = "[";
                odcb.QuoteSuffix = "]";
                //adapter.SelectCommand = new OleDbCommand("select * from [" + sheet + "] ", connection);
                adapter.Update(dt);
                connection.Close();
                //StringBuilder sb = new StringBuilder();
                //foreach (string s in Sheets[sheet])
                //{
                //    sb.Append(s);
                //    sb.Append(',');
                //}
                //string title = sb.ToString();
                //sb.Clear();
                //foreach (string s in data)
                //{
                //    sb.Append("'");
                //    sb.Append(s);
                //    sb.Append("',");
                //}
                //string values = sb.ToString();
                //values = values.Substring(0, values.Length - 1);
                //title = title.Substring(0, title.Length - 1);
                //string v = string.Format("insert into [{0}]({1})values({2})", sheet, title, values);
                //OleDbCommand cmd= connection.CreateCommand();
                //cmd.CommandText = v;
                //cmd.ExecuteNonQuery();
                //return new OleDbCommand(v, connection).ExecuteNonQuery();
                //connection.Close();
                ////OleDbCommand c = connection.CreateCommand();
                ////c.CommandText = string.Format("insert into [{0}]({1})values({2})", sheet, title,values);
                ////return c.ExecuteNonQuery();
            }
            return -1;
        }

        public int set(string sheet,int rows,string[] data)
        {
            if (Sheets.ContainsKey(sheet))
            {
                List<string> list = Sheets[sheet];
                DataTable dt = SheetData[sheet];
                DataRow dr = dt.Rows[rows];
                for (int i = 0; i < list.Count; i++)
                    dr[list[i]] = data[i];
                OleDbDataAdapter adapter = SheetAdapter[sheet];
                OleDbCommandBuilder odcb = new OleDbCommandBuilder(adapter);
                odcb.QuotePrefix = "[";
                odcb.QuoteSuffix = "]";
                return adapter.Update(dt);
            }
            return -1;
        }

        public void get(string sheet,string title,int row)
        {

        }

        public void Dispose()
        {
            connection.Close();
        }
    }
}
