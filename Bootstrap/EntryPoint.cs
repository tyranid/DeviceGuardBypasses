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
//  along with Device Guard Bypasses. If not, see<http://www.gnu.org/licenses/>..

using System;
using System.IO;
using System.Threading;

namespace Bootstrap
{
    public class EntryPoint
    {
        private static AssemblyResolver _main_resolver = new AssemblyResolver();

        static void MainThread(object resolver)
        {
            try
            {
                AppDomain.CurrentDomain.ExecuteAssemblyByName("startasm");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public EntryPoint()
        {
            // Execute the assembly named startasm.
            Thread thread = new Thread(MainThread);
            thread.Start(_main_resolver);
            thread.Join();
        }

        /// <summary>
        /// Add a path to the assembly lookup
        /// </summary>
        /// <param name="path">The path for lookup</param>
        public static void AddAssemblyPath(string path)
        {
            _main_resolver.AddAssemblyPath(Path.GetFullPath(path));
        }

        /// <summary>
        /// Reset entire cache.
        /// </summary>
        public static void ResetCache()
        {
            _main_resolver.ResetCache();
        }

        /// <summary>
        /// Reset cache to remove any assemblies which weren't found.
        /// </summary>
        public static void ResetMissingCache()
        {
            _main_resolver.ResetMissingCache();
        }

        public static bool TraceEnabled
        {
            get
            {
                return _main_resolver.TraceEnabled;
            }
            set
            {
                _main_resolver.TraceEnabled = value;
            }
        }

        /// <summary>
        /// Entrypoint for testing.
        /// </summary>
        static void Main()
        {
            try
            {
                EntryPoint ep = new EntryPoint();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
