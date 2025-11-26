using System.Drawing;

namespace AlgoTradeWithScottPlot
{
    /// <summary>
    /// Plot için tüm parametreleri tutan sınıf
    /// Pan, Zoom, Drag, Limits ve Reset durumlarını içerir
    /// </summary>
    public class PlotParameters
    {
        public int PlotIndex { get; set; }
        public string PlotId { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        // Pan Parametreleri
        public PanParameters Pan { get; set; } = new PanParameters();

        // Zoom Parametreleri  
        public ZoomParameters Zoom { get; set; } = new ZoomParameters();

        // Drag Parametreleri
        public DragParameters Drag { get; set; } = new DragParameters();

        // Axis Limits
        public AxisLimits XAxis { get; set; } = new AxisLimits();
        public AxisLimits YAxis { get; set; } = new AxisLimits();

        // Reset Durumu
        public ResetState Reset { get; set; } = new ResetState();

        // Mouse ve Interaction States
        public InteractionState Interaction { get; set; } = new InteractionState();

        public override string ToString()
        {
            return $"Plot[{PlotIndex}:{PlotId}] - Updated: {LastUpdated:HH:mm:ss}";
        }

        public PlotParameters Clone()
        {
            return new PlotParameters
            {
                PlotIndex = PlotIndex,
                PlotId = PlotId,
                LastUpdated = DateTime.Now,
                Pan = Pan.Clone(),
                Zoom = Zoom.Clone(), 
                Drag = Drag.Clone(),
                XAxis = XAxis.Clone(),
                YAxis = YAxis.Clone(),
                Reset = Reset.Clone(),
                Interaction = Interaction.Clone()
            };
        }
    }

    public class PanParameters
    {
        public bool IsActive { get; set; }
        public double OffsetX { get; set; }
        public double OffsetY { get; set; }
        public double VelocityX { get; set; }
        public double VelocityY { get; set; }
        public Point StartPosition { get; set; }
        public Point CurrentPosition { get; set; }
        public DateTime StartTime { get; set; }

        public PanParameters Clone()
        {
            return new PanParameters
            {
                IsActive = IsActive,
                OffsetX = OffsetX,
                OffsetY = OffsetY,
                VelocityX = VelocityX,
                VelocityY = VelocityY,
                StartPosition = StartPosition,
                CurrentPosition = CurrentPosition,
                StartTime = StartTime
            };
        }
    }

    public class ZoomParameters
    {
        public bool IsActive { get; set; }
        public double FactorX { get; set; } = 1.0;
        public double FactorY { get; set; } = 1.0;
        public double CenterX { get; set; }
        public double CenterY { get; set; }
        public int WheelDelta { get; set; }
        public Point MousePosition { get; set; }
        public bool IsRectangleZoom { get; set; }
        public Rectangle ZoomRectangle { get; set; }

        public ZoomParameters Clone()
        {
            return new ZoomParameters
            {
                IsActive = IsActive,
                FactorX = FactorX,
                FactorY = FactorY,
                CenterX = CenterX,
                CenterY = CenterY,
                WheelDelta = WheelDelta,
                MousePosition = MousePosition,
                IsRectangleZoom = IsRectangleZoom,
                ZoomRectangle = ZoomRectangle
            };
        }
    }

    public class DragParameters
    {
        public bool IsActive { get; set; }
        public Point StartPosition { get; set; }
        public Point EndPosition { get; set; }
        public Point DeltaPosition { get; set; }
        public MouseButtons Button { get; set; }
        public double Distance { get; set; }
        public TimeSpan Duration { get; set; }
        public bool IsValidDrag { get; set; }

        public DragParameters Clone()
        {
            return new DragParameters
            {
                IsActive = IsActive,
                StartPosition = StartPosition,
                EndPosition = EndPosition,
                DeltaPosition = DeltaPosition,
                Button = Button,
                Distance = Distance,
                Duration = Duration,
                IsValidDrag = IsValidDrag
            };
        }
    }

    public class AxisLimits
    {
        public double Min { get; set; }
        public double Max { get; set; }
        public double Range => Max - Min;
        public double Center => (Min + Max) / 2;
        public bool IsAutoScale { get; set; } = true;
        public bool IsLocked { get; set; }

        // Original limits (before any interaction)
        public double OriginalMin { get; set; }
        public double OriginalMax { get; set; }

        public AxisLimits Clone()
        {
            return new AxisLimits
            {
                Min = Min,
                Max = Max,
                IsAutoScale = IsAutoScale,
                IsLocked = IsLocked,
                OriginalMin = OriginalMin,
                OriginalMax = OriginalMax
            };
        }

        public void SetLimits(double min, double max)
        {
            Min = min;
            Max = max;
            IsAutoScale = false;
        }

        public void SetOriginalLimits(double min, double max)
        {
            OriginalMin = min;
            OriginalMax = max;
        }

        public void RestoreOriginal()
        {
            Min = OriginalMin;
            Max = OriginalMax;
        }
    }

    public class ResetState
    {
        public bool IsResetRequested { get; set; }
        public bool IsResetInProgress { get; set; }
        public DateTime LastResetTime { get; set; }
        public ResetType ResetType { get; set; }

        public ResetState Clone()
        {
            return new ResetState
            {
                IsResetRequested = IsResetRequested,
                IsResetInProgress = IsResetInProgress,
                LastResetTime = LastResetTime,
                ResetType = ResetType
            };
        }
    }

    public class InteractionState
    {
        public bool IsMouseDown { get; set; }
        public bool IsMouseOver { get; set; }
        public Point MousePosition { get; set; }
        public MouseButtons MouseButton { get; set; }
        public bool IsDragging { get; set; }
        public bool IsZooming { get; set; }
        public bool IsPanning { get; set; }
        public bool IsCrosshairActive { get; set; }

        public InteractionState Clone()
        {
            return new InteractionState
            {
                IsMouseDown = IsMouseDown,
                IsMouseOver = IsMouseOver,
                MousePosition = MousePosition,
                MouseButton = MouseButton,
                IsDragging = IsDragging,
                IsZooming = IsZooming,
                IsPanning = IsPanning,
                IsCrosshairActive = IsCrosshairActive
            };
        }
    }

    public enum ResetType
    {
        None = 0,
        ViewOnly = 1,      // Sadece görünüm (pan/zoom) reset
        DataOnly = 2,      // Sadece data reset
        Complete = 3       // Hem görünüm hem data
    }
}