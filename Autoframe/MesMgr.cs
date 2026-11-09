//26-11-2025 M : MES TRACE for TATA
//29-11-2025 M : MES TRACE add SaveLog & optimise analys response json
using AF.Common;
using AF.Data;
using AF.File;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.WebRequestMethods;

namespace Autoframe
{
    //[MESConfig]
    //Enable_MesProcessControl=True
    //Enable_MesUploadData = True
    //Enable_MesOperatorLogin=True
    //URL_MesProcessControl = http://10.10.13.28:8001/cantier-api-v1/process-control
    //URL_MesUploadData=http://10.10.13.28:8001/cantier-api-v1/pass-fail-MI
    //URL_MesOperatorLogin=http://10.10.13.28:8001/cantier-api-v1/certification-user-pass
    //OpID=11111
    //MachineID=A5F1L1SACG01

    //[TRACEConfig]
    //URL_TraceProcessControl = http://127.0.0.1:8765/v2/process_control?
    //URL_TraceInterLock=http://127.0.0.1:8765/interlock?
    //URL_TraceUploadData=http://127.0.0.1:8765/v2/logs?
    //LineID=A5-F1-S1
    //StationID = MGKK_A5 - F1 - S1_001_STATION1623
    //SoftwareName=Indomim_CgSpring
    //SoftwareVersion = V1.0.0.21
    public class MesUploadDataModel
    {
        public string reportTime = "";
        public string domainId = "";
        public string machineNo = "";
        public string woId = "";
        public string model = "";
        public string testStartTime = "";
        public string testEndTime = "";
        public double executionDuration = 0;
        public int inputQty = 0;
        public int totalPassQty = 0;
        public int totalFailQty = 0;
        public List<DefectItem> itemList = new List<DefectItem>();
    }

    public class DefectItem
    {
        public string errorCode = "";
        public string errorDesc = "";
        public int failQty = 0;
    }

    public class MesResponseModel
    {
        public bool success = false;
        public string message = "";
        public string bizErrCode = "";
        public string bizErrMsg = "";
        public BizData bizData;
    }

    public class BizData
    {
        public string model = "";
    }

    public enum RecordMode
    {
        Mes_GetWoModel,
        Mes_SaveTestData,
    }
 
    public static class MesMgr
    {
        
        public static bool GetWoModel(string wold, out string WoModel, out string strMsg)
        {
            if (!Parameter.GetBit("MesEnable"))
            {
                strMsg = "Disable Mes";
                WoModel = "";
                return true;
            }
            bool bRet = false;
            WoModel = "";
            strMsg = "";
            DateTime dt_request, dt_response;
            string str_domainId = Parameter.GetString("domainId");

            //JObject obj = new JObject();
            //obj["domainId"] = str_domainId;
            //obj["wold"] = wold;
            //string sendJson = JsonConvert.SerializeObject(obj);
            string sendUrl = DataGroup.GetGroupData("MesConfig", "GetWoModelURL").m_dbOffset.S;

            //string sendUrl = Ini.GetStringValue(Program.g_IniFile, "MesConfig", "GetWoModelURL", "");
            sendUrl += "?";
            sendUrl += $"woId={wold}&";
            sendUrl += $"domainId={str_domainId}";


            string strRes = "";
            dt_request = DateTime.Now;
            if (HttpHelper.Get(sendUrl, out strRes))
            {
                MesResponseModel reciveData = JsonConvert.DeserializeObject<MesResponseModel>(strRes);
                if (reciveData != null)
                {
                    strMsg = reciveData.message;
                    if (reciveData.success)
                    {
                        bRet = true;
                        WoModel = reciveData.bizData.model;
                    }
                    else
                    {
                        bRet = false;
                    }
                }
                else
                {
                    bRet = false;
                    strMsg = strRes;
                }
            }
            else
            {
                strMsg = strRes;
                bRet = false;
            }
            dt_response = DateTime.Now;
            RecordMgr.SaveTxtLog(RecordMode.Mes_GetWoModel, sendUrl, "", strRes);
            //string[] logArr = { PartSN, dt_request.ToString("yyyy-MM-dd HH:mm:ss:fff"), dt_response.ToString("yyyy-MM-dd HH:mm:ss:fff"), sendUrl, RecordMode.Mes_ProcessControl.ToString(), "POST", sendJson, strRes };
            //RecordMgr.GetInstance().SaveCsvLog(RecordMode.Mes_ProcessControl, logArr, bRet);
            return bRet;
        }

