using AF.Data.DB;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoFrame.DBExtend
{
    public class RedisHelperEx
    {
        private static readonly Lazy<RedisHelperEx> lazy = new Lazy<RedisHelperEx>(() => new RedisHelperEx());
        public static RedisHelperEx Instance => lazy.Value;

        private RedisHelperEx() { }
        /// <summary>存储不同服务器序号对应的 ConnectionMultiplexer 对象</summary>
        private ConcurrentDictionary<string, ConnectionMultiplexer> connDict = new ConcurrentDictionary<string, ConnectionMultiplexer>();
        /// <summary>
        /// 添加一个新的 Redis 连接。
        /// </summary>
        /// <param name="serverNo">服务器序号，作为连接标识</param>
        /// <param name="ip">Redis 服务器 IP</param>
        /// <param name="port">Redis 服务器端口</param>
        public void AddConnection(string serverNo= "0", string ip="127.0.0.1", string port="6379",string pwd = "123456")
        {
            if (connDict.ContainsKey(serverNo)) return;
            try
            {
                var options = new ConfigurationOptions
                {
                    EndPoints = { $"{ip}:{port}" },
                    AbortOnConnectFail = false,
                    ConnectTimeout = 5000,       // 适当增加超时
                    SyncTimeout = 5000,
                    AllowAdmin = true,
                    // 可选：设置 KeepAlive 以避免防火墙断开空闲连接
                    KeepAlive = 60,                // 每 60 秒发送一个 ping
                    Password = pwd

                };
                var redis = ConnectionMultiplexer.Connect(options);
                // 订阅事件以获取详细信息
                //redis.ConnectionFailed += (s, e) =>
                //    TcpManager.GlobalLog?.Invoke($"[{serverNo}] ConnectionFailed: {e.Exception?.Message}");
                //redis.ConnectionRestored += (s, e) =>
                //    TcpManager.GlobalLog?.Invoke($"[{serverNo}] ConnectionRestored");
                //redis.ErrorMessage += (s, e) =>
                //    TcpManager.GlobalLog?.Invoke($"[{serverNo}] ErrorMessage: {e.Message}");
                connDict.TryAdd(serverNo, redis);
            }
            catch (Exception ex)
            {
                // 使用全局日志输出错误信息
                //TcpManager.GlobalLog?.Invoke($"AddConnection error: {ex.Message}");
            }
        }


        /// <summary>
        /// 检查指定服务器序号的 Redis 连接是否可用。
        /// </summary>
        /// <param name="serverNo">服务器序号</param>
        /// <returns>如果连接存在且已连接，返回 true；否则返回 false。</returns>
        public bool IsConnected(string serverNo, out ConnectionMultiplexer redis)
        {
            if (connDict.TryGetValue(serverNo, out redis))
            {
                return redis.IsConnected;
            }
            return false;
        }


        /// <summary>
        /// 等待指定服务器序号的 Redis 连接完全就绪（通过发送 PING 验证）。
        /// </summary>
        /// <param name="serverNo">服务器序号</param>
        /// <param name="timeoutMilliseconds">超时时间（毫秒），默认 5000</param>
        /// <returns>连接是否就绪</returns>
        public bool WaitForConnectionReady(string serverNo, int timeoutMilliseconds = 5000)
        {
            if (!connDict.TryGetValue(serverNo, out var redis))
                return false;
            bool b = redis.IsConnected;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMilliseconds)
            {
                try
                {
                    b = redis.IsConnected;
                    var db = redis.GetDatabase();
                    db.Ping();  // 发送 PING 命令，成功即连接可用
                    return true;
                }
                catch (RedisConnectionException)
                {
                    // 连接未就绪，短暂等待后重试
                    Thread.Sleep(200);
                }
                catch (Exception ex)
                {
                    // 其他异常（如超时、服务器错误）同样等待重试
                   // TcpManager.GlobalLog?.Invoke($"等待连接时发生异常：{ex.Message}");
                    Thread.Sleep(200);
                }
            }
            return false;
        }
        /// <summary>
        /// 根据服务器序号获取对应的 IDatabase 对象。
        /// </summary>
        /// <param name="serverNo">服务器序号</param>
        /// <returns>IDatabase 实例，若连接不存在则返回 null</returns>
        private IDatabase GetDatabase(string serverNo)
        {
            if (connDict.TryGetValue(serverNo, out var redis))
            {
                //if (redis.IsConnected)
                return redis.GetDatabase();
            }
            return null;
        }


        #region Hash 操作
        /// <summary>
        /// 设置 Hash 中指定字段的值。
        /// </summary>
        /// <param name="serverNo">服务器序号</param>
        /// <param name="key">Hash 的键</param>
        /// <param name="field">字段名</param>
        /// <param name="value">字段值</param>
        /// <returns>是否设置成功（连接不存在时返回 false）</returns>
        public bool HashSet(string key, string field, string value, string serverNo="0")
        {
            var db = GetDatabase(serverNo);
            if (db == null) return false;
            db.HashSet(key, field, value);
            return true;
        }

        public bool HashSet(string key, IDictionary<string, string> values, string serverNo = "0")
        {
            if (values == null || values.Count == 0) return false;
            var db = GetDatabase(serverNo);
            if (db == null) return false;
            var entries = new HashEntry[values.Count];
            int i = 0;
            foreach (var kvp in values)
            {
                entries[i++] = new HashEntry(kvp.Key, kvp.Value ?? string.Empty);
            }
            db.HashSet(key, entries);
            return true;
        }

        /// <summary>
        /// 获取 Hash 中指定字段的值。
        /// </summary>
        /// <param name="serverNo">服务器序号</param>
        /// <param name="key">Hash 的键</param>
        /// <param name="field">字段名</param>
        /// <returns>字段值，若字段不存在或连接失败则返回 null</returns>
        public string HashGet(string key, string field, string serverNo = "0")
        {
            var db = GetDatabase(serverNo);
            if (db == null) return null;
            var val = db.HashGet(key, field);
            return val.IsNull ? null : val.ToString();
        }
        /// <summary>
        /// 删除 Hash 中的指定字段
        /// </summary>
        /// <param name="serverNo">服务器序号</param>
        /// <param name="key">Hash 的键</param>
        /// <param name="field">要删除的字段名</param>
        /// <returns>是否成功删除（字段是否存在）</returns>
        public bool HashDelete(string key, string field, string serverNo = "0")
        {
            var db = GetDatabase(serverNo);
            if (db == null) return false;
            return db.HashDelete(key, field);
        }

        /// <summary>
        /// 获取 Hash 中的所有字段及值
        /// </summary>
        public Dictionary<string, string> HashGetAll(string key, string serverNo = "0")
        {
            var db = GetDatabase(serverNo);
            if (db == null) return null;
            var hashEntries = db.HashGetAll(key);
            var dict = new Dictionary<string, string>();
            foreach (var entry in hashEntries)
            {
                dict[entry.Name] = entry.Value;
            }
            return dict;
        }
        /// <summary>
        /// 检查键是否存在
        /// </summary>
        public bool Keycheck(string serverNo, string key)
        {
            var db = GetDatabase(serverNo);
            if (db == null) return false;

            return db.KeyExists(key);
        }
        #endregion

        #region String 操作（新增）
        /// <summary>
        /// 设置字符串键值
        /// </summary>
        public bool StringSet(string serverNo, string key, string value)
        {
            var db = GetDatabase(serverNo);
            if (db == null) return false;
            return db.StringSet(key, value);
        }

        /// <summary>
        /// 获取字符串值
        /// </summary>
        public string StringGet(string serverNo, string key)
        {
            var db = GetDatabase(serverNo);
            if (db == null) return null;
            var val = db.StringGet(key);
            return val.IsNull ? null : val.ToString();
        }

        /// <summary>
        /// 删除键
        /// </summary>
        public bool KeyDelete(string serverNo, string key)
        {
            var db = GetDatabase(serverNo);
            if (db == null) return false;
            return db.KeyDelete(key);
        }
        #endregion

        #region List 操作（用于报警队列）
        /// <summary>
        /// 向列表左侧插入一个元素。
        /// </summary>
        /// <param name="serverNo">服务器序号</param>
        /// <param name="key">列表的键</param>
        /// <param name="value">要插入的值</param>
        /// <returns>插入后列表的长度，若连接失败则返回 0</returns>
        public long ListLeftPush(string serverNo, string key, string value)
        {
            var db = GetDatabase(serverNo);
            return db?.ListLeftPush(key, value) ?? 0;
        }

        /// <summary>
        /// 从列表右侧弹出一个元素。
        /// </summary>
        /// <param name="serverNo">服务器序号</param>
        /// <param name="key">列表的键</param>
        /// <returns>弹出的元素值，若列表为空或连接失败则返回 null</returns>
        public string ListRightPop(string serverNo, string key)
        {
            var db = GetDatabase(serverNo);
            var val = db?.ListRightPop(key);
            return val.HasValue && !val.Value.IsNull ? val.Value.ToString() : null;
        }
        #endregion

    }
}
