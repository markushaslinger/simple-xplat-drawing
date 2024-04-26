using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;

namespace SimpleXPlatDrawing;

/// <summary>
///     Provides a simple API for drawing basic shapes
/// </summary>
public static class SimpleDrawing
{
    /// <summary>
    ///     The min. thickness of all lines
    /// </summary>
    public const double MinThickness = 0.1D;
    
    /// <summary>
    ///     The min. radius of any ellipse
    /// </summary>
    public const double MinRadius = MinThickness;
    
    /// <summary>
    ///     The min. font size of text in em.
    /// </summary>
    public const double MinFontSize = 4;
    
    internal static readonly IBrush DefaultBrush = Brushes.Black;
    internal static readonly IBrush WhiteBrush = Brushes.White;
    
    private const string DefaultWindowTitle = "SimpleDrawing";
    private static readonly object mutex;
    private static readonly List<DrawTask> tasks;
    private static App? _app;
    private static bool _initDone;
    private static Action<Point>? _clickAction;
    private static string _windowTitle;
    private static bool _windowInitialized;

    static SimpleDrawing()
    {
        tasks = [];
        _app = null;
        _windowTitle = DefaultWindowTitle;
        mutex = new object();
        _windowInitialized = false;
    }

    private static int Width { get; set; }
    private static int Height { get; set; }

    /// <summary>
    ///     Initializes the window and canvas.
    ///     This method must be called before any other.
    /// </summary>
    /// <param name="width">Width of the canvas</param>
    /// <param name="height">Height of the canvas</param>
    /// <param name="clickAction">An optional callback for handling user clicks</param>
    /// <param name="windowTitle">Title of the window</param>
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

