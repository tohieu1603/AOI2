using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoFrame.SG
{
    /// <summary>
    /// 用于深视点激光的使用
    /// </summary>
    public class SGHelper
    {
        private string sgIP;

        /// <summary>
        /// 点激光的IP
        /// </summary>
        public string SGIP
        {
            get { return sgIP; }
            set { sgIP = value; }
        }

        SGIF_DataStorageOneTimeCallBack dataOneTimeCallBack; //一次回调
        int selectedIndex = 0;
        int batchsize = 0;
        public void InitSampling(int series)
        {
            //
            //cmb_HZ.Items.Clear();
            List<string> camList = new List<string>();
            if (series == 0)//SGI和SG3000
            {
                camList.Add("88KHz");
                camList.Add("50KHZ");
                camList.Add("20KHZ");
                camList.Add("10KHZ");
                camList.Add("5KHZ");
                camList.Add("1KHZ");
            }
            else if (series == 1)//SCI
            {
                camList.Add("33KHz");
                camList.Add("20KHZ");
                camList.Add("15KHZ");
                camList.Add("10KHZ");
                camList.Add("5KHZ");
                camList.Add("2KHZ");
                camList.Add("1KHZ");
                camList.Add("0.5KHZ");
                camList.Add("0.2KHZ");
                camList.Add("0.1KHZ");
            }
            else if (series == 2)//SG5000及其他
            {
                camList.Add("590KHz");
                camList.Add("400KHZ");
                camList.Add("200KHZ");
                camList.Add("88KHZ");
                camList.Add("50KHZ");
                camList.Add("20KHZ");
                camList.Add("10KHZ");
                camList.Add("5KHZ");
                camList.Add("2KHZ");
                camList.Add("1KHZ");
            }
            //for (int i = 0; i < camList.Count; ++i)
            //{
            //    cmb_HZ.Items.Add(camList[i]);
            //}
            //cmb_HZ.SelectedIndex = 0;
        }
        public SGHelper(string ip)
        {
            sgIP = ip;
        }

        /// <summary>
        /// 与点激光建立连接
        /// </summary>
        /// <returns>是否正常打开</returns>
        public bool Open()
        {
            SGIF_OPENPARAM_ETHERNET mIp = new SGIF_OPENPARAM_ETHERNET(sgIP);
            RC mRc = SGLinkFuc.SGIF_OpenDeviceETHER(0, out mIp);
            if (mRc == RC.RC_OK)
            {
                SGIF_DEVICE_TYPE type;
                SGLinkFuc.SGIF_GetDeviceType(0, out type);
                string name = Enum.GetName(typeof(SGIF_DEVICE_TYPE), type);
                if (name.Contains("SG3") || name.Contains("SGI"))
                {
                    InitSampling(0);
                }
                else if (name.Contains("SCI"))
                {
                    InitSampling(1);
                }
                else
                {
                    InitSampling(2);
                }
                //读取端口
                bool ischecked = false;
                SGLinkFuc.SGIF_GetStorageTarget(0, 1, out ischecked);
               // checkBox1.Checked = ischecked;

                SGLinkFuc.SGIF_GetStorageTarget(0, 2, out ischecked);
               // checkBox2.Checked = ischecked;

                SGLinkFuc.SGIF_GetStorageTarget(0, 3, out ischecked);
               // checkBox3.Checked = ischecked;

                SGLinkFuc.SGIF_GetStorageTarget(0, 4, out ischecked);
               // checkBox4.Checked = ischecked;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <returns>是否断开</returns>
        public bool Close()
        {
            RC mRc = SGLinkFuc.SGIF_CloseDevice(0);
            return mRc != RC.RC_OK;
        }

        /// <summary>
        /// 设置通信模式
        /// </summary>
        /// <param name="mode">0:一般模式  1:通信模式</param>
        public bool SetCommonicateMode(int mode)
        {
            if(mode!=0 && mode!=1)
            {
                return false;
            }
            if (mode == 0)
            {
                RC mRc = SGLinkFuc.SGIF_SetMode(0, SGIF_MODE.SGIF_MODE_NORMAL);
                return true;
            }
            else
            {
                RC mRc = SGLinkFuc.SGIF_SetMode(0, SGIF_MODE.SGIF_MODE_COMMUNICATION);
                return true;
            }
        }

        /// <summary>
        /// 获取单次数据
        /// </summary>
        /// <param name="mOutNo">获取哪个口输出的数据</param>
        /// <returns></returns>
        public float GetSingleData(int mOutNo)
        {
            SGIF_FLOATVALUE_OUT mData;
            RC mRc = SGLinkFuc.SGIF_GetCalcDataSingle(0, mOutNo, out mData);
            return mData.Value;
        }
    }
}
