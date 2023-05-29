using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HymnLibrary
{
    internal class DatabaseDirectory
    {
        public static string Dir { get; private set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
        public static Stream GetEmbeddedResourceStreamAsync(string embeddedResourcePath, Type type)
        {
            //Class name is CommonsExtensions
            var assembly = type.GetTypeInfo().Assembly;
            Stream stream = assembly.GetManifestResourceStream(embeddedResourcePath);
            return stream;
        }
    }
}