        public static bool SaveTestData(MesUploadDataModel data, out string strMsg)
        {
            if (!Parameter.GetBit("MesEnable"))
            {
                strMsg = "Disable Mes";
                return true;
            }
            bool bRet = false;
            strMsg = "";
            DateTime dt_request, dt_response;
            string str_domainId = Parameter.GetString("domainId");
            string str_machineNo = Parameter.GetString("machineNo");
            string str_model = Parameter.GetString("model");
            JObject obj = new JObject();

            JArray defectArr = new JArray();
            foreach (var item in data.itemList)
            {
                JObject defectObj = new JObject();
                defectObj["defectCode"] = item.errorCode;
                defectObj["defectName"] = item.errorDesc;
                defectObj["qty"] = item.failQty;

                defectArr.Add(defectObj);
            }                 

            obj["reportTime"] = data.reportTime;
            obj["domainId"] = str_domainId;
            obj["machineNo"] = str_machineNo;
            obj["woId"] = data.woId;
            obj["model"] = str_model;
            obj["testStartTime"] = data.testStartTime;
            obj["testEndTime"] = data.testEndTime;
            obj["executionDuration"] = data.executionDuration;
            obj["inputQty"] = data.inputQty;
            obj["totalPassQty"] = data.totalPassQty;
            obj["totalFailQty"] = data.totalFailQty;
            obj["itemList"] = defectArr;

            string sendJson = JsonConvert.SerializeObject(obj);
            string sendUrl = DataGroup.GetGroupData("MesConfig", "SaveTestDataURL").m_dbOffset.S;
            //string sendUrl = Ini.GetStringValue(Program.g_IniFile, "MesConfig", "SaveTestDataURL", "");
            string strRes = "";

            dt_request = DateTime.Now;
            if (HttpHelper.Post(sendUrl, sendJson, out strRes))
            {
                MesResponseModel reciveData = JsonConvert.DeserializeObject<MesResponseModel>(strRes);
                if (reciveData != null)
                {
                    strMsg = reciveData.message;
                    if (reciveData.success)
                    {
                        bRet = true;
                    }
                    else
                    {
                        bRet = false;
                    }
                }
                else
                {
                    bRet = false;
                    strMsg = strRes;
                }
            }
            else
            {
                strMsg = strRes;
                bRet = false;
            }
            dt_response = DateTime.Now;

            RecordMgr.SaveTxtLog(RecordMode.Mes_SaveTestData, sendUrl, sendJson, strRes);
            //string[] logArr = { data.UniqueId, dt_request.ToString("yyyy-MM-dd HH:mm:ss:fff"), dt_response.ToString("yyyy-MM-dd HH:mm:ss:fff"), sendUrl, RecordMode.Mes_UploadData.ToString(), "POST", sendJson, strRes };
            //RecordMgr.GetInstance().SaveCsvLog(RecordMode.Mes_UploadData, logArr, bRet);
            return bRet;
        }


    }

    public static class RecordMgr
    {
        public delegate void ShowLogHandler(RecordMode mode, string[] param, bool isResponseOK = true);
        public static event ShowLogHandler OnMesTraceRecord;
        private static string[] logCsvHead = { "SerialNo", "RequestTime", "ResponseTime", "URL", "Type", "Method", "RequestBody", "ResponseBody" };

        public static void RecordEvent(RecordMode mode, string[] param, bool isResponseOK = true)
        {
            if (OnMesTraceRecord != null)
            {
                OnMesTraceRecord(mode, param, isResponseOK);
            }
        }
        private static string BasePath
        {
            get
            {
                string strMonth = "";
                switch (DateTime.Now.Month)
                {
                    case 1:
                        strMonth = "January";
                        break;
                    case 2:
                        strMonth = "February";
                        break;
                    case 3:
                        strMonth = "March";
                        break;
                    case 4:
                        strMonth = "April";
                        break;
                    case 5:
                        strMonth = "May";
                        break;
                    case 6:
                        strMonth = "June";
                        break;
                    case 7:
                        strMonth = "July";
                        break;
                    case 8:
                        strMonth = "August";
                        break;
                    case 9:
                        strMonth = "September";
                        break;
                    case 10:
                        strMonth = "October";
                        break;
                    case 11:
                        strMonth = "November";
                        break;
                    case 12:
                        strMonth = "December";
                        break;
                }
                string strPath = Path.Combine("D:\\Log", DateTime.Now.Year.ToString(), strMonth);
                return strPath;
            }
        }

        public static string MesPath
        {
            get
            {
                string strPath = Path.Combine(BasePath, "MesLog");
                if (!Directory.Exists(strPath))
                {
                    Directory.CreateDirectory(strPath);
                }
                return strPath;
            }
        }

        public static string TracePath
        {
            get
            {
                string strPath = Path.Combine(BasePath, "TraceLog");
                if (!Directory.Exists(strPath))
                {
                    Directory.CreateDirectory(strPath);
                }
                return strPath;
            }
        }


        //public static void SaveCsvLog(RecordMode mode, string[] strLogArr, bool isResponseOK = true)
        //{
        //    string strPath = "";
        //    string fileName = "";
        //    switch (mode)
        //    {
        //        case RecordMode.Mes_ProcessControl:
        //        case RecordMode.Mes_UploadData:
        //        case RecordMode.Mes_OperatorLogin:
        //            strPath = MesPath;
        //            fileName = "MesLog" + DateTime.Now.ToString("yyyyMMdd") + ".csv";
        //            break;
        //        case RecordMode.Trace_ProcessControl:
        //        case RecordMode.Trace_InterLock:
        //        case RecordMode.Trace_UploadData:
        //            strPath = TracePath;
        //            fileName = "TraceLog" + DateTime.Now.ToString("yyyyMMdd") + ".csv";
        //            break;
        //    }
        //    RecordEvent(mode, strLogArr, isResponseOK);

