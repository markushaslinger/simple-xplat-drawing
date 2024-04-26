using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Threading;

namespace SimpleXPlatDrawing;

internal partial class MainWindow : Window
{
    private const double WindowExtraSize = 6D;

    public Thickness CanvasMargin
    {
        get
        {
            const double CanvasMarginValue = WindowExtraSize / 2D;

            return new Thickness(CanvasMarginValue);
        }
    }

    public MainWindow()
    {
        Width = Config.Width + WindowExtraSize;
        Height = Config.Height + WindowExtraSize;
        MinWidth = Width;
        MinHeight = Height;

        InitializeComponent();
        
        Title = Config.WindowTitle;
        CanResize = false;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        MainCanvas.PointerPressed += HandleClick;
        
        SimpleDrawing.SetWindowInitialized(Refresh);
    }

    private void Refresh()
    {
        Dispatcher.UIThread.Post(() =>
        {
            try
            {
                InvalidateVisual();
                MainCanvas.InvalidateVisual();
            } 
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }, DispatcherPriority.MaxValue);
    }

    private void HandleClick(object? sender, PointerPressedEventArgs e)
    {
        e.Handled = true;

        var pos = e.GetPosition(this);
        var isLeftClick = !e.GetCurrentPoint(this).Properties.IsRightButtonPressed;

        Config.ClickAction?.Invoke(new ClickEvent(pos, isLeftClick
                                                      ? MouseButton.Left
                                                      : MouseButton.Right));
    }
}
