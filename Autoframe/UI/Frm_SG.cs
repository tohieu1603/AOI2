using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoFrame.SG;
using System.Runtime.InteropServices;

using System.Diagnostics;
using Autoframe;
using System.Threading;

namespace AutoFrame.UI
{
    public partial class Frm_SG : Form
    {
        public Frm_SG()
        {
            InitializeComponent();
            this.cmb_OutNo.SelectedIndex = 0;
            this.cmb_DataOutNo.SelectedIndex = 0;
            this.cmb_HZ.Items.Clear();

        }
        #region 【日志方法】
        public void AddLog(string iLog)
        {
            if (!this.InvokeRequired)
            {
                try
                {
                    this.AddLogT(iLog);
                }
                catch (Exception) { }
            }
            else
            {
                Invoke(new Action(() =>
                {
                    try
                    {
                        this.AddLogT(iLog);
                    }
                    catch (Exception) { }
                }));
            }
        }
        private void AddLogT(string iLog)
        {
            iLog = iLog.Replace("\r\n", "\\r\\n");
            this.txt_Log.AppendText("[" + DateTime.Now.ToString("HH:mm:ss-fff") + "]: " + iLog + "\r\n");
        }
        public void ClearLog()
        {
            if (!this.txt_Log.InvokeRequired)
            {
                try
                {
                    this.ClearLogT();
                }
                catch (Exception) { }
            }
            else
            {
                Invoke(new Action(() =>
                {
                    try
                    {
                        this.ClearLogT();
                    }
                    catch (Exception) { }
                }));
            }
        }
        private void ClearLogT()
        {
            this.txt_Log.Text = "";
        }
        #endregion
        public void InitSampling(int series)
        {
            //
            cmb_HZ.Items.Clear();
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
            for (int i = 0; i < camList.Count; ++i)
            {
                cmb_HZ.Items.Add(camList[i]);
            }
            cmb_HZ.SelectedIndex = 0;
        }
        SGIF_DataStorageOneTimeCallBack dataOneTimeCallBack; //一次回调
        int selectedIndex = 0;
        int batchsize = 0;
        private void btn_Open_Click(object sender, EventArgs e)
        {
            string mIpStr = this.txt_Ip.Text.Trim();
            SGIF_OPENPARAM_ETHERNET mIp = new SGIF_OPENPARAM_ETHERNET(mIpStr);
            RC mRc = SGLinkFuc.SGIF_OpenDeviceETHER(0, out mIp);
            this.AddLog(mRc.ToString());

            if (mRc == RC.RC_OK)
            {
                SGIF_DEVICE_TYPE type;
                SGLinkFuc.SGIF_GetDeviceType(0,out type);
                string name = Enum.GetName(typeof(SGIF_DEVICE_TYPE),type);
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
                SGLinkFuc.SGIF_GetStorageTarget(0,1,out ischecked);
                checkBox1.Checked = ischecked;

                SGLinkFuc.SGIF_GetStorageTarget(0, 2, out ischecked);
                checkBox2.Checked = ischecked;

                SGLinkFuc.SGIF_GetStorageTarget(0, 3, out ischecked);
                checkBox3.Checked = ischecked;

                SGLinkFuc.SGIF_GetStorageTarget(0, 4, out ischecked);
                checkBox4.Checked = ischecked;



            }
        }

        private void btn_Close_Click(object sender, EventArgs e)
        {
            RC mRc = SGLinkFuc.SGIF_CloseDevice(0);
            this.AddLog(mRc.ToString());
        }

        private void btn_ToNormal_Click(object sender, EventArgs e)
        {
            RC mRc = SGLinkFuc.SGIF_SetMode(0, SGIF_MODE.SGIF_MODE_NORMAL);
            this.AddLog(mRc.ToString());
        }

        private void btn_ToCommunication_Click(object sender, EventArgs e)
        {
            RC mRc = SGLinkFuc.SGIF_SetMode(0, SGIF_MODE.SGIF_MODE_COMMUNICATION);
            this.AddLog(mRc.ToString());
        }

        private void btn_cont_Click(object sender, EventArgs e)
        {
            RC mRc = SGLinkFuc.SGIF_SetRunningMode(0, SGIF_EmRunningMode.SGIF_EmRunningMode_cont, SGIF_EmDirection.SGIF_EmDirection_all);
            this.AddLog(mRc.ToString());
        }

        private void btn_Time_Click(object sender, EventArgs e)
        {
            RC mRc = SGLinkFuc.SGIF_SetRunningMode(0, SGIF_EmRunningMode.SGIF_EmRunningMode_time, SGIF_EmDirection.SGIF_EmDirection_all);
            this.AddLog(mRc.ToString());
        }

