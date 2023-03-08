using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;

namespace SimpleXPlatDrawing;

public static class SimpleDrawing
{
    private const string DefaultWindowTitle = "SimpleDrawing";
    private const double MinThickness = 0.1D;
    private static readonly IBrush defaultBrush = Brushes.Black;
    private static readonly object mutex;
    private static readonly List<DrawTask> tasks;
    private static App? _app;
    private static bool _initDone;
    private static Action<Point>? _clickAction;
    private static string _windowTitle;

    static SimpleDrawing()
    {
        tasks = new();
        _app = null;
        _windowTitle = DefaultWindowTitle;
        mutex = new object();
    }

    private static int Width { get; set; }
    private static int Height { get; set; }

    public static void Init(int width, int height, 
                            Action<Point>? clickAction = null, string windowTitle = DefaultWindowTitle)
    {
        if (_initDone)
        {
            Console.WriteLine("Duplicate initialization!");
            return;
        }

        _initDone = true;
        _clickAction = clickAction;
        _windowTitle = windowTitle;
        Width = width;
        Height = height;
        
        // background, required for proper click events
        DrawRectangle(new (0,0), new(width, height), 
                      lineColor: Brushes.Gray, fillColor: Brushes.White);
    }

    public static bool DrawLine(Point start, Point end, double thickness = 1D, IBrush? color = null)
    {
        if (!ValidatePoints(start, end) || thickness < MinThickness)
        {
            return false;
        }

        color ??= defaultBrush;

        AddTask(new LineDrawTask(start, end, thickness, color));

        return true;
    }

    public static bool DrawRectangle(Point topLeft, Point bottomRight, double lineThickness = 1D,
                                     IBrush? lineColor = null, IBrush? fillColor = null)
    {
        if (!ValidatePoints(topLeft, bottomRight) || lineThickness < MinThickness)
        {
            return false;
        }

        lineColor ??= defaultBrush;

        AddTask(new RectDrawTask(topLeft, bottomRight, lineThickness, lineColor, fillColor));

        return true;
    }

    public static bool DrawEllipse(Point center, double radiusX, double radiusY, double lineThickness = 1D,
                                   IBrush? lineColor = null, IBrush? fillColor = null)
    {
        Point[] corners = new[]
        {
            new Point(center.X + radiusX / 2D, center.Y),
            new Point(center.X - radiusX / 2D, center.Y),
            new Point(center.X, center.Y + radiusY / 2D),
            new Point(center.X, center.Y - radiusY / 2D)
        };
        if (radiusX < MinThickness || radiusY < MinThickness
                                   || !ValidatePoints(corners.Concat(new[] { center }))
                                   || lineThickness < MinThickness)
        {
            return false;
        }

        lineColor ??= defaultBrush;

        AddTask(new EllipseDrawTask(center, radiusX, radiusY, lineThickness, lineColor, fillColor));

        return true;
    }

    public static bool DrawCircle(Point center, double radius, double lineThickness = 1D,
                                  IBrush? lineColor = null, IBrush? fillColor = null) =>
        DrawEllipse(center, radius, radius, lineThickness, lineColor, fillColor);

    public static bool DrawText(Point origin, string text, double fontSize, IBrush? textColor = null)
    {
        if (!ValidatePoint(origin) || fontSize < MinThickness)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            // invisible, don't draw at all
            return true;
        }
        
        textColor ??= defaultBrush;
        
        AddTask(new TextDrawTask(origin, text, fontSize, textColor));

