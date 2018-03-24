using SPICA.Formats.CtrGfx;
using SPICA.Formats.CtrH3D;
using SPICA.Formats.CtrH3D.Model;
using SPICA.Formats.Generic.StudioMdl;
using SPICA.Formats.Generic.WavefrontOBJ;
using SPICA.Formats.GFL2;
using SPICA.Formats.GFL2.Model;
using SPICA.Formats.GFL2.Motion;
using SPICA.Formats.GFL2.Texture;
using SPICA.Formats.ModelBinary;
using SPICA.Formats.MTFramework.Model;
using SPICA.Formats.MTFramework.Shader;
using SPICA.Formats.MTFramework.Texture;

using System.IO;
using System.Text;

namespace SPICA.WinForms.Formats
{
    internal static class FormatIdentifier
    {
        public static H3D IdentifyAndOpen(string fileName, H3DDict<H3DBone> skeleton = null)
        {
            //Formats that can by identified by extensions
            var filePath = Path.GetDirectoryName(fileName);

            switch (Path.GetExtension(fileName).ToLower())
            {
                case ".smd": return new SMD(fileName).ToH3D(filePath);
                case ".obj": return new OBJ(fileName).ToH3D(filePath);
                case ".mbn":
                    using (var input = new FileStream(fileName, FileMode.Open))
                    {
                        var baseScene = H3D.Open(File.ReadAllBytes(fileName.Replace(".mbn", ".bch")));

                        var modelBinary = new MBn(new BinaryReader(input), baseScene);

                        return modelBinary.ToH3D();
                    }
            }

            //Formats that can only be indetified by "magic numbers"
            var output = default(H3D);

            using (var fs = new FileStream(fileName, FileMode.Open))
            {
                if (fs.Length > 4)
                {
                    var reader = new BinaryReader(fs);

                    var magicNum = reader.ReadUInt32();

                    fs.Seek(-4, SeekOrigin.Current);

                    var magic = Encoding.ASCII.GetString(reader.ReadBytes(4));

                    fs.Seek(0, SeekOrigin.Begin);

                    if (magic.StartsWith("BCH"))
                        return H3D.Open(reader.ReadBytes((int)fs.Length));
                    else if (magic.StartsWith("MOD"))
                        return LoadMTModel(reader, fileName, Path.GetDirectoryName(fileName));
                    else if (magic.StartsWith("TEX"))
                        return new MTTexture(reader, Path.GetFileNameWithoutExtension(fileName)).ToH3D();
                    else if (magic.StartsWith("MFX"))
                        _mtShader = new MTShaderEffects(reader);
                    else if (magic.StartsWith("CGFX"))
                        return Gfx.Open(fs);
                    else
                    {
                        if (GFPackage.IsValidPackage(fs))
                        {
                            var packHeader = GFPackage.GetPackageHeader(fs);

                            switch (packHeader.Magic)
                            {
                                case "AD": output = GFPackedTexture.OpenAsH3D(fs, packHeader, 1); break;
                                case "BG": output = GFL2OverWorld.OpenAsH3D(fs, packHeader, skeleton); break;
                                case "BS": output = GFBtlSklAnim.OpenAsH3D(fs, packHeader, skeleton); break;
                                case "CM": output = GFCharaModel.OpenAsH3D(fs, packHeader); break;
                                case "GR": output = GFOWMapModel.OpenAsH3D(fs, packHeader); break;
                                case "MM": output = GFOWCharaModel.OpenAsH3D(fs, packHeader); break;
                                case "PC": output = GFPkmnModel.OpenAsH3D(fs, packHeader, skeleton); break;
                                case "PT": output = GFPackedTexture.OpenAsH3D(fs, packHeader, 0); break;
                                case "PK":
                                case "PB": output = GFPkmnSklAnim.OpenAsH3D(fs, packHeader, skeleton); break;
                            }
                        }
                        else
                        {
                            switch (magicNum)
                            {
                                case 0x15122117:
                                    output = new H3D();

                                    output.Models.Add(new GFModel(reader, "Model").ToH3DModel());

                                    break;

                                case 0x15041213:
                                    output = new H3D();

                                    output.Textures.Add(new GFTexture(reader).ToH3DTexture());

                                    break;

                                case 0x00010000: output = new GFModelPack(reader).ToH3D(); break;
                                case 0x00060000:
                                    if (skeleton != null)
                                    {
                                        output = new H3D();

                                        var motion = new GFMotion(reader, 0);

                                        var sklAnim = motion.ToH3DSkeletalAnimation(skeleton);
                                        var matAnim = motion.ToH3DMaterialAnimation();
                                        var visAnim = motion.ToH3DVisibilityAnimation();

                                        if (sklAnim != null) output.SkeletalAnimations.Add(sklAnim);
                                        if (matAnim != null) output.MaterialAnimations.Add(matAnim);
                                        if (visAnim != null) output.VisibilityAnimations.Add(visAnim);
                                    }

                                    break;
                            }
                        }
                    }
                }
            }

            return output;
        }

        private static MTShaderEffects _mtShader;

        private static H3D LoadMTModel(BinaryReader reader, string modelFile, string mrlSearchPath)
        {
            if (_mtShader != null)
            {
                var mrlData = new MTMaterials();

                foreach (var file in Directory.GetFiles(mrlSearchPath))
                {
                    if (file == modelFile || !file.StartsWith(modelFile.Substring(0, modelFile.LastIndexOf('.'))))
                        continue;

                    using (var input = new FileStream(file, FileMode.Open))
                    {
                        if (input.Length < 4) continue;

                        var magic = new byte[4];

                        input.Read(magic, 0, magic.Length);

                        if (Encoding.ASCII.GetString(magic) == "MRL\0")
                        {
                            input.Seek(0, SeekOrigin.Begin);

                            mrlData.Materials.AddRange(new MTMaterials(input, _mtShader).Materials);
                        }
                    }
                }

                return new MTModel(reader, mrlData, _mtShader).ToH3D();
            }
            else
            {
                // A *.lfx shader is necessary to load this format
            }

            return null;
        }

        private static void LoadMTShader(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Open))
                _mtShader = new MTShaderEffects(fs);
        }
    }
}
