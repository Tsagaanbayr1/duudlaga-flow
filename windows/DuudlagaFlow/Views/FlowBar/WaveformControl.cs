using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DuudlagaFlow.Views.FlowBar;

public class WaveformControl : Control
{
    public static readonly DependencyProperty AudioLevelsProperty =
        DependencyProperty.Register(nameof(AudioLevels), typeof(ObservableCollection<float>),
            typeof(WaveformControl), new PropertyMetadata(null, OnAudioLevelsChanged));

    public static readonly DependencyProperty DotColorProperty =
        DependencyProperty.Register(nameof(DotColor), typeof(Brush),
            typeof(WaveformControl), new PropertyMetadata(Brushes.White));

    public static readonly DependencyProperty DotCountProperty =
        DependencyProperty.Register(nameof(DotCount), typeof(int),
            typeof(WaveformControl), new PropertyMetadata(5));

    public ObservableCollection<float>? AudioLevels
    {
        get => (ObservableCollection<float>?)GetValue(AudioLevelsProperty);
        set => SetValue(AudioLevelsProperty, value);
    }

    public Brush DotColor
    {
        get => (Brush)GetValue(DotColorProperty);
        set => SetValue(DotColorProperty, value);
    }

    public int DotCount
    {
        get => (int)GetValue(DotCountProperty);
        set => SetValue(DotCountProperty, value);
    }

    private static void OnAudioLevelsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (WaveformControl)d;

        if (e.OldValue is ObservableCollection<float> oldCollection)
        {
            oldCollection.CollectionChanged -= control.OnCollectionChanged;
        }

        if (e.NewValue is ObservableCollection<float> newCollection)
        {
            newCollection.CollectionChanged += control.OnCollectionChanged;
        }

        control.InvalidateVisual();
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        var levels = AudioLevels;
        if (levels == null || levels.Count == 0)
        {
            DrawIdleDots(dc);
            return;
        }

        int dotCount = DotCount;
        double totalWidth = ActualWidth;
        double totalHeight = ActualHeight;
        double dotRadius = 2.5;
        double spacing = totalWidth / (dotCount + 1);
        double maxScale = totalHeight / (dotRadius * 2) - 1;

        for (int i = 0; i < dotCount; i++)
        {
            // Sample from different parts of the levels array
            int index = levels.Count > dotCount
                ? (int)((double)i / dotCount * levels.Count)
                : Math.Min(i, levels.Count - 1);

            float level = levels[Math.Min(index, levels.Count - 1)];
            double scale = 1.0 + level * maxScale * 3; // Amplify for visibility
            scale = Math.Min(scale, maxScale);

            double x = spacing * (i + 1);
            double y = totalHeight / 2;
            double radiusX = dotRadius;
            double radiusY = dotRadius * scale;

            dc.DrawEllipse(DotColor, null, new Point(x, y), radiusX, radiusY);
        }
    }

    private void DrawIdleDots(DrawingContext dc)
    {
        int dotCount = DotCount;
        double totalWidth = ActualWidth;
        double totalHeight = ActualHeight;
        double dotRadius = 2.0;
        double spacing = totalWidth / (dotCount + 1);

        for (int i = 0; i < dotCount; i++)
        {
            double x = spacing * (i + 1);
            double y = totalHeight / 2;
            dc.DrawEllipse(DotColor, null, new Point(x, y), dotRadius, dotRadius);
        }
    }
}