        return true;
    }

    public static void Clear()
    {
        lock (mutex)
        {
            tasks.Clear();
        }
    }

    private static void AddTask(DrawTask task)
    {
        lock (mutex)
        {
            tasks.Add(task);
        }
    }

    public static async Task Render()
    {
        if (_app == null)
        {
            var appBuilder = AppBuilder.Configure<App>().UsePlatformDetect();
            #pragma warning disable CS4014
            Task.Run(() =>
            {
                appBuilder.StartWithClassicDesktopLifetime(Array.Empty<string>());
            });
            #pragma warning restore CS4014
            await Task.Delay(TimeSpan.FromSeconds(2));
            _app = appBuilder.Instance as App;
        }
        
        _app?.Refresh();
    }

    private static bool ValidatePoints(params Point[] points) => ValidatePoints(points.AsEnumerable());
    private static bool ValidatePoints(IEnumerable<Point> points) => points.All(ValidatePoint);

    private static bool ValidatePoint(Point point) =>
        point is { X: >= 0, Y: >= 0 }
        && point.X <= Width && point.Y <= Height;

    private abstract class DrawTask
    {
        protected DrawTask(double thickness, IBrush color)
        {
            Pen = new Pen(color, thickness, lineCap: PenLineCap.Round);
        }

        protected Pen Pen { get; }

        public abstract void DrawSelf(DrawingContext context);
    }

    private sealed class LineDrawTask : DrawTask
    {
        private readonly Point _end;
        private readonly Point _start;

        public LineDrawTask(Point start, Point end, double thickness, IBrush color) : base(thickness, color)
        {
            _start = start;
            _end = end;
        }

        public override void DrawSelf(DrawingContext context)
        {
            context.DrawLine(Pen, _start, _end);
        }
    }

    private sealed class RectDrawTask : DrawTask
    {
        private readonly Point _bottomRight;
        private readonly IBrush? _fillColor;
        private readonly Point _topLeft;

        public RectDrawTask(Point topLeft, Point bottomRight, double thickness, IBrush lineColor, IBrush? fillColor)
            : base(thickness, lineColor)
        {
            _topLeft = topLeft;
            _bottomRight = bottomRight;
            _fillColor = fillColor;
        }

        public override void DrawSelf(DrawingContext context)
        {
            context.DrawRectangle(_fillColor, Pen, new(_topLeft, _bottomRight));
        }
    }

    private sealed class EllipseDrawTask : DrawTask
    {
        private readonly Point _center;
        private readonly IBrush? _fillColor;
        private readonly double _radiusX;
        private readonly double _radiusY;

        public EllipseDrawTask(Point center, double radiusX, double radiusY, double lineThickness,
                               IBrush lineColor, IBrush? fillColor) : base(lineThickness, lineColor)
        {
            _center = center;
            _radiusX = radiusX;
            _radiusY = radiusY;
            _fillColor = fillColor;
        }

        public override void DrawSelf(DrawingContext context)
        {
            context.DrawEllipse(_fillColor, Pen, _center, _radiusX, _radiusY);
        }
    }

    private sealed class TextDrawTask : DrawTask
    {
        private readonly Point _origin;
        private readonly FormattedText _text;

        public TextDrawTask(Point origin, string text, double emSize, IBrush textColor) : base(0, defaultBrush)
        {
            _origin = origin;
            _text = new(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                        Typeface.Default, emSize, textColor);
        }

        public override void DrawSelf(DrawingContext context)
        {
            context.DrawText(_text, _origin);
        }
    }

    private sealed class App : Application
    {
        private const double WindowExtraSize = 6D;
        private const double CanvasMargin = WindowExtraSize / 2D;
        private Window _mainWindow = default!;
        private Canvas? _canvas;

        public override void OnFrameworkInitializationCompleted()
        {
            Styles.Add(new FluentTheme());
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                _canvas = new Canvas
                {
                    Margin = new Thickness(CanvasMargin, CanvasMargin, CanvasMargin, CanvasMargin)
                };
                _mainWindow = new Window
                {
                    Title = _windowTitle,
                    Width = Width + WindowExtraSize,
                    Height = Height + WindowExtraSize,
                    Content = _canvas
                };
                desktop.MainWindow = _mainWindow;
                _canvas.PointerPressed += (_, clickEvent) =>
                {
                    _clickAction?.Invoke(clickEvent.GetPosition(_canvas));
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

        public void Refresh()
        {
            Dispatcher.UIThread.Post(() =>
            {
                _canvas?.InvalidateVisual();
            }, DispatcherPriority.MaxValue);
        }

        private sealed class Canvas : UserControl
        {
            public override void Render(DrawingContext context)
            {
                DrawTask[] renderTasks;
                lock (mutex)
                {
                    renderTasks = tasks.ToArray();
                }
                foreach (var drawTask in renderTasks)
                {
                    drawTask.DrawSelf(context);
                }
            }
        }
    }
}
