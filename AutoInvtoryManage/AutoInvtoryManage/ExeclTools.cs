using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Text;

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
                OleDbCommandBuilder odcb = new OleDbCommandBuilder(adapter);
                odcb.QuotePrefix = "[";
                odcb.QuoteSuffix = "]";
                adapter.Fill(SheetData[s]);
                foreach (DataColumn c in SheetData[s].Columns)
                {
                    Sheets[s].Add(c.ToString());
                }
            }
            GC.Collect();
            connection.Close();
        }

        public DataRow insert(string sheet,string[] data)
        {
            if (Sheets.ContainsKey(sheet))
            {
                List<string> list = Sheets[sheet];
                DataTable dt = SheetData[sheet];
                DataRow dr = dt.NewRow();
                for (int i = 0; i < list.Count; i++)
                    dr[list[i]] = data[i];
                dt.Rows.Add(dr);
                //connection.Open();
                SheetAdapter[sheet].Update(dt);
                return dr;
                //adapter.SelectCommand = new OleDbCommand("select * from [" + sheet + "] ", connection);
                //connection.Close();
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
            return null;
        }

        public int set(string sheet,int rows,string name,string data)
        {
            if (Sheets.ContainsKey(sheet))
            {
                DataTable dt = SheetData[sheet];
                DataRow dr = dt.Rows[rows];
                dr[name] = data;
                OleDbDataAdapter adapter = SheetAdapter[sheet];
                string sqlstr = string.Format("UPDATE [{0}] SET {1} = {2} WHERE {3} = '{4}'", sheet, name, data, dt.Columns[0].ToString(), dr.ItemArray[0].ToString());
                adapter.UpdateCommand = new OleDbCommand(sqlstr, connection);
                return adapter.Update(dt);
            }
            return -1;
        }

        public int set(string sheet, DataRow dr,string colname, string name, string data)
        {
            if (Sheets.ContainsKey(sheet))
            {
                DataTable dt = SheetData[sheet];
                dr[name] = data;
                OleDbDataAdapter adapter = SheetAdapter[sheet];
                string sqlstr = string.Format("UPDATE [{0}] SET {1} = '{2}' WHERE {3} = '{4}'", sheet, name, data,colname,dr[colname].ToString());
                adapter.UpdateCommand = new OleDbCommand(sqlstr, connection);
                return adapter.Update(dt);
            }
            return -1;
        }

        public int set(string sheet, DataRow dr, string colname, string[] data)
        {
            if (Sheets.ContainsKey(sheet))
            {
                DataTable dt = SheetData[sheet];
                OleDbDataAdapter adapter = SheetAdapter[sheet];
                StringBuilder sb = new StringBuilder();
                int len = dt.Columns.Count;
                string colvalue = dr[colname].ToString();
                for (int i = 0; i < len; i++)
                {
                    dr[dt.Columns[i]] = data[i];
                    sb.Append(dt.Columns[i].ToString());
                    sb.Append("='");
                    sb.Append(data[i]);
                    sb.Append("'");
                    if ((len - 1) != i)
                        sb.Append(',');
                }
                string sqlstr = string.Format("UPDATE [{0}] SET {1} WHERE {2} = '{3}'", sheet, sb.ToString(), colname, colvalue);
                adapter.UpdateCommand = new OleDbCommand(sqlstr, connection);
                return adapter.Update(dt);
            }
            return -1;
        }

        //public int set(string sheet, int rows, string name, string data)
        //{
        //    if (Sheets.ContainsKey(sheet))
        //    {
        //        DataTable dt = SheetData[sheet];
        //        DataRow dr = dt.Rows[rows];
        //        dr[name] = data;
        //        OleDbDataAdapter adapter = SheetAdapter[sheet];
        //        string sqlstr = string.Format("UPDATE [{0}] SET {1} = {2} WHERE {3} = '{4}'", sheet, name, data, dt.Columns[0].ToString(), dr.ItemArray[0].ToString());
        //        adapter.UpdateCommand = new OleDbCommand(sqlstr, connection);
        //        return adapter.Update(dt);
        //    }
        //    return -1;
        //}

        public int remove(string sheet,int rows,string colname)
        {
            if (Sheets.ContainsKey(sheet))
            {
                DataTable dt = SheetData[sheet];
                DataRow dr = dt.Rows[rows];
                OleDbDataAdapter adapter = SheetAdapter[sheet];
                StringBuilder sb = new StringBuilder();
                int len = dt.Columns.Count;
                string colvalue = dr[colname].ToString();
                for (int i = 0; i < len; i++)
                {
                    dr[dt.Columns[i]] = null;
                    sb.Append(dt.Columns[i].ToString());
                    sb.Append("=NULL");
                    if((len-1)!=i)
                        sb.Append(',');
                }
                string sqlstr = string.Format("UPDATE [{0}] SET {1} WHERE {2} = '{3}'", sheet, sb.ToString(), colname, colvalue);
                adapter.UpdateCommand = new OleDbCommand(sqlstr, connection);
                return adapter.Update(dt);
            }
            return -1;
        }

        public DataRow[] find(string sheet,string colname,string name)
        {
            try
            {
                if (Sheets.ContainsKey(sheet))
                {
                    return SheetData[sheet].Select("[" + colname + "] like '" + name + "'");
                }
            }
            catch { }
            return null;
        }

        public DataRow[] find(string sheet, string colname, string name,string sortname)
        {
            if (Sheets.ContainsKey(sheet))
            {
                return SheetData[sheet].Select("[" + colname + "] like '" + name + "'",sortname+" ASC");
            }
            return null;
        }

        public void get(string sheet,string title,int row)
        {

        }

        public void update(string sheet)
        {
            SheetAdapter[sheet].Update(SheetData[sheet]);
        }

        public void update(string sheet,DataTable dt)
        {
            DataTable ndt = dt.Copy();
            SheetData[sheet] = ndt;
            SheetAdapter[sheet].Update(ndt);
        }

        public void Dispose()
        {
            connection.Close();
        }
    }
}
