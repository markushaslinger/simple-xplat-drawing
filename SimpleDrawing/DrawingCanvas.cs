using System.Collections.Immutable;
using Avalonia.Controls;
using Avalonia.Media;

namespace SimpleXPlatDrawing;

internal sealed class DrawingCanvas: UserControl
{
    public override void Render(DrawingContext context)
    {
        ImmutableArray<DrawTask> renderTasks;
        lock (SimpleDrawing.Mutex)
        {
            renderTasks = SimpleDrawing.Tasks.ToImmutableArray();
        }
        
        foreach (var drawTask in renderTasks)
        {
            drawTask.DrawSelf(context, CreatePen);
        }
    }
    
    private static Pen CreatePen(PenConfig config) => new(config.Color, config.Thickness, lineCap: config.LineCap);
}

