using System;
using AF.Station;
using Microsoft.VisualBasic;


namespace Autoframe
{
    /// <summary>
    /// 产品数据，可添加定制化属性
    /// </summary>
    public class ProductData : ProductBase
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime DtStartTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime DtEndTime { get; set; } = DateTime.Now;

        /// <summary>
        /// NG原因
        /// </summary>
        public string NGReason { get; set; } = "";

        /// <summary>
        /// 位置
        /// </summary>
        public string Cavity { get; set; } = "";

        /// <summary>
        /// 暂存
        /// </summary>
        public string Temp { get; set; } = "";

        public string[] strings { get; set; } = { "11", "22" };
    }

    public class Carrier : ProductContainer
    {
        public Carrier(string strSN = null):base(strSN)
        {
            this.Length = 48;
            if(strSN == null)
                this.SN = $"Carrier_{DateTime.Now}";
        }

        public ProductData this[int n]
        {
            get { return this.Container[n] as ProductData; }
            set { this.Container[n] = value; }
        }

        public ProductData Current
        {
            get { return this.Container[Index] as ProductData; }
            set { this.Container[Index] = value; }
        }
    }

    public class Tray : Carrier
    {
        public Tray(string strSN = null) : base(strSN)
        {
            this.Length = 48;
            if (strSN == null)
                this.SN = $"Tray_{DateTime.Now}";
        }
    }

    public class Fixture : Carrier
    {
        public Fixture(string strSN = null) : base(strSN)
        {
            this.Length = 48;
            if (strSN == null)
                this.SN = $"Fixture_{DateTime.Now}";
        }
    }

    public class Magazine : ProductContainer
    {
        public Magazine(string strSN = null) : base(strSN)
        {
            this.Length = 10;
            if (strSN == null)
                this.SN = $"Magazine_{DateTime.Now}";
        }

        public Carrier this[int n]
        {
            get { return this.Container[n] as Carrier; }
            set { this.Container[n] = value; }
        }

        public Carrier Current
        {
            get { return this.Container[Index] as Carrier; }
            set { this.Container[Index] = value; }
        }
    }


    public class TrayData
    {
        public string SN { get; set; }

        //public string[] arrSlotSN = new string[Global.slotTotalCount];
        public DateTime DtStartTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime DtEndTime { get; set; } = DateTime.Now;

        public double CT { get; set; }

        public int inputQty { get; set; }
        public int goodQty { get; set; }
        public int badQty { get; set; }

        public string MoverID { get; set; }

        public CavityData[] AllSlotsData { get; set; } = new CavityData[Global.slotTotalCount];

        public TrayData()
        {
            for (int i = 0; i < AllSlotsData.Length; i++)
            {
                AllSlotsData[i] = new CavityData();
            }
        }
    }




    /// <summary>
    /// 穴位数据
    /// </summary>
    public class CavityData
    {
        public int NestID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int CavityID { get; set; }
        public SlotState Result { get; set; }
        public string NGReason { get; set; }
        public int BestNGReason { get; set; }

        public string SlotSN { get; set; }
    }
}
