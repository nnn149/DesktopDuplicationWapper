using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopDuplicationWapper
{
    /// <summary>
    /// Provides image data, cursor data, and image metadata about the retrieved desktop frame.
    /// 提供有关检索到的桌面框架的图像数据光标数据和图像元数据
    /// </summary>
    public class DesktopFrame
    {
        /// <summary>
        /// Gets the bitmap representing the last retrieved desktop frame. This image spans the entire bounds of the specified monitor.
        /// 获取代表最后一个检索的桌面框架的位图此图像跨越指定监视器的整个边界
        /// </summary>
        public Bitmap DesktopImage { get; internal set; }

        /// <summary>
        /// Gets a list of the rectangles of pixels in the desktop image that the operating system moved to another location within the same image.
        /// </summary>
        /// <remarks>
        /// To produce a visually accurate copy of the desktop, an application must first process all moved regions before it processes updated regions.
        /// </remarks>
        public MovedRegion[] MovedRegions { get; internal set; }

        /// <summary>
        /// Returns the list of non-overlapping rectangles that indicate the areas of the desktop image that the operating system updated since the last retrieved frame.
        /// </summary>
        /// <remarks>
        /// To produce a visually accurate copy of the desktop, an application must first process all moved regions before it processes updated regions.
        /// </remarks>
        public Rectangle[] UpdatedRegions { get; internal set; }
    }
}
