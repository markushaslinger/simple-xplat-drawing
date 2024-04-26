using System.Globalization;
using Avalonia;
using Avalonia.Media;

namespace SimpleDrawing.Core;

internal delegate Pen PenFactory(PenConfig config);
internal readonly record struct PenConfig(IBrush Color, double Thickness, PenLineCap LineCap);

internal abstract class DrawTask(double thickness, IBrush color)
{
    protected PenConfig PenConfig { get; } = new(color, thickness, PenLineCap.Round);

    public abstract void DrawSelf(DrawingContext context, PenFactory penFactory);
}

internal sealed class LineDrawTask(Point start, Point end, double thickness, IBrush color)
    : DrawTask(thickness, color)
{
    public override void DrawSelf(DrawingContext context, PenFactory penFactory)
    {
        var pen = penFactory(PenConfig);
        context.DrawLine(pen, start, end);
    }
}

internal sealed class RectDrawTask(
    Point topLeft,
    Point bottomRight,
    double thickness,
    IBrush lineColor,
    IBrush? fillColor)
    : DrawTask(thickness, lineColor)
{
    public override void DrawSelf(DrawingContext context, PenFactory penFactory)
    {
        context.DrawRectangle(fillColor, penFactory(PenConfig), new Rect(topLeft, bottomRight));
    }
}

internal sealed class EllipseDrawTask(
    Point center,
    double radiusX,
    double radiusY,
    double lineThickness,
    IBrush lineColor,
    IBrush? fillColor)
    : DrawTask(lineThickness, lineColor)
{
    public override void DrawSelf(DrawingContext context, PenFactory penFactory)
    {
        context.DrawEllipse(fillColor, penFactory(PenConfig), center, radiusX, radiusY);
    }
}

internal sealed class TextDrawTask(Point origin, string text, double emSize, IBrush textColor)
    : DrawTask(0, LeoCanvas.DefaultBrush)
{
    private readonly FormattedText _text = new(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                                               Typeface.Default, emSize, textColor);

    public override void DrawSelf(DrawingContext context, PenFactory penFactory)
    {
        context.DrawText(_text, origin);
    }
}

internal sealed class PathDrawTask(IReadOnlyList<Point> pathPoints, double thickness, IBrush lineColor, IBrush? fillColor)
    : DrawTask(thickness, lineColor)
{
    public override void DrawSelf(DrawingContext context, PenFactory penFactory)
    {
        var figure = new PathFigure
        {
            IsClosed = true,
            IsFilled = fillColor != null,
            Segments = new PathSegments(),
            StartPoint = pathPoints[0]
        };
        for (var i = 1; i < pathPoints.Count; i++)
        {
            figure.Segments.Add(new LineSegment
            {
                Point = pathPoints[i]
            });
        }

        var geo = new PathGeometry
        {
            Figures = new PathFigures { figure }
        };
        context.DrawGeometry(fillColor ?? LeoCanvas.WhiteBrush, penFactory(PenConfig), geo);
    }
}

internal sealed class ImageDrawTask(IImage image, Rect location) : DrawTask(LeoCanvas.DefaultThickness, LeoCanvas.DefaultBrush)
{
    public override void DrawSelf(DrawingContext context, PenFactory penFactory)
    {
        context.DrawImage(image, location);
    }
}