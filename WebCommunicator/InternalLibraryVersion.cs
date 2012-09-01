using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebCommunicator
{
    internal static class InternalLibraryVersion
    {
        internal static Version GetLibraryVersion()
        {
            //automatically incremented by build script
            return new Version("1.0.2061.0");
        }
    }
}
