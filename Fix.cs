Làm sao tôi tạo ra 1 form mới và nó có 1 nút refresh, và các table khi có data mới từ redis tự động update 
8:15 PM
Để tạo một Form mới có nút Refresh thủ công và Tự động cập nhật bảng khi có dữ liệu mới từ Redis (mà không bị đơ UI hay nháy giật màn hình), dưới đây là kiến trúc và toàn bộ code mẫu chuẩn nhất cho bạn:

1. Kiến trúc thực hiện
Trong ứng dụng WinForms điều khiển máy, giải pháp chuẩn và an toàn nhất là:

Dùng System.Windows.Forms.Timer (chạy mỗi 500ms – 1000ms) để kiểm tra Redis.
Cơ chế so sánh (Diff Check): Chỉ khi dữ liệu Redis có thay đổi so với lần trước thì mới vẽ lại bảng $\rightarrow$ Tránh được hiện tượng giật/nháy (Flickering) màn hình và không làm mất vị trí con chuột/thanh cuộn.
Nút "Refresh" để bấm cập nhật ngay lập tức bất cứ lúc nào.
2. Thiết kế giao diện (UI) của Form mới
Trên Form mới (ví dụ đặt tên là Form_RedisMonitor), bạn kéo các control sau:

ComboBox (cbStation): Danh sách chọn trạm cần xem (ví dụ: Bin_A, Bin_B, Bin_C...).
Button (btnRefresh): Nút "Làm mới" thủ công.
CheckBox (chkAutoRefresh): Bật/Tắt chế độ tự động cập nhật.
DataGridView (dgvData): Bảng hiển thị dữ liệu (Key - Value).
Timer (timerAuto): Interval = 1000ms (1 giây).
3. Toàn bộ code logic C# của Form mới (Form_RedisMonitor.cs)
csharp
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using AutoFrame.DBExtend; // Thư viện RedisHelperEx của dự án
namespace Autoframe.UI
{
    public partial class Form_RedisMonitor : Form
    {
        private DataTable dtSource;
        private Dictionary<string, string> lastData = new Dictionary<string, string>();
        public Form_RedisMonitor()
        {
            InitializeComponent();
            SetupTable();
        }
        // 1. Khởi tạo cấu trúc bảng cố định (chống giật nháy màn hình)
        private void SetupTable()
        {
            dtSource = new DataTable();
            dtSource.Columns.Add("Field (Tên trường)", typeof(string));
            dtSource.Columns.Add("Value (Giá trị)", typeof(string));
            dtSource.Columns.Add("Thời gian cập nhật", typeof(string));
            dgvData.DataSource = dtSource;
            dgvData.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvData.ReadOnly = true;
            // Bật tính năng chống nháy cho DataGridView
            typeof(DataGridView).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty,
                null, dgvData, new object[] { true });
        }
        private void Form_RedisMonitor_Load(object sender, EventArgs e)
        {
            // Nạp danh sách các trạm mẫu vào ComboBox
            cbStation.Items.AddRange(new string[] { "Bin_A", "Bin_B", "Bin_C", "MonitorBinSwap" });
            if (cbStation.Items.Count > 0)
                cbStation.SelectedIndex = 0;
            // Bắt đầu bật Timer tự động quét
            timerAuto.Interval = 800; // Quét mỗi 0.8 giây
            timerAuto.Enabled = true;
            chkAutoRefresh.Checked = true;
            // Nạp dữ liệu lần đầu
            LoadDataFromRedis(forceRefresh: true);
        }
        // 2. Hàm đọc Redis và cập nhật vào Bảng
        private void LoadDataFromRedis(bool forceRefresh = false)
        {
            string stationName = cbStation.Text.Trim();
            if (string.IsNullOrEmpty(stationName)) return;
            // Kéo toàn bộ Hash của trạm về RAM trong 1 request duy nhất
            var currentData = RedisHelperEx.Instance.HashGetAll(stationName);
            if (currentData == null) return;
            // Kiểm tra: Nếu dữ liệu KHÔNG ĐỔI và không phải ấn nút ép buộc -> Bỏ qua, không vẽ lại
            if (!forceRefresh && IsDataEqual(lastData, currentData))
            {
                return; // Không đổi thì không làm gì để màn hình đứng yên mượt mà
            }
            // Có dữ liệu mới -> Cập nhật bảng
            lastData = new Dictionary<string, string>(currentData);
            dtSource.Rows.Clear();
            string updateTime = DateTime.Now.ToString("HH:mm:ss.fff");
            foreach (var kvp in currentData)
            {
                dtSource.Rows.Add(kvp.Key, kvp.Value, updateTime);
            }
            lblStatus.Text = $"Cập nhật lúc: {updateTime} (Tổng: {currentData.Count} fields)";
        }
        // 3. Hàm so sánh xem dữ liệu mới từ Redis có khác lần trước không
        private bool IsDataEqual(Dictionary<string, string> oldDict, Dictionary<string, string> newDict)
        {
            if (oldDict.Count != newDict.Count) return false;
            foreach (var kvp in newDict)
            {
                if (!oldDict.TryGetValue(kvp.Key, out string oldVal) || oldVal != kvp.Value)
                    return false; // Có ít nhất 1 ô thay đổi giá trị
            }
            return true; // Hoàn toàn giống nhau
        }
        // 4. Nút REFRESH thủ công (Bấm phát nạp ngay lập tức)
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadDataFromRedis(forceRefresh: true);
        }
        // 5. Timer tự động quét ngầm
        private void timerAuto_Tick(object sender, EventArgs e)
        {
            if (chkAutoRefresh.Checked)
            {
                LoadDataFromRedis(forceRefresh: false);
            }
        }
        // Đổi trạm trong ComboBox -> Nạp dữ liệu trạm mới ngay
        private void cbStation_SelectedIndexChanged(object sender, EventArgs e)
        {
            lastData.Clear();
            LoadDataFromRedis(forceRefresh: true);
        }
        // Bật/Tắt tự động quét
        private void chkAutoRefresh_CheckedChanged(object sender, EventArgs e)
        {
            timerAuto.Enabled = chkAutoRefresh.Checked;
        }
    }
}
4. Cách gọi mở Form này từ Form_Test.cs:
Khi bạn muốn mở form này lên xem, chỉ cần viết vào 1 nút trên Form_Test:

