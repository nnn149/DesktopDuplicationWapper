
using SharpGen.Runtime;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Vortice;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace DesktopDuplicationWapper
{
    /// <summary>
    /// You must call Dispose() in the end
    /// </summary>
    public class DesktopDuplicator : IDisposable
    {
        private readonly IDXGIAdapter mAdapter;

        private readonly ID3D11Device mDevice;
        private OutputDescription mOutputDesc;
        private Texture2DDescription mTextureDesc;
        private readonly IDXGIOutputDuplication mDeskDupl;

        private ID3D11Texture2D desktopImageTexture;
        private OutduplFrameInfo frameInfo;



        private Bitmap finalImage1, finalImage2;
        private bool isFinalImage1 = false;
        private Bitmap FinalImage
        {
            get
            {
                return isFinalImage1 ? finalImage1 : finalImage2;
            }
            set
            {
                if (isFinalImage1)
                {
                    finalImage2 = value;
                    if (finalImage1 != null) finalImage1.Dispose();
                }
                else
                {
                    finalImage1 = value;
                    if (finalImage2 != null) finalImage2.Dispose();
                }
                isFinalImage1 = !isFinalImage1;
            }
        }

        private bool isDisposed = false;

        private static readonly FeatureLevel[] s_featureLevels = new[]
       {
            FeatureLevel.Level_11_0,
            FeatureLevel.Level_10_1,
            FeatureLevel.Level_10_0,
            FeatureLevel.Level_9_3,
            FeatureLevel.Level_9_2,
            FeatureLevel.Level_9_1,
        };


        /// <summary>
        /// If you switch to a fullscreen program, it will be an error to get a frame
        /// 如果切换全屏程序，获取帧会错误
        /// </summary>
        /// <param name="whichGraphicsCardAdapter">显卡id</param>
        /// <param name="whichMonitor">显示器id</param>
        /// <exception cref="DesktopDuplicatorException"></exception>
        public DesktopDuplicator(int whichGraphicsCardAdapter, int whichMonitor)
        {
            mAdapter = DXGI.CreateDXGIFactory1<IDXGIFactory1>().GetAdapter(whichGraphicsCardAdapter);
            if (mAdapter == null)
            {
                throw new DesktopDuplicatorException("Could not find the specified graphics card adapter.\n找不到指定的显卡适配器");
            }

            D3D11.D3D11CreateDevice(mAdapter, DriverType.Unknown, DeviceCreationFlags.None, s_featureLevels, out var device);
            if (device != null)
            {
                mDevice = device;
            }
            IDXGIOutput output = mAdapter.GetOutput(whichMonitor);
            if (output == null)
            {
                throw new DesktopDuplicatorException("Could not find the specified output device.\n 找不到指定的输出设备");
            }
            var output1 = output.QueryInterface<IDXGIOutput1>();
            mOutputDesc = output1.Description;
            mTextureDesc = new Texture2DDescription
            {
                CPUAccessFlags = CpuAccessFlags.Read,
                BindFlags = BindFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Width = mOutputDesc.DesktopCoordinates.Right - mOutputDesc.DesktopCoordinates.Left,
                Height = mOutputDesc.DesktopCoordinates.Bottom - mOutputDesc.DesktopCoordinates.Top,
                MiscFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Staging
            };

            try
            {
                mDeskDupl = output1.DuplicateOutput(mDevice);
            }
            catch (Exception ex)
            {
                throw new DesktopDuplicatorException("There is already the maximum number of applications using the Desktop Duplication API running,please dispose this and try again.\n" + ex.Message);
            }
            isDisposed = false;
        }


        public DesktopFrame GetLatestFrame(int x, int y, int width, int height)
        {
            if (isDisposed)
            {
                return null;
            }
            var frame = new DesktopFrame();
            // Try to get the latest frame; this may timeout
            if (RetrieveFrame())
            {
                return null;
            }
            try
            {
                //RetrieveFrameMetadata(frame);
                ProcessFrame(frame, x, y, width, height);
            }
            catch
            {
                throw;

            }
            finally
            {
                ReleaseFrame();
            }
            return frame;
        }



        /// <summary>
        /// 检索帧
        /// </summary>
        /// <returns>true表示检索帧失败</returns>
        private bool RetrieveFrame()
        {
            if (mDevice == null)
            {
                throw new DesktopDuplicatorException("mDevice null");
            }
            if (desktopImageTexture == null)
            {
                desktopImageTexture = mDevice.CreateTexture2D(mTextureDesc);
            }

            var result = mDeskDupl.AcquireNextFrame(500, out frameInfo, out var desktopResource);
            if (result.Code == Result.WaitTimeout)
            {
                return true;
            }
            if (result.NativeApiCode == "DXGI_ERROR_ACCESS_LOST")
            {
                throw new DesktopDuplicatorException("DXGI_ERROR_ACCESS_LOST.\n访问目标丢失");
            }
            if (result.Failure)
            {
                throw new DesktopDuplicatorException("Failed to acquire next frame.\n未能获取下一帧\nNativeApiCode:" + result.NativeApiCode);
            }
            using (var tempTexture = desktopResource.QueryInterface<ID3D11Texture2D>())
            {
                mDevice.ImmediateContext.CopyResource(desktopImageTexture, tempTexture);
            };
            desktopResource.Dispose();
            return false;
        }

        private void RetrieveFrameMetadata(DesktopFrame frame)
        {
            if (frameInfo.TotalMetadataBufferSize > 0)
            {
                // Get moved regions
                OutduplMoveRect[] movedRectangles = new OutduplMoveRect[frameInfo.TotalMetadataBufferSize];
                mDeskDupl.GetFrameMoveRects(movedRectangles.Length, movedRectangles, out var movedRegionsLength);
                frame.MovedRegions = new MovedRegion[movedRegionsLength / Marshal.SizeOf(typeof(OutduplMoveRect))];

                for (int i = 0; i < frame.MovedRegions.Length; i++)
                {
                    frame.MovedRegions[i] = new MovedRegion()
                    {
                        Source = new System.Drawing.Point(movedRectangles[i].SourcePoint.X, movedRectangles[i].SourcePoint.Y),
                        Destination = new System.Drawing.Rectangle(movedRectangles[i].DestinationRect.Left, movedRectangles[i].DestinationRect.Top, movedRectangles[i].DestinationRect.Right - movedRectangles[i].DestinationRect.Left, movedRectangles[i].DestinationRect.Bottom - movedRectangles[i].DestinationRect.Top)
                    };
                }

                // Get dirty regions
                RawRect[] dirtyRectangles = new RawRect[frameInfo.TotalMetadataBufferSize];
                mDeskDupl.GetFrameDirtyRects(dirtyRectangles.Length, dirtyRectangles, out var dirtyRegionsLength);
                frame.UpdatedRegions = new System.Drawing.Rectangle[dirtyRegionsLength / Marshal.SizeOf(typeof(RawRect))];
                for (int i = 0; i < frame.UpdatedRegions.Length; i++)
                {
                    frame.UpdatedRegions[i] = new System.Drawing.Rectangle(dirtyRectangles[i].Left, dirtyRectangles[i].Top, dirtyRectangles[i].Right - dirtyRectangles[i].Left, dirtyRectangles[i].Bottom - dirtyRectangles[i].Top);
                }

            }
            else
            {
                frame.MovedRegions = Array.Empty<MovedRegion>();
                frame.UpdatedRegions = Array.Empty<Rectangle>();
            }
        }

        private void ProcessFrame(DesktopFrame frame, int x, int y, int width, int height)
        {
            // Get the desktop capture texture
            // 获取桌面捕获纹理
            if (mDevice == null || desktopImageTexture == null)
            {
                throw new DesktopDuplicatorException("mDevice or desktopImageTexture null");
            }
            var mapSource = mDevice.ImmediateContext.Map(desktopImageTexture, 0, MapMode.Read, Vortice.Direct3D11.MapFlags.None);
            // Copy pixels from screen capture Texture to GDI bitmap
            // 将像素从屏幕捕获纹理复制到GDI位图
            FinalImage = new Bitmap(width, height, PixelFormat.Format32bppRgb);
            var mapDest = FinalImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, FinalImage.PixelFormat);
            int byteSize = 4;
            var destPtr = mapDest.Scan0;
            var sourcePtr = IntPtr.Add(mapSource.DataPointer, (y * mapSource.RowPitch) + (x * byteSize));
            if (x == 0)
            {
                //整块拷贝 The whole copy
                MemoryHelpers.CopyMemory(destPtr, sourcePtr, width * byteSize * height);
            }
            else
            {
                for (int tempY = y; tempY < y + height; tempY++)
                {
                    // Copy a single line 
                    MemoryHelpers.CopyMemory(destPtr, sourcePtr, width * byteSize);
                    // Advance pointers
                    sourcePtr = IntPtr.Add(sourcePtr, mapSource.RowPitch);
                    destPtr = IntPtr.Add(destPtr, mapDest.Stride);
                }
            }

            // Release source and dest locks
            // 释放源和DEST锁
            FinalImage.UnlockBits(mapDest);
            mDevice.ImmediateContext.Unmap(desktopImageTexture, 0);
            frame.DesktopImage = FinalImage;
        }

        private void ReleaseFrame()
        {
            var result = mDeskDupl.ReleaseFrame();
            if (!result.Success)
            {
                throw new DesktopDuplicatorException("Failed to release frame.无法释放帧\n" + result.Description);
            }
        }

        public void Dispose()
        {
            isDisposed = true;
            mDeskDupl?.Dispose();
            desktopImageTexture?.Dispose();
            mDevice?.Dispose();
            mAdapter?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
