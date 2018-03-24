using SPICA.Formats.CtrH3D;
using SPICA.Formats.GFL2;
using SPICA.Formats.GFL2.Texture;

using System.IO;

namespace SPICA.WinForms.Formats
{
    internal static class GFCharaModel
    {
        public static H3D OpenAsH3D(Stream input, GFPackage.Header header)
        {
            var output = default(H3D);

            //Model
            input.Seek(header.Entries[0].Address, SeekOrigin.Begin);

            var mdlPack = new GFModelPack(input);

            output = mdlPack.ToH3D();

            //Animations
            input.Seek(header.Entries[1].Address, SeekOrigin.Begin);

            var motPack = new GFMotionPack(input);

            foreach (var mot in motPack)
            {
                var    sklAnim = mot.ToH3DSkeletalAnimation(mdlPack.Models[0].Skeleton);
                var matAnim = mot.ToH3DMaterialAnimation();
                var    visAnim = mot.ToH3DVisibilityAnimation();

                if (sklAnim != null)
                {
                    sklAnim.Name = $"Motion_{mot.Index}";

                    output.SkeletalAnimations.Add(sklAnim);
                }

                if (matAnim != null)
                {
                    matAnim.Name = $"Motion_{mot.Index}";

                    output.MaterialAnimations.Add(matAnim);
                }

                if (visAnim != null)
                {
                    visAnim.Name = $"Motion_{mot.Index}";

                    output.VisibilityAnimations.Add(visAnim);
                }
            }

            //Texture
            if (header.Entries.Length > 3 && header.Entries[3].Length >= 4)
            {
                input.Seek(header.Entries[3].Address, SeekOrigin.Begin);

                var reader = new BinaryReader(input);

                var magicNum = reader.ReadUInt32();

                if (magicNum == 0x15041213)
                {
                    input.Seek(-4, SeekOrigin.Current);

                    var tex = new GFTexture(reader);

                    output.Textures.Add(tex.ToH3DTexture());
                }
            }

            return output;
        }
    }
}