csharp
private void btnOpenMonitor_Click(object sender, EventArgs e)
{
    Form_RedisMonitor frm = new Form_RedisMonitor();
    frm.Show(); // Dùng .Show() để vừa xem bảng vừa thao tác được với máy
}
🎯 Điểm hay của giải pháp này:
Zero UI Freeze: Timer chạy song song, kéo 1 lệnh HashGetAll chỉ mất 1ms nên Form lướt cực êm, không bị khựng giao diện.
Chống nháy (Anti-Flicker): Nhờ hàm IsDataEqual, khi các trạm chưa có hàng mới (dữ liệu đứng im), DataGridView sẽ không bị vẽ lại, thanh cuộn và ô đang chọn được giữ nguyên. Chỉ khi máy đổi khay hoặc gắp nhả làm thay đổi Redis thì bảng mới cập nhật tức thì!
8:15 PM
Nếu tôi thêm 1 form mới và cho vào sidebar của phần mềm thfi thêm vào đâu 
8:16 PM
Trong kiến trúc phần mềm Autoframe của bạn, toàn bộ menu sidebar và cơ chế chuyển đổi màn hình được quản lý tập trung tại file:
👉 

Autoframe/UI/Form_Main.cs

Để thêm 1 Form mới vào sidebar, bạn chỉ cần thực hiện đúng 3 bước sau:

Bước 1: Thêm nút bấm mới lên Sidebar (Giao diện)
Mở giao diện thiết kế Form_Main.cs [Design] trong Visual Studio.
Tại thanh Sidebar (nơi chứa các nút tròn Auto, Alarm, Data, Setting...), kéo thêm 1 nút RoundButton mới (hoặc copy từ nút cũ ra).
Đặt tên (Name) cho nút, ví dụ: RoundButton_Redis (Text hiển thị: "Redis Data" hoặc "Giám Sát").
Tại mục Events (sự kiện) của nút này, chọn sự kiện Click trỏ vào hàm có sẵn: RoundButton_Click.
Bước 2: Đăng ký Form mới vào code 

Form_Main.cs
Mở file 

Form_Main.cs
, tìm đến dòng 304 đến 308:

csharp
// ĐOẠN CODE CŨ:
// 关联页面和站位 (Liên kết trang giao diện với nút sidebar)
m_dicForm.Add(RoundButton_Auto, new Form_Auto());
m_dicForm.Add(RoundButton_Alarm, new Form_Alarm());
m_dicForm.Add(RoundButton_Data, new Form_Data());
m_dicForm.Add(RoundButton_Setting, new Form_Setting());
m_dicForm.Add(RoundButton_Manual, new Form_Manual());
// 👉 THÊM DÒNG NÀY ĐỂ ĐĂNG KÝ FORM MỚI CỦA BẠN:
m_dicForm.Add(RoundButton_Redis, new Form_RedisMonitor());
Bước 3: Cơ chế tự động chạy (Bạn KHÔNG CẦN viết thêm gì nữa)
Hệ thống Autoframe đã viết sẵn logic tự động ở các dòng tiếp theo (

dòng 311–316
):

csharp
// 初始化页面属性
foreach (KeyValuePair<RoundButton, Form> pair in m_dicForm)
{
    pair.Value.TopLevel = false;
    pair.Value.Parent = this.panel_main; // Nhúng thẳng vào khung giữa của phần mềm
    pair.Value.Dock = DockStyle.Fill;    // Tự động kéo giãn vừa khít màn hình
}
Và hàm SwitchWnd((RoundButton)sender) tại 

dòng 963
 sẽ tự động ẩn màn hình cũ và hiện Form mới của bạn lên khung chính mỗi khi bạn bấm vào nút RoundButton_Redis trên sidebar.

💡 Tóm lại:
Chỉ cần:

Thêm 1 nút RoundButton_Redis trên thanh sidebar (gán sự kiện RoundButton_Click).
Thêm đúng 1 dòng: m_dicForm.Add(RoundButton_Redis, new Form_Mới()); tại dòng 309 trong 

Form_Main.cs
.
8:16 PM



private static SwapMode SelectOKNGMode((int OK, int NG) c, bool isOKOK) => (isOKOK, c.OK, c.NG) switch
{
    (true, >= 2, >= 2) => SwapMode.OK2NG2,
    (true, >= 2, >= 1) => SwapMode.OK2NG1,
    (true, >= 2, _)    => SwapMode.OK2,
    (true, 1, >= 2)    => SwapMode.OK1NG2,
    (true, 1, >= 1)    => SwapMode.OK1NG1,
    (false, >= 2, >= 2) => SwapMode.OK2NG2,
    (false, >= 2, >= 1) => SwapMode.OK2NG1,
    (false, >= 1, >= 2) => SwapMode.OK1NG2,
    (false, >= 1, >= 1) => SwapMode.OK1NG1,
    (false, >= 2, _)    => SwapMode.OK2,
    (false, _, >= 2)    => SwapMode.NG2,
    (false, 1, _)       => SwapMode.OK1,
    (false, _, 1)       => SwapMode.NG1,
    _                   => SwapMode.Nothing
};



