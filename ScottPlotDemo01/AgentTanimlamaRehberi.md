# Claude Code Agent Tanımlama Rehberi

## Agent Nedir?

Agent (veya subagent), Claude Code içinde belirli görevler için uzmanlaşmış bağımsız AI asistanlarıdır. Ana Claude'dan bağımsız çalışarak özel görevleri yerine getirir.

---

## Klasör Yapısı

```
Proje/
├── .claude/
│   ├── agents.json              # Agent tanımları (ana dosya)
│   ├── agents/                  # Agent prompt dosyaları
│   │   ├── scottplot-helper.md
│   │   ├── trading-analyzer.md
│   │   └── code-reviewer.md
│   ├── commands/                # Custom slash komutları
│   └── hooks/                   # Pre/post command hooks
├── CLAUDE.md                    # Proje talimatları
└── src/
```

---

## 1. agents.json (Ana Konfigürasyon)

**Konum:** `.claude/agents.json`

```json
{
  "agents": [
    {
      "id": "scottplot-helper",
      "name": "ScottPlot Helper Agent",
      "description": "ScottPlot grafik entegrasyonu ve konfigürasyonu uzmanı",
      "path": ".claude/agents/scottplot-helper.md",
      "enabled": true,
      "capabilities": [
        "chart-creation",
        "data-binding",
        "styling",
        "performance-optimization"
      ]
    },
    {
      "id": "trading-analyzer",
      "name": "Trading Strategy Analyzer",
      "description": "Trading stratejisi analizi ve optimizasyon uzmanı",
      "path": ".claude/agents/trading-analyzer.md",
      "enabled": true,
      "capabilities": [
        "strategy-analysis",
        "indicator-optimization",
        "backtest-evaluation",
        "performance-metrics"
      ]
    },
    {
      "id": "code-reviewer",
      "name": "C# Code Reviewer",
      "description": "C# kod kalitesi ve best practices kontrolü",
      "path": ".claude/agents/code-reviewer.md",
      "enabled": true,
      "capabilities": [
        "code-quality",
        "security-check",
        "performance-review",
        "architecture-analysis"
      ]
    }
  ],
  "defaultAgent": "scottplot-helper"
}
```

---

## 2. Agent Tanım Dosyası Yapısı

Her agent için `.claude/agents/` altında bir markdown dosyası oluşturulur.

### Temel Yapı:

```markdown
# Agent Adı

## Purpose (Amaç)
Bu ajanın ne işe yaradığı

## Capabilities (Yetenekler)
- Yetenek 1
- Yetenek 2
- Yetenek 3

## Instructions (Talimatlar)
Ajanın nasıl çalışacağına dair detaylı talimatlar

## Context (Bağlam)
Proje hakkında bilgiler

## Guidelines (Kurallar)
- Kural 1
- Kural 2

## Tools Available (Kullanılabilir Araçlar)
Ajanın kullanabileceği araçlar
```

---

## 3. Örnek Agent 1: ScottPlot Helper

**Dosya:** `.claude/agents/scottplot-helper.md`

```markdown
# ScottPlot Helper Agent

## Purpose
ScottPlot 5.x kütüphanesinin WinForms uygulamasına entegrasyonu, konfigürasyonu
ve kullanımı konusunda uzman destek sağlar.

## Capabilities
- Chart creation and configuration
- Real-time data binding
- Multi-series management
- Custom styling and themes
- Performance optimization
- Interactive features (zoom, pan, crosshair)

## Instructions

You are a ScottPlot expert specializing in version 5.1.57 integration with WinForms.

### Core Responsibilities:

1. **Chart Setup**
   - FormsPlot initialization
   - Axis configuration
   - Legend setup
   - Grid and styling

2. **Data Visualization**
   - OHLC/Candlestick charts
   - Line charts for indicators
   - Volume histograms
   - Multiple y-axis handling

3. **Real-time Updates**
   - Efficient data append methods
   - Render optimization
   - Memory management
   - Update throttling

4. **User Interaction**
   - Mouse events
   - Zoom/pan configuration
   - Crosshair implementation
   - Tooltip customization

### Code Example Pattern:

```csharp
// Always provide complete, copy-paste ready examples like:
var plt = formsPlot1.Plot;
plt.Add.Signal(data);
plt.Axes.AutoScale();
formsPlot1.Refresh();
```

## Context
This is a financial trading system that displays:
- Price data (OHLC candles)
- Technical indicators (MA, RSI, MACD, Bollinger Bands)
- Volume data
- Multiple timeframes (1m, 5m, 15m, 1h, 1d)

Current ScottPlot version: 5.1.57

## Guidelines
- Provide WinForms-specific examples
- Consider real-time performance
- Use Turkish variable names when they appear in codebase
- Reference ScottPlot 5.x API (not 4.x)
- Include performance considerations
- Explain memory management best practices

## Common Tasks
1. Creating candlestick charts for financial data
2. Adding moving average overlays
3. Creating sub-plots for indicators (RSI, MACD)
4. Synchronizing axis across multiple plots
5. Implementing crosshair with data tooltip
6. Optimizing for large datasets (10k+ candles)

## Tools Available
- File reading and code analysis
- Documentation lookup
- Code generation and refactoring
- Performance profiling suggestions
```

