using Avalonia.Input;
using Avalonia.Media;
using SimpleXPlatDrawing;

await SimpleDrawing.Init(400, 400, clickAction: HandleClick);

SimpleDrawing.DrawLine(new(100, 100), new(300, 300));
SimpleDrawing.DrawLine(new(300, 100), new(100, 300), thickness: 4, color: Brushes.Green);
SimpleDrawing.DrawRectangle(new(50, 50), new(100, 80), lineColor: Brushes.Red);
SimpleDrawing.DrawRectangle(new(325, 325), new(375, 375), fillColor: Brushes.Azure);
SimpleDrawing.DrawEllipse(new(200, 300), 50, 75, 
                          lineColor: Brushes.Firebrick, fillColor: Brushes.Firebrick);
SimpleDrawing.DrawCircle(new(350, 75), 40, lineThickness: 2.5D, 
                         lineColor: Brushes.BlueViolet, fillColor: Brushes.Gold);
SimpleDrawing.DrawText(new(25, 350), "Hello World", 24, Brushes.Lime);

SimpleDrawing.Render();

SimpleDrawing.DrawLine(new(200, 25), new(200, 375), 8);
SimpleDrawing.Render();

await Task.Delay(TimeSpan.FromSeconds(2));
SimpleDrawing.Clear();
var x = 50;
for (var i = 0; i < 10; i++)
{
    SimpleDrawing.DrawLine(new(x, 25), new(x, 375), 8);
    x += 25;
    SimpleDrawing.Render();
    await Task.Delay(TimeSpan.FromSeconds(1));
}

Console.Write("Press any key to exit...");
Console.ReadKey();


static void HandleClick(ClickEvent @event)
{
    Console.WriteLine($"{@event.Button} click at {@event.ClickedPoint}");
}

static void DrawCircleOnClick(ClickEvent @event)
{
    var radius = @event.Button switch
    {
        MouseButton.Left => 25,
        MouseButton.Right => 50,
        _ => 10
    };
    SimpleDrawing.DrawCircle(@event.ClickedPoint, radius);
    SimpleDrawing.Render();
}