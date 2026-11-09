using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Autoframe
{
    /// <summary>托盘网格配置实体，支持XML序列化</summary>
    [XmlRoot("TrayGridSetting")]
    public class TrayGridConfig
    {
        /// <summary>托盘行数</summary>
        public int RowCount { get; set; }
        /// <summary>托盘列数</summary>
        public int ColumnCount { get; set; }

        /// <summary>托盘行间距</summary>
        public double RowGap { get; set; }
        /// <summary>托盘列间距</summary>
        public double ColumnGap { get; set; }

        public string TrayTypeName { get; set; }

        /// <summary>XML配置文件路径（exe同级目录）</summary>
        private static readonly string _xmlPath = Path.Combine(Application.StartupPath, "Config", "TrayConfig.xml");

        #region 序列化保存/读取静态方法
        /// <summary>保存配置到本地XML</summary>
        public void SaveToXml()
        {
            using (var fs = new FileStream(_xmlPath, FileMode.Create, FileAccess.Write))
            {
                var serializer = new XmlSerializer(typeof(TrayGridConfig));
                serializer.Serialize(fs, this);
            }
        }

        /// <summary>加载本地XML配置；文件不存在返回默认8*8</summary>
        public static TrayGridConfig LoadFromXml()
        {
            if (!File.Exists(_xmlPath))
            {
                // 默认8行8列
                return new TrayGridConfig()
                {
                    RowCount = 6,
                    ColumnCount = 6,
                    RowGap = 19.90,
                    ColumnGap = 19.90,
                    TrayTypeName = "PKK4-WIDE"
                };
            }

            try
            {
                using (var fs = new FileStream(_xmlPath, FileMode.Open, FileAccess.Read))
                {
                    var serializer = new XmlSerializer(typeof(TrayGridConfig));
                    return serializer.Deserialize(fs) as TrayGridConfig;
                }
             
            }
            catch
            {
                // XML损坏，返回默认值
                return new TrayGridConfig() { 
                    RowCount = 6, 
                    ColumnCount = 6,
                    RowGap = 19.90,
                    ColumnGap = 19.90,
                    TrayTypeName = "PKK4-WIDE" };
            }
        }
        #endregion
    }
}