---

## 4. Örnek Agent 2: Trading Strategy Analyzer

**Dosya:** `.claude/agents/trading-analyzer.md`

```markdown
# Trading Strategy Analyzer Agent

## Purpose
Trading stratejilerinin performans analizi, optimizasyonu ve iyileştirme
önerileri sunar. Backtest sonuçlarını değerlendirir ve risk yönetimini inceler.

## Capabilities
- Strategy performance analysis
- Indicator parameter optimization
- Risk/reward evaluation
- Backtest result interpretation
- Entry/exit signal quality assessment
- Drawdown analysis

## Instructions

You are a quantitative trading analyst specializing in algorithmic trading strategies.

### Analysis Areas:

1. **Performance Metrics**
   - Win rate (kazanma oranı)
   - Profit factor (kâr faktörü)
   - Sharpe ratio
   - Maximum drawdown
   - Average trade duration
   - Risk/reward ratio

2. **Strategy Logic**
   - Entry signal quality
   - Exit signal effectiveness
   - Filter effectiveness (time, trend, volatility)
   - Position sizing appropriateness
   - Stop loss and take profit placement

3. **Indicator Analysis**
   - Parameter optimization suggestions
   - Overfitting detection
   - Indicator combination effectiveness
   - Lag and sensitivity analysis

4. **Risk Management**
   - Position sizing review
   - Drawdown tolerance
   - Leverage usage
   - Correlation analysis

### Analysis Output Format:

```
## Strategy: [Strategi Adı]

### Performance Summary
- Win Rate: X%
- Profit Factor: X.XX
- Max Drawdown: X%
- Total Trades: XXX

### Strengths
1. [Güçlü yön 1]
2. [Güçlü yön 2]

### Weaknesses
1. [Zayıf yön 1]
2. [Zayıf yön 2]

### Optimization Suggestions
1. [Öneri 1]
2. [Öneri 2]

### Code Improvements
[Kod önerileri]
```

## Context
Project: Trading system with these components:
- CSystemWrapper: Main orchestrator
- CTrader: Trading logic and order management
- CIndicatorManager: Technical indicators (MA, RSI, MACD, etc.)
- CKarAlZararKes: Take profit and stop loss management
- CStatistics: Performance tracking
- CVarlikManager: Portfolio management

Common indicators used:
- Moving Averages (SMA, EMA, WMA, HMA, ZLEMA)
- RSI (Relative Strength Index)
- MACD (Moving Average Convergence Divergence)
- Bollinger Bands
- ATR (Average True Range)

## Guidelines
- Focus on actionable insights
- Consider market conditions (trend, ranging)
- Turkish terminology is acceptable (e.g., "kâr", "zarar")
- Provide specific parameter ranges for optimization
- Warn about overfitting risks
- Consider transaction costs and slippage
- Suggest realistic performance expectations

## Analysis Workflow
1. Read strategy file (e.g., ons_ma_v1.cs)
2. Identify entry/exit logic
3. Examine indicator parameters
4. Review risk management rules
5. Analyze backtest results (if available)
6. Provide comprehensive report
7. Suggest specific code improvements

## Tools Available
- Code file reading and analysis
- Statistics calculation
- Pattern recognition
- Risk assessment
- Optimization algorithms
```

---

## 5. Agent Kullanımı

### A. Direct Agent Call (Claude Code'da)

```bash
# Agent'ı çağırma
/@scottplot-helper ScottPlot'ta candlestick chart nasıl oluştururum?

/@trading-analyzer Bu stratejiyi analiz edebilir misin?

/@code-reviewer Form1.cs dosyasını incele
```

