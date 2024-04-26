﻿using System.Globalization;
using Avalonia;
using Avalonia.Media;

namespace SimpleXPlatDrawing;

internal abstract class DrawTask(double thickness, IBrush color)
{
    protected Pen Pen { get; } = new(color, thickness, lineCap: PenLineCap.Round);

    public abstract void DrawSelf(DrawingContext context);
}

internal sealed class LineDrawTask(Point start, Point end, double thickness, IBrush color)
    : DrawTask(thickness, color)
{
    public override void DrawSelf(DrawingContext context)
    {
        context.DrawLine(Pen, start, end);
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
    public override void DrawSelf(DrawingContext context)
    {
        context.DrawRectangle(fillColor, Pen, new Rect(topLeft, bottomRight));
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
    public override void DrawSelf(DrawingContext context)
    {
        context.DrawEllipse(fillColor, Pen, center, radiusX, radiusY);
    }
}

internal sealed class TextDrawTask(Point origin, string text, double emSize, IBrush textColor)
    : DrawTask(0, SimpleDrawing.DefaultBrush)
{
    private readonly FormattedText _text = new(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                                               Typeface.Default, emSize, textColor);

    public override void DrawSelf(DrawingContext context)
    {
        context.DrawText(_text, origin);
    }
}

internal sealed class PathDrawTask(IReadOnlyList<Point> pathPoints, double thickness, IBrush lineColor, IBrush? fillColor)
    : DrawTask(thickness, lineColor)
{
    public override void DrawSelf(DrawingContext context)
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
        context.DrawGeometry(fillColor ?? SimpleDrawing.WhiteBrush, Pen, geo);
    }
}
