# SimpleDrawing - Cross Platform

- Allows for the simple drawing of basic shapes on a canvas
- Renders the canvas in a window
- Works on multiple platforms (tested Windows, Linux, Mac)
- Based on SkiaSharp
  - via Avalonia UI 
- User click events are registered

This library is meant for simple applications so only basic functions are available to keep the API easy to use.

## Sample Usage

```csharp
using Avalonia.Media;
using SimpleXPlatDrawing;

SimpleDrawing.Init(400, 400);

SimpleDrawing.DrawLine(new(100, 100), new(300, 300));
SimpleDrawing.DrawLine(new(300, 100), new(100, 300), thickness: 4, color: Brushes.Green);
SimpleDrawing.DrawRectangle(new(50, 50), new(100, 80), lineColor: Brushes.Red);
SimpleDrawing.DrawRectangle(new(325, 325), new(375, 375), fillColor: Brushes.Azure);
SimpleDrawing.DrawEllipse(new(200, 300), 50, 75, 
                          lineColor: Brushes.Firebrick, fillColor: Brushes.Firebrick);
SimpleDrawing.DrawCircle(new(350, 75), 40, lineThickness: 2.5D, 
                         lineColor: Brushes.BlueViolet, fillColor: Brushes.Gold);
SimpleDrawing.DrawText(new(25, 350), "Hello World", 24, Brushes.Lime);

await SimpleDrawing.Render();

SimpleDrawing.DrawLine(new(200, 25), new(200, 375), 8);
await SimpleDrawing.Render();

await Task.Delay(TimeSpan.FromSeconds(2));
SimpleDrawing.Clear();
var x = 50;
for (var i = 0; i < 10; i++)
{
    SimpleDrawing.DrawLine(new(x, 25), new(x, 375), 8);
    x += 25;
    await SimpleDrawing.Render();
    await Task.Delay(TimeSpan.FromSeconds(1));
}

Console.Write("Press any key to exit...");
Console.ReadKey();
```

## Click Events

An `Action` can be passed to the `Init` function to register a click handler which will receive the location the user clicked within the canvas.

```csharp
SimpleDrawing.Init(600, 600, DrawCircleOnClick);

async void DrawCircleOnClick(Point clickLocation)
{
    SimpleDrawing.DrawCircle(clickLocation, 25);
    await SimpleDrawing.Render();
}
```