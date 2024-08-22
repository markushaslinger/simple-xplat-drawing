using Avalonia.Controls;
using Avalonia.Media;

namespace SimpleDrawing.Core;

internal sealed class DrawingCanvas: UserControl
{
    public override void Render(DrawingContext context)
    {
        IReadOnlyCollection<DrawTask> renderTasks;
        lock (LeoCanvas.Mutex)
        {
            renderTasks = [..LeoCanvas.Tasks];
        }
        
        foreach (var drawTask in renderTasks)
        {
            drawTask.DrawSelf(context, CreatePen);
        }
    }
    
    private static Pen CreatePen(PenConfig config) => new(config.Color, config.Thickness, lineCap: config.LineCap);
}

