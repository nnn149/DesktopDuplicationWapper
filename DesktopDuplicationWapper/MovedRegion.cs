using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopDuplicationWapper
{
    /// <summary>
    /// Describes the movement of an image rectangle within a desktop frame.
    /// 描述图像矩形在桌面帧内的运动
    /// </summary>
    /// <remarks>
    /// Move regions are always non-stretched regions so the source is always the same size as the destination.
    /// 移动区域始终是非区域，因此源始终与目的地相同
    /// </remarks>
    public struct MovedRegion
    {
        /// <summary>
        /// Gets the location from where the operating system copied the image region.
        /// 从操作系统复制图像区域的位置获取位置
        /// </summary>
        public Point Source { get; internal set; }

        /// <summary>
        /// Gets the target region to where the operating system moved the image region.
        /// 将目标区域转移到操作系统移动图像区域的位置
        /// </summary>
        public Rectangle Destination { get; internal set; }
    }
}