        private void btn_Tir_Click(object sender, EventArgs e)
        {
            RC mRc = SGLinkFuc.SGIF_SetRunningMode(0, SGIF_EmRunningMode.SGIF_EmRunningMode_trig, SGIF_EmDirection.SGIF_EmDirection_all);
            this.AddLog(mRc.ToString());
        }
        private void btn_GetAllOut_Click(object sender, EventArgs e)
        {
            RC mRc;
            SGIF_FLOATVALUE_OUT[] mDataList = new SGIF_FLOATVALUE_OUT[4];
            using (Pt mPt = new Pt(mDataList))
            {
                mRc = SGLinkFuc.SGIF_GetCalcDataALL(0, mPt.Ptr);
            }
            this.AddLog(mRc.ToString());

            this.lab_RealVal1.Text = mDataList[0].Value.ToString();
            this.lab_RealVal2.Text = mDataList[1].Value.ToString();
            this.lab_RealVal3.Text = mDataList[2].Value.ToString();
            this.lab_RealVal4.Text = mDataList[3].Value.ToString();
        }

        private void btn_GetSingleOut_Click(object sender, EventArgs e)
        {
            int mOutNo = this.cmb_OutNo.SelectedIndex + 1;
            SGIF_FLOATVALUE_OUT mData;
            RC mRc = SGLinkFuc.SGIF_GetCalcDataSingle(0, mOutNo, out mData);
            this.lab_RealVal.Text = mData.Value.ToString();
            this.AddLog(mRc.ToString());
        }


        private void btn_GetStorageData2_Click(object sender, EventArgs e)
        {
            int mOutNo = this.cmb_DataOutNo.SelectedIndex + 1;
            int mGetNum = Convert.ToInt32(this.txt_GetDataNum.Text.Trim());
            int mRecNum = 0;
            float[] mData = new float[mGetNum];//该数据由调用方开辟内存
            for (int i = 0; i < mData.Length; i++)
            {
                mData[i] = -100000;//实例化时置为无效值
            }
            RC mRc;
            using (Pt mPt = new Pt(mData))
            {
                mRc = SGLinkFuc.SGIF_DataStorageGetData(0, mOutNo, mData.Length, mPt.Ptr, out mRecNum);
            }
            this.AddLog(mRc.ToString());
            this.AddLog("数据个数：" + mRecNum.ToString());

        }

        private void btn_ClearStorage_Click(object sender, EventArgs e)
        {
            RC mRc;
            mRc = SGLinkFuc.SGIF_DataStorageInit(0);
            this.AddLog(mRc.ToString());
            this.lab_OutNum1.Text = "0";
            this.lab_OutNum2.Text = "0";
            this.lab_OutNum3.Text = "0";
            this.lab_OutNum4.Text = "0";
        }

        private void btn_StopStorage_Click(object sender, EventArgs e)
        {
            RC mRc;
            mRc = SGLinkFuc.SGIF_DataStorageStop(0);
            this.AddLog(mRc.ToString());
        }

        private void btn_StartStorage_Click(object sender, EventArgs e)
        {

            //开启扫描
            RC mRc;
            mRc = SGLinkFuc.SGIF_DataStorageStart(0);
            this.AddLog(mRc.ToString());
            this.btn_MonitorStorage.PerformClick();
        }

        private void btn_MonitorStorage_Click(object sender, EventArgs e)
        {
            bool mState = false;
            int[] mNumList = new int[4];
            RC mRc;


            //using (Pt mPt2 = new Pt(mNumList))
            //{
            //    mRc = SGLinkFuc.SGIF_DataStorageGetStatus(0, out mState, mPt2.Ptr);
            //}
            //if (mState)
            //{
            //    this.lab_State.Text = "采集中";
            //}
            //else
            //{
            //    this.lab_State.Text = "停止";
            //}
            //this.lab_OutNum1.Text = mNumList[0].ToString();
            //this.lab_OutNum2.Text = mNumList[1].ToString();
            //this.lab_OutNum3.Text = mNumList[2].ToString();
            //this.lab_OutNum4.Text = mNumList[3].ToString();

            Task.Run(() =>
            {
                ReGet:
                using (Pt mPt2 = new Pt(mNumList))
                {
                    mRc = SGLinkFuc.SGIF_DataStorageGetStatus(0, out mState, mPt2.Ptr);
                }
                this.Invoke(new Action(() =>
                {
                    if (mState)
                    {
                        this.lab_State.Text = "采集中";
                    }
                    else
                    {
                        this.lab_State.Text = "停止";
                    }
                    bool[] portStatus = { checkBox1.Checked, checkBox2.Checked, checkBox3.Checked, checkBox4.Checked };

                    int[] resultList = new int[4];
                    int resultNum = 0;

                    for (int i = 0; i < portStatus.Length; i++)
                    {
                        resultList[i] = portStatus[i] ? mNumList[resultNum++] : 0;
                    }

                    // 批量设置 Label
                    Label[] labels = { lab_OutNum1, lab_OutNum2, lab_OutNum3, lab_OutNum4 };
                    for (int i = 0; i < labels.Length; i++)
                    {
                        labels[i].Text = resultList[i].ToString();
                    }
                }));
                if (mState)
                {
                    goto ReGet;
                }
            });
        }

