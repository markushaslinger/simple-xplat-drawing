namespace SimpleDrawing.Core;

internal static class Config
{
    internal const string DefaultWindowTitle = "SimpleDrawing";
    
    public static int Width { get; set; }
    public static int Height { get; set; }
    public static string WindowTitle { get; set; } = DefaultWindowTitle;
    public static Action<ClickEvent>? ClickAction { get; set; }
}
