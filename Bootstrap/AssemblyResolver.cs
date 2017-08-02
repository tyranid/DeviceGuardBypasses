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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Bootstrap
{
    class AssemblyResolver
    {
        private Dictionary<string, Assembly> _resolved_asms =
                new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);

        private HashSet<string> _resolver_paths = 
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private bool _trace_enabled;

        public bool TraceEnabled
        {
            get
            {
                return _trace_enabled;
            }

            set
            {
                _trace_enabled = value;
            }
        }

        private void Trace(string str)
        {
            if (_trace_enabled)
            {
                Console.WriteLine(str);
            }
        }

        private void Trace(string fmt, params object[] objs)
        {
            Trace(String.Format(fmt, objs));
        }

        internal void ResetCache()
        {
            _resolved_asms.Clear();
            Assembly asm = typeof(EntryPoint).Assembly;
            _resolved_asms["bootstrap"] = asm;
            _resolved_asms[asm.FullName] = asm;
        }

        internal void ResetMissingCache()
        {
            foreach (var pair in _resolved_asms.ToArray())
            {
                if (pair.Value == null)
                {
                    _resolved_asms.Remove(pair.Key);
                }
            }
        }

        internal AssemblyResolver()
        {
            ResetCache();
            // Get list of assembly paths from environment if ASSEMBLY_PATH exists.
            string asm_path = Environment.GetEnvironmentVariable("ASSEMBLY_PATH");
            if (!String.IsNullOrWhiteSpace(asm_path))
            {
                foreach (string path in asm_path.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    _resolver_paths.Add(Path.GetFullPath(path));
                }
            }
            else
            {
                // Default to Documents\assembly
                _resolver_paths.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "assembly"));
            }

            // Setup resolver.
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        internal void AddAssemblyPath(string path)
        {
            _resolver_paths.Add(Path.GetFullPath(path));
        }

        private string FindAssemblyPath(AssemblyName name, string extension)
        {
            foreach (string path in
                _resolver_paths.Select(p =>
                Path.ChangeExtension(Path.Combine(p, name.Name), extension)))
            {
                Trace("Checking {0}", path);
                if (File.Exists(path))
                {
                    return path;
                }
            }
            return null;
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Trace("Looking up {0}", args.Name);
            if (!_resolved_asms.ContainsKey(args.Name))
            {
                AssemblyName name = new AssemblyName(args.Name);
                string path = FindAssemblyPath(name, ".exe") ?? FindAssemblyPath(name, ".dll");
                if (path != null)
                {
                    Assembly asm = Assembly.Load(File.ReadAllBytes(path));
                    _resolved_asms[args.Name] = asm;
                    _resolved_asms[asm.FullName] = asm;
                }
                else
                {
                    _resolved_asms[args.Name] = null;
                }
            }

            return _resolved_asms[args.Name];
        }

    }
}
