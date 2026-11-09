// 1. Tạo 1 HttpClient tĩnh dùng chung (tránh cạn kiệt socket)
private static readonly HttpClient _httpClient = new HttpClient()
{
    Timeout = TimeSpan.FromSeconds(10) // Timeout 10 giây (FromSeconds, KHÔNG dùng FromMilliseconds)
};

// 2. Hàm Async chính thức
public static async Task<(bool success, string message)> SaveTestDataAsync(MesUploadDataModel data)
{
    if (!Parameter.GetBit("MesEnable"))
        return (true, "Disable Mes");

    // Nạp thông số máy
    data.domainId  = Parameter.GetString("domainId");
    data.machineNo = Parameter.GetString("machineNo");
    data.model     = Parameter.GetString("model");

    string sendJson = JsonConvert.SerializeObject(data);
    string sendUrl  = DataGroup.GetGroupData("MesConfig", "SaveTestDataURL").m_dbOffset.S;
    string strRes   = "";
    bool isHttpSuccess = false;

    try
    {
        var content = new StringContent(sendJson, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _httpClient.PostAsync(sendUrl, content);
        strRes = await response.Content.ReadAsStringAsync();
        isHttpSuccess = response.IsSuccessStatusCode;
    }
    catch (Exception ex)
    {
        strRes = ex.Message;
        isHttpSuccess = false;
    }

    var reciveData = JsonConvert.DeserializeObject<MesResponseModel>(strRes);
    bool bRet = isHttpSuccess && (reciveData?.success ?? false);
    string strMsg = reciveData?.message ?? strRes;

    RecordMgr.SaveTxtLog(RecordMode.Mes_SaveTestData, sendUrl, sendJson, strRes);
    return (bRet, strMsg);
}

// 3. Hàm wrapper đồng bộ để tương thích ngược cho Station (không cần sửa code cũ nếu chưa muốn)
public static bool SaveTestData(MesUploadDataModel data, out string strMsg)
{
    var task = SaveTestDataAsync(data);
    var result = task.GetAwaiter().GetResult();
    strMsg = result.message;
    return result.success;
}



// Thêm từ khóa 'async' ở đây
private async void button5_Click(object sender, EventArgs e)
{
    MesUploadDataModel model = new MesUploadDataModel();
    model.reportTime = DateTime.Now.ToString("yyyy-MM-dd");
    model.testStartTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    model.testEndTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    model.executionDuration = 300;
    model.inputQty = 36;
    model.totalPassQty = 30;
    model.totalFailQty = 6;
    model.itemList.Add(new DefectItem() { errorCode = "D01", errorDesc = "不良1", failQty = 1 });
    model.itemList.Add(new DefectItem() { errorCode = "D02", errorDesc = "不良2", failQty = 2 });
    model.itemList.Add(new DefectItem() { errorCode = "D03", errorDesc = "不良3", failQty = 3 });

    // Dùng await ở đây -> Giao diện hoàn toàn mượt mà, không bị khựng
    var result = await MesMgr.SaveTestDataAsync(model);

    if (result.success)
        MessageBox.Show("Gửi MES thành công!");
    else
        MessageBox.Show($"Thất bại: {result.message}");
}



3. Sửa tại trạm máy: 
Autoframe/Station/StationUnloadBinEx.cs
Ở trạm dỡ khay, bạn có 2 sự lựa chọn:

Lựa chọn A (Khuyên dùng trong sản xuất): Trạm đứng chờ MES xác nhận
Vì ProduceRun chạy trên luồng ngầm của trạm, việc chờ kết quả là bình thường và hoàn toàn an toàn (không bị deadlock):

csharp


case "UploadData":
    if (Parameter.GetBit("MesEnable"))
    {
        // Gọi thẳng hàm đồng bộ (nó đã bọc hàm Async bên trong)
        string strMsg = "";
        if (!MesMgr.SaveTestData(currentBinTray.ConvertToMesData(), out strMsg))
        {
            int iRet = ShowMessage($"MES上传数据失败:{strMsg}", "MesUploadFail", 
                        new string[] { "重新上传".txtTrans(TransType.Trans), "跳过".txtTrans(TransType.Trans) });
            if (iRet == 0)
                return "UploadData"; // Tải lại
            else
                Logger.Process(this.Name, "MES upload fail!");
        }
        else
            Logger.Process(this.Name, "MES upload successfully!");
    }                  
    return "UpdateData";
Lựa chọn B: Bắn MES chạy ngầm hoàn toàn (Fire & Forget)
Nếu xưởng của bạn không cần đợi MES trả về kết quả, muốn máy chạy tiếp ngay lập tức để tiết kiệm chu kỳ (takt time):

csharp


case "UploadData":
    if (Parameter.GetBit("MesEnable"))
    {
        var mesData = currentBinTray.ConvertToMesData();
        // Bắn ngầm, trạm chuyển sang bước tiếp theo ngay lập tức (tiết kiệm 200-500ms)
        Task.Run(async () =>
        {
            var res = await MesMgr.SaveTestDataAsync(mesData);
            if (!res.success)
                Logger.Process(this.Name, $"MES async upload fail: {res.message}");
        });
    }                  
    return "UpdateData"; // Sang bước tiếp theo ngay tức thì
🎯 Tóm lại:
Sửa chính ở Autoframe/MesMgr.cs (viết hàm SaveTestDataAsync và HttpClient).
Ở Form_UnloadInfo.cs: Dùng async void button5_Click + await.
Ở StationUnloadBinEx.cs: Giữ nguyên gọi SaveTestData (Lựa chọn A) hoặc dùng Task.Run nếu muốn máy chạy vụt qua không cần chờ MES (Lựa chọn B).



Lỗi 1: Bug Timeout cực nặng ở dòng 503 (MesMgr.cs)

Hãy nhìn vào đoạn code có sẵn trong file:

csharp
public static async Task<string> PostAsync(string Url, string contentEnc, int timeout = 5)
{
    using (var client = new HttpClient())
    {
        client.Timeout = TimeSpan.FromMilliseconds(timeout); // <-- BUG CHẾT NGƯỜI Ở ĐÂY!
        ...
Tham số truyền vào là timeout = 5 (ý người viết là 5 giây).
Nhưng code lại viết: TimeSpan.FromMilliseconds(5) 
→
→ Tức là 5 phần nghìn giây (0.005 giây)!
Một request HTTP qua mạng nội bộ mất ít nhất 20ms – 100ms. Đặt timeout 
5
 ms
5 ms thì vừa ấn gửi là bị hủy ngay lập tức (TaskCanceledException), không kịp nhận bất cứ thứ gì từ server!

👉 Sửa lại: Phải đổi thành FromSeconds:

csharp
client.Timeout = TimeSpan.FromSeconds(timeout); // Chờ 5 giây thay vì 5 mili-giây
🚨 Lỗi 2: Bị treo vĩnh viễn (Deadlock) do gọi .Result trong WinForms

Vì HttpClient.PostAsync là hàm bất đồng bộ (async Task), nếu bạn gọi nó trong WinForms theo kiểu:

csharp
// ❌ CÁCH GỌI NÀY SẼ GÂY TREO PHẦN MỀM / DEADLOCK TRONG WINFORMS:
string res = HttpHelper.PostAsync(url, json).Result; 
// hoặc:
string res = HttpHelper.PostAsync(url, json).GetAwaiter().GetResult();
Tại sao bị treo? Trong WinForms, luồng UI bị lệnh .Result chặn đứng lại để chờ. Trong khi đó, await client.PostAsync(...) bên trong lại cần luồng UI rảnh rỗi để nhả kết quả về 
→
→ Hai bên chờ nhau mãi mãi 
→
→ Giao diện đơ cứng, không bao giờ có dữ liệu trả về!
🚨 Lỗi 3: Quên từ khóa await

Nếu bạn viết:

csharp
// ❌ Quên await
HttpHelper.PostAsync(url, json);

Hàm sẽ bắn ngầm đi ở background và hàm cha kết thúc ngay lập tức, bạn sẽ thấy biến kết quả rỗng không có gì.

🚨 Lỗi 4: Máy tính chưa thông mạng tới IP 10.18.80.21

Địa chỉ 10.18.80.21:8081 là IP mạng nội bộ (LAN) của nhà xưởng:

Nếu bạn đang test trên máy tính cá nhân hoặc chưa cắm dây mạng/VPN vào xưởng thì không thể kết nối tới IP này.
Hãy mở CMD gõ thử:
cmd
ping 10.18.80.21
Nếu Request timed out thì HttpClient hay curl đều sẽ không nhận được dữ liệu.
🛠️ Cách viết Post bằng HttpClient chuẩn, chạy được cho cả Sync và Async:

Nếu muốn dùng HttpClient mượt mà, không bị deadlock trong dự án này, bạn hãy thay hàm PostAsync bằng hàm sau:

csharp
private static readonly HttpClient _httpClient = new HttpClient() 
{ 
    Timeout = TimeSpan.FromSeconds(10) // Timeout 10 giây
};
public static bool PostWithHttpClient(string url, string jsonContent, out string strResponse)
{
    strResponse = "";
    try
    {
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        // Dùng Task.Run để tránh Deadlock trên UI thread WinForms
        var response = Task.Run(() => _httpClient.PostAsync(url, content)).GetAwaiter().GetResult();
        
        strResponse = Task.Run(() => response.Content.ReadAsStringAsync()).GetAwaiter().GetResult();
        return response.IsSuccessStatusCode;
    }
    catch (Exception ex)
    {
        strResponse = "Error: " + ex.Message;
        return false;
    }
}

Bạn đang gọi HttpClient ở đoạn code nào? Bạn gửi đoạn đó lên tôi xem cách gọi cụ thể để chỉ đúng điểm nghẽn nhé!




public static bool SaveTestData(MesUploadDataModel data, out string strMsg)
{
    if (!Parameter.GetBit("MesEnable"))
    {
        strMsg = "Disable Mes";
        return true;
    }

    // Nạp thông số máy từ XML
    data.domainId  = Parameter.GetString("domainId");
    data.machineNo = Parameter.GetString("machineNo");
    data.model     = Parameter.GetString("model");

    // Tự động serialize toàn bộ data thành JSON chuẩn
    string sendJson = JsonConvert.SerializeObject(data);
    string sendUrl  = DataGroup.GetGroupData("MesConfig", "SaveTestDataURL").m_dbOffset.S;
    string strRes   = "";

    bool success = HttpHelper.Post(sendUrl, sendJson, out strRes);
    var reciveData = JsonConvert.DeserializeObject<MesResponseModel>(strRes);

    RecordMgr.SaveTxtLog(RecordMode.Mes_SaveTestData, sendUrl, sendJson, strRes);

    strMsg = reciveData?.message ?? strRes;
    return success && (reciveData?.success ?? false);
} không cần foreach à 



1. Sửa tại Autoframe/MesMgr.cs (Tạo hàm Async)

Thêm một HttpClient dùng chung và hàm SaveTestDataAsync dùng async/await:

csharp
// 1. Tạo 1 HttpClient tĩnh dùng chung (tránh cạn kiệt socket)
private static readonly HttpClient _httpClient = new HttpClient()
{
    Timeout = TimeSpan.FromSeconds(10) // Timeout 10 giây (FromSeconds, KHÔNG dùng FromMilliseconds)
};
// 2. Hàm Async chính thức
public static async Task<(bool success, string message)> SaveTestDataAsync(MesUploadDataModel data)
{
    if (!Parameter.GetBit("MesEnable"))
        return (true, "Disable Mes");
    // Nạp thông số máy
    data.domainId  = Parameter.GetString("domainId");
    data.machineNo = Parameter.GetString("machineNo");
    data.model     = Parameter.GetString("model");
    string sendJson = JsonConvert.SerializeObject(data);
    string sendUrl  = DataGroup.GetGroupData("MesConfig", "SaveTestDataURL").m_dbOffset.S;
    string strRes   = "";
    bool isHttpSuccess = false;
    try
    {
        var content = new StringContent(sendJson, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _httpClient.PostAsync(sendUrl, content);
        strRes = await response.Content.ReadAsStringAsync();
        isHttpSuccess = response.IsSuccessStatusCode;
    }
    catch (Exception ex)
    {
        strRes = ex.Message;
        isHttpSuccess = false;
    }
    var reciveData = JsonConvert.DeserializeObject<MesResponseModel>(strRes);
    bool bRet = isHttpSuccess && (reciveData?.success ?? false);
    string strMsg = reciveData?.message ?? strRes;
    RecordMgr.SaveTxtLog(RecordMode.Mes_SaveTestData, sendUrl, sendJson, strRes);
    return (bRet, strMsg);
}
// 3. Hàm wrapper đồng bộ để tương thích ngược cho Station (không cần sửa code cũ nếu chưa muốn)
public static bool SaveTestData(MesUploadDataModel data, out string strMsg)
{
    var task = SaveTestDataAsync(data);
    var result = task.GetAwaiter().GetResult();
    strMsg = result.message;
    return result.success;
}




2. Sửa tại UI test: Autoframe/UI/Form_UnloadInfo.cs

Thêm từ khóa async vào sự kiện nút bấm:

csharp
// Thêm từ khóa 'async' ở đây
private async void button5_Click(object sender, EventArgs e)
{
    MesUploadDataModel model = new MesUploadDataModel();
    model.reportTime = DateTime.Now.ToString("yyyy-MM-dd");
    model.testStartTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    model.testEndTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    model.executionDuration = 300;
    model.inputQty = 36;
    model.totalPassQty = 30;
    model.totalFailQty = 6;
    model.itemList.Add(new DefectItem() { errorCode = "D01", errorDesc = "不良1", failQty = 1 });
    model.itemList.Add(new DefectItem() { errorCode = "D02", errorDesc = "不良2", failQty = 2 });
    model.itemList.Add(new DefectItem() { errorCode = "D03", errorDesc = "不良3", failQty = 3 });
    // Dùng await ở đây -> Giao diện hoàn toàn mượt mà, không bị khựng
    var result = await MesMgr.SaveTestDataAsync(model);
    if (result.success)
        MessageBox.Show("Gửi MES thành công!");
    else
        MessageBox.Show($"Thất bại: {result.message}");
}


3. Sửa tại trạm máy: Autoframe/Station/StationUnloadBinEx.cs

Ở trạm dỡ khay, bạn có 2 sự lựa chọn:

Lựa chọn A (Khuyên dùng trong sản xuất): Trạm đứng chờ MES xác nhận

Vì ProduceRun chạy trên luồng ngầm của trạm, việc chờ kết quả là bình thường và hoàn toàn an toàn (không bị deadlock):

csharp
case "UploadData":
    if (Parameter.GetBit("MesEnable"))
    {
        // Gọi thẳng hàm đồng bộ (nó đã bọc hàm Async bên trong)
        string strMsg = "";
        if (!MesMgr.SaveTestData(currentBinTray.ConvertToMesData(), out strMsg))
        {
            int iRet = ShowMessage($"MES上传数据失败:{strMsg}", "MesUploadFail", 
                        new string[] { "重新上传".txtTrans(TransType.Trans), "跳过".txtTrans(TransType.Trans) });
            if (iRet == 0)
                return "UploadData"; // Tải lại
            else
                Logger.Process(this.Name, "MES upload fail!");
        }
        else
            Logger.Process(this.Name, "MES upload successfully!");
    }                  
    return "UpdateData";
Lựa chọn B: Bắn MES chạy ngầm hoàn toàn (Fire & Forget)

Nếu xưởng của bạn không cần đợi MES trả về kết quả, muốn máy chạy tiếp ngay lập tức để tiết kiệm chu kỳ (takt time):

csharp
case "UploadData":
    if (Parameter.GetBit("MesEnable"))
    {
        var mesData = currentBinTray.ConvertToMesData();
        // Bắn ngầm, trạm chuyển sang bước tiếp theo ngay lập tức (tiết kiệm 200-500ms)
        Task.Run(async () =>
        {
            var res = await MesMgr.SaveTestDataAsync(mesData);
            if (!res.success)
                Logger.Process(this.Name, $"MES async upload fail: {res.message}");
        });
    }                  
    return "UpdateData"; // Sang bước tiếp theo ngay tức thì
🎯 Tóm lại:
Sửa chính ở Autoframe/MesMgr.cs (viết hàm SaveTestDataAsync và HttpClient).
Ở Form_UnloadInfo.cs: Dùng async void button5_Click + await.
Ở StationUnloadBinEx.cs: Giữ nguyên gọi SaveTestData (Lựa chọn A) hoặc dùng Task.Run nếu muốn máy chạy vụt qua không cần chờ MES (Lựa chọn B).




🌟 Cách 1: Dùng JToken + Getter tự động ép kiểu (Khuyên dùng — Đơn giản nhất)

Bạn đổi bizData sang JToken (JToken chấp nhận mọi kiểu dữ liệu: boolean, object, null, string). Sau đó viết thêm 1 property bizData để tự động parse:

Sửa trong Autoframe/MesMgr.cs (dòng 62-74):

csharp
public class MesResponseModel
{
    public bool success = false;
    public string message = "";
    public string bizErrCode = "";
    public string bizErrMsg = "";
    // JToken hứng được cả boolean (false), object ({}), null mà KHÔNG BAO GIỜ bị crash
    [JsonProperty("bizData")]
    public JToken rawBizData { get; set; }
    // Property này giúp giữ nguyên cách gọi code cũ: reciveData.bizData.model
    [JsonIgnore]
    public BizData bizData
    {
        get
        {
            // Chỉ parse khi bizData thực sự là một JSON Object
            if (rawBizData != null && rawBizData.Type == JTokenType.Object)
            {
                return rawBizData.ToObject<BizData>();
            }
            return null; // Nếu là false, null, hoặc chuỗi rỗng thì trả về null an toàn
        }
    }
}
public class BizData
{
    public string model = "";
}
Ưu điểm:
Không bị văng Exception dù server trả về false, null hay bất cứ thứ gì.
Code gọi bên dưới không phải sửa gì cả, reciveData.bizData?.model vẫn chạy bình thường:
csharp
if (reciveData.success && reciveData.bizData != null)
{
    WoModel = reciveData.bizData.model;
}
🚀 Cách 2: Dùng JsonConverter tùy biến (Chuẩn công nghiệp)

Nếu không muốn sửa model mà muốn can thiệp trực tiếp vào bộ đọc JSON, bạn viết 1 bộ chuyển đổi:

csharp
public class SafeBizDataConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(BizData);
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        // Nếu server trả về boolean (false/true) hoặc null thì bỏ qua, coi như null
        if (reader.TokenType == JsonToken.Boolean || reader.TokenType == JsonToken.Null || reader.TokenType == JsonToken.String)
            return null;
        // Nếu là Object thì parse bình thường vào BizData
        return JObject.Load(reader).ToObject<BizData>();
    }
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }
}

Sau đó gắn nhãn lên thuộc tính bizData:

csharp
public class MesResponseModel
{
    public bool success = false;
    public string message = "";
    public string bizErrCode = "";
    public string bizErrMsg = "";
    [JsonConverter(typeof(SafeBizDataConverter))] // <-- Gắn thêm dòng này
    public BizData bizData;
}