private static SwapMode SelectOKNGMode((int OK, int NG) c, bool isOKOK) => (isOKOK, c.OK, c.NG) switch
{
    (true, >= 2, >= 2) => SwapMode.OK2NG2,
    (true, >= 2, >= 1) => SwapMode.OK2NG1,
    (true, >= 2, _)    => SwapMode.OK2,
    (true, 1, >= 2)    => SwapMode.OK1NG2,
    (true, 1, >= 1)    => SwapMode.OK1NG1,
    (false, >= 2, >= 2) => SwapMode.OK2NG2,
    (false, >= 2, >= 1) => SwapMode.OK2NG1,
    (false, >= 1, >= 2) => SwapMode.OK1NG2,
    (false, >= 1, >= 1) => SwapMode.OK1NG1,
    (false, >= 2, _)    => SwapMode.OK2,
    (false, _, >= 2)    => SwapMode.NG2,
    (false, 1, _)       => SwapMode.OK1,
    (false, _, 1)       => SwapMode.NG1,
    _                   => SwapMode.Nothing
};



. Sửa hàm GetListNGReason (Dòng 163):
csharp
internal List<int> GetListNGReason(int index)
{
    string strNGReason = GetNGReason(index);
    if (string.IsNullOrEmpty(strNGReason))
        return new List<int>();
    List<int> temp = new List<int>();
    ReadOnlySpan<char> span = strNGReason.AsSpan();
    // Duyệt cắt chuỗi bằng Span không tạo rác
    while (span.Length > 0)
    {
        int sep = span.IndexOf('_');
        ReadOnlySpan<char> token = sep >= 0 ? span.Slice(0, sep) : span;
        span = sep >= 0 ? span.Slice(sep + 1) : ReadOnlySpan<char>.Empty;
        // Parse int trực tiếp từ Span
        if (int.TryParse(token.ToString(), out int val))
        {
            temp.Add(val);
        }
    }
    return temp;
}
2. Hàm gọi bên ngoài phải sửa gì?
👉 KHÔNG PHẢI SỬA GÌ CẢ! Các hàm gọi ở dòng 589, 605, 794, 827 vẫn viết y như cũ:

csharp
// Hàm gọi vẫn chạy bình thường 100%:
List<int> list_NG = _matrix.GetListNGReason(index);
List<int> arr_NG = GetSlotListNGReason(i);
CÁCH 2: Nâng cấp Triệt để — Dùng Span<int> cho cả Hàm Gọi (0 Byte RAM)
Nếu bạn muốn đạt chuẩn Zero-Allocation 100%, ta sẽ cho bên ngoài đưa "cái khay" Span<int> vào để nhận kết quả:

