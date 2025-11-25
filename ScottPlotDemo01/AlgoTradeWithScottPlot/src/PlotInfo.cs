using ScottPlot;
using ScottPlot.Plottables;
using System.Drawing;

namespace AlgoTradeWithScottPlot.Components
{
    /// <summary>
    /// Bir plot hakkında bilgileri tutan sınıf
    /// Her plot'un metadata'sını ve ayarlarını saklar
    /// </summary>
    public class PlotInfo
    {
        /// <summary>
        /// Plot'un benzersiz kimliği
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Plot'un adı (görüntülenecek isim)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Plot'un tipi (PlotType enum'undan)
        /// </summary>
        public PlotType PlotType { get; set; }

        /// <summary>
        /// Plot'un görünür olup olmadığı
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// Plot'un rengi
        /// </summary>
        public System.Drawing.Color Color { get; set; }

        /// <summary>
        /// Plot'un kalınlığı (çizgi kalınlığı vb.)
        /// </summary>
        public float LineWidth { get; set; }

        /// <summary>
        /// Plot'un Z-order'ı (hangi sırada çizileceği)
        /// </summary>
        public int ZOrder { get; set; }

        /// <summary>
        /// ScottPlot'un gerçek plottable nesnesi
        /// </summary>
        public IPlottable? Plottable { get; set; }

        /// <summary>
        /// Plot'un açıklaması
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Plot'un oluşturulma tarihi
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Plot'un son güncelleme tarihi
        /// </summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Plot'un ait olduğu grup (isteğe bağlı)
        /// </summary>
        public string? Group { get; set; }

        /// <summary>
        /// Plot'un ekstra özellikleri için dictionary
        /// </summary>
        public Dictionary<string, object> Properties { get; set; }

        /// <summary>
        /// Plot'un hangi Y eksenini kullandığı (Left, Right, vb.)
        /// </summary>
        public string YAxis { get; set; }

        /// <summary>
        /// Plot'un legend'da gösterilip gösterilmeyeceği
        /// </summary>
        public bool ShowInLegend { get; set; }

        /// <summary>
        /// PlotInfo constructor
        /// </summary>
        /// <param name="id">Plot'un benzersiz kimliği</param>
        /// <param name="name">Plot'un adı</param>
        /// <param name="plotType">Plot'un tipi</param>
        public PlotInfo(string id, string name, PlotType plotType)
        {
            Id = id;
            Name = name;
            PlotType = plotType;
            IsVisible = true;
            Color = System.Drawing.Color.Blue;
            LineWidth = 1.0f;
            ZOrder = 0;
            Description = "";
            CreatedDate = DateTime.Now;
            LastUpdated = DateTime.Now;
            Properties = new Dictionary<string, object>();
            YAxis = "Left";
            ShowInLegend = true;
        }

        /// <summary>
        /// Plot bilgilerini günceller
        /// </summary>
        public void Update()
        {
            LastUpdated = DateTime.Now;
        }

        /// <summary>
        /// Plot'un özelliğini ayarlar
        /// </summary>
        /// <param name="key">Özellik anahtarı</param>
        /// <param name="value">Özellik değeri</param>
        public void SetProperty(string key, object value)
        {
            Properties[key] = value;
            Update();
        }

        /// <summary>
        /// Plot'un özelliğini alır
        /// </summary>
        /// <typeparam name="T">Dönüş tipi</typeparam>
        /// <param name="key">Özellik anahtarı</param>
        /// <param name="defaultValue">Varsayılan değer</param>
        /// <returns>Özellik değeri</returns>
        public T GetProperty<T>(string key, T defaultValue = default(T))
        {
            if (Properties.TryGetValue(key, out var value) && value is T)
            {
                return (T)value;
            }
            return defaultValue;
        }

        /// <summary>
        /// Plot'un string temsilini döndürür
        /// </summary>
        /// <returns>Plot bilgisi</returns>
        public override string ToString()
        {
            return $"{Name} ({PlotType}) - {(IsVisible ? "Visible" : "Hidden")}";
        }

        /// <summary>
        /// Plot'u kopyalar (deep copy)
        /// </summary>
        /// <returns>Kopya PlotInfo</returns>
        public PlotInfo Clone()
        {
            var clone = new PlotInfo(Id + "_copy", Name + "_copy", PlotType)
            {
                IsVisible = IsVisible,
                Color = Color,
                LineWidth = LineWidth,
                ZOrder = ZOrder,
                Description = Description,
                Group = Group,
                YAxis = YAxis,
                ShowInLegend = ShowInLegend,
                Properties = new Dictionary<string, object>(Properties)
            };
            return clone;
        }
    }
}