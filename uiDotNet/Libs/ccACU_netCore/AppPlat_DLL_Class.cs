using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace ccACU_netCore
{
    public static class ccACUnative
    {
        // C# doesn't support varargs so all arguments must be explicitly defined.
        // CallingConvention.Cdecl must be used since the stack is
        // cleaned up by the caller.
        // int printf(const char *format [, argument]...)
        [DllImport("ccACU_DLL.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int printf(string format, int i, double d);

        [DllImport("ccACU_DLL.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int printf(string format, int i, string s);
    }
}