using SPICA.Formats.CtrH3D;

using System.IO;

namespace SPICA.WinForms.Formats
{
    internal class GFOWMapModel
    {
        public static H3D OpenAsH3D(Stream input, GFPackage.Header header)
        {
            var output = default(H3D);

            //Model
            var buffer = new byte[header.Entries[1].Length];

            input.Seek(header.Entries[1].Address, SeekOrigin.Begin);

            input.Read(buffer, 0, buffer.Length);

            using (var ms = new MemoryStream(buffer))
                output = H3D.Open(ms);

            return output;
        }
    }
}