# -*- coding: utf-8 -*-
import docx
from docx import Document
from docx.shared import Inches, Pt, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.table import WD_TABLE_ALIGNMENT
from docx.oxml import OxmlElement, parse_xml
from docx.oxml.ns import nsdecls, qn

def create_full_document():
    doc = Document()

    # Configure Margins: 1 inch (25.4 mm)
    for section in doc.sections:
        section.top_margin = Inches(1)
        section.bottom_margin = Inches(1)
        section.left_margin = Inches(1)
        section.right_margin = Inches(1)

    # Base Styling
    normal_style = doc.styles['Normal']
    normal_style.font.name = 'Times New Roman'
    normal_style.font.size = Pt(11)
    normal_style.font.color.rgb = RGBColor(0x22, 0x22, 0x22)
    normal_style.paragraph_format.line_spacing = 1.15
    normal_style.paragraph_format.space_after = Pt(4)

    def set_cell_background(cell, fill_hex):
        tcPr = cell._tc.get_or_add_tcPr()
        shd = parse_xml(f'<w:shd {nsdecls("w")} w:fill="{fill_hex}"/>')
        tcPr.append(shd)

    def set_cell_margins(cell, top=100, bottom=100, left=150, right=150):
        tcPr = cell._tc.get_or_add_tcPr()
        tcMar = parse_xml(f'<w:tcMar {nsdecls("w")}><w:top w:w="{top}" w:type="dxa"/><w:bottom w:w="{bottom}" w:type="dxa"/><w:left w:w="{left}" w:type="dxa"/><w:right w:w="{right}" w:type="dxa"/></w:tcMar>')
        tcPr.append(tcMar)

    def add_title(text):
        p = doc.add_paragraph()
        p.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p.paragraph_format.space_before = Pt(0)
        p.paragraph_format.space_after = Pt(12)
        run = p.add_run(text)
        run.bold = True
        run.font.size = Pt(18)
        run.font.color.rgb = RGBColor(0x1B, 0x36, 0x5D)

    def add_subtitle(text):
        p = doc.add_paragraph()
        p.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p.paragraph_format.space_after = Pt(24)
        run = p.add_run(text)
        run.italic = True
        run.font.size = Pt(12)
        run.font.color.rgb = RGBColor(0x4A, 0x55, 0x68)

    def add_heading_1(text):
        p = doc.add_paragraph()
        p.paragraph_format.space_before = Pt(16)
        p.paragraph_format.space_after = Pt(6)
        p.paragraph_format.keep_with_next = True
        run = p.add_run(text)
        run.bold = True
        run.font.size = Pt(14)
        run.font.color.rgb = RGBColor(0x1B, 0x36, 0x5D)

    def add_heading_2(text):
        p = doc.add_paragraph()
        p.paragraph_format.space_before = Pt(12)
        p.paragraph_format.space_after = Pt(4)
        p.paragraph_format.keep_with_next = True
        run = p.add_run(text)
        run.bold = True
        run.font.size = Pt(12)
        run.font.color.rgb = RGBColor(0x2C, 0x52, 0x82)

    def add_heading_3(text):
        p = doc.add_paragraph()
        p.paragraph_format.space_before = Pt(8)
        p.paragraph_format.space_after = Pt(2)
        p.paragraph_format.keep_with_next = True
        run = p.add_run(text)
        run.bold = True
        run.italic = True
        run.font.size = Pt(11)
        run.font.color.rgb = RGBColor(0x2B, 0x6C, 0xB0)

    def add_body(text):
        p = doc.add_paragraph()
        p.paragraph_format.space_after = Pt(4)
        p.paragraph_format.line_spacing = 1.15
        run = p.add_run(text)
        run.font.size = Pt(11)

    def add_bullet(text):
        p = doc.add_paragraph(style='List Bullet')
        p.paragraph_format.space_after = Pt(2)
        p.paragraph_format.line_spacing = 1.15
        run = p.add_run(text)
        run.font.size = Pt(11)

    def add_code(code_str):
        tbl = doc.add_table(rows=1, cols=1)
        tbl.alignment = WD_TABLE_ALIGNMENT.CENTER
        cell = tbl.cell(0, 0)
        cell.width = Inches(6.5)
        set_cell_background(cell, "F7FAFC")
        set_cell_margins(cell, top=120, bottom=120, left=180, right=180)
        
        tcPr = cell._tc.get_or_add_tcPr()
        borders = parse_xml(f'<w:tcBorders {nsdecls("w")}><w:top w:val="none"/><w:left w:val="single" w:sz="18" w:space="0" w:color="2B6CB0"/><w:bottom w:val="none"/><w:right w:val="none"/></w:tcBorders>')
        tcPr.append(borders)

        p = cell.paragraphs[0]
        p.paragraph_format.space_before = Pt(0)
        p.paragraph_format.space_after = Pt(0)
        p.paragraph_format.line_spacing = 1.05
        run = p.add_run(code_str.strip())
        run.font.name = 'Consolas'
        run.font.size = Pt(9.5)
        run.font.color.rgb = RGBColor(0x1A, 0x20, 0x2C)

        p_after = doc.add_paragraph()
        p_after.paragraph_format.space_before = Pt(0)
        p_after.paragraph_format.space_after = Pt(4)

    # ---------------- DOCUMENT CONTENT ----------------
    add_title("CHUYEN DE CHUYEN SAU: GENERICS, MULTITHREADING VA ASYNC/AWAIT TRONG NEN TANG .NET")
    add_subtitle("Tai lieu kien truc he thong, nguyen ly CLR va huong dan lap trinh nang cao")

    # PHAN 1: GENERICS
    add_heading_1("PHAN 1: KIEN TRUC VA NGUYEN LY GENERICS TRONG CLR")
    
    add_heading_2("1.1. Co che bien dich JIT va to chuc bo nho")
    add_body("Trong moi truong thuc thi CLR (Common Language Runtime), Generics khong phai la co che thay the van ban o muc bien dich ma duoc ho tro nguyen ban tu muc ma trung gian IL (Intermediate Language) cho den ma may cuc bo (Native Machine Code).")
    add_body("Qua trinh bien dich JIT (Just-In-Time) xu ly Generic Type theo hai mo hinh tach biet dua vao ban chat kieu du lieu:")
    add_bullet("Kieu gia tri (Value Types nhu int, double, struct): JIT tao ra mot phan vung ma may doc lap cho tung kieu du lieu duy nhat. Vi du, List<int> va List<double> so huu hai khoi ma lenh assembly rieng biet trong bo nho code heap. Dieu nay bat buoc vi kich thuoc bo nho cua int (4 bytes) va double (8 bytes) khac nhau, dan den cac chi thi tinh toan offset va day vao ngan xep khong the dung chung.")
    add_bullet("Kieu tham chieu (Reference Types nhu string, class): JIT chia se dung mot khoi ma may duy nhat cho tat ca cac kieu tham chieu. List<string>, List<Stream>, List<MyClass> deu thuc thi chung mot tap lenh nhan. Nguyen nhan la tren cung mot kien truc phan cung (vi du 64-bit), moi con tro tham chieu den Heap deu co kich thuoc dong nhat la 8 bytes. Ma may chi thao tac tren dia chi con tro ma khong phu thuoc vao kich thuoc thuc te cua doi tuong.")

    add_heading_2("1.2. Rang buoc kieu du lieu (Generic Constraints)")
    add_body("Rang buoc giup han che cac kieu du lieu co the truyen vao tham so kieu T, dong thoi cho phep lap trinh vien truy cap truc tiep vao cac thanh phan cua kieu du lieu do ma khong can ep kieu run-time:")
    add_bullet("where T : struct -> Rang buoc T phai la kieu gia tri khong duoc chua null.")
    add_bullet("where T : class -> Rang buoc T phai la kieu tham chieu.")
    add_bullet("where T : notnull -> Rang buoc T khong the nhan gia tri null (ap dung tu C# 8.0).")
    add_bullet("where T : unmanaged -> Rang buoc T la kieu khong chua tham chieu va co the dung con tro truc tiep.")
    add_bullet("where T : new() -> Rang buoc T phai co ham tao khong tham so cong khai.")
    add_bullet("where T : BaseClass / Interface -> Rang buoc T phai ke thua hoac thuc thi giao dien chi dinh.")

    add_code("""using System;
using System.Collections.Generic;

public interface IEntity<TKey>
{
    TKey Id { get; set; }
}

public interface IAuditTrackable
{
    DateTime CreatedAt { get; set; }
    string CreatedBy { get; set; }
}

public class BaseRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>, IAuditTrackable, new()
    where TKey : struct, IComparable<TKey>
{
    private readonly Dictionary<TKey, TEntity> _storage = new Dictionary<TKey, TEntity>();

    public void Insert(TEntity entity)
    {
        if (entity.Id.CompareTo(default(TKey)) <= 0)
        {
            throw new ArgumentException("Khoa chinh khong hop le.");
        }
        entity.CreatedAt = DateTime.UtcNow;
        _storage[entity.Id] = entity;
    }

    public TEntity FindById(TKey id)
    {
        return _storage.TryGetValue(id, out var entity) ? entity : null;
    }
}""")

    add_heading_2("1.3. Covariance va Contravariance")
    add_body("Khai niem Variance dieu khien tinh tuong thich phep gan giua cac kieu Generic co quan he ke thua. Tinh nang nay chi ap dung cho Interface va Delegate tren cac Reference Type:")
    add_bullet("Covariance (out T): Cho phep gan mot kieu du lieu con (cu the) cho kieu du lieu cha (truu tuong hon). T chi duoc phep dong vai tro la gia tri tra ve (Output).")
    add_bullet("Contravariance (in T): Cho phep gan mot kieu cha (truu tuong hon) cho kieu con. T chi duoc phep dong vai tro la tham so dau vao (Input).")

    add_code("""using System;

public class Component { public string SerialNumber { get; set; } }
public class OpticalLens : Component { public double FocalLength { get; set; } }

// Covariance: output only
public interface IFactory<out T> where T : Component
{
    T Produce(string serial);
}

// Contravariance: input only
public interface IQualityChecker<in T> where T : Component
{
    bool Verify(T target);
}

public class LensFactory : IFactory<OpticalLens>
{
    public OpticalLens Produce(string serial) => new OpticalLens { SerialNumber = serial, FocalLength = 50.0 };
}

public class ComponentChecker : IQualityChecker<Component>
{
    public bool Verify(Component target) => !string.IsNullOrEmpty(target.SerialNumber);
}

public class VarianceRunner
{
    public static void Run()
    {
        // Covariance: OpticalLens ke thua Component nen IFactory<OpticalLens> gan duoc cho IFactory<Component>
        IFactory<OpticalLens> lensFactory = new LensFactory();
        IFactory<Component> componentFactory = lensFactory;
        Component comp = componentFactory.Produce("SN-001");

        // Contravariance: IQualityChecker<Component> gan duoc cho IQualityChecker<OpticalLens>
        IQualityChecker<Component> compChecker = new ComponentChecker();
        IQualityChecker<OpticalLens> lensChecker = compChecker;
        bool isValid = lensChecker.Verify(new OpticalLens { SerialNumber = "SN-001" });
    }
}""")

    add_heading_2("1.4. So sanh hieu nang va chi phi Boxing/Unboxing")
    add_body("Boxing la tien trinh CLR dong goi mot gia tri kieu struct/int tren Stack vao mot doi tuong nam tren Managed Heap. Unboxing trich xuat con tro du lieu goc tu Heap tro lai Stack.")
    add_body("Su dung ArrayList gay ra hang loat phep Boxing/Unboxing khien bo nho phan manh va tao ap luc cuc lon len Garbage Collector (GC). Nguoc lai, List<T> tao truc tiep mang kieu du lieu lien tuc tren RAM, toi uu CPU Cache va giam 100% chi phi boxing.")

    # PHAN 2: MULTITHREADING
    add_heading_1("PHAN 2: LAP TRINH DA LUONG (MULTITHREADING) CHUYEN SAU")

    add_heading_2("2.1. Thread, ThreadPool va Thuat toan Work-Stealing")
    add_body("Moi Dedicated Thread do he dieu hanh tao ra tieu ton mac dinh 1MB bo nho Stack tren Windows x64 va doi hoi hang ngan chu ky CPU cho thao tac Context Switch. ThreadPool duoc thiet ke de tai su dung cac luong da khoi tao.")
    add_body("Kien truc hang doi ThreadPool trong .NET:")
    add_bullet("Global Queue: Hang doi toan cuc danh cho cac tac vu duoc dua vao tu cac luong ngoai ThreadPool. Cac luong phai gianh quyen khoa dong bo khi lay viec tu day.")
    add_bullet("Local Queue: Moi ThreadPool Worker so huu mot hang doi cuc bo. Cac tac vu do chinh luong do sinh ra se duoc dua vao Local Queue theo thu tu LIFO (Last-In-First-Out) nham tan dung toi da bo nho dem CPU Cache L1/L2.")
    add_bullet("Work-Stealing Algorithm: Khi mot luong xu ly het viec trong Local Queue cua no, no se bien thanh luong 'danh cap' (thief) va quet sang phia cuoi hang doi (FIFO) cua cac luong worker khac de lay viec ve lam. Co che nay triet tieu tranh chap khoa va dam bao can bang tai toi da giua tat ca cac loi CPU.")

    add_heading_2("2.2. Cac co che dong bo hoa va khoa tai nguyen")
    add_body("Khi nhieu luong cung truy cap mot vung nho chia se (Critical Section), viec dong bo hoa la bat buoc de ngan chan Race Condition.")
    add_bullet("Monitor / lock: Khai niem can ban nhat trong .NET. Dua tren SyncBlock Index nam tren Object Header cua kieu tham chieu. Hoat dong o muc User-Mode ket hop Kernel-Mode.")
    add_bullet("ReaderWriterLockSlim: Toi uu hoa cho tinh huong tai nguyen duoc doc lien tuc boi nhieu luong nhung hiem khi ghi. Cho phep hang tram luong doc cung luc, nhung khi co luong ghi thi se doc quyen truy cap.")
    add_bullet("SemaphoreSlim: Co che gioi han so luong luong duoc phep truy cap vao mot tai nguyen cung mot luc. Khac voi Semaphore he dieu hanh, SemaphoreSlim hoat dong chu yeu o muc User-mode va ho tro lap trinh bat dong bo (WaitAsync).")
    add_bullet("Mutex: Khoa dong bo muc Kernel cua he dieu hanh. Cho phep dong bo hoa giua cac tien trinh (Process) khac nhau tren cung mot may tinh, chi phi chuyen doi cao hon lock.")

    add_code("""using System;
using System.Collections.Generic;
using System.Threading;

public class ThreadSafeCache<TKey, TValue>
{
    private readonly Dictionary<TKey, TValue> _cache = new Dictionary<TKey, TValue>();
    private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();

    public TValue Get(TKey key)
    {
        _rwLock.EnterReadLock();
        try
        {
            return _cache.TryGetValue(key, out var val) ? val : default;
        }
        finally
        {
            _rwLock.ExitReadLock();
        }
    }

    public void Set(TKey key, TValue value)
    {
        _rwLock.EnterWriteLock();
        try
        {
            _cache[key] = value;
        }
        finally
        {
            _rwLock.ExitWriteLock();
        }
    }
}""")

    add_heading_2("2.3. Tap hop du lieu an toan luong (Concurrent Collections)")
    add_body("Cac lop trong System.Collections.Concurrent su dung ky thuat Lock-Free dua tren chi thi phan cung Interlocked (CAS - Compare-And-Swap) hoac phan vung khoa (Fine-grained striping lock) thay vi khoa toan bo tap hop:")
    add_bullet("ConcurrentQueue<T>: Hang doi FIFO Lock-Free hoan toan, su dung danh sach lien ket cac mang phan doan 32 phan tu. Toi uu tuyet doi cho mo hinh Producer-Consumer.")
    add_bullet("ConcurrentDictionary<TKey, TValue>: Su dung mang cac khoa con rieng biet (mac dinh 4 * so core CPU). Nhieu luong co the ghi dong thoi vao cac bucket khac nhau ma khong gay nghen.")
    add_bullet("BlockingCollection<T>: Bo boc da nang cung cap co che chan luong khi hang doi day (Bounded Capacity) hoac khi hang doi rong, ho tro hoan hao cac pipeline xu ly song song.")

    add_heading_2("2.4. Lap trinh dong bo su kien: AutoResetEvent va ManualResetEventSlim")
    add_bullet("AutoResetEvent: Giong nhu cua xoay mot chieu. Khi Set() duoc goi, dung mot luong duy nhat dang doi tai WaitOne() duoc phep di qua, sau do cong tu dong dong lai ve trang thai Reset.")
    add_bullet("ManualResetEventSlim: Giong nhu cong lon. Khi Set() duoc goi, cong mo va tat ca cac luong dang doi deu duoc giai phong. Cong chi dong lai khi lap trinh vien goi Reset() chu dong. Phien ban Slim su dung spin-wait o User-mode truoc khi goi xuong Kernel giup toi uu hieu nang gap nhieu lan so voi phien ban truyen thong.")

    # PHAN 3: ASYNC/AWAIT
    add_heading_1("PHAN 3: MO HINH BAT DONG BO VA CO CHE ASYNC/AWAIT")

    add_heading_2("3.1. Kien truc Task-Based Asynchronous Pattern (TAP)")
    add_body("Lap trinh bat dong bo trong .NET khong dong nghia voi viec tao them luong moi. No duoc thiet ke de giai phong luong dang cho doi cac thao tac I/O (I/O Completion Ports - IOCP nhu doc ghi o dia, truy van co so du lieu, goi mang HTTP).")
    add_body("Khi ung dung goi mot thao tac I/O bat dong bo, he dieu hanh gui tin hieu xuong phan cung va luong CPU duoc tra ve cho ThreadPool de phuc vu cac cong viec khac. Khi phan cung hoan tat, mot tin hieu ngat (Hardware Interrupt) duoc kich hoat, danh thuc IOCP va ThreadPool phan bo mot luong de tiep tuc phan ma con lai sau lenh await.")

    add_heading_2("3.2. Ban chat State Machine duoc sinh boi Trinh bien dich")
    add_body("Khi mot phuong thuc duoc danh dau voi tu khoa async, trinh bien dich C# se tao ra mot struct hoac class noi tai an danh ke thua giao dien IAsyncStateMachine. Phuong thuc goc duoc chuyen hoa thanh mot co che may trang thai:")
    add_bullet("Moi vi tri xuat hien tu khoa await dai dien cho mot diem ngat trang thai (State).")
    add_bullet("Khi gap await tren mot Task chua hoan thanh, State Machine se ghi nhan trang thai hien tai vao mot bien chi muc (vi du: state = 0), dang ky phuong thuc MoveNext() lam callback tiep tuc qua TaskAwaiter.OnCompleted, va tra quyen kiem soat ngay lap tuc cho luong goi.")
    add_bullet("Khi Task hoan thanh, luong duoc dieu phoi se goi lai MoveNext(), nhay vao dung nhanh ma lenh sau vi tri await dua tren gia tri chi muc trang thai da ghi nho.")

    add_code("""// Doan ma nguon goc viet boi lap trinh vien
public async Task<int> FetchDataLengthAsync(string url)
{
    using (HttpClient client = new HttpClient())
    {
        string data = await client.GetStringAsync(url);
        return data.Length;
    }
}

// Mo ta ban chat cau truc ma may State Machine ma trinh bien dich sinh ra ngam
/*
public struct FetchDataLengthAsync_StateMachine : IAsyncStateMachine
{
    public int state;
    public AsyncTaskMethodBuilder<int> builder;
    public string url;
    private HttpClient client;
    private string data;
    private TaskAwaiter<string> awaiter;

    public void MoveNext()
    {
        int result;
        try
        {
            if (state != 0)
            {
                client = new HttpClient();
                awaiter = client.GetStringAsync(url).GetAwaiter();
                if (!awaiter.IsCompleted)
                {
                    state = 0;
                    builder.AwaitUnsafeOnCompleted(ref awaiter, ref this);
                    return; // Giai phong luong ngay tai day
                }
            }
            else
            {
                state = -1; // Phuc hoi trang thai sau khi hoan tat I/O
            }
            data = awaiter.GetResult();
            result = data.Length;
            client.Dispose();
        }
        catch (Exception ex)
        {
            state = -2;
            builder.SetException(ex);
            return;
        }
        state = -2;
        builder.SetResult(result);
    }
}
*/""")

    add_heading_2("3.3. SynchronizationContext va Hien tuong Deadlock")
    add_body("SynchronizationContext chiu trach nhiem dieu phoi phan ma phia sau await quay tro lai dung moi truong thuc thi ban dau:")
    add_bullet("Trong ung dung giao dien (WinForms, WPF): SynchronizationContext dam bao cac dong lenh sau await duoc dua tro lai Message Pump cua UI Thread de cap nhat control giao dien an toan.")
    add_bullet("Trong ASP.NET Core: SynchronizationContext da duoc loai bo hoan toan de toi uu hoa hieu nang. Bat ky luong ThreadPool nao cung co the tiep tuc xu ly sau await.")
    add_body("Hien tuong Deadlock xay ra khi goi dong bo tren phuong thuc bat dong bo (Anti-pattern: Sync-over-Async nhu task.Result hoac task.Wait()):")
    add_bullet("Buoc 1: Luong UI Thread goi task.Result va bi khoa chan tai cho de cho Task hoan thanh.")
    add_bullet("Buoc 2: Ben trong Task, phuong thuc I/O hoan tat va State Machine co gang dispatch phan ma con lai quay ve UI Thread thong qua SynchronizationContext.")
    add_bullet("Buoc 3: Luong UI Thread van dang bi chan boi buoc 1, khong the nhan them tin nhan xu ly. Hai tien trinh cho doi lan nhau dan den he thong bi treo cung vinh vien (Deadlock).")
    add_body("Giai phap phong ngua Deadlock:")
    add_bullet("Tuan thu triet de nguyen tac: 'Async tat ca tu tren xuong duoi' (Async all the way).")
    add_bullet("Su dung ConfigureAwait(false) trong cac thu vien class library. Lenh nay yeu cau State Machine khong can phai quay tro lai SynchronizationContext ban dau ma co the thuc thi tiep tren bat ky luong nao cua ThreadPool.")

    add_heading_2("3.4. Xu ly ngoai le va CancellationToken")
    add_body("Trong mo hinh async/await, ngoai le duoc bat vao doi tuong Task duoi dang AggregateException. Khi ban su dung await, CLR tu dong unwrap de nem ra ngoai le thuc su dau tien giup viec try/catch de dang hon rat nhieu so voi mo hinh cu.")
    add_body("CancellationTokenSource cung cap co che huy bo tac vu hop tac (Cooperative Cancellation). Tac vu phai chu dong kiem tra token thong qua ThrowIfCancellationRequested() hoac truyen token vao cac ham I/O cap duoi.")

    # PHAN 4: PROJECT MAU
    add_heading_1("PHAN 4: HE THONG THUC TE TICH HOP TOAN DIEN")
    add_body("Du an duoi day minh hoa mot he thong xu ly anh kiem tra quang hoc (AOI Pipeline) ket hop toan bo cac ky thuat: Generic Repository, ConcurrentQueue, SemaphoreSlim, CancellationToken va Async/Await.")

    add_code("""using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class InspectionItem<TData>
{
    public string ItemId { get; set; }
    public TData RawPayload { get; set; }
    public bool IsPassed { get; set; }
    public string FailureReason { get; set; }
    public DateTime InspectedAt { get; set; }
}

public interface IDataStore<T>
{
    void Save(T item);
    IEnumerable<T> GetAll();
}

public class MemoryDataStore<T> : IDataStore<T>
{
    private readonly ConcurrentBag<T> _items = new ConcurrentBag<T>();
    public void Save(T item) => _items.Add(item);
    public IEnumerable<T> GetAll() => _items;
}

public class OpticalInspectionEngine<TPayload> where TPayload : class
{
    private readonly ConcurrentQueue<InspectionItem<TPayload>> _queue = new ConcurrentQueue<InspectionItem<TPayload>>();
    private readonly IDataStore<InspectionItem<TPayload>> _dataStore;
    private readonly SemaphoreSlim _networkThrottler = new SemaphoreSlim(4); // Toi da 4 ket noi MES dong thoi
    private readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
    private readonly Func<TPayload, bool> _inspectionAlgorithm;

    public OpticalInspectionEngine(IDataStore<InspectionItem<TPayload>> dataStore, Func<TPayload, bool> algorithm)
    {
        _dataStore = dataStore;
        _inspectionAlgorithm = algorithm;
    }

    public void EnqueueWork(string itemId, TPayload payload)
    {
        _queue.Enqueue(new InspectionItem<TPayload>
        {
            ItemId = itemId,
            RawPayload = payload,
            InspectedAt = DateTime.UtcNow
        });
    }

    public async Task StartProcessingAsync(string mesEndpoint, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (_queue.TryDequeue(out var item))
            {
                // Buoc 1: Chay thuat toan kiem tra quang hoc tren CPU
                item.IsPassed = _inspectionAlgorithm(item.RawPayload);
                if (!item.IsPassed)
                {
                    item.FailureReason = "Phat hien vet tray xuoc quang hoc tren be mat";
                }

                // Buoc 2: Luu tru noi bo an toan luong
                _dataStore.Save(item);

                // Buoc 3: Ban du lieu len MES bat dong bo khong chan luong san xuat
                _ = UploadToMesAsync(item, mesEndpoint, cancellationToken);
            }
            else
            {
                await Task.Delay(10, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private async Task UploadToMesAsync(InspectionItem<TPayload> item, string url, CancellationToken ct)
    {
        await _networkThrottler.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            string jsonBody = $"{{\\"itemId\\":\\"{item.ItemId}\\",\\"passed\\":{item.IsPassed.ToString().ToLower()}}}";
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(url, content, ct).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Gui MES that bai cho Item {item.ItemId}. Ma loi: {response.StatusCode}");
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"Tien trinh upload Item {item.ItemId} da bi huy bo.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Loi ngoai le mang khi upload Item {item.ItemId}: {ex.Message}");
        }
        finally
        {
            _networkThrottler.Release();
        }
    }
}

public class ProgramRunner
{
    public static async Task Main()
    {
        var store = new MemoryDataStore<InspectionItem<string>>();
        var engine = new OpticalInspectionEngine<string>(store, payload => payload.Length > 5);

        using (var cts = new CancellationTokenSource())
        {
            // Khoi chay bo xu ly bat dong bo
            Task processingTask = engine.StartProcessingAsync("http://127.0.0.1:8080/api/mes", cts.Token);

            // Mo phong nhieu luong nap du lieu san xuat lien tuc
            Parallel.For(0, 10, i =>
            {
                engine.EnqueueWork($"ITEM-{i:D4}", $"IMAGE_DATA_PAYLOAD_{i}");
            });

            // Chay trong 2 giay sau do yeu cau dung an toan
            await Task.Delay(2000);
            cts.Cancel();

            try
            {
                await processingTask;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Pipeline kiem tra quang hoc da dung thanh cong.");
            }
        }
    }
}""")

    output_path = "/Users/admin/HieuTo/Test/Kien_Truc_DotNet_Generics_Multithreading_AsyncAwait.docx"
    doc.save(output_path)
    print(f"File created successfully at: {output_path}")

if __name__ == "__main__":
    create_full_document()