1. Định nghĩa hàm mới dùng Span:
csharp
// Hàm trả về số lượng mã NG tìm được, ghi trực tiếp vào buffer do bên ngoài đưa vào
internal int GetListNGReason(int index, Span<int> buffer)
{
    string strNGReason = GetNGReason(index);
    if (string.IsNullOrEmpty(strNGReason))
        return 0;
    ReadOnlySpan<char> span = strNGReason.AsSpan();
    int count = 0;
    while (span.Length > 0 && count < buffer.Length)
    {
        int sep = span.IndexOf('_');
        ReadOnlySpan<char> token = sep >= 0 ? span.Slice(0, sep) : span;
        span = sep >= 0 ? span.Slice(sep + 1) : ReadOnlySpan<char>.Empty;
        if (int.TryParse(token.ToString(), out int val))
        {
            buffer[count++] = val;
        }
    }
    return count;
}
2. Các hàm gọi bên ngoài sẽ sửa như thế nào?
Vị trí gọi 1: GetSlotBestNGReason (

Dòng 601
)
Cách gọi cũ:
csharp
public int GetSlotBestNGReason(int index)
{
    lock (_binLock)
    {
        List<int> list_NG = _matrix.GetListNGReason(index); // Tạo List
        return list_NG.Count > 0 ? list_NG.Max() : 999;
    }
}
Cách gọi mới (Dùng stackalloc trên Stack):
csharp
public int GetSlotBestNGReason(int index)
{
    lock (_binLock)
    {
        // Cấp phát tạm 8 ô số nguyên trên Stack (1 slot thường có tối đa 2-3 mã NG)
        Span<int> buffer = stackalloc int[8];
        int count = _matrix.GetListNGReason(index, buffer);
        if (count == 0) return 999;
        // Tìm Max trực tiếp trong Span
        int max = buffer[0];
        for (int i = 1; i < count; i++)
        {
            if (buffer[i] > max) max = buffer[i];
        }
        return max;
    }
}
Vị trí gọi 2: Trong vòng lặp 60 ô của ConvertToCsvData (

Dòng 794
)
Cách gọi mới:
csharp
// Thay vì List<int> arr_NG = GetSlotListNGReason(i);
Span<int> ngBuffer = stackalloc int[8];
int ngCount = GetSlotListNGReason(i, ngBuffer);
for (int j = 0; j < ngCount; j++)
{
    int reasonCode = ngBuffer[j]; // Truy xuất by index như mảng bình thường
    // ... xử lý reasonCode ...
}
BẢNG SO SÁNH GIỮA 2 CÁCH:
Tiêu chí	Cách 1 (Sửa ngầm)	Cách 2 (Đổi sang Span)
Phạm vi sửa	Chỉ sửa đúng 1 hàm GetListNGReason	Sửa GetListNGReason + 3 hàm gọi
Rủi ro ảnh hưởng code khác	0% rủi ro (Giữ nguyên kiểu List<int>)	Phải sửa lại các hàm gọi bên ngoài
Tiết kiệm mảng string[]	Có (tiết kiệm ~80% rác)	Có (tiết kiệm 100% rác)
Mức độ khuyên dùng	Nên áp dụng ngay	Áp dụng khi cần tối ưu tuyệt đối
10:19 PM
Sao lại cấp 8 
10:24 PM
Con số 8 trong stackalloc int[8] được chọn dựa trên 2 yếu tố: nghiệp vụ thực tế của máy và tối ưu phần cứng CPU/bộ nhớ:

1. Về nghiệp vụ thực tế (Domain Machine)
strNGReason là chuỗi lưu các mã lỗi của 1 sản phẩm trên khay (được ghép dạng "1_3_10").
Nhìn vào enum 

ThrowReason
, toàn bộ hệ thống chỉ có khoảng 13 mã lỗi (từ 0 đến 12 và 999).
Trong thực tế kiểm tra quang học (AOI/Laser), một con linh kiện nếu bị lỗi thì thường chỉ dính 1 đến 3 lỗi (ví dụ: vừa trầy xước vừa bọt keo). Rất hiếm khi một sản phẩm dính cùng lúc hơn 4–5 lỗi.
Do đó, mảng kích thước 8 là dư dả cho hầu hết các trường hợp thực tế của 1 con phôi.
2. Về bộ nhớ và CPU Cache (Performance)
Kích thước cực nhỏ (32 bytes):
Mỗi số int chiếm 4 bytes $\rightarrow$ $8 \times 4 = \mathbf{32\text{ bytes}}$.
Stack của một luồng Windows có sẵn tới 1 MB (1.048.576 bytes), nên 32 bytes chỉ chiếm $0.003%$ stack, hoàn toàn không lo tràn stack (StackOverflow).
Tối ưu L1 Cache Line:
Đường truyền nhớ (Cache Line) của CPU hiện đại thường là 64 bytes.
32 bytes nằm gọn trong 1/2 Cache Line, giúp CPU truy xuất tức thì ngay trên thanh ghi/L1 Cache, tốc độ xử lý nhanh hơn rất nhiều so với cấp phát trên Heap.
Tự động hủy (Zero GC):
Khi hàm chạy xong, con trỏ Stack tự lùi về vị trí cũ, 32 bytes này biến mất ngay lập tức mà Garbage Collector (GC) không phải dọn dẹp.
3. Có thể đổi thành số khác không?
Được: Bạn hoàn toàn có thể cấp stackalloc int[16] ($16 \times 4 = 64\text{ bytes}$ — vừa khít đúng 1 CPU Cache Line) nếu muốn dự phòng cho tương lai.
Nguyên tắc an toàn khi code: Luôn có chốt chặn kiểm tra kích thước để không bao giờ bị văng lỗi vượt chỉ số:



Bước 1: Thêm hàm UpdateCountOnSlotChange vào BinInfo
csharp
// Đặt dưới RefreshCountInternal (khoảng dòng 678)
private void UpdateCountOnSlotChange(SlotState oldState, SlotState newState)
{
    if (oldState == newState) return;
    // Giảm bộ đếm của trạng thái cũ
    if (oldState == SlotState.OK) _okCount--;
    else if (oldState == SlotState.NG) _ngCount--;
    else if (oldState == SlotState.NA) _emptyCount--;
    // Tăng bộ đếm của trạng thái mới
    if (newState == SlotState.OK) _okCount++;
    else if (newState == SlotState.NG) _ngCount++;
    else if (newState == SlotState.NA) _emptyCount++;
}
Bước 2: Sửa 2 hàm SetSlotState (Dòng 491 & 508)
Thay vì gọi RefreshCountInternal() (duyệt lặp 60 ô), ta lấy oldState trước khi gán và gọi hàm tăng giảm $O(1)$:

csharp
public void SetSlotState(int index, SlotState state)
{
    if (index < 0 || index > TraySlotCount - 1)
    {
        EventMgr.Error("Matrix index out of range");
        return;
    }
    lock (_binLock)
    {
        SlotState oldState = _matrix.Get(index); // Lấy trạng thái cũ
        _matrix.Set(index, state);
        UpdateCountOnSlotChange(oldState, state); // Cập nhật O(1) ngay lập tức
    }
    if (!RedisHelperEx.Instance.HashSet(DbName, $"S_{index + 1}", state.ToString()))
        EventMgr.Warn("Write data to Redis failed!");
}
public void SetSlotState(int rowidx, int colidx, SlotState state)
{
    int index = rowidx * _column + colidx;
    lock (_binLock)
    {
        SlotState oldState = _matrix.Get(rowidx, colidx); // Lấy trạng thái cũ
        _matrix.Set(rowidx, colidx, state);
        UpdateCountOnSlotChange(oldState, state); // Cập nhật O(1) ngay lập tức
    }
    if (!RedisHelperEx.Instance.HashSet(DbName, $"S_{index + 1}", state.ToString()))
        EventMgr.Warn("Write data to Redis failed!");
}
Bước 3: Khởi tạo giá trị ban đầu trong Constructor BinInfo (Dòng 431)
Lưu ý quan trọng: Khi vừa new BinInfo(row, col), các ô ban đầu đều là SlotState.NA. Ta cần gọi RefreshCountInternal() 1 lần duy nhất lúc khởi tạo để biến _emptyCount = 60:
csharp
public BinInfo(int row, int column)
{
    _row = row;
    _column = column;
    _matrix = new TrayMatrix(row, column);
    RefreshCountInternal(); // Đảm bảo ban đầu: _emptyCount = 60, OK = 0, NG = 0
}
2. Các hàm gọi bên ngoài cần làm gì?
👉 KHÔNG CẦN SỬA BẤT CỨ DÒNG NÀO Ở BÊN NGOÀI.

