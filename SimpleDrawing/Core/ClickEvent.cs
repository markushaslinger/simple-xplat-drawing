using Avalonia;
using Avalonia.Input;

namespace SimpleDrawing.Core;

/// <summary>
///     Represents a click event.
/// </summary>
/// <param name="ClickedPoint">Position the user clicked at</param>
/// <param name="Button">Mouse button used for the click</param>
public readonly record struct ClickEvent(Point ClickedPoint, MouseButton Button);
