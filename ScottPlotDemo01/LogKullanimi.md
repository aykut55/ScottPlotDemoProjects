# C# Log Tutma Seçenekleri (Memo/TextBox Türleri)

## 1. RichTextBox (En Önerilen)

### Özellikler
- Renkli log yazma desteği
- Formatlanmış metin
- Auto-scroll
- En esnek çözüm

### Örnek Kod
```csharp
RichTextBox logBox = new RichTextBox();
logBox.Multiline = true;
logBox.ScrollBars = ScrollBars.Vertical;
logBox.ReadOnly = true;
logBox.Dock = DockStyle.Fill;

// Renkli log yazma
public void LogInfo(string message)
{
    logBox.SelectionColor = Color.Green;
    logBox.AppendText($"[{DateTime.Now:HH:mm:ss}] INFO: {message}\n");
    logBox.ScrollToCaret();
}

public void LogError(string message)
{
    logBox.SelectionColor = Color.Red;
    logBox.AppendText($"[{DateTime.Now:HH:mm:ss}] ERROR: {message}\n");
    logBox.ScrollToCaret();
}

public void LogWarning(string message)
{
    logBox.SelectionColor = Color.Orange;
    logBox.AppendText($"[{DateTime.Now:HH:mm:ss}] WARNING: {message}\n");
    logBox.ScrollToCaret();
}
```

---

## 2. TextBox (Basit Çözüm)

### Özellikler
- Hafif ve hızlı
- Tek renkli metin
- Basit kullanım

### Örnek Kod
```csharp
TextBox logBox = new TextBox();
logBox.Multiline = true;
logBox.ScrollBars = ScrollBars.Vertical;
logBox.ReadOnly = true;
logBox.Dock = DockStyle.Fill;

// Log yazma
public void Log(string message)
{
    logBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
    logBox.SelectionStart = logBox.Text.Length;
    logBox.ScrollToCaret();
}
```

---

## 3. ListBox (Satır Bazlı)

### Özellikler
- Her log satır olarak eklenir
- Kolay filtreleme
- Satır seçimi mümkün

### Örnek Kod
```csharp
ListBox logBox = new ListBox();
logBox.Dock = DockStyle.Fill;

// Log ekleme
public void Log(string message)
{
    logBox.Items.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
    logBox.TopIndex = logBox.Items.Count - 1; // Auto-scroll
}

// Tüm logları temizle
public void ClearLogs()
{
    logBox.Items.Clear();
}
```

---

## 4. ListView (Yapılandırılmış)

### Özellikler
- Kolonlu yapı (Zaman, Seviye, Mesaj)
- Sıralama ve filtreleme
- Profesyonel görünüm

### Örnek Kod
```csharp
ListView logView = new ListView();
logView.View = View.Details;
logView.FullRowSelect = true;
logView.GridLines = true;
logView.Dock = DockStyle.Fill;

// Kolonlar
logView.Columns.Add("Zaman", 100);
logView.Columns.Add("Seviye", 80);
logView.Columns.Add("Mesaj", 400);

// Log ekleme
public void Log(string level, string message)
{
    var item = new ListViewItem(DateTime.Now.ToString("HH:mm:ss"));
    item.SubItems.Add(level);
    item.SubItems.Add(message);

    // Seviyeye göre renklendirme
    switch (level)
    {
        case "ERROR":
            item.ForeColor = Color.Red;
            break;
        case "WARNING":
            item.ForeColor = Color.Orange;
            break;
        case "SUCCESS":
            item.ForeColor = Color.Green;
            break;
    }

    logView.Items.Add(item);
    logView.EnsureVisible(logView.Items.Count - 1);
}
```

---

## 5. Önerilen Çözüm: RichTextBox + Helper Class

### Tam Özellikli Log Helper
```csharp
public class LogHelper
{
    private RichTextBox logBox;
    private int maxLines = 1000; // Performans için limit

    public LogHelper(RichTextBox textBox)
    {
        logBox = textBox;
        logBox.ReadOnly = true;
        logBox.ScrollBars = RichTextBoxScrollBars.Vertical;
    }

    public void Log(string message, Color color)
    {
        // Thread-safe işlem
        if (logBox.InvokeRequired)
        {
            logBox.Invoke(new Action(() => Log(message, color)));
            return;
        }

        logBox.SelectionColor = color;
        logBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
        logBox.ScrollToCaret();

        // Satır sayısı kontrolü (performans için)
        if (logBox.Lines.Length > maxLines)
        {
            var lines = logBox.Lines.Skip(100).ToArray();
            logBox.Lines = lines;
        }
    }

    public void Info(string msg) => Log(msg, Color.Black);
    public void Success(string msg) => Log(msg, Color.Green);
    public void Warning(string msg) => Log(msg, Color.Orange);
    public void Error(string msg) => Log(msg, Color.Red);
    public void Debug(string msg) => Log(msg, Color.Gray);

    public void Clear()
    {
        if (logBox.InvokeRequired)
        {
            logBox.Invoke(new Action(Clear));
            return;
        }
        logBox.Clear();
    }
}
```

### Kullanım
```csharp
// Form1.cs
public partial class Form1 : Form
{
    private LogHelper logger;

    public Form1()
    {
        InitializeComponent();
        logger = new LogHelper(richTextBox1);
    }

    private void button1_Click(object sender, EventArgs e)
    {
        logger.Info("İşlem başlatıldı");

        try
        {
            // İşlemler...
            logger.Success("İşlem başarılı");
        }
        catch (Exception ex)
        {
            logger.Error($"Hata: {ex.Message}");
        }
    }
}
```

---

## Karşılaştırma

| Özellik | RichTextBox | TextBox | ListBox | ListView |
|---------|-------------|---------|---------|----------|
| Renkli Metin | ✅ | ❌ | ❌ | ✅ |
| Formatlanmış Metin | ✅ | ❌ | ❌ | ❌ |
| Performans | Orta | Yüksek | Yüksek | Orta |
| Kolon Desteği | ❌ | ❌ | ❌ | ✅ |
| Filtreleme | Zor | Zor | Kolay | Kolay |
| Kullanım Kolaylığı | Kolay | Çok Kolay | Kolay | Orta |

---

## Trading Projeleri İçin Öneriler

**Geliştirme/Debug:** RichTextBox + LogHelper (renkli loglar için)

**Production:** ListView (yapılandırılmış ve filtrelenebilir)

**Basit İhtiyaçlar:** TextBox (hafif ve hızlı)

**Performans Kritik:** ListBox (hızlı ekleme/silme)
