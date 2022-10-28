using ServiceStack.Redis;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Util.Core.Helpers
{
    public class RedisHelper
    {

        private static RedisConfig _instance;
        public static string RedisAddressWrite;
        public static string RedisAddressRead;
        public static int MaxWritePoolSize;
        public static int MaxReadPoolSize;
        public static int Timeout;

        public static RedisConfig GetInstance()
        {
            if (_instance == null)
            {
                _instance = new RedisConfig();
            }
            return _instance;
        }


        public static void InitRedisConfig(string redisAddressWrite, string redisAddressRead, int maxWritePoolSize, int maxReadPoolSize, int connectTimeOut = 120)
        {
            RedisAddressWrite = redisAddressWrite;
            RedisAddressRead = redisAddressRead;
            MaxWritePoolSize = maxWritePoolSize;
            MaxReadPoolSize = maxReadPoolSize;
            Timeout = connectTimeOut;
            CreateManager();
        }
        /// <summary>  
        /// redis配置文件信息  
        /// </summary>  
        private static PooledRedisClientManager _prcm;

  

        /// <summary>  
        /// 创建链接池管理对象  
        /// </summary>  
        private static void CreateManager()
        {
            //支持多个地址 用“,”分割
            string writePath = RedisAddressWrite;
            string readPath = RedisAddressRead;
            _prcm = CreateManager(SplitString(writePath, ",").ToArray(), SplitString(readPath, ",").ToArray());
        }


        private static PooledRedisClientManager CreateManager(string[] readWriteHosts, string[] readOnlyHosts)
        {
            //WriteServerList：可写的Redis链接地址。  
            //ReadServerList：可读的Redis链接地址。  
            //MaxWritePoolSize：最大写链接数。  
            //MaxReadPoolSize：最大读链接数。  
            //AutoStart：自动重启。  
            //LocalCacheTime：本地缓存到期时间，单位:秒。  
            //RecordeLog：是否记录日志,该设置仅用于排查redis运行时出现的问题,如redis工作正常,请关闭该项。  
            //RedisConfigInfo类是记录redis连接信息，此信息和配置文件中的RedisConfig相呼应  

            // 支持读写分离，均衡负载   
            PooledRedisClientManager pooledRedisClientManager = new PooledRedisClientManager(readWriteHosts, readOnlyHosts, new RedisClientManagerConfig
            {
                MaxWritePoolSize = MaxWritePoolSize, // “写”链接池链接数   
                MaxReadPoolSize = MaxReadPoolSize, // “读”链接池链接数   
                AutoStart = true,
            });
            pooledRedisClientManager.ConnectTimeout = Timeout;
            JsConfig.DateHandler = DateHandler.ISO8601;//设置 对 日期时区的处理
            return pooledRedisClientManager;

        }

        private static IEnumerable<string> SplitString(string strSource, string split)
        {
            return strSource.Split(split.ToArray());
        }

        /// <summary>  
        /// 客户端缓存操作对象  
        /// </summary>  
        public static IRedisClient GetClient()
        {
            if (_prcm == null)
            {
                CreateManager();
            }
            return _prcm.GetClient();
        }


        /// <summary>
        /// 设置单体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="t"></param>
        /// <param name="exSecconds">过期</param>
        /// <returns></returns>
        public static bool Set<T>(string key, T t, int exSecconds)
        {
            using (IRedisClient redis = GetClient())
            {
                return redis.Set<T>(key, t, new TimeSpan(0, 0, exSecconds));
            }
        }

        /// <summary>
        /// 设置单体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="t"></param>
        /// <param name="exTime">过期时间</param>
        /// <returns></returns>
        public static bool Set<T>(string key, T t, TimeSpan exTime)
        {
            bool bol = false;
            try
            {
                using (IRedisClient redis = GetClient())
                {
                    bol = redis.Set<T>(key, t, exTime);
                }
            }
            catch (Exception ex)
            {
                //logger.Error(string.Format("Set error 发生异常，异常信息={0}", ex.Message), ex);
            }
            return bol;
        }
        /// <summary>
        /// 设置单体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="t"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public static bool Set<T>(string key, T t)
        {
            bool bol = false;
            using (IRedisClient redis = GetClient())
            {
                bol = redis.Set<T>(key, t);
            }
            return bol;
        }

        /// <summary>
        /// 获取单体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T Get<T>(string key) where T : class
        {
            using (IRedisClient redis = GetClient())
            {
                return redis.Get<T>(key);
            }
        }

        /// <summary>
        /// 移除单体
        /// </summary>
        /// <param name="key"></param>
        public static bool Remove(string key)
        {
            bool bol = false;
            using (IRedisClient redis = GetClient())
            {
                bol = redis.Remove(key);
            }
            return bol;
        }

        /// <summary>
        /// 判断是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool Exist(string key)
        {
            bool bol = false;
            using (IRedisClient redis = GetClient())
            {
                bol = redis.ContainsKey(key);
            }
            return bol;
        }

        /// <summary>
        /// 存在即返回实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T Exist<T>(string key) where T : class
        {
            using (IRedisClient redis = GetClient())
            {
                if (redis.ContainsKey(key))
                {
                    return redis.Get<T>(key);
                }
            }
            return null;
        }
        /// <summary>
        /// 计数器
        /// </summary>
        /// <param name="key"></param>
        /// <param name="exSecconds">过期时间 ，单位 秒</param>
        /// <returns></returns>
        public static long Incr(string key, int exSecconds = 0)
        {
            using (IRedisClient redis = GetClient())
            {
                long value = redis.IncrementValue(key);
                if (exSecconds > 0)
                {
                    redis.ExpireEntryIn(key, new TimeSpan(0, 0, exSecconds));
                }
                return value;
            }
        }
        /// <summary>
        /// 减一
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static long Decr(string key)
        {
            using (IRedisClient redis = GetClient())
            {
                long value = redis.DecrementValue(key);
                return value;
            }
        }

        /// <summary>
        /// 自增1，返回自增后的值
        /// </summary>
        public static long Incr(string key)
        {
            using (IRedisClient redis = GetClient())
            {
                return redis.IncrementValue(key);
            }
        }

        public static List<string> GetHashKeys(string hashID)
        {
            using (IRedisClient redis = GetClient())
            {
                return redis.GetHashKeys(hashID);
            }
        }
        public static List<string> GetValuesFromHash(string hashID,string[] keys)
        {
            using (IRedisClient redis = GetClient())
            {
                return redis.GetValuesFromHash(hashID, keys);
            }
        }
        /// <summary>
        /// /设置hash值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T GetHashValue<T>(string hashId, string key)
        {
            using (IRedisClient redis = GetClient())
            {
                string value = redis.GetValueFromHash(hashId, key);
                return JsonSerializer.DeserializeFromString<T>(value);
            }
        }
        /// <summary>
        /// 设置hash值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashId"></param>
        /// <param name="key"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool SetHashValue<T>(string hashId, string key, T t)
        {
            var value = JsonSerializer.SerializeToString<T>(t);
            using (IRedisClient redis = GetClient())
            {
                return redis.SetEntryInHash(hashId, key, value);
            }
        }
        /// <summary>
        /// 批量设置hash值
        /// </summary>
        /// <param name="hashId"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static void SetRangeInHash(string hashId, IEnumerable<KeyValuePair<string,string>> kes)
        {
            using (IRedisClient redis = GetClient())
            {
                redis.SetRangeInHash(hashId, kes);
            }
        }
    }

}
