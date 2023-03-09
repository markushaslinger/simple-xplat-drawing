using System.Globalization;
using Avalonia;
using Avalonia.Media;

namespace SimpleXPlatDrawing;

internal abstract class DrawTask
{
    protected DrawTask(double thickness, IBrush color)
    {
        Pen = new Pen(color, thickness, lineCap: PenLineCap.Round);
    }

    protected Pen Pen { get; }

    public abstract void DrawSelf(DrawingContext context);
}

internal sealed class LineDrawTask : DrawTask
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

internal sealed class RectDrawTask : DrawTask
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

internal sealed class EllipseDrawTask : DrawTask
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

internal sealed class TextDrawTask : DrawTask
{
    private readonly Point _origin;
    private readonly FormattedText _text;

    public TextDrawTask(Point origin, string text, double emSize, IBrush textColor)
        : base(0, SimpleDrawing.DefaultBrush)
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

internal sealed class PathDrawTask : DrawTask
{
    private readonly IBrush? _fillColor;
    private readonly Point[] _pathPoints;

    public PathDrawTask(Point[] pathPoints, double thickness, IBrush lineColor, IBrush? fillColor)
        : base(thickness, lineColor)
    {
        _fillColor = fillColor;
        _pathPoints = pathPoints;
    }

    public override void DrawSelf(DrawingContext context)
    {
        var figure = new PathFigure
        {
            IsClosed = true,
            IsFilled = _fillColor != null,
            Segments = new PathSegments(),
            StartPoint = _pathPoints[0]
        };
        for (var i = 1; i < _pathPoints.Length; i++)
        {
            figure.Segments.Add(new LineSegment
            {
                Point = _pathPoints[i]
            });
        }

        var geo = new PathGeometry
        {
            Figures = new PathFigures { figure }
        };
        context.DrawGeometry(_fillColor ?? SimpleDrawing.WhiteBrush, Pen, geo);
    }
}
