using Avalonia.Input;
using Avalonia.Media;
using SimpleDrawing;

await Canvas.Init(400, 400, clickAction: DrawCircleOnClick);

Canvas.DrawLine(new(100, 100), new(300, 300));
Canvas.DrawLine(new(300, 100), new(100, 300), thickness: 4, color: Brushes.Green);
Canvas.DrawRectangle(new(50, 50), new(100, 80), lineColor: Brushes.Red);
Canvas.DrawRectangle(new(325, 325), new(375, 375), fillColor: Brushes.Azure);
Canvas.DrawEllipse(new(200, 300), 50, 75, 
                   lineColor: Brushes.Firebrick, fillColor: Brushes.Firebrick);
Canvas.DrawCircle(new(350, 75), 40, lineThickness: 2.5D, 
                  lineColor: Brushes.BlueViolet, fillColor: Brushes.Gold);
Canvas.DrawText(new(25, 350), "Hello World", 24, Brushes.Lime);

Canvas.Render();

Canvas.DrawLine(new(200, 25), new(200, 375), 8);
Canvas.Render();

await Task.Delay(TimeSpan.FromSeconds(2));
Canvas.Clear();
var x = 50;
for (var i = 0; i < 10; i++)
{
    Canvas.DrawLine(new(x, 25), new(x, 375), 8);
    x += 25;
    Canvas.Render();
    await Task.Delay(TimeSpan.FromSeconds(1));
}

Console.Write("Press any key to exit...");
Console.ReadKey();

static void DrawCircleOnClick(ClickEvent @event)
{
    var radius = @event.Button switch
    {
        MouseButton.Left => 25,
        MouseButton.Right => 50,
        _ => 10
    };
    Canvas.DrawCircle(@event.ClickedPoint, radius);
    Canvas.Render();
}