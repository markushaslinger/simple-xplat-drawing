using Avalonia.Input;
using Avalonia.Media;
using SimpleDrawing;
using SimpleDrawing.Core;

LeoCanvas.Init(Run, 400, 400, clickAction: DrawCircleOnClick);

return;

static async void Run()
{
    try
    {
        LeoCanvas.DrawLine(new(100, 100), new(300, 300));
        LeoCanvas.DrawLine(new(300, 100), new(100, 300), thickness: 4, color: Brushes.Green);
        LeoCanvas.DrawRectangle(new(50, 50), new(100, 80), lineColor: Brushes.Red);
        LeoCanvas.DrawRectangle(new(325, 325), new(375, 375), fillColor: Brushes.Azure);
        LeoCanvas.DrawEllipse(new(200, 300), 50, 75,
                              lineColor: Brushes.Firebrick, fillColor: Brushes.Firebrick);
        LeoCanvas.DrawCircle(new(350, 75), 40, lineThickness: 2.5D,
                             lineColor: Brushes.BlueViolet, fillColor: Brushes.Gold);
        LeoCanvas.DrawText(new(25, 350), "Hello World", 24, Brushes.Lime);

        LeoCanvas.Render();

        LeoCanvas.DrawLine(new(200, 25), new(200, 375), 8);
        LeoCanvas.Render();

        await Task.Delay(TimeSpan.FromSeconds(2));
        LeoCanvas.Clear();
        var x = 50;
        for (var i = 0; i < 10; i++)
        {
            LeoCanvas.DrawLine(new(x, 25), new(x, 375), 8);
            x += 25;
            LeoCanvas.Render();
            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "Data/logo.png");
        LeoCanvas.DrawImageAtLocation(imagePath, new(50, 100), new(350, 175));
        LeoCanvas.Render();

        Console.Write("Press any key to exit...");
        Console.ReadKey();
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}

static void DrawCircleOnClick(ClickEvent @event)
{
    var radius = @event.Button switch
    {
        MouseButton.Left => 25,
        MouseButton.Right => 50,
        _ => 10
    };
    LeoCanvas.DrawCircle(@event.ClickedPoint, radius);
    LeoCanvas.Render();
}