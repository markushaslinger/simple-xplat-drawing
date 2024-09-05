using System.Globalization;
using Avalonia;
using Avalonia.Media;

namespace SimpleDrawing.Core;

internal delegate Pen PenFactory(PenConfig config);

internal readonly record struct PenConfig(IBrush Color, double Thickness, PenLineCap LineCap);

internal abstract class DrawTask(double thickness, IBrush color)
{
    protected PenConfig PenConfig { get; } = new(color, thickness, PenLineCap.Round);

    public bool Equals(DrawTask? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return PenConfig.Equals(other.PenConfig);
    }

    public abstract void DrawSelf(DrawingContext context, PenFactory penFactory);

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((DrawTask) obj);
    }

    public override int GetHashCode() => PenConfig.GetHashCode();

    public static bool operator ==(DrawTask? left, DrawTask? right) => Equals(left, right);

    public static bool operator !=(DrawTask? left, DrawTask? right) => !Equals(left, right);
}

internal sealed class LineDrawTask(Point start, Point end, double thickness, IBrush color)
    : DrawTask(thickness, color), IEquatable<LineDrawTask>
{
    private readonly Point _end = end;
    private readonly Point _start = start;

    public bool Equals(LineDrawTask? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return base.Equals(other) && ((_start.Equals(other._start) && _end.Equals(other._end))
                                      || (_start.Equals(other._end) && _end.Equals(other._start)));
    }

    public override void DrawSelf(DrawingContext context, PenFactory penFactory)
    {
        var pen = penFactory(PenConfig);
        context.DrawLine(pen, _start, _end);
    }

    public override bool Equals(object? obj) =>
        ReferenceEquals(this, obj) || (obj is LineDrawTask other && Equals(other));

    public override int GetHashCode()
    {
        var s = _start.GetHashCode();
        var e = _end.GetHashCode();
        var a = Math.Min(s, e);
        var b = Math.Max(s, e);

        return HashCode.Combine(base.GetHashCode(), a, b);
    }

    public static bool operator ==(LineDrawTask? left, LineDrawTask? right) => Equals(left, right);

    public static bool operator !=(LineDrawTask? left, LineDrawTask? right) => !Equals(left, right);
}

internal sealed class RectDrawTask(
    Point topLeft,
    Point bottomRight,
    double thickness,
    IBrush lineColor,
    IBrush? fillColor)
    : DrawTask(thickness, lineColor), IEquatable<RectDrawTask>
{
    private readonly Point _bottomRight = bottomRight;
    private readonly IBrush? _fillColor = fillColor;
    private readonly Point _topLeft = topLeft;

    public bool Equals(RectDrawTask? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return base.Equals(other) && Equals(_fillColor, other._fillColor)
                                  && ((_bottomRight.Equals(other._bottomRight) && _topLeft.Equals(other._topLeft))
                                      || (_bottomRight.Equals(other._topLeft) && _topLeft.Equals(other._bottomRight)));
    }

    public override void DrawSelf(DrawingContext context, PenFactory penFactory)
    {
        context.DrawRectangle(_fillColor, penFactory(PenConfig), new Rect(_topLeft, _bottomRight));
    }

    public override bool Equals(object? obj) =>
        ReferenceEquals(this, obj) || (obj is RectDrawTask other && Equals(other));

    public override int GetHashCode()
    {
        var top = _topLeft.GetHashCode();
        var bottom = _bottomRight.GetHashCode();
        var a = Math.Min(top, bottom);
        var b = Math.Max(top, bottom);

        return HashCode.Combine(base.GetHashCode(), _fillColor, a, b);
    }

    public static bool operator ==(RectDrawTask? left, RectDrawTask? right) => Equals(left, right);

    public static bool operator !=(RectDrawTask? left, RectDrawTask? right) => !Equals(left, right);
}

internal sealed class EllipseDrawTask(
    Point center,
    double radiusX,
    double radiusY,
    double lineThickness,
    IBrush lineColor,
    IBrush? fillColor)
    : DrawTask(lineThickness, lineColor), IEquatable<EllipseDrawTask>
{
    private readonly Point _center = center;
    private readonly IBrush? _fillColor = fillColor;
    private readonly double _radiusX = radiusX;
    private readonly double _radiusY = radiusY;

    public bool Equals(EllipseDrawTask? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return base.Equals(other) && _center.Equals(other._center) && Equals(_fillColor, other._fillColor) &&
               _radiusX.Equals(other._radiusX) && _radiusY.Equals(other._radiusY);
    }

    public override void DrawSelf(DrawingContext context, PenFactory penFactory)
    {
        context.DrawEllipse(_fillColor, penFactory(PenConfig), _center, _radiusX, _radiusY);
    }

    public override bool Equals(object? obj) =>
        ReferenceEquals(this, obj) || (obj is EllipseDrawTask other && Equals(other));

    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), _center, _fillColor, _radiusX, _radiusY);

    public static bool operator ==(EllipseDrawTask? left, EllipseDrawTask? right) => Equals(left, right);

    public static bool operator !=(EllipseDrawTask? left, EllipseDrawTask? right) => !Equals(left, right);
}

