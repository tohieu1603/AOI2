using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoFrame
{
    /// <summary>不良品表格绑定实体</summary>
    public class NGData
    {
        /// <summary>不良名称(界面显示中英文)</summary>
        public string NGName { get; set; }
        /// <summary>不良总数</summary>
        public int NGCount { get; set; }
        /// <summary>不良占比 0~100百分比</summary>
        public float NGRate { get; set; }
    }
}
