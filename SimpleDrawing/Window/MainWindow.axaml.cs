using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Threading;
using SimpleDrawing.Core;

namespace SimpleDrawing.Window;

internal partial class MainWindow : Avalonia.Controls.Window
{
    private const double WindowExtraSize = 6D;
    private static readonly object refreshMutex = new();

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

    public Thickness CanvasMargin
    {
        get
        {
            const double CanvasMarginValue = WindowExtraSize / 2D;

            return new Thickness(CanvasMarginValue);
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        MainCanvas.PointerPressed += HandleClick;

        LeoCanvas.SetWindowInitialized(Refresh);
    }

    private void Refresh()
    {
        Task.Run(() =>
        {
            try
            {
                DoRefresh();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        });

        return;

        void DoRefresh()
        {
            lock (refreshMutex)
            {
                using var @event = new ManualResetEventSlim();
                Dispatcher.UIThread.Post(() =>
                {
                    try
                    {
                        InvalidateVisual();
                        MainCanvas.InvalidateVisual();

                        // ReSharper disable once AccessToDisposedClosure - we are waiting for the set before disposing
                        @event.Set();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }, DispatcherPriority.MaxValue);

                @event.Wait();
            }
        }
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
