﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace IDevice.Plugins.Browsers
{
    public class DefaultHandler : AbstractBrowsable
    {
        public DefaultHandler() : base("*") { }

        public override Form Initialize(System.IO.FileInfo file)
        {
            System.Diagnostics.Process.Start(file.FullName);
            return null;
        }
    }
}