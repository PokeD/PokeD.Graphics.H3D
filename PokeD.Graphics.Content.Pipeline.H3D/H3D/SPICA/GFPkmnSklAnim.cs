using SPICA.Formats.CtrH3D;
using SPICA.Formats.CtrH3D.Model;
using SPICA.Formats.GFL;

using System.IO;

namespace SPICA.WinForms.Formats
{
    internal class GFPkmnSklAnim
    {
        public static H3D OpenAsH3D(Stream input, GFPackage.Header header, H3DDict<H3DBone> skeleton)
        {
            var output = new H3D();

            var reader = new BinaryReader(input);

            var index = 0;

            foreach (var entry in header.Entries)
            {
                input.Seek(entry.Address, SeekOrigin.Begin);

                if (index == 20) break;

                if (index == 0)
                {
                    var motPack = new GF1MotionPack(reader);

                    foreach (var mot in motPack)
                    {
                        var anim = mot.ToH3DSkeletalAnimation(skeleton);

                        anim.Name = $"Motion_{index++}";

                        output.SkeletalAnimations.Add(anim);
                    }
                }
                else
                {
                    var data = reader.ReadBytes(entry.Length);

                    if (data.Length > 4 &&
                        data[0] == 'B' &&
                        data[1] == 'C' &&
                        data[2] == 'H' &&
                        data[3] == '\0')
                    {
                        output.Merge(H3D.Open(data));
                    }
                }
            }

            return output;
        }
    }
}