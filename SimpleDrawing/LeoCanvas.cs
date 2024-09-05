using Avalonia;
using Avalonia.Media;
using SimpleDrawing.Core;
using SimpleDrawing.Window;

namespace SimpleDrawing;

/// <summary>
///     Provides a simple API for drawing basic shapes
/// </summary>
public static class LeoCanvas
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

    internal const double DefaultThickness = 1D;

    internal static readonly IBrush DefaultBrush = Brushes.Black;
    internal static readonly IBrush WhiteBrush = Brushes.White;
    internal static readonly object Mutex;
    internal static readonly List<DrawTask> Tasks;
    private static readonly Dictionary<Type, HashSet<int>> existingTaskHashes = new();
    private static bool _initDone;
    private static bool _windowInitialized;
    private static Action? _refreshWindow;

    static LeoCanvas()
    {
        Tasks = [];
        Mutex = new object();
        _windowInitialized = false;
        _refreshWindow = null;
    }

    /// <summary>
    ///     Initializes the window and canvas.
    ///     This method must be called before any other.
    /// </summary>
    /// <param name="mainMethod">Application main method</param>
    /// <param name="width">Width of the canvas</param>
    /// <param name="height">Height of the canvas</param>
    /// <param name="clickAction">An optional callback for handling user clicks</param>
    /// <param name="windowTitle">Title of the window</param>
    public static void Init(Action mainMethod, int width, int height,
                            Action<ClickEvent>? clickAction = null,
                            string windowTitle = Config.DefaultWindowTitle)
    {
        if (_initDone)
        {
            Console.WriteLine("Duplicate initialization!");

            return;
        }

        Config.ClickAction = clickAction;
        Config.Width = width;
        Config.Height = height;
        Config.WindowTitle = windowTitle;

        Task.Run(async () =>
        {
            try
            {
                var initialDelay = TimeSpan.FromSeconds(0.5D);
                var finalDelay = TimeSpan.FromSeconds(1);
                var delay = TimeSpan.FromMilliseconds(50);
                var maxWaitTime = TimeSpan.FromSeconds(10);

                await Task.Delay(initialDelay);

                var waitStartTime = DateTime.Now;
                var waitEndTime = waitStartTime + maxWaitTime;
                while (!_windowInitialized && DateTime.Now < waitEndTime)
                {
                    await Task.Delay(delay);
                }

                if (!_windowInitialized)
                {
                    Console.WriteLine("Failed to initialize application window!");
                }
                else
                {
                    await Task.Delay(finalDelay);
                    _initDone = true;

                    Clear();
                    mainMethod();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in console-app thread: {e.Message}");
            }
        });

        TriggerWindowCreation();
    }

    /// <summary>
    ///     Draws a line on the canvas.
    ///     Both points have to be within the bounds of the canvas.
    /// </summary>
    /// <param name="start">Starting point of the line</param>
    /// <param name="end">End point of the line</param>
    /// <param name="thickness">Thickness of the line; min. <see cref="MinThickness" /></param>
    /// <param name="color">Color of the line</param>
    /// <returns>True if the line config is valid; false otherwise</returns>
    public static bool DrawLine(Point start, Point end, double thickness = DefaultThickness, IBrush? color = null)
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
    /// <param name="lineThickness">Thickness of the border line; min. <see cref="MinThickness" /></param>
    /// <param name="lineColor">Color of the border line</param>
    /// <param name="fillColor">Fill color of the rectangle</param>
    /// <returns>True if the rectangle config is valid; false otherwise</returns>
    public static bool DrawRectangle(Point topLeft, Point bottomRight, double lineThickness = DefaultThickness,
                                     IBrush? lineColor = null, IBrush? fillColor = null)
    {
        if (!_initDone
            || !ValidateRectPoints(topLeft, bottomRight)
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
    /// <param name="radiusX">Radius of the ellipse on the x-axis; min. <see cref="MinRadius" /></param>
    /// <param name="radiusY">Radius of the ellipse on the y-axis; min. <see cref="MinRadius" /></param>
    /// <param name="lineThickness">Thickness of the border line; min. <see cref="MinThickness" /></param>
    /// <param name="lineColor">Color of the border line</param>
    /// <param name="fillColor">Fill color of the ellipse</param>
    /// <returns>True if the ellipse config is valid; false otherwise</returns>
    public static bool DrawEllipse(Point center, double radiusX, double radiusY,
                                   double lineThickness = DefaultThickness,
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
    /// <param name="lineThickness">Thickness of the border line; min. <see cref="MinThickness" /></param>
    /// <param name="lineColor">Color of the border line</param>
    /// <param name="fillColor">Fill color of the circle</param>
    /// <returns>True if the circle config is valid; false otherwise</returns>
    public static bool DrawCircle(Point center, double radius, double lineThickness = DefaultThickness,
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
    /// <param name="fontSize">Font size of the text specified in em; min. <see cref="MinFontSize" /></param>
    /// <param name="textColor">Color of the text</param>
    /// <returns>True if the text config is valid; false otherwise</returns>
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
    /// <returns>True if polygon config is valid; false otherwise</returns>
    public static bool DrawPolygonByPath(Point[] pathPoints, double lineThickness = DefaultThickness,
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
    ///     Draws an image on the canvas at the specified location.
    ///     Does not ensure proper scaling of the image.
    /// </summary>
    /// <param name="image">The image to draw</param>
    /// <param name="topLeft">Top left corner of the destination rectangle</param>
    /// <param name="bottomRight">Bottom right corner of the destination rectangle</param>
    /// <param name="rotationAngles">The rotation of the image in euler angles</param>
    /// <returns>True if the image config is valid; false otherwise</returns>
    public static bool DrawImageAtLocation(IImage image, Point topLeft, Point bottomRight, double rotationAngles = 0D)
    {
        if (!_initDone
            || !ValidateRectPoints(topLeft, bottomRight))
        {
            return false;
        }

        AddTask(new ImageDrawTask(image, new Rect(topLeft, bottomRight), rotationAngles));

        return true;
    }

    /// <summary>
    ///     Draws the image located at the specified path on the canvas at the specified location.
    ///     Only a limited number of image formats are supported.
    /// </summary>
    /// <param name="imagePath">Absolute path to the image file</param>
    /// <param name="topLeft">Top left corner of the destination rectangle</param>
    /// <param name="bottomRight">Bottom right corner of the destination rectangle</param>
    /// <param name="rotationAngles">The rotation of the image in euler angles</param>
    /// <returns>True if the image is found and the config is valid; false otherwise</returns>
    public static bool DrawImageAtLocation(string imagePath, Point topLeft, Point bottomRight,
                                           double rotationAngles = 0D)
    {
        if (TryLoadImage(imagePath, out var image) && image is not null)
        {
            return DrawImageAtLocation(image, topLeft, bottomRight, rotationAngles);
        }

        return false;
    }

    /// <summary>
    ///     Tries to load an image from the specified file path.
    /// </summary>
    /// <param name="imageFilePath">Absolute path to the image file</param>
    /// <param name="image">Set to the loaded image if loading is successful; null otherwise</param>
    /// <returns>True if the image could be loaded; false otherwise</returns>
    public static bool TryLoadImage(string imageFilePath, out IImage? image)
    {
        var imageLoader = ImageLoader.FromPath(imageFilePath);
        image = null;

        switch (imageLoader.ImageState)
        {
            case ImageLoader.State.Uninitialized:
            case ImageLoader.State.UnknownError:
            case ImageLoader.State.Loaded when imageLoader.Image is null:
            {
                Console.WriteLine("Failed to load image");

                return false;
            }
            case ImageLoader.State.FileDoesNotExist:
            {
                Console.WriteLine($"Failed to load image: file '{imageFilePath}' does not exist");

                return false;
            }
            case ImageLoader.State.FileFormatNotSupported:
            {
                Console.WriteLine("Failed to load image: file format not supported");

                return false;
            }
            case ImageLoader.State.Loaded when imageLoader.Image is not null:
            {
                image = imageLoader.Image;

                return true;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(imageLoader.ImageState), imageLoader.ImageState,
                                                      "Unknown image loading state");
        }
    }

    /// <summary>
    ///     Clears the canvas of all previously drawn shapes.
    /// </summary>
    public static void Clear()
    {
        lock (Mutex)
        {
            Tasks.Clear();
            existingTaskHashes.Clear();
            PrepareBackground();
        }
    }

    /// <summary>
    ///     Refreshes the canvas.
    ///     Has to be called all time changes (e.g., newly added shapes) are to be displayed to the user.
    ///     The first call will take some time until the window is initialized.
    /// </summary>
    public static void Render()
    {
        if (!_windowInitialized)
        {
            Console.WriteLine("Window not initialized yet!");

            return;
        }

        _refreshWindow?.Invoke();
    }

    internal static void SetWindowInitialized(Action refreshWindow)
    {
        _windowInitialized = true;
        _refreshWindow = refreshWindow;
    }

    private static void TriggerWindowCreation()
    {
        try
        {
            var exitCode = BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime([]);
            Console.WriteLine($"Window exited with code {exitCode}");
        }
        catch (Exception e)
        {
            _windowInitialized = false;
            Console.WriteLine($"Failed to initialize the window: {e.Message}");
        }

        return;

        static AppBuilder BuildAvaloniaApp() =>
            AppBuilder.Configure<App>()
                      .UsePlatformDetect()
                      .WithInterFont()
                      .LogToTrace();
    }

    private static void AddTask<T>(T task) where T : DrawTask
    {
        lock (Mutex)
        {
            Tasks.Add(task);
        }
        
        /*var hash = task.GetHashCode();
        var type = typeof(T);
        lock (Mutex)
        {
            var addTask = true;
            if (!existingTaskHashes.TryGetValue(type, out var existingHashes))
            {
                existingTaskHashes[type] = [hash];
            }
            else
            {
                if (!existingHashes.Add(hash))
                {
                    addTask = false;
                }
            }

            if (addTask)
            {
                Tasks.Add(task);
            }
        }*/
    }

    private static bool ValidatePoints(params Point[] points) => ValidatePoints(points.AsEnumerable());
    private static bool ValidatePoints(IEnumerable<Point> points) => points.All(ValidatePoint);

    private static bool ValidateRectPoints(Point topLeft, Point bottomRight) =>
        ValidatePoint(topLeft)
        && ValidatePoint(bottomRight)
        && topLeft.X < bottomRight.X
        && topLeft.Y < bottomRight.Y;

    private static bool ValidatePoint(Point point) =>
        point is { X: >= 0, Y: >= 0 }
        && point.X <= Config.Width
        && point.Y <= Config.Height;

    private static void PrepareBackground()
    {
        // 'background' => required for proper click events
        DrawRectangle(new Point(0, 0), new Point(Config.Width, Config.Height),
                      lineColor: Brushes.Gray, fillColor: WhiteBrush);
    }
}
