using SPICA.Formats.CtrH3D;
using SPICA.Formats.CtrH3D.Model;
using SPICA.Formats.GFL;

using System.IO;

namespace SPICA.WinForms.Formats
{
    internal class GFBtlSklAnim
    {
        public static H3D OpenAsH3D(Stream input, GFPackage.Header header, H3DDict<H3DBone> skeleton)
        {
            var output = new H3D();

            //Skeletal Animations
            input.Seek(header.Entries[0].Address, SeekOrigin.Begin);

            var motPack = new GF1MotionPack(input);

            foreach (var mot in motPack)
            {
                var sklAnim = mot.ToH3DSkeletalAnimation(skeleton);

                sklAnim.Name = $"Motion_{mot.Index}";

                output.SkeletalAnimations.Add(sklAnim);
            }

            //Material Animations
            input.Seek(header.Entries[1].Address, SeekOrigin.Begin);

            var packHeader = GFPackage.GetPackageHeader(input);

            foreach (var entry in packHeader.Entries)
            {
                input.Seek(entry.Address, SeekOrigin.Begin);

                if (entry.Length > 0)
                {
                    var data = new byte[entry.Length];

                    input.Read(data, 0, data.Length);

                    var matAnims = H3D.Open(data);

                    output.Merge(matAnims);
                }
            }

            return output;
        }
    }
}