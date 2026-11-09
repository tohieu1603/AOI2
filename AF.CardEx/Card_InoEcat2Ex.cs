using AF.Card;
using AF.Common;
using Inovance.InoMotionCotrolCard2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AF.CardEx
{
    public class Card_InoEcat2Ex : Card_InoEcat2
    {
        public Card_InoEcat2Ex(int nCardTotalIndex, int nCardIndex, string strName)
            : base(nCardTotalIndex, nCardIndex, strName)
        {

        }

      

      
        Imc60.TTaskStopMove tAxstopMvPara = new Imc60.TTaskStopMove();

        /// <summary>
        /// 设置IO轴绑定事件
        /// </summary>
        /// <param name="DiMotionGrpNo"></param>
        /// <param name="inPortNo"></param>
        /// <param name="motionAxNo"></param>
        /// <param name="triggerType"></param>
        /// <param name="tgtPos"></param>
        /// <param name="tgtVel"></param>
        /// <param name="acc"></param>
        /// <param name="dec"></param>
        /// <returns></returns>
        public  bool SetDiMotionEvent(int DiMotionGrpNo, int inPortNo, int motionAxNo, int triggerType, double tgtPos, double tgtVel, double acc, double dec)
        {
            Imc60.TEvent eventVar;
            eventVar.type = Imc60.EVENT_TYPE_DI;                     //事件类型 DI
            eventVar.index = (short)inPortNo;                        //事件变量索引
            eventVar.value = 0;                                      //设置比较值                      
            eventVar.condition = Imc60.EVENT_CONDITION_EQ;           //变量值等于设定值
            eventVar.loop = 1;
            //事件最大允许触发次数
            //设置事件
            //m_hCardHandle = (Card_InoEcat)(CardMgr.Card[cardIndex]).m_hCardHandle;
            //Card_InoEcat2 card = (Card_InoEcat2)(CardMgr.Card[cardIndex]);
            //m_hCardHandle = card.m_hCardHandle;

            uint ret = Imc60.IMC_SetEvent((short)nCardIndex, (short)DiMotionGrpNo, ref eventVar);
            if (ret != Imc60.EXE_SUCCESS)
            {
                EventMgr.Error( "IMC_SetEvent 失败，错误码为：0x" + ret.ToString("x8"));
                return false;
            }

            tAxstopMvPara.axNo = (short)(motionAxNo);
            tAxstopMvPara.stopType = 0;

            IntPtr intp = Marshal.AllocHGlobal(Marshal.SizeOf(tAxstopMvPara));   //0000028b18ac1890
            Marshal.StructureToPtr(tAxstopMvPara, intp, false);
            //设置任务0为改变0轴的JOG运动参数 
            ret = Imc60.IMC_SetTask((short)nCardIndex, (short)DiMotionGrpNo, Imc60.TASK_TYPE_STOPMOVE, intp);   //设置任务
            if (ret != Imc60.EXE_SUCCESS)
            {
                EventMgr.Error("IMC_SetTask 失败，错误码为：0x" + ret.ToString("x8"));
                return false;
            }

            short taskTpye = 0;
            //Imc60.TTaskUpdateJogMvPara tjogMvParaGet = new Imc60.TTaskUpdateJogMvPara();
            //IntPtr intpGet = Marshal.AllocHGlobal(Marshal.SizeOf(tjogMvPara));
            //获取任务0的配置参数  
            ret = Imc60.IMC_GetTask((short)nCardIndex, (short)DiMotionGrpNo, ref taskTpye, intp);   //获取任务
            if (ret != Imc60.EXE_SUCCESS)
            {
                EventMgr.Error("IMC_GetTask 失败，错误码为：0x" + ret.ToString("x8"));
                return false;
            }

            tAxstopMvPara = Marshal.PtrToStructure<Imc60.TTaskStopMove>(intp);
            short[] taskIndexArray = new short[1];
            taskIndexArray[0] = (short)DiMotionGrpNo;
            //设置事件0与任务0连接 
            ret = Imc60.IMC_SetEventTaskLink((short)nCardIndex, (short)DiMotionGrpNo, taskIndexArray, 1);    //设置事件与任务连接 
            if (ret != Imc60.EXE_SUCCESS)
            {
                EventMgr.Error("IMC_SetEventTaskLink 失败，错误码为：0x" + ret.ToString("x8"));
                return false;
            }

            //开启设置的事件0 
            ret = Imc60.IMC_EventOn((short)nCardIndex, (short)DiMotionGrpNo);   //事件开启
            if (ret != Imc60.EXE_SUCCESS)
            {
                EventMgr.Error("IMC_EventOn 失败，错误码为：0x" + ret.ToString("x8"));
                return false;
            }
            return true;
        }

        /// <summary>
        /// 清除IO轴绑定事件
        /// </summary>
        /// <param name="DiMotionGrpNo"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool DestroyEventDiMotion(int DiMotionGrpNo)
        {
            //Card_InoEcat2 card = (Card_InoEcat2)(CardMgr.Card[cardIndex]);
            //m_hCardHandle = card.m_hCardHandle;
            uint ret = Imc60.IMC_EventOff((short)nCardIndex, (short)DiMotionGrpNo);
            if (ret != Imc60.EXE_SUCCESS)
            {
                EventMgr.Error("IMC_EventOff 失败，错误码为：0x" + ret.ToString("x8"));
                return false;
            }
            ret = Imc60.IMC_ClearEventTaskLink((short)nCardIndex, (short)DiMotionGrpNo);
            if (ret != Imc60.EXE_SUCCESS)
            {
                EventMgr.Error("IMC_ClearEventTaskLink 失败，错误码为：0x" + ret.ToString("x8"));
                return false;
            }
            //ret = Imc60.IMC_EventOff((short)nCardIndex, (short)DiMotionGrpNo);
            //if (ret != Imc60.EXE_SUCCESS)
            //{
            //    EventMgr.Error("IMC_EventOff 失败，错误码为：0x" + ret.ToString("x8"));
            //    return false;
            //}
            ret = Imc60.IMC_ClearEvent((short)nCardIndex, (short)DiMotionGrpNo);
            if (ret != Imc60.EXE_SUCCESS)
            {
                EventMgr.Error("IMC_ClearEvent 失败，错误码为：0x" + ret.ToString("x8"));
                return false;
            }
            return true;
        }



    }
}
