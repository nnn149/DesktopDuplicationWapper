﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopDuplicator
{
    public class DesktopDuplicatorException : Exception
    {
        public DesktopDuplicatorException(string message)
              : base(message) { }
    }
}
