using System.IO;
using System.Text;

using SPICA.Formats.CtrH3D;

namespace SPICA.WinForms.Formats
{
    internal class GFPackedTexture
    {
        public static H3D OpenAsH3D(Stream input, GFPackage.Header header, int startIndex)
        {
            var output = new H3D();

            //Textures and animations
            for (var i = startIndex; i < header.Entries.Length; i++)
            {
                var buffer = new byte[header.Entries[i].Length];

                input.Seek(header.Entries[i].Address, SeekOrigin.Begin);

                input.Read(buffer, 0, buffer.Length);

                if (buffer.Length < 4 || Encoding.ASCII.GetString(buffer, 0, 4) != "BCH\0") continue;

                using (var ms = new MemoryStream(buffer))
                    output.Merge(H3D.Open(ms));
            }

            return output;
        }
    }
}