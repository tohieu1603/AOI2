using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoFrame.PLCExtend
{
    public class SingleInstance<T> where T : class, new()
    {
        //
        // 摘要:
        //     实例化对象
        private static T m_instance;

        //
        // 摘要:
        //     线程互斥对象
        private static readonly object syslock = new object();

        //
        // 摘要:
        //     获取实例
        public static T GetInstance()
        {
            if (m_instance == null)
            {
                object obj = syslock;
                lock (obj)
                {
                    m_instance = new T();
                }
            }

            return m_instance;
        }
    }
}