        private void btn_SetStorageNum_Click(object sender, EventArgs e)
        {
            int mNum = Convert.ToInt32(this.txt_SetStorageNum.Text);
            SGIF_STORAGECYCLE mCycle = SGIF_STORAGECYCLE.SGIF_STORAGECYCLE_1;
            RC mRc = SGLinkFuc.SGIF_SetDataStorage(0, mNum, mCycle);
            this.AddLog(mRc.ToString());
        }

        private void btn_SetHZ_Click(object sender, EventArgs e)
        {
            int index = this.cmb_HZ.SelectedIndex;
            RC mRc = SGLinkFuc.SGIF_SetSamplingCycle(0, index);
            this.AddLog(mRc.ToString());
        }

        public void DataOneTimeCallBack(int deviceid)
        {
            this.AddLog("enter callback");
            int mRecNum = 0;
            float[] mData = new float[batchsize];//该数据由调用方开辟内存
            for (int i = 0; i < batchsize; i++)
            {
                mData[i] = -100000;//实例化时置为无效值
            }
            RC mRc;
            using (Pt mPt = new Pt(mData))
            {
                mRc = SGLinkFuc.SGIF_DataStorageGetData(0, selectedIndex, batchsize, mPt.Ptr, out mRecNum);
            }
            this.AddLog(mRc.ToString());
            this.AddLog("获取数据完毕,数据个数：" + mRecNum.ToString());
            //初始化
            mRc = SGLinkFuc.SGIF_DataStorageInit(0);
        }
        private void btn_setcallback_Click(object sender, EventArgs e)
        {
            //先初始化
            RC mRc = SGLinkFuc.SGIF_DataStorageInit(0);
            dataOneTimeCallBack = new SGIF_DataStorageOneTimeCallBack(DataOneTimeCallBack);
            mRc = SGLinkFuc.SGIF_SetOneDataStorage(0, dataOneTimeCallBack);
            this.AddLog("CALLBACK_SETTING:"+ mRc.ToString());
        }

        private void btn_startcallbcak_Click(object sender, EventArgs e)
        {
            //设置
            int mNum = Convert.ToInt32(this.txt_SetStorageNum.Text);
            SGIF_STORAGECYCLE mCycle = SGIF_STORAGECYCLE.SGIF_STORAGECYCLE_1;
            RC mRc = SGLinkFuc.SGIF_SetDataStorage(0, mNum, mCycle);
            if (mRc != RC.RC_OK)
                return;

            batchsize = mNum;
            selectedIndex = this.cmb_DataOutNo.SelectedIndex + 1;
            //开始采集
            mRc = SGLinkFuc.SGIF_DataStorageStart(0);
            this.AddLog("CALLBACK_START:"+mRc.ToString());
        }

        private void btn_setStorgePort_Click(object sender, EventArgs e)
        {
            //使能OUT口
            RC nRet = SGLinkFuc.SGIF_SetMode(0, SGIF_MODE.SGIF_MODE_COMMUNICATION);
            bool ischeck = checkBox1.Checked;
            nRet = SGLinkFuc.SGIF_SetStorageTarget(0, 1, checkBox1.Checked);
            nRet = SGLinkFuc.SGIF_SetStorageTarget(0, 2, checkBox2.Checked);
            SGLinkFuc.SGIF_SetStorageTarget(0, 3, checkBox3.Checked);
            SGLinkFuc.SGIF_SetStorageTarget(0, 4, checkBox4.Checked);
            SGLinkFuc.SGIF_SetMode(0, SGIF_MODE.SGIF_MODE_NORMAL);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int i = 0;
            while (true)
            {
                DO.CCD2下AOI检测拍照.Set(true);
                Thread.Sleep(10);
                DO.CCD2下AOI检测拍照.Set(false);

                Thread.Sleep(10);

                i++;

                if (i> 50)
                {
                    break;
                }
            }
        }
    }
}