### B. Custom Slash Command ile

`.claude/commands/analyze.md` oluşturun:

```markdown
# /analyze

Trading stratejisini analiz etmek için trading-analyzer agent'ını çağırır.

**Usage:**
```
/analyze <strategy-file>
```

**Example:**
```
/analyze ons_ma_v1.cs
/analyze libDevelopmentWithinSystemWrapperOpt.cs
```
```

### C. Programmatic Usage

```csharp
// Claude Code API üzerinden (eğer kullanıyorsanız)
var result = await claude.CallAgent("scottplot-helper",
    "Candlestick chart nasıl oluştururum?");
```

---

## 6. CLAUDE.md'ye Agent Bilgisi Ekleme

Proje kök dizinindeki `CLAUDE.md` dosyasına ekleyin:

```markdown
## Available Agents

### ScottPlot Helper Agent
- **Command:** `/@scottplot-helper`
- **Purpose:** ScottPlot grafik entegrasyonu ve konfigürasyonu
- **Use Cases:**
  - Chart oluşturma
  - Data binding
  - Styling ve tema ayarları
  - Performance optimizasyonu

### Trading Strategy Analyzer
- **Command:** `/@trading-analyzer`
- **Purpose:** Trading stratejisi analizi ve optimizasyon
- **Use Cases:**
  - Backtest sonuçları analizi
  - İndikatör optimizasyonu
  - Risk yönetimi değerlendirmesi
  - Entry/exit signal kalitesi

### Code Reviewer
- **Command:** `/@code-reviewer`
- **Purpose:** C# kod kalitesi incelemesi
- **Use Cases:**
  - Kod kalitesi kontrolü
  - Güvenlik açığı tespiti
  - Performance review
  - Best practices kontrolü
```

---

## 7. Agent Test Etme

```bash
# 1. Agent dosyalarını oluştur
mkdir .claude
mkdir .claude/agents

# 2. agents.json ve agent .md dosyalarını oluştur

# 3. Claude Code'da test et
/@scottplot-helper Merhaba, çalışıyor musun?

/@trading-analyzer Test mesajı
```

---

## 8. Best Practices

### Agent Tasarımı
- ✅ Tek sorumluluk prensibi (her agent bir göreve odaklanmalı)
- ✅ Clear ve spesifik talimatlar
- ✅ Proje context'i dahil et
- ✅ Örnek kod formatları belirt
- ✅ Tool listesini net tanımla

### Agent Kullanımı
- ✅ Doğru agent'ı seç (göreve uygun)
- ✅ Net ve spesifik sorular sor
- ✅ Context sağla (dosya, kod parçası)
- ✅ Beklenen çıktı formatını belirt

### Performans
- ✅ Agent talimatlarını kısa tut (token tasarrufu)
- ✅ Gereksiz capability listesi ekleme
- ✅ Caching için benzer promptlar kullan

---

## 9. Debugging

### Agent çalışmıyor?
1. `agents.json` formatını kontrol et (valid JSON)
2. `path` alanlarının doğru olduğunu kontrol et
3. `.md` dosyalarının varlığını doğrula
4. `enabled: true` olduğunu kontrol et

### Agent yanlış davranıyor?
1. Instructions bölümünü daha spesifik yap
2. Context bilgisini artır
3. Guidelines ekle veya güncelle
4. Example output formatları ekle

---

## 10. Gelişmiş Özellikler

### A. Agent Chaining
Bir agent başka bir agent'ı çağırabilir:

```markdown
## Instructions
If code review reveals performance issues,
call the trading-analyzer agent for deeper analysis.
```

### B. Conditional Agent Activation
```json
{
  "id": "emergency-agent",
  "triggers": ["error", "exception", "crash"],
  "autoActivate": true
}
```

### C. Agent Hooks
Pre/post command hooks ile otomatik agent çağırma:

```markdown
# .claude/hooks/pre-commit.md
Before every commit, call code-reviewer agent to check code quality.
```

---

## Özet

1. ✅ `.claude/agents.json` oluştur
2. ✅ `.claude/agents/*.md` dosyaları oluştur
3. ✅ Her agent için Purpose, Capabilities, Instructions tanımla
4. ✅ CLAUDE.md'ye agent bilgileri ekle
5. ✅ `/@agent-id` ile test et

**Sonraki Adımlar:**
- Custom slash commands ekle
- Hooks ile otomasyon kur
- Agent chaining kur
- Performance monitoring ekle
