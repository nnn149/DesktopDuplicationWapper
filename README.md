# DesktopDuplicationWapper



C# wrapper for the Desktop Duplication Api.

[Nuget](https://docs.microsoft.com/en-gb/windows/win32/direct3ddxgi/desktop-dup-api)

### Features

- Support NET Standard 2.0; .NET 6
- Custom screenshot size
- Only support system of Windows 8 and above



### Example

```c#
DesktopDuplicator desktopDuplicator = new DesktopDuplicator(0, 0);
DesktopFrame frame = desktopDuplicator.GetLatestFrame(x, y, Width, Height);
if (frame != null)
{
	frame.DesktopImage.Save("1.png");
}
desktopDuplicator.Dispose();
```



### Todo

- MovedRegion

- DrawCursor

  

### Reference:

> [Desktop Duplication API - Win32 apps](https://docs.microsoft.com/en-gb/windows/win32/direct3ddxgi/desktop-dup-api)
>
> [amerkoleci/Vortice.Windows](https://github.com/amerkoleci/Vortice.Windows)
>
> [jasonpang/desktop-duplication-net](https://github.com/jasonpang/desktop-duplication-net)
>
> [diogotr7/DesktopDuplicationSamples](https://github.com/diogotr7/DesktopDuplicationSamples)
>
> [MathewSachin/Captura](https://github.com/MathewSachin/Captura)
