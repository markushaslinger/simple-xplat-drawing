# SimpleDrawing - Cross Platform

- Allows for the simple drawing of basic shapes on a canvas
- Renders the canvas in a window
- Works on multiple platforms (tested Windows, Linux; should also work on Mac)
- Based on [SkiaSharp](https://github.com/mono/SkiaSharp)
  - via [Avalonia UI](https://github.com/AvaloniaUI/Avalonia)
- User click events are registered

This library is meant for simple applications, so only basic functions are available to keep the API easy to use.

## Sample Usage

```csharp
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
```

### Main Method

To also support macOS (which requires the UI thread to be the main thread), it is necessary to pass the actual main method to the `LeoCanvas.Init` method.
It will be run in a background thread, hiding most of the complexity from the students, but this method reference is a technical necessity.

## Click Events

An `Action` can be passed to the `Init` function to register a click handler which will receive the location the user clicked within the canvas as well as information if the left or right mouse button was used.

```csharp
LeoCanvas.Init(Run, 600, 600, clickAction: DrawCircleOnClick);

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
```

## Rendering Images

Images can be rendered on the canvas using the `DrawImageAtLocation` function. 
The image path should be passed as the first parameter and the location as the second parameter.

It is also possible to directly pass an `IImage` to an overload. 
Images can be (pre)loaded via the `TryLoadImage` method to avoid loading them from the disk every time.

This library should support all image formats handled by SkiaSharp (e.g., PNG, JPEG, BMP).

```csharp
var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "Data/logo.png");
LeoCanvas.DrawImageAtLocation(imagePath, new(50, 100), new (350, 175));
```

Scale of the images is not checked, make sure to use a proper target rectangle.