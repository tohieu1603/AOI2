//m97 - extension class for process
using AF.Common;
using AF.Data;
using AF.File;
using AF.Station;
using Autoframe;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autoframe
{
    public class MaintenanceHelper
    {
        public delegate void MaintenanceModeChangeDel(bool bMode);

        public static event MaintenanceModeChangeDel MaintenanceModeChangeEvent;

        public static void PushlishEvent(bool bMode)
        {
            if (MaintenanceModeChangeEvent != null)
            {
                MaintenanceModeChangeEvent.Invoke(bMode);
            }
        }
    }

    public static class ExtensionHelper
    {
        public static readonly ConcurrentQueue<TrayData> productQueue = new ConcurrentQueue<TrayData>();
        public static string g_ProductDataPath = "D:\\exe\\ProductData";

        private static ThreadEx runThread;

        static ExtensionHelper()
        {
            runThread = new ThreadEx();
        }

        public static void Start()
        {
            runThread.Start(WorkLoop, 100, "ProductHandle");
            Init();
        }
        public static void Stop()
        {
            runThread.Stop();
            while (productQueue.TryDequeue(out _))
            {
            }
        }

        public static Dictionary<string, int> dicDecfect = new Dictionary<string, int>();

        public static void Init()
        {
            dicDecfect.Clear();
            var ArrayThrowReason = Enum.GetValues(typeof(ThrowReason)).Cast<ThrowReason>().ToList();
            for (int i = 0; i < ArrayThrowReason.Count; i++)
            {
                dicDecfect.Add(ArrayThrowReason[i].ToString().Split('_')[1],0);
            }
          
        }
        private static void WorkLoop()
        {
            try
            {
                if (productQueue.TryDequeue(out TrayData data))
                {
                    string NGCavity = "";
                    string detailInfo = string.Empty;
                   
                    foreach (var item in data.AllSlotsData)
                    {
                        if (item.Result == SlotState.NA)
                            continue;
                        ProductData showData = new ProductData();
                        showData.SN = item.SlotSN == null ? "" : item.SlotSN;
                        showData.DtStartTime = data.DtStartTime;
                        showData.DtEndTime = data.DtEndTime;
                        showData.Cavity = item.CavityID.ToString();
                        showData.Result = item.Result == SlotState.OK;
                        showData.NGReason = item.NGReason;

                        ThrowReason reason = ThrowReason.OpticalLens_定位失败;
                        if (!showData.Result)
                        {
                            reason = (ThrowReason)item.BestNGReason;
                        }
                            

                        showData.ShowData();

                        if (showData.Result)
                        {
                            Statistics.UpdateMaterial(Material.OpticalLens);
                            detailInfo += $"[{item.CavityID}_OK]:({item.SlotSN})";
                        }
                        else
                        {
                            Statistics.UpdateMaterial(Material.OpticalLens, reason);
                            if (dicDecfect.Keys.Contains(reason.ToString().Split('_')[1]))
                            {
                                dicDecfect[reason.ToString().Split('_')[1]] += 1;
                            }
                            NGCavity += $"{item.CavityID}_";
                            detailInfo += $"[{item.CavityID}_NG]:({item.SlotSN})";
                        }
                    }

                    StringBuilder csvInfo = new StringBuilder();

                    string csvHead = "TraySN,StartTime,EndTime,Input,GoodCount,BadCount,NGCavitys,DetailInfo";
                    string strPath = Path.Combine(g_ProductDataPath, DateTime.Now.ToString("yyyy-MM-dd") ?? "", $"{Parameter.GetString("MachineSide")}Side_Hour_" + DateTime.Now.ToString("HH") + "_TrayUnloadData.csv");
                  

                    csvInfo.Append(data.SN);
                    csvInfo.Append(",");
                    csvInfo.Append(data.DtStartTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                    csvInfo.Append(",");
                    csvInfo.Append(data.DtEndTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                    csvInfo.Append(",");
                    csvInfo.Append(data.inputQty);
                    csvInfo.Append(",");
                    csvInfo.Append(data.goodQty);
                    csvInfo.Append(",");
                    csvInfo.Append(data.badQty);
                    csvInfo.Append(",");
                    csvInfo.Append(NGCavity);
                    csvInfo.Append(",");
                    csvInfo.Append(detailInfo);




                    Task.Run(delegate
                    {
                        CsvWriter.WriteLine(strPath, csvInfo.ToString(), csvHead.ToString());
                    });
                }
            }
            catch
            {

            }
        }
    }
}
