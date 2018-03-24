using SPICA.Formats.CtrH3D;
using SPICA.Formats.GFL;

using System.IO;

namespace SPICA.WinForms.Formats
{
    internal class GFOWCharaModel
    {
        public static H3D OpenAsH3D(Stream input, GFPackage.Header header)
        {
            var output = default(H3D);

            //Model
            var buffer = new byte[header.Entries[0].Length];

            input.Seek(header.Entries[0].Address, SeekOrigin.Begin);

            input.Read(buffer, 0, buffer.Length);

            using (var ms = new MemoryStream(buffer))
                output = H3D.Open(ms);

            //Skeletal Animations
            if (header.Entries.Length > 1)
            {
                input.Seek(header.Entries[1].Address, SeekOrigin.Begin);

                var motPack = new GF1MotionPack(input);

                foreach (var mot in motPack)
                {
                    var sklAnim = mot.ToH3DSkeletalAnimation(output.Models[0].Skeleton);

                    sklAnim.Name = $"Motion_{mot.Index}";

                    output.SkeletalAnimations.Add(sklAnim);
                }
            }

            //Material Animations
            if (header.Entries.Length > 2)
            {
                input.Seek(header.Entries[2].Address, SeekOrigin.Begin);

                var data = new byte[header.Entries[2].Length];

                input.Read(data, 0, data.Length);

                var matAnims = H3D.Open(data);

                output.Merge(matAnims);
            }

            return output;
        }
    }
}