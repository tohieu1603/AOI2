/*
 * ====================================================================================================
 * 📌 BẢNG TỔNG HỢP CÁC CHỖ SỬA VÀ NÂNG CẤP HIỆU NĂNG CHO FORM_AUTO.CS
 * File đích: Autoframe/UI/Form_Auto.cs
 * Mục tiêu: Tối ưu hiển thị OnShowProductData đạt chuẩn Real-time 50 FPS, không đơ, không giật lag.
 * ====================================================================================================
 *
 * ┌───┬──────────────────────────────────┬────────────────┬─────────────────────────────────────────────────────────────┬───────────────────────────────────────────────────────────────────┬───────────────────────────────────────────────────────────┐
 * │ # │ Vị trí / Phương thức             │ Dòng mới       │ Nội dung bản cũ                                             │ Nội dung bản mới (Cấp 3: Virtual Mode + Ring Buffer + Throttle)   │ Lợi ích thực tế                                           │
 * ├───┼──────────────────────────────────┼────────────────┼─────────────────────────────────────────────────────────────┼───────────────────────────────────────────────────────────────────┼───────────────────────────────────────────────────────────┤
 * │ 1 │ Thư viện import                  │ L28            │ Chưa có thư viện collections đồng thời                      │ using System.Collections.Concurrent;                              │ Cung cấp ConcurrentQueue an toàn đa luồng                 │
 * │ 2 │ Class bổ trợ ngoài Form          │ L33 - L115     │ Không có                                                    │ DataGridViewExtensions + ProductDataRingBuffer                    │ Circular Buffer O(1), thu hồi rác GC ngay, DoubleBuffer  │
 * │ 3 │ Khai báo biến trong Form_Auto    │ L124 - L131    │ Chỉ có Timer timer cập nhật chartPie                        │ _ringBuffer, _pendingProducts, _throttleTimer, _columnKeys...     │ Quản lý bộ đệm và cấu hình nhịp quét (20ms ~ 50 FPS)      │
 * │ 4 │ Form_Auto_Load()                 │ L265 - L270    │ Chưa khởi tạo bộ đếm nhịp vẽ bảng                           │ Khởi tạo và Start _throttleTimer chu kỳ REFRESH_INTERVAL_MS       │ Điều tiết nhịp hiển thị ổn định, không ép GPU/CPU vẽ dồn  │
 * │ 5 │ InitProductDataGridViewHead()    │ L320 - L371    │ DGV thông thường, dgv.Rows.Clear(), vẽ trực tiếp            │ dgv.RowCount = 0, VirtualMode, EnableDoubleBuffered, bắt sự kiện  │ Khắc phục crash InvalidOperationException, bảng siêu nhẹ  │
 * │ 6 │ OnShowProductData()              │ L378 - L419    │ BeginInvoke -> new Row -> Insert(0) -> RemoveAt -> Refresh()│ Guard check null/empty -> _pendingProducts.Enqueue(dicProduct)    │ Zero-blocking: Luồng station nhả hàng xong thoát ngay     │
 * │ 7 │ Thêm hàm Virtual Mode & Throttle │ L421 - L513    │ Không có                                                    │ Dgv_CellValueNeeded, Dgv_CellFormatting, ThrottleFlush            │ Gom N sản phẩm vẽ 1 lần/khung hình, kiểm tra IsDisposed   │
 * │ 8 │ button_clear_Click()             │ L565 - L571    │ Chỉ xóa biến đếm ProductConfig                              │ Thêm _ringBuffer.Clear(); dataGridViewProductDataShow.RowCount = 0│ Xóa sạch buffer và reset trắng bảng hiển thị ngay lập tức │
 * └───┴──────────────────────────────────┴────────────────┴─────────────────────────────────────────────────────────────┴───────────────────────────────────────────────────────────────────┴───────────────────────────────────────────────────────────┘
 *
 * ====================================================================================================
 * 🔍 CHI TIẾT SO SÁNH CODE 3 VỊ TRÍ THEN CHỐT
 * ====================================================================================================
 *
 * 1. Sự kiện OnShowProductData:
 * ----------------------------------------------------------------------------------------------------
 * [BẢN CŨ - GÂY GIẬT LAG NẶNG]:
 *     this.BeginInvoke((Action)delegate
 *     {
 *         UpdateProductCounter(); // Lại gọi thêm 1 BeginInvoke lồng bên trong!
 *         DataGridViewRow row = new DataGridViewRow();
 *         row.CreateCells(dataGridViewProductDataShow, dicProduct.Values.ToArray()); // Cấp phát Row mới
 *         dataGridViewProductDataShow.Rows.Insert(0, row); // Dịch chuyển 100 dòng xuống (O(N))
 *         if (dataGridViewProductDataShow.Rows.Count > ShowCount)
 *             dataGridViewProductDataShow.Rows.RemoveAt(dataGridViewProductDataShow.Rows.Count - 1);
 *         dataGridViewProductDataShow.Refresh(); // Ép vẽ lại toàn bộ bảng ngay lập tức
 *     });
 *
 * [BẢN MỚI - SIÊU NHẸ, KHÔNG CHẶN LUỒNG]:
 *     if (this.IsDisposed || !this.IsHandleCreated || dicProduct == null || dicProduct.Count < 1)
 *         return;
 *     _pendingProducts.Enqueue(dicProduct);
 *     _hasPendingData = true;
 *
 * ----------------------------------------------------------------------------------------------------
 * 2. Cấu hình bảng trong InitProductDataGridViewHead:
 * ----------------------------------------------------------------------------------------------------
 * [BẢN CŨ]:
 *     dataGridViewProductDataShow.Rows.Clear();
 *     dataGridViewProductDataShow.Columns.Clear();
 *     // Tạo cột thông thường, để DGV tự quản lý từng Cell/Row
 *
 * [BẢN MỚI]:
 *     if (dgv.VirtualMode)
 *         dgv.RowCount = 0;   // Không gọi Rows.Clear() vì VirtualMode sẽ throw Exception!
 *     else
 *         dgv.Rows.Clear();
 *     dgv.Columns.Clear();
 *     dgv.VirtualMode = true;         // Bật chế độ ảo
 *     dgv.ReadOnly = true;
 *     dgv.AllowUserToAddRows = false;
 *     dgv.CellValueNeeded += Dgv_CellValueNeeded;   // Cung cấp dữ liệu khi vẽ
 *     dgv.CellFormatting += Dgv_CellFormatting;     // Đổi màu đỏ khi linh kiện NG
 *     dgv.EnableDoubleBuffered();                   // Bật Double Buffer khử giật
 *     _ringBuffer = new ProductDataRingBuffer(ShowCount); // Buffer tròn 100 dòng O(1)
 *
 * ----------------------------------------------------------------------------------------------------
 * 3. Bộ gom vẽ chu kỳ ThrottleFlush (20ms ~ 50 FPS):
 * ----------------------------------------------------------------------------------------------------
 * [NGUYÊN LÝ HOẠT ĐỘNG]:
 *     - Mỗi chu kỳ 20ms, timer kiểm tra xem có dữ liệu mới không (_pendingProducts.IsEmpty).
 *     - Rút toàn bộ danh sách trong queue (_pendingProducts) nạp vào _ringBuffer theo thứ tự O(1).
 *     - Cập nhật số liệu Total, Pass, Fail, Yield trực tiếp (không dùng BeginInvoke lồng).
 *     - Gọi dgv.RowCount = _ringBuffer.Count và dgv.Invalidate() ĐÚNG 1 LẦN cho cả batch sản phẩm.
 * ====================================================================================================
 */