        PrepareBackground();
    }

    /// <summary>
    ///     Draws a line on the canvas.
    ///     Both points have to be within the bounds of the canvas.
    /// </summary>
    /// <param name="start">Starting point of the line</param>
    /// <param name="end">End point of the line</param>
    /// <param name="thickness">Thickness of the line; min. <see cref="MinThickness"/></param>
    /// <param name="color">Color of the line</param>
    /// <returns>True if the line can and will be drawn; false otherwise</returns>
    public static bool DrawLine(Point start, Point end, double thickness = 1D, IBrush? color = null)
    {
        if (!_initDone 
            || thickness < MinThickness
            || !ValidatePoints(start, end))
        {
            return false;
        }

        color ??= DefaultBrush;

        AddTask(new LineDrawTask(start, end, thickness, color));

        return true;
    }

    /// <summary>
    ///     Draws a rectangle on the canvas.
    ///     All four points of the rectangle have to be within the bounds of the canvas.
    /// </summary>
    /// <param name="topLeft">Top left corner of the rectangle</param>
    /// <param name="bottomRight">Bottom right corner of the rectangle</param>
    /// <param name="lineThickness">Thickness of the border line; min. <see cref="MinThickness"/></param>
    /// <param name="lineColor">Color of the border line</param>
    /// <param name="fillColor">Fill color of the rectangle</param>
    /// <returns>True if the rectangle can and will be drawn; false otherwise</returns>
    public static bool DrawRectangle(Point topLeft, Point bottomRight, double lineThickness = 1D,
                                     IBrush? lineColor = null, IBrush? fillColor = null)
    {
        if (!_initDone
            || !ValidatePoints(topLeft, bottomRight) 
            || lineThickness < MinThickness)
        {
            return false;
        }

        lineColor ??= DefaultBrush;

        AddTask(new RectDrawTask(topLeft, bottomRight, lineThickness, lineColor, fillColor));

        return true;
    }

    /// <summary>
    ///     Draws an ellipse on the canvas.
    ///     The entirety of the ellipse has to be within the bounds of the canvas.
    /// </summary>
    /// <param name="center">Center point of the ellipse</param>
    /// <param name="radiusX">Radius of the ellipse on the x-axis; min. <see cref="MinRadius"/></param>
    /// <param name="radiusY">Radius of the ellipse on the y-axis; min. <see cref="MinRadius"/></param>
    /// <param name="lineThickness">Thickness of the border line; min. <see cref="MinThickness"/></param>
    /// <param name="lineColor">Color of the border line</param>
    /// <param name="fillColor">Fill color of the ellipse</param>
    /// <returns>True if the ellipse can and will be drawn; false otherwise</returns>
    public static bool DrawEllipse(Point center, double radiusX, double radiusY, double lineThickness = 1D,
                                   IBrush? lineColor = null, IBrush? fillColor = null)
    {
        Point[] corners =
        [
            new Point(center.X + radiusX / 2D, center.Y),
            new Point(center.X - radiusX / 2D, center.Y),
            new Point(center.X, center.Y + radiusY / 2D),
            new Point(center.X, center.Y - radiusY / 2D)
        ];
        
        if (!_initDone
            || radiusX < MinRadius
            || radiusY < MinRadius
            || !ValidatePoints(corners.Concat(new[] { center }))
            || lineThickness < MinThickness)
        {
            return false;
        }

        lineColor ??= DefaultBrush;

        AddTask(new EllipseDrawTask(center, radiusX, radiusY, lineThickness, lineColor, fillColor));

        return true;
    }

    /// <summary>
    ///     Draws a circle on the canvas.
    ///     The entirety of the circle has to be within the bounds of the canvas.
    /// </summary>
    /// <param name="center">Center point of the circle</param>
    /// <param name="radius">Radius of the circle</param>
    /// <param name="lineThickness">Thickness of the border line; min. <see cref="MinThickness"/></param>
    /// <param name="lineColor">Color of the border line</param>
    /// <param name="fillColor">Fill color of the circle</param>
    /// <returns>True if the circle can and will be drawn; false otherwise</returns>
    public static bool DrawCircle(Point center, double radius, double lineThickness = 1D,
                                  IBrush? lineColor = null, IBrush? fillColor = null) =>
        DrawEllipse(center, radius, radius, lineThickness, lineColor, fillColor);

    /// <summary>
    ///     Draws text on the canvas.
    ///     The starting point of the text has to be within the bounds of the canvas.
    ///     Caution: except for the starting point boundaries of the text are not validated,
    ///     be careful to keep it within the canvas area.
    /// </summary>
    /// <param name="origin">Starting point of the text</param>
    /// <param name="text">The text to draw</param>
    /// <param name="fontSize">Font size of the text specified in em; min. <see cref="MinFontSize"/></param>
    /// <param name="textColor">Color of the text</param>
    /// <returns>True if the text can and will be drawn; false otherwise</returns>
    public static bool DrawText(Point origin, string text, double fontSize, IBrush? textColor = null)
    {
        if (!_initDone 
            || !ValidatePoint(origin) 
            || fontSize < MinFontSize)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            // invisible, don't draw at all
            return true;
        }

        textColor ??= DefaultBrush;

        AddTask(new TextDrawTask(origin, text, fontSize, textColor));

        return true;
    }

    /// <summary>
    ///     Draws a polygon defined by a closed path.
    ///     The path is defined as a series of points, which have to be within the boundaries of the canvas.
    ///     Use one of the specific methods for drawing simple figures like a rectangle.
    /// </summary>
    /// <param name="pathPoints">Points on the path; min. 3</param>
    /// <param name="lineThickness">Thickness of the border line</param>
    /// <param name="lineColor">Color of the border line</param>
    /// <param name="fillColor">Fill color of the polygon</param>
    /// <returns>True if polygon can and will be drawn; false otherwise</returns>
    public static bool DrawPolygonByPath(Point[] pathPoints, double lineThickness = 1D,
                                         IBrush? lineColor = null, IBrush? fillColor = null)
    {
        if (!_initDone
            || pathPoints.Length < 3
            || lineThickness < MinThickness
            || !ValidatePoints(pathPoints))
        {
            return false;
        }

        lineColor ??= DefaultBrush;

        AddTask(new PathDrawTask(pathPoints, lineThickness, lineColor, fillColor));

        return true;
    }

    /// <summary>
    ///     Clears the canvas of all previously drawn shapes.
    /// </summary>
    public static void Clear()
    {
        lock (mutex)
        {
            tasks.Clear();
            PrepareBackground();
        }
    }

    /// <summary>
    ///     Refreshes the canvas.
    ///     Has to be called all time changes (e.g., newly added shapes) are to be displayed to the user.
    ///     The first call will take some time until the window is initialized.
    /// </summary>
    public static async ValueTask Render()
    {
        if (_app == null)
        {
            await OpenWindow();
        }

        _app?.Refresh();
    }

    private static async Task OpenWindow()
    {
        var initialDelay = TimeSpan.FromSeconds(1);
        var delay = TimeSpan.FromMilliseconds(50);
        var maxWaitTime = TimeSpan.FromSeconds(10);
        
        var appBuilder = AppBuilder.Configure<App>()
                                   .UsePlatformDetect()
                                   .WithInterFont()
                                   .LogToTrace();
        
#pragma warning disable CS4014
        Task.Run(() => { appBuilder.StartWithClassicDesktopLifetime([]); });
#pragma warning restore CS4014
        
        await Task.Delay(initialDelay);
        
        var waitStartTime = DateTime.Now;
        while (!_windowInitialized && DateTime.Now - waitStartTime < maxWaitTime)
        {
            await Task.Delay(delay);
        }

        if (!_windowInitialized || appBuilder.Instance is not App app)
        {
            throw new InvalidOperationException("Failed to initialize the window!");
        }

        _app = app;
    }
    
    private static void AddTask(DrawTask task)
    {
        lock (mutex)
        {
            tasks.Add(task);
        }
    }

    private static bool ValidatePoints(params Point[] points) => ValidatePoints(points.AsEnumerable());
    private static bool ValidatePoints(IEnumerable<Point> points) => points.All(ValidatePoint);

    private static bool ValidatePoint(Point point) =>
        point is { X: >= 0, Y: >= 0 }
        && point.X <= Width && point.Y <= Height;

    private static void PrepareBackground()
    {
        // 'background' => required for proper click events
        DrawRectangle(new Point(0, 0), new Point(Width, Height),
                      lineColor: Brushes.Gray, fillColor: WhiteBrush);
    }

    private sealed class App : Application
    {
        private const double WindowExtraSize = 6D;
        private const double CanvasMargin = WindowExtraSize / 2D;
        private Canvas? _canvas;
        private Window _mainWindow = default!;

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
                _canvas.PointerPressed += (_, clickEvent) => { _clickAction?.Invoke(clickEvent.GetPosition(_canvas)); };
            }

            base.OnFrameworkInitializationCompleted();
            _windowInitialized = true;
        }

        public void Refresh()
        {
            Dispatcher.UIThread.Post(() => { _canvas?.InvalidateVisual(); }, DispatcherPriority.MaxValue);
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
