using AF.Common;
using AF.Data.DB;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AutoFrame
{
    public class SqlLiteHelperEx : SQLiteHelper
    {
		private object objLock = new object();
		public SqlLiteHelperEx(string strServerPath = "D:\\test.db") : base(strServerPath)
        {

        }

		protected string GetConnectionString(string strServerPath)
		{
			SQLiteConnectionStringBuilder connectionString = new SQLiteConnectionStringBuilder();
			connectionString.DataSource = strServerPath;
			return connectionString.ToString();
		}

		/// <summary>
		/// 执行非查询的数据库操作
		/// </summary>
		/// <param name="sqlString">要执行的sql语句</param>
		/// <returns>返回受影响的条数</returns>
		public int ExecuteNonQuery(string sqlString)
		{
			lock (objLock)
			{
				try
				{
					using (SQLiteConnection conn = new SQLiteConnection(base.ConnectionString))
					{
						conn.Open();
						using (SQLiteCommand cmd = conn.CreateCommand())
                        {
							cmd.CommandText = sqlString;
							return cmd.ExecuteNonQuery();
						}							
					}
					
				}
				catch (Exception ex)
				{
					EventMgr.Warn(ex.Message);
					return -1;
				}
			}
		}

		/// <summary>
		/// 执行查询并返回查询结果第一行第一列
		/// </summary>
		/// <param name="sqlString">SQL语句</param>
		/// <returns></returns>
		//protected override object ExecuteScalar(string sqlString)
		//{
		//	lock (objLock)
		//	{
		//		try
		//		{
		//			using SQLiteConnection conn = new SQLiteConnection(base.ConnectionString);
		//			conn.Open();
		//			using SQLiteCommand cmd = conn.CreateCommand();
		//			cmd.CommandText = sqlString;
		//			return cmd.ExecuteScalar();
		//		}
		//		catch (Exception ex)
		//		{
		//			EventMgr.Warn(ex.Message);
		//			return null;
		//		}
		//	}
		//}

		/// <summary>
		/// 查询多条数据
		/// </summary>
		/// <param name="sqlString">SQL语句</param>
		/// <returns>返回查询的数据表</returns>
		//protected override DataTable ExecuteDataTable(string sqlString)
		//{
		//	lock (objLock)
		//	{
		//		try
		//		{
		//			using SQLiteConnection conn = new SQLiteConnection(base.ConnectionString);
		//			conn.Open();
		//			using SQLiteCommand cmd = conn.CreateCommand();
		//			cmd.CommandText = sqlString;
		//			DataSet ds = new DataSet();
		//			SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd);
		//			adapter.Fill(ds);
		//			conn.Close();
		//			return ds.Tables[0];
		//		}
		//		catch (Exception ex)
		//		{
		//			EventMgr.Warn(ex.Message);
		//			return null;
		//		}
		//	}
		//}

		/// <summary>
		/// 查询多条数据
		/// </summary>
		/// <param name="strCmd">SQL语句</param>
		/// <param name="lsValues">查询结果集合</param>
		/// <returns></returns>
		public bool ExecuteReader(string strCmd, ref List<object[]> lsValues)
		{
			lock (objLock)
			{
				lsValues.Clear();
				bool bRtn = false;
				try
				{
					using (SQLiteConnection conn = new SQLiteConnection(base.ConnectionString))
                    {
						conn.Open();
						using (SQLiteCommand cmd = conn.CreateCommand())
						{
							cmd.CommandText = strCmd;
							using (SQLiteDataReader reader = cmd.ExecuteReader())
								if (reader.HasRows)
								{
									while (reader.Read())
									{
										object[] obj = new object[reader.FieldCount];
										reader.GetValues(obj);
										lsValues.Add(obj);
									}
								}
						}
						conn.Close();
						bRtn = true;
					}						
				}
				catch (Exception ex)
				{
					EventMgr.Warn(ex.Message);
				}
				return bRtn;
			}
		}

		/// <summary>
		/// 获取更新表格SQL语句
		/// </summary>
		/// <typeparam name="T">对象类型</typeparam>
		/// <param name="obj">更新对象</param>
		/// <param name="strPropertys">要更新的列</param>
		/// <param name="strTableName">表名字</param>
		/// <returns></returns>
		public string GetSqlString_Update<T>(T obj, string[] strPropertys = null, string strTableName = null)
		{
			try
			{
				Type t = typeof(T);
				DbAttribute attr = typeof(T).GetCustomAttribute<DbAttribute>();
				if (strTableName == null)
				{
					strTableName = attr.strName;
				}
				List<PropertyInfo> properties = (from a in t.GetProperties()
												 orderby a.GetCustomAttribute<DbAttribute>().nIndex
												 select a).ToList();
				if (strPropertys == null)
				{
					strPropertys = new string[properties.Count];
					for (int i = 0; i < properties.Count; i++)
					{
						strPropertys[i] = properties[i].GetCustomAttribute<DbAttribute>().strName;
					}
				}
				List<PropertyInfo> lsSelPropertyInfo = new List<PropertyInfo>();
				string[] array = strPropertys;
				foreach (string item in array)
				{
					lsSelPropertyInfo.Add(properties.Find((PropertyInfo a) => a.GetCustomAttribute<DbAttribute>().strName == item));
				}
				string sql = "INSERT OR REPLACE INTO " + strTableName + "(";
				for (int i2 = 0; i2 < strPropertys.Length; i2++)
				{
					sql = sql + " `" + strPropertys[i2] + "`";
					if (i2 < strPropertys.Length - 1)
					{
						sql += ",";
					}
				}
				sql += ") VALUES ";
				sql += "(";
				for (int j = 0; j < strPropertys.Length; j++)
				{
					PropertyInfo pi = lsSelPropertyInfo[j];
					object objValue = pi.GetValue(obj);
					if (objValue == null)
					{
						objValue = "";
					}
					if (objValue is DateTime dateTime)
					{
						objValue = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
					}
					sql = sql + "'" + objValue.ToString().Replace('\'', ' ') + "'";
					if (j < strPropertys.Length - 1)
					{
						sql += ",";
					}
				}
				return sql + ");";
			}
			catch (Exception ex)
			{
				EventMgr.Warn(ex.Message);
				return "";
			}
		}

		/// <summary>
		/// 获取更新表格SQL语句
		/// </summary>
		/// <typeparam name="T">对象类型</typeparam>
		/// <param name="lsObject">更新对象的集合</param>
		/// <param name="strPropertys">要更新的列</param>
		/// <param name="strTableName">表名字</param>
		/// <returns></returns>
		public string GetSqlString_Update<T>(List<T> lsObject, string[] strPropertys = null, string strTableName = null)
		{
			try
			{
				Type t = typeof(T);
				DbAttribute attr = typeof(T).GetCustomAttribute<DbAttribute>();
				if (strTableName == null)
				{
					strTableName = attr.strName;
				}
				List<PropertyInfo> properties = (from a in t.GetProperties()
												 orderby a.GetCustomAttribute<DbAttribute>().nIndex
												 select a).ToList();
				if (strPropertys == null)
				{
					strPropertys = new string[properties.Count];
					for (int i = 0; i < properties.Count; i++)
					{
						strPropertys[i] = properties[i].GetCustomAttribute<DbAttribute>().strName;
					}
				}
				List<PropertyInfo> lsSelPropertyInfo = new List<PropertyInfo>();
				string[] array = strPropertys;
				foreach (string item in array)
				{
					lsSelPropertyInfo.Add(properties.Find((PropertyInfo a) => a.GetCustomAttribute<DbAttribute>().strName == item));
				}
				string sql = "INSERT OR REPLACE INTO " + strTableName + "(";
				for (int i2 = 0; i2 < strPropertys.Length; i2++)
				{
					sql = sql + " `" + strPropertys[i2] + "`";
					if (i2 < strPropertys.Length - 1)
					{
						sql += ",";
					}
				}
				sql += ") VALUES ";
				for (int i3 = 0; i3 < lsObject.Count; i3++)
				{
					sql += "(";
					for (int j = 0; j < strPropertys.Length; j++)
					{
						PropertyInfo pi = lsSelPropertyInfo[j];
						object objValue = pi.GetValue(lsObject[i3]);
						if (objValue is DateTime dateTime)
						{
							objValue = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
						}
						sql += $"'{objValue}'";
						if (j < strPropertys.Length - 1)
						{
							sql += ",";
						}
					}
					sql += ")";
					if (i3 < lsObject.Count - 1)
					{
						sql += ",";
					}
				}
				return sql + ";";
			}
			catch (Exception ex)
			{
				EventMgr.Warn(ex.Message);
				return "";
			}
		}
	}
}