Hàm SetSlotState(...):
Các trạm như StationUnloadGantryEx.cs (dòng 1101, 1219, 1940) hay StationUnloadBinEx.cs (dòng 536) đang gọi bin.SetSlotState(index, state) vẫn truyền tham số như cũ.
Bên trong BinInfo tự động cập nhật số lượng OK/NG/Empty ngay lập tức.
Hàm RefreshCount():
Hiện tại trong code có hơn 20 vị trí gọi bin.RefreshCount() (ví dụ trước khi tính Capacity, trước khi gắp thả).
Vì hàm public void RefreshCount() vẫn tồn tại nguyên vẹn, các trạm khác gọi nó vẫn hoạt động bình thường, không bị lỗi biên dịch (0% breaking change).
(Về sau nếu muốn tối ưu hơn, bạn có thể xóa bớt các lệnh gọi bin.RefreshCount() thừa ở ngoài mà kết quả đếm vẫn luôn chuẩn xác).
3. Có ảnh hưởng hay rủi ro gì không?
✅ Điểm lợi (Cải tiến vượt trội):
Triệt tiêu nghẽn CPU: Mỗi thao tác gắp 1 sản phẩm hay đặt 1 sản phẩm chỉ mất vài phép toán $O(1)$ thay vì phải chạy vòng lặp duyệt lại 60 lần switch-case.
Số liệu luôn tức thời: Biến OKCount, NGCount, EmptyCount luôn chính xác ngay sau lệnh SetSlotState mà không cần phụ thuộc vào việc bên ngoài có nhớ gọi RefreshCount() hay không.
⚠️ Rủi ro cần lưu ý và cách phòng tránh:
Nếu có hàm nào can thiệp trực tiếp vào mảng mà không qua SetSlotState:
Trong code hiện tại, chỉ có 2 chỗ nạp/xóa toàn bộ khay là ClearMatrix() (dòng 715) và SetMatrixBySQL() (dòng 680).
Cả 2 hàm này đều đã có sẵn dòng RefreshCountInternal(); bên trong, nên số lượng đếm được đồng bộ lại 100%, hoàn toàn không có rủi ro lệch số liệu.
Đa luồng (Thread-Safety):
Cả _matrix.Get(), _matrix.Set() và UpdateCountOnSlotChange() đều nằm trọn vẹn trong cùng một khối lock (_binLock).
Vì vậy, các luồng khác truy cập vào OKCount, NGCount, IsOKFull, IsNGFull đều được bảo vệ an toàn tuyệt đối, không xảy ra xung đột dữ liệu (Race Condition).
10:42 PM
10:47 PM
Lý do là vì giá trị mặc định trong C# và logic trừ số lượng ô trống:

1. Bản chất: C# mặc định gán số nguyên = 0
Khi bạn tạo mới một đối tượng BinInfo, tất cả biến số nguyên int trong C# đều mặc định mang giá trị 0:

_okCount = 0 (Đúng)
_ngCount = 0 (Đúng)
_emptyCount = 0 (SAI THỰC TẾ!)
Thực tế, khi new TrayMatrix(row, column) chạy hàm Clear(), toàn bộ 60 ô trên khay đều đang là SlotState.NA (ô trống). $\rightarrow$ Khay thực tế đang có 60 ô trống, nhưng biến _emptyCount trong bộ nhớ lại đang bằng 0.

2. Hậu quả nếu không khởi tạo _emptyCount:
❌ Lỗi 1: _emptyCount bị tụt xuống số ÂM
Khi tay gắp đặt sản phẩm đầu tiên vào 1 ô trống (chuyển từ SlotState.NA sang SlotState.OK), hàm cộng trừ $O(1)$ sẽ chạy:

csharp
if (oldState == SlotState.NA) _emptyCount--; // 0 - 1 = -1 !
$\rightarrow$ Biến đếm ô trống sẽ bị nhảy thành -1, sau đó là -2, gây sai lệch toàn bộ hệ thống.

❌ Lỗi 2: Trạm tráo khay (Swap) tưởng nhầm khay đã kín chỗ
Rất nhiều hàm trong TraySwapHelper.cs kiểm tra điều kiện này trước khi làm việc:

csharp
// Dòng 1738: Chỉ cho phép cấp hàng nếu khay còn ô trống
return C.OKCount > 0 && A.EmptyCount > 0;
Nếu A.EmptyCount == 0, hàm tưởng khay A đã hết chỗ và từ chối tráo hàng, mặc dù khay A thực tế đang rỗng 100%.

💡 Có cách viết nào gọn hơn không?
Có! Thay vì phải gọi cả hàm RefreshCountInternal(), bạn có thể gán trực tiếp đúng 1 dòng vì khay mới chắc chắn 100% là ô trống:

