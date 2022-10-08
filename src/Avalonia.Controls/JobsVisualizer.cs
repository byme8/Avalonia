using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using Avalonia.Threading;

namespace Avalonia.Controls;

public class JobsVisualizer : Control
{
    private readonly Pen _yellow;
    private readonly Pen _red;
    private readonly Pen _green;
    private readonly Pen _other;
    private readonly Dictionary<int, Pen> _colors;
    private readonly JobTelemetryRecipient _jobTelemetryRecipient;

    private const int LineWidth = 1;
    private const double LineOpacity = 0.8;

    public JobsVisualizer()
    {
        _red = new Pen(new SolidColorBrush(Colors.Red, LineOpacity), LineWidth);
        _yellow = new Pen(new SolidColorBrush(Colors.Yellow, LineOpacity), LineWidth);
        _green = new Pen(new SolidColorBrush(Colors.Green, LineOpacity), LineWidth);
        _other = new Pen(new SolidColorBrush(Colors.LightGray, LineOpacity), LineWidth);

        _colors = new Dictionary<DispatcherPriority, Color>
            {
                {
                    DispatcherPriority.Background, Colors.Blue
                },
                {
                    DispatcherPriority.Input, Colors.BlueViolet
                },
                {
                    DispatcherPriority.Loaded, Colors.Orange
                },
                {
                    DispatcherPriority.Render, Colors.OrangeRed
                },
                {
                    DispatcherPriority.Composition, Colors.GreenYellow
                },
                {
                    DispatcherPriority.Layout, Colors.Red
                },
                {
                    DispatcherPriority.Send, Colors.Yellow
                },
            }
            .ToDictionary(o => o.Key.Value, o => new Pen(new SolidColorBrush(o.Value), 5));

        _jobTelemetryRecipient = new JobTelemetryRecipient(100);
        _jobTelemetryRecipient.FrameReady += () => Dispatcher.UIThread.InvokeAsync(InvalidateVisual);

        Dispatcher.UIThread.JobRunner.JobTelemetryRecipient = _jobTelemetryRecipient;
    }

    public override void Render(DrawingContext context)
    {
        const int _16ms = 16;
        const int _32ms = 32;
        const int _100ms = 100;
        const int maxDuration = 300;
        
        var history = _jobTelemetryRecipient.History;
        var currentIndex = _jobTelemetryRecipient.CurrentFrameIndex;

        var controlWidth = Bounds.Width;
        var controlHeight = Bounds.Height;

        var virtualWidthPixel = controlWidth / history.Length;
        var virtualHeightPixel = controlHeight / maxDuration;

        for (int i = 0; i < history.Length; i++)
        {
            var frameToRenderIndex = (i + currentIndex) % history.Length;
            var historyFrame = history[frameToRenderIndex];
            if (historyFrame == null)
            {
                continue;
            }

            foreach (var frame in historyFrame)
            {
                if (!_colors.TryGetValue(frame.Key, out var pen))
                {
                    pen = _other;
                }

                var jobDuration = frame.Value;
                context.DrawLine(pen, new Point(i * virtualWidthPixel, controlHeight), new Point(i * virtualWidthPixel, controlHeight - (jobDuration * virtualHeightPixel)));
            }
        }

        context.DrawLine(_green, new Point(0, controlHeight - (_16ms * virtualHeightPixel)), new Point(controlWidth, controlHeight - (_16ms * virtualHeightPixel)));
        context.DrawLine(_yellow, new Point(0, controlHeight - (_32ms * virtualHeightPixel)), new Point(controlWidth, controlHeight - (_32ms * virtualHeightPixel)));
        context.DrawLine(_red, new Point(0, controlHeight - (_100ms * virtualHeightPixel)), new Point(controlWidth, controlHeight - (_100ms * virtualHeightPixel)));
    }
}
