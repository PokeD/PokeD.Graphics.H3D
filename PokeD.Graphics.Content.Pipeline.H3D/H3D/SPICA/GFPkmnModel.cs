using SPICA.Formats.CtrH3D;
using SPICA.Formats.CtrH3D.Model;
using SPICA.Formats.GFL2;
using SPICA.Formats.GFL2.Model;
using SPICA.Formats.GFL2.Motion;
using SPICA.Formats.GFL2.Shader;
using SPICA.Formats.GFL2.Texture;

using System.IO;

namespace SPICA.WinForms.Formats
{
    internal class GFPkmnModel
    {
        private const uint GFModelConstant   = 0x15122117;
        private const uint GFTextureConstant = 0x15041213;
        private const uint GFMotionConstant  = 0x00060000;

        private const uint BCHConstant = 0x00484342;

        public static H3D OpenAsH3D(Stream input, GFPackage.Header header, H3DDict<H3DBone> skeleton = null)
        {
            var output = default(H3D);

            var reader = new BinaryReader(input);

            input.Seek(header.Entries[0].Address, SeekOrigin.Begin);

            var magicNum = reader.ReadUInt32();

            switch (magicNum)
            {
                case GFModelConstant:
                    var mdlPack = new GFModelPack();

                    //High Poly Pokémon model
                    input.Seek(header.Entries[0].Address, SeekOrigin.Begin);

                    mdlPack.Models.Add(new GFModel(reader, "PM_HighPoly"));

                    //Low Poly Pokémon model
                    input.Seek(header.Entries[1].Address, SeekOrigin.Begin);
                    
                    mdlPack.Models.Add(new GFModel(reader, "PM_LowPoly"));

                    //Pokémon Shader package
                    input.Seek(header.Entries[2].Address, SeekOrigin.Begin);

                    var psHeader = GFPackage.GetPackageHeader(input);

                    foreach (var entry in psHeader.Entries)
                    {
                        input.Seek(entry.Address, SeekOrigin.Begin);

                        mdlPack.Shaders.Add(new GFShader(reader));
                    }

                    //More shaders
                    input.Seek(header.Entries[3].Address, SeekOrigin.Begin);

                    if (GFPackage.IsValidPackage(input))
                    {
                        var pcHeader = GFPackage.GetPackageHeader(input);

                        foreach (var entry in pcHeader.Entries)
                        {
                            input.Seek(entry.Address, SeekOrigin.Begin);

                            mdlPack.Shaders.Add(new GFShader(reader));
                        }
                    }

                    output = mdlPack.ToH3D();

                    break;

                case GFTextureConstant:
                    output = new H3D();

                    foreach (var entry in header.Entries)
                    {
                        input.Seek(entry.Address, SeekOrigin.Begin);

                        output.Textures.Add(new GFTexture(reader).ToH3DTexture());
                    }

                    break;

                case GFMotionConstant:
                    output = new H3D();

                    if (skeleton == null) break;

                    for (var index = 0; index < header.Entries.Length; index++)
                    {
                        input.Seek(header.Entries[index].Address, SeekOrigin.Begin);

                        if (input.Position + 4 > input.Length) break;
                        if (reader.ReadUInt32() != GFMotionConstant) continue;

                        input.Seek(-4, SeekOrigin.Current);

                        var mot = new GFMotion(reader, index);

                        var    sklAnim = mot.ToH3DSkeletalAnimation(skeleton);
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

                    break;

                case BCHConstant:
                    output = new H3D();

                    foreach (var entry in header.Entries)
                    {
                        input.Seek(entry.Address, SeekOrigin.Begin);

                        magicNum = reader.ReadUInt32();

                        if (magicNum != BCHConstant) continue;

                        input.Seek(-4, SeekOrigin.Current);

                        var buffer = reader.ReadBytes(entry.Length);

                        output.Merge(H3D.Open(buffer));
                    }

                    break;
            }

            return output;
        }
    }
}