Thêm using System.Collections.Concurrent; (Dòng 28).
Thêm class ProductDataRingBuffer và DataGridViewExtensions (Dòng 33-113):
Bộ đệm vòng O(1) lưu 100 dòng mới nhất, không tạo object thừa, không rác GC.
Hàm kích hoạt DoubleBuffered triệt tiêu hiện tượng giật/nháy hình.
Thêm biến quản lý Virtual Mode & Throttle (Dòng 122-129):
_pendingProducts: Hàng đợi ConcurrentQueue nhận dữ liệu từ các station.
_throttleTimer: Timer gom chu kỳ 33ms (~30 FPS).
Khởi động Timer trong Form_Auto_Load (Dòng 263-268).
Cấu hình InitProductDataGridViewHead sang Virtual Mode (Dòng 310-360):
Gắn 2 sự kiện: CellValueNeeded (lấy chữ) và CellFormatting (tô màu đỏ khi NG).
Sửa OnShowProductData thành non-blocking (Dòng 368-380):
Không gọi BeginInvoke, không tạo DataGridViewRow, chỉ đưa vào Queue.
Hàm gom nhịp ThrottleFlush (Dòng 438-499):
Gom toàn bộ hàng đợi, cập nhật counter và gọi Invalidate() 1 lần duy nhất cho cả batch.
Đồng bộ nút button_clear_Click (Dòng 557-563):
Xóa sạch buffer và reset bảng về 0 dòng tức thì khi bấm clear data.