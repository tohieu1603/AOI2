using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AutoFrame.SG
{
    public sealed class Pt : IDisposable
    {
        private GCHandle mHandle;
        public IntPtr Ptr
        {
            get { return this.mHandle.AddrOfPinnedObject(); }
        }
        public Pt(object _Val)
        {
            this.mHandle = GCHandle.Alloc(_Val, GCHandleType.Pinned);

        }
        public void Dispose()
        {
            this.mHandle.Free();
            this.mHandle = new GCHandle();
        }
    }
}
