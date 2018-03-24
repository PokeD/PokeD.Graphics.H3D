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
    internal class GFL2OverWorld
    {
        private const uint GFModelConstant = 0x15122117;
        private const uint GFTextureConstant = 0x15041213;
        private const uint GFMotionConstant = 0x00060000;

        public static H3D OpenAsH3D(Stream input, GFPackage.Header header, H3DDict<H3DBone> skeleton = null)
        {
            var reader = new BinaryReader(input);

            var mdlPack = new GFModelPack();
            var motPack = new GFMotionPack();

            ReadModelsBG(header.Entries[0], reader, mdlPack); //Textures
            ReadModelsBG(header.Entries[1], reader, mdlPack); //Shaders
            ReadModelsBG(header.Entries[2], reader, mdlPack); //Models
            ReadModelsBG(header.Entries[3], reader, mdlPack); //Models?
            ReadModelsBG(header.Entries[4], reader, mdlPack); //More models
            ReadAnimsBG(header.Entries[5], reader, motPack); //Animations
            ReadAnimsBG(header.Entries[6], reader, motPack); //More animations

            var output = mdlPack.ToH3D();

            foreach (var mot in motPack)
            {
                var matAnim = mot.ToH3DMaterialAnimation();
                var    visAnim = mot.ToH3DVisibilityAnimation();

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

            return output;
        }

        private static void ReadModelsBG(GFPackage.Entry file, BinaryReader reader, GFModelPack mdlPack)
        {
            if (file.Length < 0x80) return;

            reader.BaseStream.Seek(file.Address, SeekOrigin.Begin);

            var header = GFPackage.GetPackageHeader(reader.BaseStream);

            foreach (var entry in header.Entries)
            {
                if (entry.Length < 4) continue;

                reader.BaseStream.Seek(entry.Address, SeekOrigin.Begin);

                var magicNum = reader.ReadUInt32();

                switch (magicNum)
                {
                    case GFModelConstant:
                        reader.BaseStream.Seek(-4, SeekOrigin.Current);

                        mdlPack.Models.Add(new GFModel(reader, $"Model_{mdlPack.Models.Count}"));

                        break;

                    case GFTextureConstant:
                        var count = reader.ReadUInt32();

                        var signature = string.Empty;

                        for (var i = 0; i < 8; i++)
                        {
                            var value = reader.ReadByte();

                            if (value < 0x20 || value > 0x7e) break;

                            signature += (char)value;
                        }

                        reader.BaseStream.Seek(entry.Address, SeekOrigin.Begin);

                        if (signature == "texture")
                            mdlPack.Textures.Add(new GFTexture(reader));
                        else
                            mdlPack.Shaders.Add(new GFShader(reader));

                        break;
                }
            }
        }

        private static void ReadAnimsBG(GFPackage.Entry file, BinaryReader reader, GFMotionPack motPack)
        {
            if (file.Length < 0x80) return;

            reader.BaseStream.Seek(file.Address, SeekOrigin.Begin);

            var header = GFPackage.GetPackageHeader(reader.BaseStream);

            foreach (var entry in header.Entries)
            {
                if (entry.Length < 4) continue;

                reader.BaseStream.Seek(entry.Address, SeekOrigin.Begin);

                var magicNum = reader.ReadUInt32();

                if (magicNum == GFMotionConstant)
                {
                    reader.BaseStream.Seek(-4, SeekOrigin.Current);

                    motPack.Add(new GFMotion(reader, motPack.Count));
                }
            }
        }
    }
}