internal sealed class TextDrawTask(Point origin, string text, double emSize, IBrush textColor)
    : DrawTask(0, LeoCanvas.DefaultBrush), IEquatable<TextDrawTask>
{
    private readonly double _emSize = emSize;

    private readonly FormattedText _formattedText = new(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                                                        Typeface.Default, emSize, textColor);

    private readonly Point _origin = origin;
    private readonly string _text = text;
    private readonly IBrush _textColor = textColor;

    public bool Equals(TextDrawTask? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return base.Equals(other) && _origin.Equals(other._origin) && _text == other._text &&
               _emSize.Equals(other._emSize) && _textColor.Equals(other._textColor);
    }

    public override void DrawSelf(DrawingContext context, PenFactory penFactory)
    {
        context.DrawText(_formattedText, _origin);
    }

    public override bool Equals(object? obj) =>
        ReferenceEquals(this, obj) || (obj is TextDrawTask other && Equals(other));

    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), _origin, _text, _emSize, _textColor);

    public static bool operator ==(TextDrawTask? left, TextDrawTask? right) => Equals(left, right);

    public static bool operator !=(TextDrawTask? left, TextDrawTask? right) => !Equals(left, right);
}

internal sealed class PathDrawTask(
    IReadOnlyList<Point> pathPoints,
    double thickness,
    IBrush lineColor,
    IBrush? fillColor)
    : DrawTask(thickness, lineColor), IEquatable<PathDrawTask>
{
    private readonly IBrush? _fillColor = fillColor;
    private readonly IReadOnlyList<Point> _pathPoints = pathPoints;

    public bool Equals(PathDrawTask? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return base.Equals(other) && Equals(_fillColor, other._fillColor) &&
               _pathPoints.SequenceEqual(other._pathPoints);
    }

    public override void DrawSelf(DrawingContext context, PenFactory penFactory)
    {
        var figure = new PathFigure
        {
            IsClosed = true,
            IsFilled = _fillColor != null,
            Segments = [],
            StartPoint = _pathPoints[0]
        };
        for (var i = 1; i < _pathPoints.Count; i++)
        {
            figure.Segments.Add(new LineSegment
            {
                Point = _pathPoints[i]
            });
        }

        var geo = new PathGeometry
        {
            Figures = [figure]
        };
        context.DrawGeometry(_fillColor ?? LeoCanvas.WhiteBrush, penFactory(PenConfig), geo);
    }

    public override bool Equals(object? obj) =>
        ReferenceEquals(this, obj) || (obj is PathDrawTask other && Equals(other));

    public override int GetHashCode()
    {
        var hash = HashCode.Combine(base.GetHashCode(), _fillColor);
        foreach (var point in _pathPoints)
        {
            hash = HashCode.Combine(hash, point);
        }

        return hash;
    }

    public static bool operator ==(PathDrawTask? left, PathDrawTask? right) => Equals(left, right);

    public static bool operator !=(PathDrawTask? left, PathDrawTask? right) => !Equals(left, right);
}

internal sealed class ImageDrawTask(IImage image, Rect location, double rotationAngle)
    : DrawTask(LeoCanvas.DefaultThickness, LeoCanvas.DefaultBrush), IEquatable<ImageDrawTask>
{
    private readonly IImage _image = image;
    private readonly Rect _location = location;
    private readonly double _rotationAngleRadians = Math.PI * Math.Clamp(rotationAngle, 0D, 360D) / 180D;

    public bool Equals(ImageDrawTask? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return base.Equals(other) && _image.Equals(other._image) && _location.Equals(other._location) &&
               _rotationAngleRadians.Equals(other._rotationAngleRadians);
    }

    public override void DrawSelf(DrawingContext context, PenFactory penFactory)
    {
        if (Math.Abs(_rotationAngleRadians) < double.Epsilon)
        {
            context.DrawImage(_image, _location);

            return;
        }

        using (PrepareRotationMatrix(context))
        {
            context.DrawImage(_image, _location);
        }
    }

    private DrawingContext.PushedState PrepareRotationMatrix(DrawingContext context)
    {
        var locationCenter = new Point(_location.X + _location.Width / 2D, _location.Y + _location.Height / 2D);
        var rotationTransform = Matrix.CreateTranslation(-locationCenter.X, -locationCenter.Y) *
                                Matrix.CreateRotation(_rotationAngleRadians) *
                                Matrix.CreateTranslation(locationCenter.X, locationCenter.Y);

        return context.PushTransform(rotationTransform);
    }

    public override bool Equals(object? obj) =>
        ReferenceEquals(this, obj) || (obj is ImageDrawTask other && Equals(other));

    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), _image, _location, _rotationAngleRadians);

    public static bool operator ==(ImageDrawTask? left, ImageDrawTask? right) => Equals(left, right);

    public static bool operator !=(ImageDrawTask? left, ImageDrawTask? right) => !Equals(left, right);
}
