using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace SimpleDrawing.Core;

internal sealed class ImageLoader
{
    public enum State
    {
        Uninitialized,
        FileDoesNotExist,
        FileFormatNotSupported,
        UnknownError,
        Loaded
    }

    private readonly string _path;

    private ImageLoader(string path)
    {
        _path = path;
        Image = null;
        ImageState = State.Uninitialized;
    }

    public State ImageState { get; private set; }

    public IImage? Image { get; private set; }

    private void Load()
    {
        if (!File.Exists(_path))
        {
            ImageState = State.FileDoesNotExist;

            return;
        }

        try
        {
            Image = new Bitmap(_path);
            ImageState = State.Loaded;
        }
        catch (NotSupportedException)
        {
            ImageState = State.FileFormatNotSupported;
        }
        catch (Exception)
        {
            ImageState = State.UnknownError;
        }
    }

    public static ImageLoader FromPath(string path)
    {
        var loader = new ImageLoader(path);
        loader.Load();

        return loader;
    }
}