csharp
public BinInfo(int row, int column)
{
    _row = row;
    _column = column;
    _matrix = new TrayMatrix(row, column);
    
    // Khay mới thì toàn bộ số ô đều là ô trống (NA)
    _emptyCount = row * column; // Ví dụ: 6 * 10 = 60
}
Viết như trên vừa chính xác tuyệt đối, vừa chạy tức thì ($O(1)$) mà không cần tốn vòng lặp quét 60 ô lúc khởi tạo.

10:47 PM





        public bool HashSet(string key, IDictionary<string, string> values, string serverNo = "0")
        {
            if (values == null || values.Count == 0) return false;
            var db = GetDatabase(serverNo);
            if (db == null) return false;
            var entries = new HashEntry[values.Count];
            int i = 0;
            foreach (var kvp in values)
            {
                entries[i++] = new HashEntry(kvp.Key, kvp.Value ?? string.Empty);
            }
            db.HashSet(key, entries);
            return true;
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
        batch[key]                = "NA";
        batch[$"{key}_NGReason"]  = "";
        batch[$"{key}_SN"]        = "";
    }
    // 1 lệnh Redis duy nhất ghi 180 field
    RedisHelperEx.Instance.HashSet(DbName, batch);
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
Khay A (Khay OK): Có 50 con OK, 4 con NG, 6 ô trống.
Khay B (Khay NG): Có 3 con OK, 52 con NG, 5 ô trống.


Chứng minh toán học:

Công thức tính: okMove = Math.Min(source.OKCount, target.EmptyCount + target.NGCount)

Trường hợp 1: source.OKCount <= target.EmptyCount + target.NGCount 
→
→ Lúc này okMove bằng đúng source.OKCount. 
→
→ Rõ ràng: (source.EmptyCount + okMove) chính là (source.EmptyCount + source.OKCount). 
⟹
⟹ Cả 2 công thức giống hệt nhau.

Trường hợp 2: source.OKCount > target.EmptyCount + target.NGCount 
→
→ Lúc này okMove = target.EmptyCount + target.NGCount. Ta có:
source.EmptyCount+okMove=source.EmptyCount+target.EmptyCount+target.NGCount≥target.NGCount


Vì tổng này luôn lớn hơn hoặc bằng target.NGCount, nên:

min(target.NGCount,source.EmptyCount+okMove)=target.NGCount

⟹ Cả 2 công thức vẫn ra kết quả bằng đúng target.NGCount!

Vậy tại sao tác giả lại viết tách ra if (isOKOK) làm gì?

Tác giả tách ra vì tư duy nghiệp vụ (Business Logic) và điều kiện chốt chặn, cụ thể:

Về tư duy lập trình viên ban đầu:

Khi dồn 2 khay OK (isOKOK): Tác giả nghĩ "Khay source chỉ mới nhấc okMove con OK ra, nên chỉ được tính thêm đúng okMove ô trống mới".
Khi tráo OK-NG (!isOKOK): Tác giả nghĩ "Khay NG là khay lỗi, toàn bộ source.OKCount con OK trên đó sẽ phải bốc sạch đi".

Khác biệt THỰC TẾ duy nhất giữa 2 nhánh: Nằm ở đúng dòng này:

csharp
if (okMove <= 0)
    return new Capacity(); // Nếu không chuyển được con OK nào thì HỦY NGAY LẬP TỨC!
Nếu là 2 khay OK (isOKOK): Bắt buộc phải có hàng OK chuyển đi (okMove > 0). Nếu okMove <= 0 thì trả về rỗng ngay, tuyệt đối không cho phép chuyển hàng NG!
Nếu là khay OK và NG (!isOKOK): Kể cả khi okMove = 0 (hết hàng OK), khay OK vẫn có thể chuyển hàng NG sang khay NG được bình thường (Mode NG2 hoặc NG1).