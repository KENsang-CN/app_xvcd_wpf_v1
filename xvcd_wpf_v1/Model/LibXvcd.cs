using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class LibXvcd
    {
        /// <summary>
        /// Get Library Version Information
        /// </summary>
        /// <param name="ver">version string buffer</param>
        /// <returns></returns>
        [DllImport("libxvcd.dll",
             EntryPoint = "version",
             ExactSpelling = true,
             CallingConvention = CallingConvention.Cdecl)]
        public static extern int version(byte[] ver);

        /// <summary>
        /// FTDI Device Scan
        /// </summary>
        /// <returns>number of found devices</returns>
        [DllImport("libxvcd.dll",
             EntryPoint = "ftdi_device_scan",
             ExactSpelling = true,
             CallingConvention = CallingConvention.Cdecl)]
        public static extern int ftdi_device_scan();

        /// <summary>
        /// FTDI Device Information
        /// </summary>
        /// <param name="index">specific device index</param>
        /// <param name="chipname">chip name string</param>
        /// <param name="id">chip 32 bit id number</param>
        /// <param name="serialnum">chip serial number string</param>
        /// <param name="desc">chip description string</param>
        /// <returns>operation status</returns>
        [DllImport("libxvcd.dll",
             EntryPoint = "ftdi_device_info",
             ExactSpelling = true,
             CallingConvention = CallingConvention.Cdecl)]
        public static extern int ftdi_device_info(int index, byte[] chipname,
            ref UInt32 id, byte[] serialnum, byte[] desc);

        [DllImport("libxvcd.dll",
             EntryPoint = "xvcd_start",
             ExactSpelling = true,
             CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr xvcd_start(int index, int port, int max_vector_len, double freq);

        [DllImport("libxvcd.dll",
             EntryPoint = "xvcd_stop",
             ExactSpelling = true,
             CallingConvention = CallingConvention.Cdecl)]
        public static extern void xvcd_stop(IntPtr handle);

        [DllImport("libxvcd.dll",
             EntryPoint = "xvcd_connect_info",
             ExactSpelling = true,
             CallingConvention = CallingConvention.Cdecl)]
        public static extern bool xvcd_connect_info(IntPtr handle, byte[] ip, ref int port, ref double freq);
    }
}
