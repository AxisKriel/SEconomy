using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.InteropServices;

namespace Wolfje.Plugins.SEconomy.Packets {
    static class PacketMarshal {

        /// <summary>
        /// Generically will marshal a structure from a supplied byte buffer.
        /// </summary>
        /// <typeparam name="T">The type of structure to return</typeparam>
        /// <param name="buffer">The input byte buffer</param>
        /// <returns>A populated T where T is the structure supplied. :)</returns>
        public static T MarshalFromBuffer<T>(byte[] buffer) where T : struct {
            T packetStruct;
            int size = Marshal.SizeOf(new T());
            IntPtr heapAlloc = Marshal.AllocHGlobal(size);

            try {
                //copy from the heap
                Marshal.Copy(buffer, 0, heapAlloc, size);

                packetStruct = (T)Marshal.PtrToStructure(heapAlloc, typeof(T));
            }
            finally {
                //free HGLOBAL.  we do not want memory leaks.
                Marshal.FreeHGlobal(heapAlloc);
            }

            return packetStruct;
        }

    }
}
