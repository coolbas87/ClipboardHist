using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardHistory
{
    public class ClipboardWatcher
    {
        private IntPtr windowHandle;
        private IntPtr viewerHandle;

        public event EventHandler OnClipboardChanged;

        public ClipboardWatcher(IntPtr wndHandle)
        {
            windowHandle = wndHandle;
        }

        public IntPtr ReceiveMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case User32.WM_DRAWCLIPBOARD:
                    {
                        OnClipboardChanged(null, EventArgs.Empty);
                        User32.SendMessage(viewerHandle, User32.WM_DRAWCLIPBOARD, wParam, lParam);
                        break;
                    }
                case User32.WM_CHANGECBCHAIN:
                    {
                        if (wParam == viewerHandle)
                        {
                            viewerHandle = lParam;
                        }
                        else
                        {
                            User32.SendMessage(viewerHandle, User32.WM_CHANGECBCHAIN, wParam, lParam);
                        }
                        break;
                    }
                case User32.WM_CLIPBOARDUPDATE:
                    {
                        OnClipboardChanged(null, EventArgs.Empty);
                        break;
                    }
            }
            return IntPtr.Zero;
        }

        public void SetClipboardHandle()
        {
            OperatingSystem osInfo = Environment.OSVersion;
            if (osInfo.Version.Major >= 6)
            {
                if (!User32.AddClipboardFormatListener(windowHandle))
                {
                    string errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).Message;
                    throw new Exception(errorMessage);
                }
            }
            else 
            {
                viewerHandle = User32.SetClipboardViewer(windowHandle);
            }
        }

        public void RemoveClipboardHandle()
        {
            OperatingSystem osInfo = Environment.OSVersion;
            if (osInfo.Version.Major >= 6)
            {
                if (!User32.RemoveClipboardFormatListener(windowHandle))
                {
                    string errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).Message;
                    throw new Exception(errorMessage);
                }
            }
            else
            {
                User32.ChangeClipboardChain(windowHandle, viewerHandle);
            }
        }
    }
}
