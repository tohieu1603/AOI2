//m97 - Extension class for swap tray
using AF.Common;
using AF.Data;
using AF.Station;
using Autoframe;
using AutoFrame.DBExtend;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Autoframe
{
    public enum SlotState
    {
        NA,
        OK,
        NG,

    }
    public enum BinRole
    {
        OK,
        NG,
        NA
    }
    public enum BinStatus
    {
        NoTray,

        TranferPutTray,

        SnapOK,

        SnapNG,

        Ready,

        Processing,

        ProcessCompleted
    }

    /// <summary>
    /// TrayMatrix data
    /// </summary>
    public class TrayMatrix
    {
        private readonly int _row;
        private readonly int _col;
        private readonly SlotState[,] _slots;
        private readonly string[,] _NGreasons;
        private readonly string[,] _SN;
        public TrayMatrix(int row, int col)
        {
            _row = row;
            _col = col;

            _slots = new SlotState[_row, _col];
            _NGreasons = new string[_row, _col];
            _SN = new string[_row, _col];
            Clear();
        }

        internal void Clear()
        {
            for (int r = 0; r < _row; r++)
            {
                for (int c = 0; c < _col; c++)
                {
                    _slots[r, c] = SlotState.NA;
                    _NGreasons[r, c] = "";
                    _SN[r, c] = "";
                }
            }
        }
        internal SlotState Get(int index)
        {
            int r = index / _col;
            int c = index % _col;

            return _slots[r, c];
        }

        internal SlotState Get(int r, int c)
        {
            return _slots[r, c];
        }

        internal void Set(int index, SlotState state)
        {
            int r = index / _col;
            int c = index % _col;

            _slots[r, c] = state;
        }
        internal void Set(int r, int c, SlotState state)
        {
            _slots[r, c] = state;
        }

        internal string GetNGReason(int index)
        {
            int r = index / _col;
            int c = index % _col;

            return _NGreasons[r, c];
        }

        internal string GetNGReason(int r, int c)
        {
            return _NGreasons[r, c];
        }

        internal void SetNGReason(int index, string strReason)
        {
            int r = index / _col;
            int c = index % _col;

            _NGreasons[r, c] = strReason;
        }
        internal void SetNGReason(int r, int c, string strReason)
        {
            _NGreasons[r, c] = strReason;
        }

        internal string GetSN(int index)
        {
            int r = index / _col;
            int c = index % _col;

            return _SN[r, c];
        }

        internal string GetSN(int r, int c)
        {
            return _SN[r, c];
        }

        internal void SetSN(int index, string strSN)
        {
            int r = index / _col;
            int c = index % _col;

            _SN[r, c] = strSN;
        }
        internal void strSN(int r, int c, string strSN)
        {
            _SN[r, c] = strSN;
        }

        internal List<int> GetListNGReason(int index)
        {
            try
            {
                string strNGReason = GetNGReason(index);
                string[] arr = strNGReason?.Split('_');
                if (arr?.Length > 0)
                {
                    List<int> temp = new List<int>();
                    for (int i = 0; i < arr.Length; i++)
                    {
                        if (int.TryParse(arr[i], out int iRet))
                            temp.Add(iRet);
                    }
                    return temp;
                }
                else
                {
                    return new List<int>();
                }
            }
            catch (Exception)
            {
                return new List<int>();
            }

        }

        internal List<int> GetListNGReason(int r, int c)
        {
            string strNGReason = GetNGReason(r, c);
            string[] arr = strNGReason.Split('_');
            if (arr.Length > 0)
            {
                List<int> temp = new List<int>();
                for (int i = 0; i < arr.Length; i++)
                {
                    if (int.TryParse(arr[i], out int iRet))
                        temp.Add(iRet);
                }
                return temp;
            }
            else
            {
                return new List<int>();
            }
        }
    }

    /// <summary>
    /// Bin Tray information
    /// </summary>
    public class BinInfo
    {
        public string Name;

        public string DbName;

        public int iRegion = 0;

        public string strRegion = "";

        public double RowDis = 1;

        public double ColDis = 1;
        public int TraySlotCount => _row * _column;

        public BinRole Role;

        public DateTime StartTime;

        public DateTime EndTime;

        public double CycleTime
        {
            get
            {
                try
                {
                    double dCT = EndTime.Subtract(StartTime).TotalSeconds;
                    if (dCT < 0)
                        return 0;
                    else
                        return dCT;
                }
                catch
                {
                    return 0;
                }
            }
        }

        public string OrderID;

        private readonly object _binLock = new object();

        // lock vật lý giữa bin và gantry
        private readonly object _gantryLock = new object();

        private readonly TrayMatrix _matrix;

        private readonly int _row;
        public int Row
        {
            get { return _row; }
        }

        private readonly int _column;
        public int Column
        {
            get { return _column; }
        }

        private int _okCount;
        public int OKCount
        {
            get
            {
                lock (_binLock)
                {
                    return _okCount;
                }
            }
        }

        private int _ngCount;
        public int NGCount
        {
            get
            {
                lock (_binLock)
                {
                    return _ngCount;
                }
            }
        }

        private int _inputQty;
        public int InputQty
        {
            get
            {
                lock (_binLock)
                {
                    return _inputQty;
                }
            }

            set
            {
                lock (_binLock)
                {
                    _inputQty = value;
                }
            }
        }

        private int _emptyCount;
        public int EmptyCount
        {
            get
            {
                lock (_binLock)
                {
                    return _emptyCount;
                }
            }
        }

        private int _unloadCount;
        public int UnloadCount
        {
            get
            {
                lock (_binLock)
                {
                    return _unloadCount;
                }
            }
        }

        private BinStatus _status;
        public BinStatus Status
        {
            get
            {
                lock (_binLock)
                {
                    return _status;
                }
            }
        }

        public bool IsReady
        {
            get
            {
                lock (_binLock)
                {
                    return _status == BinStatus.Ready;
                }
            }
        }

        private bool _isPairing;
        public bool IsPairing
        {
            get
            {
                lock (_binLock)
                {
                    return _isPairing;
                }
            }
            set
            {
                lock (_binLock)
                {
                    _isPairing = value;
                }
            }
        }

        public bool IsOKFull
        {
            get
            {
                lock (_binLock)
                {
                    return _okCount == TraySlotCount;
                }
            }
        }

        public bool IsNGFull
        {
            get
            {
                lock (_binLock)
                {
                    return _ngCount == TraySlotCount;
                }
            }
        }

        private bool _gantryUsing;
        public bool IsGantryUsing
        {
            get
            {
                lock (_gantryLock)
                {
                    return _gantryUsing;
                }
            }
        }

        public BinInfo(int row, int column)
        {
            _row = row;
            _column = column;
            _matrix = new TrayMatrix(row, column);
            RefreshCountInternal();
        }
        public void IncreaseUnloadCount()
        {
            lock (_binLock)
            {
                _unloadCount++;
            }
        }
        public void ClearUnloadCount()
        {
            lock (_binLock)
            {
                _unloadCount = 0;
            }
        }
        public void SetStatus(BinStatus status)
        {
            lock (_binLock)
            {
                _status = status;
            }
        }
        public SlotState GetSlotState(int index)
        {
            if (index < 0 || index > TraySlotCount - 1)
            {
                EventMgr.Error("Matrix index out of range");
                return SlotState.NA;
            }
            lock (_binLock)
            {
                return _matrix.Get(index);
            }
        }
        public SlotState[] GetAllSlotState()
        {
            SlotState[] temp = new SlotState[TraySlotCount];
            lock (_binLock)
            {
                for (int i = 0; i < TraySlotCount; i++)
                {
                    temp[i] = _matrix.Get(i);
                }
            }
            return temp;
        }

        public SlotState GetSlotState(int row, int col)
        {
            lock (_binLock)
            {
                return _matrix.Get(row, col);
            }
        }

        public void SetSlotState(int index, SlotState state)
        {
            if (index < 0 || index > TraySlotCount - 1)
            {
                EventMgr.Error("Matrix index out of range");
                return;
            }
            lock (_binLock)
            {
                _matrix.Set(index, state);
                RefreshCountInternal();
            }

            if (!RedisHelperEx.Instance.HashSet(DbName, $"S_{index + 1}", state.ToString()))
                EventMgr.Warn("Write data to Redis failed!");
        }

        public void SetSlotState(int rowidx, int colidx, SlotState state)
        {
            int index = rowidx * _column + colidx;

            lock (_binLock)
            {
                _matrix.Set(rowidx, colidx, state);
                RefreshCountInternal();
            }

            if (!RedisHelperEx.Instance.HashSet(DbName, $"S_{index + 1}", state.ToString()))
                EventMgr.Warn("Write data to Redis failed!");
        }

        public string GetSlotNGReason(int index)
        {
            if (index < 0 || index > TraySlotCount - 1)
            {
                EventMgr.Error("Matrix index out of range");
                return null;
            }
            lock (_binLock)
            {
                return _matrix.GetNGReason(index);
            }
        }
        public string[] GetAllSlotNGReason()
        {
            string[] temp = new string[TraySlotCount];
            lock (_binLock)
            {
                for (int i = 0; i < TraySlotCount; i++)
                {
                    temp[i] = _matrix.GetNGReason(i);
                }
            }
            return temp;
        }

        public string GetSlotNGReason(int row, int col)
        {
            lock (_binLock)
            {
                return _matrix.GetNGReason(row, col);
            }
        }

        public void SetSlotNGReason(int index, string strReason)
        {
            if (index < 0 || index > TraySlotCount - 1)
            {
                EventMgr.Error("Matrix index out of range");
                return;
            }
            lock (_binLock)
            {
                _matrix.SetNGReason(index, strReason);
            }

            if (!RedisHelperEx.Instance.HashSet(DbName, $"S_{index + 1}_NGReason", strReason))
                EventMgr.Warn("Write data to Redis failed!");
        }

        public void SetSlotNGReason(int rowidx, int colidx, string strReason)
        {
            int index = rowidx * _column + colidx;

            lock (_binLock)
            {
                _matrix.SetNGReason(rowidx, colidx, strReason);
            }

            if (!RedisHelperEx.Instance.HashSet(DbName, $"S_{index + 1}_NGReason", strReason))
                EventMgr.Warn("Write data to Redis failed!");
        }


        public List<int> GetSlotListNGReason(int index)
        {
            lock (_binLock)
            {
                return _matrix.GetListNGReason(index);
            }
        }

        public List<int> GetSlotListNGReason(int rowIndex, int colIndex)
        {
            lock (_binLock)
            {
                return _matrix.GetListNGReason(rowIndex, colIndex);
            }
        }

        public int GetSlotBestNGReason(int index)
        {
            lock (_binLock)
            {
                List<int> list_NG = _matrix.GetListNGReason(index);

                return list_NG.Count > 0 ? list_NG.Max() : 999;
            }
        }


        public string GetSlotSN(int index)
        {
            if (index < 0 || index > TraySlotCount - 1)
            {
                EventMgr.Error("Matrix index out of range");
                return null;
            }
            lock (_binLock)
            {
                return _matrix.GetSN(index);
            }
        }

        public void SetSlotSN(int index, string strSn)
        {
            if (index < 0 || index > TraySlotCount - 1)
            {
                EventMgr.Error("Matrix index out of range");
                return;
            }
            lock (_binLock)
            {
                _matrix.SetSN(index, strSn);
            }

            if (!RedisHelperEx.Instance.HashSet(DbName, $"S_{index + 1}_SN", strSn))
                EventMgr.Warn("Write data to Redis failed!");
        }

        public void RefreshCount()
        {
            lock (_binLock)
            {
                RefreshCountInternal();
            }
        }

        private void RefreshCountInternal()
        {
            int ok = 0;
            int ng = 0;
            int empty = 0;

            for (int i = 0; i < TraySlotCount; i++)
            {
                switch (_matrix.Get(i))
                {
                    case SlotState.OK:
                        ok++;
                        break;


                    case SlotState.NG:
                        ng++;
                        break;


                    case SlotState.NA:
                        empty++;
                        break;
                }
            }

            _okCount = ok;
            _ngCount = ng;
            _emptyCount = empty;
        }

        public void SetMatrixBySQL()
        {
            SlotState[] states = new SlotState[TraySlotCount];
            string[] NgReasons = new string[TraySlotCount];

            // 1. đọc DB trước
            for (int i = 0; i < TraySlotCount; i++)
            {
                string strState = RedisHelperEx.Instance.HashGet(DbName, $"S_{(i + 1)}");
                NgReasons[i] = RedisHelperEx.Instance.HashGet(DbName, $"S_{(i + 1)}_NGReason");

                if (Enum.TryParse(strState, out SlotState state))
                {
                    states[i] = state;
                }
                else
                {
                    states[i] = SlotState.NA;
                }
            }

            // 2. update atomically
            lock (_binLock)
            {
                for (int i = 0; i < TraySlotCount; i++)
                {
                    _matrix.Set(i, states[i]);
                    _matrix.SetNGReason(i, NgReasons[i]);
                }

                RefreshCountInternal();
            }
        }

        public void ClearMatrix()
        {
            lock (_binLock)
            {
                _matrix.Clear();
                RefreshCountInternal();
            }
            // Gom TẤT CẢ dữ liệu vào 1 Dictionary, gửi 1 lần
            var batch = new Dictionary<string, string>();
            for (int i = 0; i < TraySlotCount; i++)
            {
                string key = $"S_{i + 1}";
                batch[key] = "NA";
                batch[$"{key}_NGReason"] = "";
                batch[$"{key}_SN"] = "";
            }
            // 1 lệnh Redis duy nhất ghi 180 field
            RedisHelperEx.Instance.HashSet(DbName, batch);
        }
        //public void ClearMatrix()
        //{
        //    List<int> updateIndex = new List<int>();

        //    lock (_binLock)
        //    {
        //        for (int r = 0; r < _row; r++)
        //        {
        //            for (int c = 0; c < _column; c++)
        //            {
        //                _matrix.Set(r, c, SlotState.NA);
        //                _matrix.SetNGReason(r, c, "");
        //                updateIndex.Add(r * _column + c);
        //            }
        //        }

        //        RefreshCountInternal();
        //    }

        //    foreach (var index in updateIndex)
        //    {
        //        RedisHelperEx.Instance.HashSet(DbName, $"S_{index + 1}", SlotState.NA.ToString());
        //        RedisHelperEx.Instance.HashSet(DbName, $"S_{index + 1}_NGReason", "");
        //        RedisHelperEx.Instance.HashSet(DbName, $"S_{index + 1}_SN", "");
        //    }
        //}

        /// <summary>
        /// Gantry chiếm quyền thao tác với bin
        /// </summary>
        public bool LockGantry()
        {
            lock (_gantryLock)
            {
                if (_gantryUsing)
                    return false;

                _gantryUsing = true;

                return true;
            }
        }



        /// <summary>
        /// Trả quyền điều khiển bin cho bin station
        /// </summary>
        public void UnlockGantry()
        {
            lock (_gantryLock)
            {
                _gantryUsing = false;
            }
        }



        /// <summary>
        /// Bin có được phép chạy trục không
        /// </summary>
        public bool CanMove()
        {
            lock (_gantryLock)
            {
                return !_gantryUsing;
            }
        }

        public TrayData ConvertToTrayData()
        {
            TrayData data = new TrayData();
            data.SN = $"{strRegion}_{DateTime.Now.ToString("yyyyMMddTHH:mm:ss")}";
            data.DtStartTime = StartTime;
            data.DtEndTime = EndTime;
            data.inputQty = _inputQty;
            data.goodQty = _okCount;
            data.badQty = _ngCount;
            data.CT = CycleTime;
            for (int i = 0; i < TraySlotCount; i++)
            {
                data.AllSlotsData[i] = new CavityData();
                data.AllSlotsData[i].CavityID = i + 1;
                data.AllSlotsData[i].Result = GetSlotState(i);
                data.AllSlotsData[i].SlotSN = GetSlotSN(i);

                List<int> arr_NG = GetSlotListNGReason(i);
                List<int> arr_proNG = new List<int>();
                foreach (var item in arr_NG)
                {
                    if (!arr_proNG.Contains(item))
                        arr_proNG.Add(item);
                }
                List<string> arr_str = new List<string>();
                foreach (var item in arr_proNG)
                {
                    arr_str.Add(((ThrowReason)item).ToString());
                }
                data.AllSlotsData[i].NGReason = string.Join("_", arr_str.ToArray()).Replace("OpticalLens_", "");
                data.AllSlotsData[i].BestNGReason = GetSlotBestNGReason(i);
            }
            return data;
        }
        public MesUploadDataModel ConvertToMesData()
        {
            MesUploadDataModel model = new MesUploadDataModel();
            model.reportTime = EndTime.ToString("yyyy-MM-dd");
            model.testStartTime = StartTime.ToString("yyyy-MM-dd HH:mm:ss");
            model.testEndTime = EndTime.ToString("yyyy-MM-dd HH:mm:ss");
            model.executionDuration = EndTime.Subtract(StartTime).TotalSeconds;
            model.inputQty = _inputQty;
            model.totalPassQty = _okCount;
            model.totalFailQty = _ngCount;



            List<List<int>> NgData_rawList = new List<List<int>>();
            for (int i = 0; i < TraySlotCount; i++)
            {
                NgData_rawList.Add(GetSlotListNGReason(i));
            }

            Dictionary<ThrowReason, int> dic_NGreason = new Dictionary<ThrowReason, int>();
            foreach (var item in NgData_rawList)
            {
                if (item.Count > 0)
                {
                    int iData = item.Max();
                    if (!dic_NGreason.ContainsKey((ThrowReason)iData))
                    {
                        dic_NGreason.Add((ThrowReason)iData, 1);
                    }
                    else
                    {
                        dic_NGreason[(ThrowReason)iData]++;
                    }
                }
            }

            foreach (var item in dic_NGreason)
            {
                model.itemList.Add(new DefectItem()
                {
                    errorCode = $"D{(int)item.Key}",
                    errorDesc = item.Key.ToString(),
                    failQty = item.Value
                });
            }

            return model;
        }
    }

    public enum PairType
    {
        OKNG,   // A-B hoặc C-B

        OKOK,    // A-C

        SupplyOK,

        SupplyNG,
    }

    public enum PairState
    {
        NoMove,

        Working,

        Completed,
    }
    public class PairCandidate
    {
        public BinInfo Bin1;

        public BinInfo Bin2;

        public string PairDesp => $"BestPair: OK-{Bin1.Name} vs NG-{Bin2.Name}";

        public PairType Type;


        public PairScore Score;


        public string Reason;
    }

    public class PairContext
    {
        public PairCandidate Pair { get; }

        public int RetryCount { get; set; }
        public bool IsWorking { get; private set; }

        public int StackCount { get; private set; }

        public string PairName { get; }

        public PairContext(PairCandidate pair)
        {
            Pair = pair;
            IsWorking = true;
            Pair.Bin1.IsPairing = true;
            Pair.Bin2.IsPairing = true;
            PairName = $"{pair.Bin1.Name}&{pair.Bin2.Name}_{DateTime.Now.ToString("yyyyMMddTHH:mm:ss.fff")}";
        }

        public void Finish()
        {
            IsWorking = false;
            Pair.Bin1.IsPairing = false;
            Pair.Bin2.IsPairing = false;
        }

        public void IncreaseStack()
        {
            StackCount++;
        }

        public void PlannerFail()
        {
            RetryCount++;
        }
    }

    public class PairScore
    {
        // AB > BC > AC
        public int PairPriority;

        // bonus cực lớn nếu hoàn thành
        public int CompleteOK;

        // bonus cực lớn nếu hoàn thành
        public int CompleteNG;

        // số OK sau swap
        public int FutureOK;

        // số NG sau swap
        public int FutureNG;

        public int MoveCount;

        public int TotalScore =>
            PairPriority * 10000
            + CompleteOK * 5000
            + CompleteNG * 1000
            + FutureOK * 100
            + FutureNG * 20
            + MoveCount;
    }

    /// <summary>
    /// Base evaluate
    /// </summary>
    public interface IPairEvaluator
    {
        PairCandidate Evaluate(BinInfo a, BinInfo b);

    }
    /// <summary>
    /// Evaluate OK & NG score
    /// </summary>
    public class OKNGEvaluator : IPairEvaluator
    {
        public PairCandidate Evaluate(BinInfo okBin, BinInfo ngBin)
        {
            PairScore score = CalculateOKNGScore(okBin, ngBin);
            if (score.MoveCount == 0)
                return null;
            return new PairCandidate
            {
                Bin1 = okBin,

                Bin2 = ngBin,

                Type = PairType.OKNG,

                Score = score,

                Reason = $"OKNG {okBin.Name}-{ngBin.Name}"
            };
        }


        private static PairScore CalculateOKNGScore(BinInfo okBin, BinInfo ngBin)
        {
            int needOK = okBin.TraySlotCount - okBin.OKCount;

            int availableOK = ngBin.OKCount;

            int putOK = Math.Min(needOK, availableOK);

            int needNG = okBin.TraySlotCount - ngBin.NGCount;

            int availableNG = okBin.NGCount;

            int putNG = Math.Min(needNG, availableNG);

            int futureOK = okBin.OKCount + putOK;

            int futureNG = ngBin.NGCount + putNG;

            return new PairScore
            {
                CompleteOK = futureOK == okBin.TraySlotCount ? 1 : 0,

                CompleteNG = futureNG == okBin.TraySlotCount ? 1 : 0,

                FutureOK = futureOK,

                //FutureNG = futureNG,

                MoveCount = (putOK + putNG) * -1
            };
        }
    }

    /// <summary>
    /// Evaluate OK & OK score
    /// </summary>
    public class OKOKEvaluator : IPairEvaluator
    {
        private class OKOKResult
        {
            public BinInfo TargetOkBin;
            public BinInfo SourceBin;

            public int Move;

            public int FutureTargetOK;

            public bool Complete;
        }
        private OKOKResult CalculateDirection(BinInfo TargetOkBin, BinInfo SourceBin)
        {
            int move = Math.Min(SourceBin.OKCount, TargetOkBin.EmptyCount + TargetOkBin.NGCount);

            return new OKOKResult()
            {
                SourceBin = SourceBin,

                TargetOkBin = TargetOkBin,

                Move = move,

                FutureTargetOK = TargetOkBin.OKCount + move,

                Complete = TargetOkBin.OKCount + move >= TargetOkBin.TraySlotCount
            };
        }

        public PairCandidate Evaluate(BinInfo a, BinInfo c)
        {
            OKOKResult ac = CalculateDirection(a, c);
            OKOKResult ca = CalculateDirection(c, a);
            OKOKResult[] oKOKResults = new OKOKResult[] { ac, ca };

            //var temp = oKOKResults.Where(x => x.Move > 0).OrderByDescending(x => x.Complete).ThenByDescending(x => x.FutureTargetOK);
            //OKOKResult best = temp.Where(x => x.Move > 0).OrderByDescending(x => x.Complete).ThenByDescending(x => x.FutureTargetOK).FirstOrDefault();
            OKOKResult best = oKOKResults.Where(x => x.Move > 0)
                .OrderByDescending(x => x.Complete)
                .ThenByDescending(x => x.FutureTargetOK)
                .ThenBy(x => x.Move).FirstOrDefault();
            if (best == null)
                return null;

            // không full tray thì bỏ AC
            if (!best.Complete)
                return null;

            return new PairCandidate()
            {
                Bin1 = best.TargetOkBin,
                Bin2 = best.SourceBin,
                Type = PairType.OKOK,

                Score = new PairScore()
                {
                    CompleteOK = 1,
                    FutureOK = best.FutureTargetOK,
                    MoveCount = best.Move
                },

                Reason =
                 $"OKOK {best.SourceBin.Name}->{best.TargetOkBin.Name}"
            };
        }
    }

    public enum VacuumState
    {
        Created,

        Picked,

        Placed,
    }

    /// <summary>
    /// Vaccum work information
    /// </summary>
    public class VacuumTask
    {
        public int VacuumId;

        public string VaccumName;

        public BinInfo Source;

        public int SourceSlot;

        public BinInfo Target;

        public int TargetSlot = -1;

        public SlotState Material;

        public string NGInfo;

        public string SN;

        public double PickX = 0;

        public double PickY = 0;

        public double PickU = 0;

        public double PlaceX = 0;

        public double PlaceY = 0;

        public double PlaceU = 0;

        public DI DISuck;

        public DO DOSuck;

        public DO DOBlow;

        public VacuumState State = VacuumState.Created;

        public void ApplyPick()
        {
            Source.SetSlotState(SourceSlot, SlotState.NA);
            Source.SetSlotNGReason(SourceSlot, "");
            Source.SetSlotSN(SourceSlot, "");
            //DbSet(Source.DbName, SourceSlot, SlotState.NA);
            Source.RefreshCount();
            State = VacuumState.Picked;
            if (SwapController.CurrentPair != null)
                SwapController.ShowLog($"{SwapController.CurrentPair.PairName}-Stack{SwapController.CurrentPair.StackCount} UZ{VacuumId} Pick {Material} from {Source.Name} slot{SourceSlot + 1}, set slot{SourceSlot + 1}=NA");
        }

        public void ApplyPlace()
        {
            Target.SetSlotState(TargetSlot, Material);
            Target.SetSlotNGReason(TargetSlot, NGInfo);
            Target.SetSlotSN(TargetSlot, SN);
            //DbSet(Target.DbName, TargetSlot, Material);
            Target.RefreshCount();
            State = VacuumState.Placed;
            if (SwapController.CurrentPair != null)
                SwapController.ShowLog($"{SwapController.CurrentPair.PairName}-Stack{SwapController.CurrentPair.StackCount} UZ{VacuumId} Place {Material} to {Target.Name} slot{TargetSlot + 1}, set slot{TargetSlot + 1}={Material}");
        }

        private void DbSet(string DbName, int SlotIdx, SlotState State)
        {
            RedisHelperEx.Instance.HashSet(DbName, $"S_{SlotIdx}", State.ToString());
        }
    }
    public enum SwapMode
    {
        OK2NG2,
        OK1NG1,
        OK2NG1,
        OK1NG2,

        OK2,
        NG2,

        OK1,
        NG1,
        SupplyOK,
        SupplyNG,
        Nothing
    }

    public enum StackPhase
    {
        Created,

        Picking,

        PickSourceCompleted,

        PickTargetCompleted,
        PlacingSource,
        PlacingTarget,

        PlaceSourceCompleted,

        PlaceTargetCompleted,

        Completed,

        Failed
    }

    /// <summary>
    /// Swap stack infomation
    /// </summary>
    public class SwapStack
    {
        public BinInfo BinOK;

        public BinInfo BinNG;

        public SwapMode Mode;

        public StackPhase Phase;

        public VacuumTask UZ1;

        public VacuumTask UZ2;

        public VacuumTask UZ3;

        public VacuumTask UZ4;

        public bool UZ1Work => UZ1 != null;
        public bool UZ2Work => UZ2 != null;
        public bool UZ3Work => UZ3 != null;
        public bool UZ4Work => UZ4 != null;

        public void DestroyTask(int VaccumId)
        {
            switch (VaccumId)
            {
                case 1:
                    UZ1 = null;
                    break;
                case 2:
                    UZ2 = null;
                    break;
                case 3:
                    UZ3 = null;
                    break;
                case 4:
                    UZ4 = null;
                    break;
                default:
                    break;
            }
        }
        public bool bIsCCD5 => BinOK.iRegion == 1;

        private readonly HashSet<(BinInfo, int)> PlaceReservations = new HashSet<(BinInfo, int)>();


        public bool IsPlaceReserved(BinInfo bin, int slot)
        {
            return PlaceReservations.Contains((bin, slot));
        }


        public void ReservePlace(BinInfo bin, int slot)
        {
            PlaceReservations.Add((bin, slot));
        }

        public void SetPhase(StackPhase phase)
        {
            this.Phase = phase;
        }

        public void ReleaseReservation()
        {
            PlaceReservations.Clear();
        }

        public void SetOK(int index, VacuumTask task)
        {
            switch (index)
            {
                case 0:
                    UZ3 = task;
                    UZ3.VaccumName = "OK1";
                    UZ3.DISuck = DI.下料站吸嘴3真空到位;
                    UZ3.DOSuck = DO.下料站吸嘴3吸真空;
                    UZ3.DOBlow = DO.下料站吸嘴3破真空;
                    break;

                case 1:
                    UZ4 = task;
                    UZ4.VaccumName = "OK2";
                    UZ4.DISuck = DI.下料站吸嘴4真空到位;
                    UZ4.DOSuck = DO.下料站吸嘴4吸真空;
                    UZ4.DOBlow = DO.下料站吸嘴4破真空;
                    break;
            }
        }

        public void SetNG(int index, VacuumTask task)
        {
            switch (index)
            {
                case 0:
                    UZ1 = task;
                    UZ1.VaccumName = "NG1";
                    UZ1.DISuck = DI.下料站吸嘴1真空到位;
                    UZ1.DOSuck = DO.下料站吸嘴1吸真空;
                    UZ1.DOBlow = DO.下料站吸嘴1破真空;
                    break;

                case 1:
                    UZ2 = task;
                    UZ2.VaccumName = "NG2";
                    UZ2.DISuck = DI.下料站吸嘴2真空到位;
                    UZ2.DOSuck = DO.下料站吸嘴2吸真空;
                    UZ2.DOBlow = DO.下料站吸嘴2破真空;
                    break;
            }
        }

        public IEnumerable<VacuumTask> Tasks()
        {
            if (UZ1 != null)
                yield return UZ1;


            if (UZ2 != null)
                yield return UZ2;


            if (UZ3 != null)
                yield return UZ3;


            if (UZ4 != null)
                yield return UZ4;
        }

        public bool Empty => !Tasks().Any();

        public int Count => Tasks().Count();

        public string Info
        {
            get
            {
                switch (Mode)
                {
                    case SwapMode.SupplyOK:
                        return $"SupplyOK-{BinOK.Name}->{BinNG.Name}";

                    case SwapMode.SupplyNG:
                        return $"SupplyNG-{BinOK.Name}->{BinNG.Name}";

                    default:
                        return $"SwapStack-{BinNG.Name}vs{BinOK.Name},Mode:{Mode}";
                }
            }
        }

        public void ApplyPick(bool bIsUZ12)
        {

            BinOK.RefreshCount();
            BinNG.RefreshCount();
            SetPhase(bIsUZ12 ? StackPhase.PickSourceCompleted : StackPhase.PickTargetCompleted);
        }

        public void ApplyPlace(bool bIsUZ12)
        {

            BinOK.RefreshCount();
            BinNG.RefreshCount();
            SetPhase(bIsUZ12 ? StackPhase.PlaceTargetCompleted : StackPhase.PlaceSourceCompleted);
        }
    }

    /// <summary>
    /// Generate pair and swapstack helper
    /// </summary>
    public static class SwapTrayHelper
    {
        private class Capacity
        {
            public int OK;

            public int NG;
        }

        public static PairCandidate FindBest(BinInfo A, BinInfo B, BinInfo C)
        {
            try
            {
                if (A == null && B == null && C == null)
                {
                    return null;
                }
                List<PairCandidate> candidates = new List<PairCandidate>();

                if (A != null && A.IsReady && !A.IsGantryUsing && B != null && B.IsReady && !B.IsGantryUsing)
                {
                    var pair = new OKNGEvaluator().Evaluate(A, B);
                    if (pair != null)
                    {
                        pair.Score.PairPriority = 3;
                        candidates.Add(pair);
                    }
                }

                if (C != null && C.IsReady && !C.IsGantryUsing && B != null && B.IsReady && !B.IsGantryUsing)
                {
                    var pair = new OKNGEvaluator().Evaluate(C, B);
                    if (pair != null)
                    {
                        pair.Score.PairPriority = 3;
                        candidates.Add(pair);
                    }
                }

                if (A != null && A.IsReady && !A.IsGantryUsing && C != null && C.IsReady && !C.IsGantryUsing)
                {
                    var pair = new OKOKEvaluator().Evaluate(A, C);
                    if (pair != null)
                    {
                        pair.Score.PairPriority = 1;
                        candidates.Add(pair);
                    }
                }
                if (candidates.Count < 1)
                {
                    return null;
                }
                var best = candidates.OrderByDescending(x => x.Score.TotalScore).FirstOrDefault();
                return best;
            }
            catch (Exception ex)
            {
                EventMgr.Error("Find BestPair error: " + ex.Message);
                return null;
            }

        }


        #region Swap Version2
        public static PairCandidate FindBestVer2(BinInfo A, BinInfo B, BinInfo C)
        {
            try
            {
                // ==========================================
                // PRIORITY 3
                // A <-> B
                // ==========================================
                if (CanMainAB(A, B))
                {
                    var pair = new OKNGEvaluator().Evaluate(A, B);

                    if (pair != null)
                    {
                        pair.Score.PairPriority = 3;
                        return pair;
                    }
                }


                // ==========================================
                // PRIORITY 2
                // C -> A : OK
                // ==========================================
                if (CanSupplyOK(C, A))
                {
                    return new PairCandidate
                    {
                        Bin1 = C,
                        Bin2 = A,
                        Type = PairType.SupplyOK,
                        Score = new PairScore
                        {
                            PairPriority = 2
                        }
                    };
                }


                // ==========================================
                // PRIORITY 1
                // C -> B : NG
                // ==========================================
                if (CanSupplyNG(C, B))
                {
                    return new PairCandidate
                    {
                        Bin1 = C,
                        Bin2 = B,
                        Type = PairType.SupplyNG,
                        Score = new PairScore
                        {
                            PairPriority = 1
                        }
                    };
                }


                return null;
            }
            catch (Exception ex)
            {
                EventMgr.Error(
                    "FindBest error: " + ex);

                return null;
            }
        }

        private static bool CanSupplyOK(BinInfo C, BinInfo A)
        {
            if (C == null || A == null)
                return false;

            if (!C.IsReady || C.IsGantryUsing)
                return false;

            if (!A.IsReady || A.IsGantryUsing)
                return false;

            C.RefreshCount();
            A.RefreshCount();

            return C.OKCount > 0 &&
                   A.EmptyCount > 0;
        }

        private static bool CanSupplyNG(BinInfo C, BinInfo B)
        {
            if (C == null || B == null)
                return false;

            if (!C.IsReady || C.IsGantryUsing)
                return false;

            if (!B.IsReady || B.IsGantryUsing)
                return false;

            C.RefreshCount();
            B.RefreshCount();

            return C.NGCount > 0 &&
                   B.EmptyCount > 0;
        }


        private static bool CanMainAB(BinInfo A, BinInfo B)
        {
            if (A == null || B == null)
                return false;

            if (!A.IsReady || A.IsGantryUsing)
                return false;

            if (!B.IsReady || B.IsGantryUsing)
                return false;

            A.RefreshCount();
            B.RefreshCount();

            bool aToB = A.NGCount > 0 && (B.EmptyCount + B.OKCount) > 0;

            bool bToA = B.OKCount > 0 && (A.EmptyCount + A.NGCount) > 0;

            return aToB || bToA;
        }

        private static SwapStack CreateSupplyOKStack(BinInfo C, BinInfo A)
        {
            if (C == null || A == null)
                return null;

            if (C.Role != BinRole.OK && C.Role != BinRole.NG)
            {
                // C có thể là tray hỗn hợp
                // nên không ép Role ở đây.
            }

            if (C.Status != BinStatus.Processing ||
                A.Status != BinStatus.Processing)
            {
                return null;
            }

            C.RefreshCount();
            A.RefreshCount();

            int count = Math.Min(Math.Min(C.OKCount, A.EmptyCount), 2);

            if (count <= 0)
                return null;

            SwapStack stack = new SwapStack
            {
                BinOK = C,
                BinNG = A,
                Mode = SwapMode.SupplyOK
            };

            if (!AddOK(stack, C, A, count))
                return null;

            return stack.Empty ? null : stack;
        }

        private static SwapStack CreateSupplyNGStack(BinInfo C, BinInfo B)
        {
            if (C == null || B == null)
                return null;

            if (C.Status != BinStatus.Processing ||
                B.Status != BinStatus.Processing)
            {
                return null;
            }

            C.RefreshCount();
            B.RefreshCount();

            int count = Math.Min(Math.Min(C.NGCount, B.EmptyCount), 2);

            if (count <= 0)
                return null;

            SwapStack stack = new SwapStack
            {
                BinOK = C,
                BinNG = B,
                Mode = SwapMode.SupplyNG
            };

            if (!AddNG(stack, C, B, count))
                return null;

            return stack.Empty ? null : stack;
        }

        #endregion


        public static SwapStack CreateNextStack(PairCandidate pair)
        {
            if (pair == null)
                return null;
            switch (pair.Type)
            {
                case PairType.OKNG:
                    return CreateSwapStack(pair.Bin1, pair.Bin2, false);
                case PairType.OKOK:
                    return CreateSwapStack(pair.Bin1, pair.Bin2, true);
                case PairType.SupplyOK:
                    // C -> A
                    return CreateSupplyOKStack(pair.Bin1, pair.Bin2);


                case PairType.SupplyNG:
                    // C -> B
                    return CreateSupplyNGStack(pair.Bin1, pair.Bin2);
            }
            return null;
        }

        private static SwapStack CreateSwapStack(BinInfo binOK, BinInfo binNG, bool bIsOKOK)
        {
            if (binOK == null || binNG == null)
            {
                EventMgr.Warn($"Invalid pair: OK or NG bin null");
                return null;
            }


            if (!bIsOKOK)
            {
                if (binOK.Role != BinRole.OK || binNG.Role != BinRole.NG)
                {
                    EventMgr.Warn($"Invalid bin role when swapstack");
                    return null;
                }
            }
            else
            {
                if (binOK.Role != BinRole.OK || binNG.Role != BinRole.OK)
                {
                    EventMgr.Warn($"Invalid bin role when swapstack");
                    return null;
                }
            }

            if (binOK == binNG)
            {
                EventMgr.Warn("Same bin in pair");
                return null;
            }

            if (binOK.Status != BinStatus.Processing || binNG.Status != BinStatus.Processing)
                return null;

            binOK.RefreshCount();
            binNG.RefreshCount();


            Capacity capacity = CalculateCapacity(binOK, binNG, bIsOKOK);
            if (capacity.OK == 0 && capacity.NG == 0)
                return null;

            SwapMode mode = SelectOKNGMode(capacity, bIsOKOK);
            SwapStack stack = new SwapStack()
            {
                BinOK = binOK,
                BinNG = binNG,
                Mode = mode
            };
            bool bRet1 = true;
            bool bRet2 = true;
            switch (mode)
            {
                case SwapMode.OK2NG2:
                    bRet1 = AddOK(stack, binNG, binOK, 2);
                    bRet2 = AddNG(stack, binOK, binNG, 2);
                    break;

                case SwapMode.OK2NG1:
                    bRet1 = AddOK(stack, binNG, binOK, 2);
                    bRet2 = AddNG(stack, binOK, binNG, 1);
                    break;

                case SwapMode.OK1NG2:
                    bRet1 = AddOK(stack, binNG, binOK, 1);
                    bRet2 = AddNG(stack, binOK, binNG, 2);
                    break;

                case SwapMode.OK1NG1:
                    bRet1 = AddOK(stack, binNG, binOK, 1);
                    bRet2 = AddNG(stack, binOK, binNG, 1);
                    break;

                case SwapMode.OK2:
                    bRet1 = AddOK(stack, binNG, binOK, 2);
                    break;

                case SwapMode.NG2:
                    bRet2 = AddNG(stack, binOK, binNG, 2);
                    break;

                case SwapMode.OK1:
                    bRet1 = AddOK(stack, binNG, binOK, 1);
                    break;

                case SwapMode.NG1:
                    bRet2 = AddNG(stack, binOK, binNG, 1);
                    break;
                default:
                    return null;
            }

            if (!bRet1 || !bRet2)
            {
                EventMgr.Error($"Create stack failed {binOK.Name},{binNG.Name}");
                return null;
            }

            if (stack.Empty)
                return null;

            return stack;
        }
        private static Capacity CalculateCapacity(BinInfo target, BinInfo source, bool isOKOK)
        {
            if (target == null || source == null)
                return new Capacity();

            target.RefreshCount();
            source.RefreshCount();

            int okMove = Math.Min(source.OKCount, target.EmptyCount + target.NGCount);

            if (isOKOK)
            {

                int ngMove = Math.Min(target.NGCount, source.EmptyCount + okMove);
                if (okMove <= 0)
                    return new Capacity();

                return new Capacity()
                {
                    OK = okMove,
                    NG = ngMove
                };
            }
            int ngMoveOKNG = Math.Min(target.NGCount, source.EmptyCount + source.OKCount);

            return new Capacity()
            {
                OK = okMove,
                NG = ngMoveOKNG
            };
        }

private static SwapMode SelectOKNGMode(Capacity c, bool bIsOKOK)
{
    if (c == null) return SwapMode.Nothing;

    if (bIsOKOK)
    {
        if (c.OK <= 0) return SwapMode.Nothing;
        
        return (c.OK, c.NG) switch
        {
            ( >= 2,  >= 2) => SwapMode.OK2NG2,
            ( >= 2,  >= 1) => SwapMode.OK2NG1,
            ( >= 2,   _  ) => SwapMode.OK2,
            (  1,    >= 2) => SwapMode.OK1NG2,
            (  1,    >= 1) => SwapMode.OK1NG1,
            _              => SwapMode.Nothing
        };
    }

    // Trường hợp thông thường (!bIsOKOK)
    return (c.OK, c.NG) switch
    {
        ( >= 2,  >= 2) => SwapMode.OK2NG2,
        ( >= 2,  >= 1) => SwapMode.OK2NG1,
        ( >= 1,  >= 2) => SwapMode.OK1NG2,
        ( >= 1,  >= 1) => SwapMode.OK1NG1,
        ( >= 2,   _  ) => SwapMode.OK2,
        (  _  ,  >= 2) => SwapMode.NG2,
        ( >= 1,   _  ) => SwapMode.OK1,
        (  _  ,  >= 1) => SwapMode.NG1,
        _              => SwapMode.Nothing
    };
}

        private static bool AddOK(SwapStack stack, BinInfo binNG, BinInfo binOK, int count)
        {
            var from = FindSourceSlots(binNG, SlotState.OK, count);
            if (from.Count != count)
            {
                EventMgr.Warn($"Not enough OK slot {binNG.Name}, need {count}, got {from.Count}");
                return false;
            }
            for (int i = 0; i < from.Count; i++)
            {
                VacuumTask task = new VacuumTask()
                {
                    VacuumId = i + 3,

                    Source = binNG,

                    SourceSlot = from[i],

                    Target = binOK,

                    Material = SlotState.OK,

                    NGInfo = binNG.GetSlotNGReason(from[i]),

                    SN = binNG.GetSlotSN(from[i])
                };


                stack.SetOK(i, task);
            }
            return true;
        }

        private static bool AddNG(SwapStack stack, BinInfo binOK, BinInfo binNG, int count)
        {
            var from = FindSourceSlots(binOK, SlotState.NG, count);
            if (from.Count != count)
            {
                EventMgr.Warn($"Not enough NG slot {binOK.Name}, need {count}, got {from.Count}");
                return false;
            }
            for (int i = 0; i < from.Count; i++)
            {
                VacuumTask task = new VacuumTask()
                {
                    VacuumId = i + 1,

                    Source = binOK,

                    SourceSlot = from[i],

                    Target = binNG,

                    Material = SlotState.NG,

                    NGInfo = binOK.GetSlotNGReason(from[i]),

                    SN = binOK.GetSlotSN(from[i])
                };

                stack.SetNG(i, task);
            }
            return true;
        }


        private static List<int> FindSourceSlots(BinInfo bin, SlotState material, int count)
        {
            List<int> result = new List<int>();

            for (int r = 0; r < bin.Row; r++)
            {
                for (int c = 0; c < bin.Column; c++)
                {
                    if (bin.GetSlotState(r, c) == material)
                    {
                        result.Add(r * bin.Column + c);

                        if (result.Count == count)
                            return result;
                    }
                }
            }

            return result;
        }

        public static PairState CheckPairState(PairCandidate pair)
        {
            if (pair == null)
                return PairState.NoMove;

            switch (pair.Type)
            {
                case PairType.OKNG:
                    BinInfo okBin = pair.Bin1.Role == BinRole.OK ? pair.Bin1 : pair.Bin2;
                    BinInfo ngBin = pair.Bin1.Role == BinRole.NG ? pair.Bin1 : pair.Bin2;

                    if (okBin.IsOKFull || ngBin.IsNGFull)
                        return PairState.Completed;
                    bool okMove = ngBin.OKCount > 0 && (okBin.EmptyCount + okBin.NGCount) > 0;

                    bool ngMove = okBin.NGCount > 0 && (ngBin.EmptyCount + ngBin.OKCount) > 0;

                    return okMove || ngMove ? PairState.Working : PairState.NoMove;

                case PairType.OKOK:
                    BinInfo OkBin = pair.Bin1;

                    BinInfo sourceBin = pair.Bin2;

                    // có tray nào full OK

                    if (OkBin.IsOKFull)
                    {
                        return PairState.Completed;
                    }
                    if (sourceBin.OKCount == 0)
                    {
                        return PairState.NoMove;
                    }
                    // còn slot để gom không
                    bool canMoveOK = sourceBin.OKCount > 0 && (OkBin.EmptyCount + OkBin.NGCount) > 0;

                    if (canMoveOK)
                        return PairState.Working;

                    return PairState.NoMove;

                case PairType.SupplyOK:
                    BinInfo C = pair.Bin1;
                    BinInfo A = pair.Bin2;

                    if (C == null || A == null)
                        return PairState.NoMove;

                    C.RefreshCount();
                    A.RefreshCount();

                    // A đã full -> pair kết thúc
                    if (A.IsOKFull)
                        return PairState.Completed;

                    // C không còn OK -> pair kết thúc
                    if (C.OKCount <= 0)
                        return PairState.NoMove;

                    // C còn OK nhưng A không còn chỗ
                    if (A.EmptyCount <= 0)
                        return PairState.NoMove;

                    return PairState.Working;


                case PairType.SupplyNG:
                    C = pair.Bin1;
                    BinInfo B = pair.Bin2;
                    if (C == null || B == null)
                        return PairState.NoMove;

                    C.RefreshCount();
                    B.RefreshCount();

                    // B đã full
                    if (B.IsNGFull)
                        return PairState.Completed;

                    // C hết NG
                    if (C.NGCount <= 0)
                        return PairState.NoMove;

                    // B hết chỗ
                    if (B.EmptyCount <= 0)
                        return PairState.NoMove;

                    return PairState.Working;

                default:
                    return PairState.NoMove;
            }
        }

//        PreparePlace bắt đầu:

//enumerator = EnumerateEmptySlots(bin, stack)    ← Chưa chạy gì!

//Task 1: emptySlots.MoveNext()
//  → Iterator bắt đầu chạy vòng for: i=0, i=1, ... i=45
//  → Ô 45 trống! yield return 45
//  → ★ ĐÓNG BĂNG tại i = 45, lưu toàn bộ trạng thái ★
//  → Trả 45 về cho caller
//  → stack.ReservePlace(bin, 45)

//Task 2: emptySlots.MoveNext()
//  → ★ GIẢI BĂNG — tiếp tục từ i = 46 ★ (KHÔNG quét lại từ 0!)
//  → Ô 46 trống! yield return 46
//  → ★ ĐÓNG BĂNG tại i = 46 ★
//  → Trả 46 về cho caller
//  → stack.ReservePlace(bin, 46)

//Tổng: 47 phép so sánh(thay vì 93)

        public static void PreparePlace(SwapStack stack, bool bIsUZ12)
        {

            var emptySlots = FindEmptySlot(stack.Tasks().First(t => bIsUZ12 ? (t.VacuumId <= 2) : (t.VacuumId >= 3)).Target, stack).GetEnumerator();

            foreach (var task in stack.Tasks())
            {
                if (bIsUZ12 && (task.VacuumId == 3 || task.VacuumId == 4)) continue;
                if (!bIsUZ12 && (task.VacuumId == 1 || task.VacuumId == 2)) continue;

                if (!emptySlots.MoveNext())
                    throw new InvalidOperationException("No empty slot after pick.");

                int empty = emptySlots.Current;
                stack.ReservePlace(task.Target, empty);
                task.TargetSlot = empty;
            }

            emptySlots.Dispose();
            stack.SetPhase(bIsUZ12 ? StackPhase.PlacingSource : StackPhase.PlacingTarget);
        }

        private static IEnumerable<int> FindEmptySlot(BinInfo bin, SwapStack stack)
        {

            for (int i = 0; i < bin.TraySlotCount; i++)
            {
                if (bin.GetSlotState(i) == SlotState.NA && !stack.IsPlaceReserved(bin, i))
                {
                    yield return i;
                }
            }

        }



        public static bool CompareMatrix(BinInfo bin, string[] data, out List<int> mismatchIdxs)
        {
            mismatchIdxs = new List<int>();
            if (bin == null || data == null)
                return false;
            if (data.Length != bin.TraySlotCount)
                return false;

            for (int i = 0; i < data.Length; i++)
            {
                SlotState state = bin.GetSlotState(i);

                int expected = state == SlotState.NA ? 0 : 1;

                int actual = data[i] == "1" ? 1 : 0;

                if (expected != actual)
                {
                    mismatchIdxs.Add(i);
                }
            }

            return mismatchIdxs.Count == 0;
        }

    }


    public enum BinRegion
    {
        A = 1,
        B,
        C
    }

    public enum LogType
    {
        Log,
        Warn,
        Error
    }

    /// <summary>
    /// GENERATE SWAP PLANNER
    /// </summary>
    public static class SwapController
    {
        private static ThreadEx runThread;

        public static BinInfo binA;
        public static BinInfo binB;
        public static BinInfo binC;
        public static readonly ConcurrentQueue<SwapStack> swapQueue = new ConcurrentQueue<SwapStack>();

        private static int _unloadMode = 0;

        private static PairContext _currentPair;

        private static SwapStack _currentStack = null;
        public static PairContext CurrentPair
        {
            get
            {
                return _currentPair;
            }
        }
        public static SwapStack CurrentStack
        {
            get
            {
                return _currentStack;
            }
        }

        public static int UnloadMode => _unloadMode;

        public static bool b_DisableToPair = false;

        public delegate void ShowStackLogDelegate(string strLog, LogType type = LogType.Log);

        public static event ShowStackLogDelegate ShowStackLogEvent;

        private static string strLogPath = "";
        private static readonly object _pairLock = new object();
        private static bool _stackWaiting;       // Có stack đang gửi cho gantry
        private static bool _waitingStackDone;   // Planner đang đứng ở WaitOne

        private static volatile bool _running;

        private static readonly object _stackLock = new object();
        private static readonly AutoResetEvent stackDone = new AutoResetEvent(false);
        static SwapController()
        {
            strLogPath = @"D:\exe\Log\SwapProductInfo";
            runThread = new ThreadEx();
        }

        public static void Start()
        {
            _running = true;

            lock (_stackLock)
            {
                _stackWaiting = false;
                _waitingStackDone = false;
            }

            _currentPair = null;
            _currentStack = null;

            while (swapQueue.TryDequeue(out _))
            {
            }

            // Xóa signal cũ tránh WaitOne ăn nhầm
            while (stackDone.WaitOne(0))
            {
            }

            _unloadMode = Parameter.GetInt("UnloadMode");
            runThread.Start(WorkLoop, 100, "MonitorBinSwap");
            ShowLog("SWAP THREAD START!", LogType.Warn);
        }

        public static void Stop()
        {
            ShowLog("SWAP THREAD STOP!", LogType.Error);
            _running = false;

            lock (_stackLock)
            {
                _stackWaiting = false;
                _waitingStackDone = false;
            }

            // đánh thức planner nếu đang đợi
            stackDone.Set();

            bool stopped = runThread.Stop();
            if (!stopped)
                EventMgr.Error("MonitorBinSwap stop timeout");

            binA?.UnlockGantry();
            binB?.UnlockGantry();
            binC?.UnlockGantry();

            _currentStack = null;

            if (_currentPair != null)
                FinishPair();

            while (swapQueue.TryDequeue(out _))
            {
            }

            // tránh signal tồn
            while (stackDone.WaitOne(0))
            {
            }

        }

        private static void WorkLoop()
        {
            if (!_running)
                return;

            try
            {
                // Không có pair đang chạy
                if (_currentPair == null)
                {

                    PairCandidate bestPair = null;

                    switch (_unloadMode)
                    {
                        case 0:
                            bestPair = SwapTrayHelper.FindBest(binA, binB, binC);
                            break;
                        case 1:
                            bestPair = SwapTrayHelper.FindBestVer2(binA, binB, binC);
                            break;
                        default:
                            break;
                    }


                    if (bestPair == null)
                    {
                        if (binA != null && binA.IsReady && binB != null && binB.IsReady && binC != null && binC.IsReady)
                            b_DisableToPair = true;
                        else
                            b_DisableToPair = false;

                        Thread.Sleep(10);
                        return;
                    }

                    b_DisableToPair = false;
                    if (!LockPairGantry(bestPair))
                    {
                        return;
                    }
                    _currentPair = new PairContext(bestPair);
                    ReservePair(_currentPair.Pair);

                    ShowLog($"CURRENT PAIR:{_currentPair.PairName} START...", LogType.Warn);
                }

                ExecuteCurrentPair();

                if (_currentPair != null)
                {
                    if (_currentPair.Pair.Type == PairType.OKOK)
                    {
                        if (binB.IsReady)
                        {
                            FinishPair();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                EventMgr.Error($"SwapController error : {ex}");
                _currentPair = null;
            }

        }
        private static void ExecuteCurrentPair()
        {
            if (_currentPair == null)
                return;

            // Check trạng thái pair
            PairState state = SwapTrayHelper.CheckPairState(_currentPair.Pair);

            if (state == PairState.Completed || state == PairState.NoMove)
            {
                FinishPair();
                return;
            }

            lock (_stackLock)
            {
                if (_stackWaiting)
                    return;
            }

            // Tạo stack tiếp theo       
            SwapStack stack = SwapTrayHelper.CreateNextStack(_currentPair.Pair);
            if (stack == null)
            {
                _currentPair.RetryCount++;
                if (_currentPair.RetryCount > 5)
                {
                    EventMgr.Error("Planner stuck");
                    FinishPair();
                    return;
                }
                return;
            }
            stack.SetPhase(StackPhase.Created);
            _currentPair.RetryCount = 0;

            // Queue cho gantry
            lock (_stackLock)
            {
                if (_stackWaiting)
                    return;
                _stackWaiting = true;

                if (swapQueue.Count > 0)
                {
                    EventMgr.Error($"ABNORMAL: Queue already contains item. Count={swapQueue.Count}. Enqueue blocked.");
                    return;
                }

                swapQueue.Enqueue(stack);

                //ShowLog($"ENQUEUE {stack.Info}, Queue={swapQueue.Count}");             
            }

            _currentPair.IncreaseStack();

            // ============================
            // WAIT GANTRY
            // ============================
            lock (_stackLock)
            {
                _waitingStackDone = true;
            }

            stackDone.WaitOne();

            lock (_stackLock)
            {
                _waitingStackDone = false;
            }


            if (!_running)
            {
                return;
            }

            Thread.Sleep(100);
            state = SwapTrayHelper.CheckPairState(_currentPair.Pair);

            if (state == PairState.Completed)
                FinishPair();

        }

        public static bool TryGetSwapStack(out SwapStack stack)
        {
            stack = null;

            lock (_stackLock)
            {
                if (!_stackWaiting)
                    return false;

                if (_currentPair == null)
                    return false;

                if (!_currentPair.Pair.Bin1.IsGantryUsing || !_currentPair.Pair.Bin2.IsGantryUsing)
                {
                    return false;
                }

                if (!swapQueue.TryDequeue(out stack))
                    return false;
                _currentStack = stack;

                return true;
            }
        }

        public static void NotifyStackDone(SwapStack stack, bool success = true)
        {
            bool wake = false;

            lock (_stackLock)
            {
                _stackWaiting = false;

                if (_waitingStackDone)
                {
                    wake = true;
                }
            }

            _currentStack = null;

            if (wake)
            {
                stackDone.Set();
            }
            //ShowLog($"DONE {stack.Info}, Queue={swapQueue.Count}" + Environment.NewLine);
        }

        private static void FinishPair()
        {
            PairContext pair = _currentPair;

            if (pair == null)
                return;

            lock (_stackLock)
            {
                if (_stackWaiting || _waitingStackDone)
                {
                    ShowLog("Cannot finish pair while stack running", LogType.Error);
                    return;
                }
            }

            ReleasePair(pair.Pair);

            // giải phóng quyền gantry
            UnlockPairGantry(pair.Pair);

            pair.Finish();
            ShowLog($"PAIR:{pair.PairName} FINISHED\r\n", LogType.Warn);
            _currentPair = null;

        }

        private static bool LockPairGantry(PairCandidate pair)
        {
            if (pair == null)
                return false;

            if (!pair.Bin1.LockGantry())
            {
                return false;
            }

            if (!pair.Bin2.LockGantry())
            {
                pair.Bin1.UnlockGantry();
                return false;
            }

            return true;
        }



        private static void UnlockPairGantry(PairCandidate pair)
        {
            if (pair == null)
                return;

            pair.Bin1.UnlockGantry();

            pair.Bin2.UnlockGantry();
        }

        private static void ReservePair(PairCandidate pair)
        {
            pair.Bin1.SetStatus(BinStatus.Processing);
            pair.Bin2.SetStatus(BinStatus.Processing);
        }


        private static void ReleasePair(PairCandidate pair)
        {

            ReleaseBin(pair.Bin1);

            ReleaseBin(pair.Bin2);
        }

        private static void ReleaseBin(BinInfo bin)
        {
            switch (_unloadMode)
            {
                case 0:
                    if (bin.IsOKFull || bin.IsNGFull)
                    {
                        bin.SetStatus(BinStatus.ProcessCompleted);
                    }
                    else
                    {
                        bin.SetStatus(BinStatus.Ready);
                    }
                    break;
                case 1:
                    if (bin == null)
                        return;

                    bin.RefreshCount();

                    // Tray C
                    if (bin == binC)
                    {
                        if (bin.OKCount == 0 && bin.NGCount == 0)
                        {
                            bin.SetStatus(BinStatus.ProcessCompleted);
                        }
                        else
                        {
                            bin.SetStatus(BinStatus.Ready);
                        }

                        return;
                    }

                    // Tray A / B
                    if (bin.IsOKFull || bin.IsNGFull)
                    {
                        bin.SetStatus(BinStatus.ProcessCompleted);
                    }
                    else
                    {
                        bin.SetStatus(BinStatus.Ready);
                    }
                    break;
                default:
                    if (bin.IsOKFull || bin.IsNGFull)
                    {
                        bin.SetStatus(BinStatus.ProcessCompleted);
                    }
                    else
                    {
                        bin.SetStatus(BinStatus.Ready);
                    }
                    break;
            }

        }

        public static void ShowLog(string strLog, LogType type = LogType.Log)
        {
            if (!Directory.Exists(strLogPath))
                Directory.CreateDirectory(strLogPath);

            string filePath = Path.Combine(strLogPath, $"SwapProductInfo{DateTime.Now.ToString("yyyyMMdd")}.txt");

            string strFullLog = DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff") + "-----" + strLog;

            if (!File.Exists(filePath))
            {
                using (File.Create(filePath)) ;
            }

            if (ShowStackLogEvent != null)
            {
                ShowStackLogEvent.Invoke(strFullLog, type);
            }

            try
            {
                File.AppendAllText(filePath, strFullLog + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static void ShowLogWithCheck(string strLog, LogType type = LogType.Log)
        {
            if (_currentPair == null)
                return;
            ShowLog(strLog, type);
        }
    }
}