        //    CsvOperation csv = new CsvOperation(strPath, fileName);
        //    int row = 0, col = 0;
        //    if (!System.IO.File.Exists(csv.FileName))
        //    {
        //        col = 0;
        //        foreach (var item in logCsvHead)
        //        {
        //            csv[row, col++] = item;
        //        }
        //        row = 1;
        //    }
        //    int i = 0;
        //    foreach (var item in strLogArr)
        //    {
        //        csv[row, i] = item;
        //        i++;
        //    }
        //    csv.Save();
        //}

        public static void SaveTxtLog(RecordMode mode, string url, string request, string response)
        {
            string strPath = "";
            switch (mode)
            {
                case RecordMode.Mes_GetWoModel:
                case RecordMode.Mes_SaveTestData:
                    strPath = MesPath + "\\MesLog" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                    break;
                //case RecordMode.Trace_ProcessControl:
                //case RecordMode.Trace_InterLock:
                //case RecordMode.Trace_UploadData:
                //    strPath = TracePath + "\\TraceLog" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                //    break;
            }
            if (!System.IO.File.Exists(strPath))
            {
                using (System.IO.File.Create(strPath)) { };
            }
         
            string strLog = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "----->";
            strLog += mode.ToString() + $"--> {url}\r\n";
            strLog += $"REQUEST---> {request}\r\n";
            strLog += $"RESPONSE---> {response}\r\n\r\n";

            try
            {
                System.IO.File.AppendAllText(strPath, strLog);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
    }

    #region Http Helper
    public class HttpHelper
    {
        static object obj = new object();
        public static bool Get(string Url, out string strGetContent)
        {
            string strURL = Url;
            //创建一个HTTP请求  
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strURL);
            //Post请求方式  
            request.Method = "GET";
            //内容类型
            request.ContentType = "application/json";

            HttpWebResponse response;
            try
            {
                //获得响应流
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                response = ex.Response as HttpWebResponse;
            }
            Stream s = response.GetResponseStream();
            StreamReader sRead = new StreamReader(s);
            string postContent = sRead.ReadToEnd();
            sRead.Close();
            strGetContent = postContent;//返回Json数据
            return true;
        }

        public static bool Post(string strUrl, string strJsonParas, out string strPostContent)
        {
            DateTime StartTime = DateTime.Now;
            lock (obj)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strUrl);
                request.Method = "POST";
                request.ContentType = "application/json";
                string paraUrlCoded = strJsonParas;//System.Web.HttpUtility.UrlEncode(jsonParas);   
                byte[] payload = System.Text.Encoding.ASCII.GetBytes(paraUrlCoded);
                request.ContentLength = payload.Length;
                Stream writer;
                try
                {
                    writer = request.GetRequestStream();
                }
                catch (Exception ex)
                {
                    writer = null;
                    strPostContent = ex.Message;
                    EventMgr.Warn(ex.Message);
                    return false;
                }
                writer.Write(payload, 0, payload.Length);
                writer.Close();
                HttpWebResponse response;
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (WebException ex)
                {
                    response = ex.Response as HttpWebResponse;
                }
                Stream s = response.GetResponseStream();
                StreamReader sRead = new StreamReader(s);
                strPostContent = sRead.ReadToEnd();
                sRead.Close();
                //SaveData((DateTime.Now - StartTime).TotalMilliseconds.ToString(), strJsonParas, strPostContent);
            }
            return true;
        }

        public static bool PostNoBody(string strUrl, out string strPostContent)
        {
            DateTime StartTime = DateTime.Now;
            lock (obj)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strUrl);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.ContentLength = 0;

                HttpWebResponse response;
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (WebException ex)
                {
                    response = ex.Response as HttpWebResponse;
                }
                Stream s = response.GetResponseStream();
                StreamReader sRead = new StreamReader(s);
                strPostContent = sRead.ReadToEnd();
                sRead.Close();
                //SaveData((DateTime.Now - StartTime).TotalMilliseconds.ToString(), strJsonParas, strPostContent);
            }
            return true;
        }
        public static async Task<string> PostAsync(string Url, string contentEnc, int timeout = 5)
        {
            string strRes = "";
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMilliseconds(timeout);
                var content = new StringContent(contentEnc, Encoding.UTF8, "application/json");
                try
                {
                    HttpResponseMessage resp = await client.PostAsync(Url, content);
                    string res = await resp.Content.ReadAsStringAsync();
                    strRes = res;
                }
                catch (Exception ex)
                {
                    strRes = "PostError: " + ex.Message;
                }
            }
            return strRes;
        }
    }
    #endregion
}
