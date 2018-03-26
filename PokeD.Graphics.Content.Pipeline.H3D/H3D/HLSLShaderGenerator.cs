using System.Globalization;
using System.Text;

using PokeD.Graphics.Content.Pipeline.Properties;

using SPICA.Formats.CtrH3D.Model.Material;
using SPICA.Math3D;
using SPICA.PICA.Commands;
using SPICA.PICA.Shader;

namespace PokeD.Graphics.Content.Pipeline.H3D
{
    internal class HLSLShaderGenerator
    {
        private const string Constant0Uniform = "Constant0Color";
        private const string Constant1Uniform = "Constant1Color";
        private const string Constant2Uniform = "Constant2Color";
        private const string Constant3Uniform = "Constant3Color";
        private const string Constant4Uniform = "Constant4Color";
        private const string Constant5Uniform = "Constant5Color";

        private H3DMaterialParams Params { get; }
        public int BoneCount { get; set; } = 0;

        private StringBuilder _sb;
        private bool[] _hasTexColor;

        public HLSLShaderGenerator(H3DMaterialParams @params) => Params = @params;

        public string GetShader()
        {
            _sb = new StringBuilder();
            _sb.AppendLine(Resources.Macros);
            _sb.AppendLine();
            _sb.AppendLine();
            _sb.AppendLine($"#define MAX_BONES {BoneCount}");
            _sb.AppendLine(Resources.Declaration);
            _sb.AppendLine();
            _sb.AppendLine();
            GenAlphaTest();
            _sb.AppendLine();
            _sb.AppendLine();
            _sb.AppendLine(Resources.VertexShader);
            _sb.AppendLine();
            _sb.AppendLine();
            _sb.AppendLine(Resources.PixelShader);
            _sb.AppendLine();
            _sb.AppendLine();
            _sb.AppendLine("// PokeD.Graphics/SPICA auto-generated Pixel Shader");
            _sb.AppendLine("float4 PSCustom(PSInput input) : SV_Target0");
            _sb.AppendLine("{");
            _sb.AppendLine("\tfloat4 output;");
            _sb.AppendLine("\tfloat4 previous;");
            _sb.AppendLine($"\tfloat4 combBuffer = {GetFloat4(Params.TexEnvBufferColor)};");
            _sb.AppendLine("\tfloat4 fragPriColor = float4(1, 1, 1, 1);");
            _sb.AppendLine("\tfloat4 fragSecColor = float4(0, 0, 0, 1);");

            var index = 0;
            _hasTexColor = new[] { false, false, false };
            foreach (var stage in Params.TexEnvStages)
            {
                var hasColor = !stage.IsColorPassThrough;
                var hasAlpha = !stage.IsAlphaPassThrough;

                var colorArgs = new string[3];
                var alphaArgs = new string[3];

                string constant;

                switch (Params.GetConstantIndex(index++))
                {
                    default:
                    case 0: constant = Constant0Uniform; break;
                    case 1: constant = Constant1Uniform; break;
                    case 2: constant = Constant2Uniform; break;
                    case 3: constant = Constant3Uniform; break;
                    case 4: constant = Constant4Uniform; break;
                    case 5: constant = Constant5Uniform; break;
                }

                for (var param = 0; param < 3; param++)
                {
                    //Check if any of the texture units are used
                    for (var unit = 0; unit < 3; unit++)
                    {
                        if (!_hasTexColor[unit] && (
                            stage.Source.Color[param] == PICATextureCombinerSource.Texture0 + unit ||
                            stage.Source.Alpha[param] == PICATextureCombinerSource.Texture0 + unit))
                        {
                            GenTexColor(unit);
                            _hasTexColor[unit] = true;
                        }
                    }

                    var colorArg = GetCombinerSource(stage.Source.Color[param], constant);
                    var alphaArg = GetCombinerSource(stage.Source.Alpha[param], constant);

                    switch ((PICATextureCombinerColorOp)((int)stage.Operand.Color[param] & ~1))
                    {
                        case PICATextureCombinerColorOp.Alpha: colorArg = $"{colorArg}.aaaa"; break;
                        case PICATextureCombinerColorOp.Red: colorArg = $"{colorArg}.rrrr"; break;
                        case PICATextureCombinerColorOp.Green: colorArg = $"{colorArg}.gggg"; break;
                        case PICATextureCombinerColorOp.Blue: colorArg = $"{colorArg}.bbbb"; break;
                    }

                    switch ((PICATextureCombinerAlphaOp)((int)stage.Operand.Alpha[param] & ~1))
                    {
                        case PICATextureCombinerAlphaOp.Alpha: alphaArg = $"{alphaArg}.a"; break;
                        case PICATextureCombinerAlphaOp.Red: alphaArg = $"{alphaArg}.r"; break;
                        case PICATextureCombinerAlphaOp.Green: alphaArg = $"{alphaArg}.g"; break;
                        case PICATextureCombinerAlphaOp.Blue: alphaArg = $"{alphaArg}.b"; break;
                    }

                    if (((int) stage.Operand.Color[param] & 1) != 0)
                        colorArg = $"1 - {colorArg}";

                    if (((int) stage.Operand.Alpha[param] & 1) != 0)
                        alphaArg = $"1 - {alphaArg}";

                    colorArgs[param] = colorArg;
                    alphaArgs[param] = alphaArg;
                }

                if (hasColor)
                    GenCombinerColor(stage, colorArgs);

                if (hasAlpha)
                    GenCombinerAlpha(stage, alphaArgs);

                var colorScale = 1 << (int)stage.Scale.Color;
                var alphaScale = 1 << (int)stage.Scale.Alpha;

                if (colorScale != 1)
                    _sb.AppendLine($"\toutput.rgb = min(output.rgb * {colorScale}, 1);");

                if (alphaScale != 1)
                    _sb.AppendLine($"\toutput.a = min(output.a * {alphaScale}, 1);");

                if (stage.UpdateColorBuffer)
                    _sb.AppendLine("\tcombBuffer.rgb = previous.rgb;");

                if (stage.UpdateAlphaBuffer)
                    _sb.AppendLine("\tcombBuffer.a = previous.a;");

                if (index < 6 && (hasColor || hasAlpha))
                    _sb.AppendLine("\tprevious = output;");
            }

            _sb.AppendLine("\tAlphaTest(output);");
            _sb.AppendLine("\treturn output;");
            _sb.AppendLine("}");
            _sb.AppendLine("// End of auto-generated code");
            _sb.AppendLine();
            _sb.AppendLine();
            _sb.AppendLine(Resources.Techniques);
            return _sb.ToString();
        }

        private void GenAlphaTest()
        {
            _sb.AppendLine("void AlphaTest(float4 output)");
            _sb.AppendLine("{");
            if (Params.AlphaTest.Enabled)
            {
                var reference = (Params.AlphaTest.Reference / 255f).ToString(CultureInfo.InvariantCulture);

                //Note: This is the condition to pass the test, so we actually test the inverse to discard
                switch (Params.AlphaTest.Function)
                {
                    case PICATestFunc.Never: _sb.AppendLine("\tdiscard;"); break;
                    case PICATestFunc.Equal: _sb.AppendLine($"\tif (output.a != {reference}) discard;"); break;
                    case PICATestFunc.Notequal: _sb.AppendLine($"\tif (output.a == {reference}) discard;"); break;
                    case PICATestFunc.Less: _sb.AppendLine($"\tif (output.a >= {reference}) discard;"); break;
                    case PICATestFunc.Lequal: _sb.AppendLine($"\tif (output.a > {reference}) discard;"); break;
                    case PICATestFunc.Greater: _sb.AppendLine($"\tif (output.a <= {reference}) discard;"); break;
                    case PICATestFunc.Gequal: _sb.AppendLine($"\tif (output.a < {reference}) discard;"); break;
                }
            }
            _sb.AppendLine("}");
        }

        private void GenTexColor(int index)
        {
            var texCoord = Params.TextureCoords[index];

            string texture;

            var texCoord0 = $"{ShaderOutputRegName.TexCoord0}";
            var texCoord1 = $"{ShaderOutputRegName.TexCoord1}";
            var texCoord2 = $"{ShaderOutputRegName.TexCoord2}";

            if (index == 0)
            {
                switch (Params.TextureCoords[0].MappingType)
                {
                    case H3DTextureMappingType.CameraCubeEnvMap:
                        texture = $"SAMPLE_TEXTURE(TextureCube, input.{texCoord0}.xyz)";
                        break;

                    case H3DTextureMappingType.ProjectionMap:
                        // Texture = $"textureProj(Textures[0], {TexCoord0})";
                        return; // -- Not available in HLSL

                    default:
                        texture = $"SAMPLE_TEXTURE(Texture0, input.{texCoord0}.xy)";
                        break;
                }
            }
            else
            {
                var coordIndex = index;

                if (coordIndex == 2 && (
                    Params.TexCoordConfig == H3DTexCoordConfig.Config0110 ||
                    Params.TexCoordConfig == H3DTexCoordConfig.Config0111 ||
                    Params.TexCoordConfig == H3DTexCoordConfig.Config0112))
                {
                    coordIndex = 1;
                }

                switch (coordIndex)
                {
                    default:
                    case 0: texture = $"SAMPLE_TEXTURE(Texture{index}, input.{texCoord0}.xy)"; break;
                    case 1: texture = $"SAMPLE_TEXTURE(Texture{index}, input.{texCoord1}.xy)"; break;
                    case 2: texture = $"SAMPLE_TEXTURE(Texture{index}, input.{texCoord2}.xy)"; break;
                }
            }

            _sb.AppendLine($"\tfloat4 color{index} = {texture};");
        }

        private void GenCombinerColor(PICATexEnvStage stage, string[] colorArgs)
        {
            switch (stage.Combiner.Color)
            {
                case PICATextureCombinerMode.Replace:
                    _sb.AppendLine($"\toutput.rgb = ({colorArgs[0]}).rgb;");
                    break;
                case PICATextureCombinerMode.Modulate:
                    _sb.AppendLine($"\toutput.rgb = ({colorArgs[0]}).rgb * ({colorArgs[1]}).rgb;");
                    break;
                case PICATextureCombinerMode.Add:
                    _sb.AppendLine($"\toutput.rgb = min(({colorArgs[0]}).rgb + ({colorArgs[1]}).rgb, 1);");
                    break;
                case PICATextureCombinerMode.AddSigned:
                    _sb.AppendLine($"\toutput.rgb = clamp(({colorArgs[0]}).rgb + ({colorArgs[1]}).rgb - 0.5, 0, 1);");
                    break;
                case PICATextureCombinerMode.Interpolate:
                    _sb.AppendLine($"\toutput.rgb = lerp(({colorArgs[1]}).rgb, ({colorArgs[0]}).rgb, ({colorArgs[2]}).rgb);");
                    break;
                case PICATextureCombinerMode.Subtract:
                    _sb.AppendLine($"\toutput.rgb = max(({colorArgs[0]}).rgb - ({colorArgs[1]}).rgb, 0);");
                    break;
                case PICATextureCombinerMode.DotProduct3Rgb:
                    _sb.AppendLine($"\toutput.rgb = float3(min(dot(({colorArgs[0]}).rgb, ({colorArgs[1]}).rgb), 1));");
                    break;
                case PICATextureCombinerMode.DotProduct3Rgba:
                    _sb.AppendLine($"\toutput.rgb = float3(min(dot(({colorArgs[0]}), ({colorArgs[1]})), 1));");
                    break;
                case PICATextureCombinerMode.MultAdd:
                    _sb.AppendLine($"\toutput.rgb = min(({colorArgs[0]}).rgb * ({colorArgs[1]}).rgb + ({colorArgs[2]}).rgb, 1);");
                    break;
                case PICATextureCombinerMode.AddMult:
                    _sb.AppendLine($"\toutput.rgb = min(({colorArgs[0]}).rgb + ({colorArgs[1]}).rgb, 1) * ({colorArgs[2]}).rgb;");
                    break;
            }
        }

        private void GenCombinerAlpha(PICATexEnvStage stage, string[] alphaArgs)
        {
            switch (stage.Combiner.Alpha)
            {
                case PICATextureCombinerMode.Replace:
                    _sb.AppendLine($"\toutput.a = ({alphaArgs[0]});");
                    break;
                case PICATextureCombinerMode.Modulate:
                    _sb.AppendLine($"\toutput.a = ({alphaArgs[0]}) * ({alphaArgs[1]});");
                    break;
                case PICATextureCombinerMode.Add:
                    _sb.AppendLine($"\toutput.a = min(({alphaArgs[0]}) + ({alphaArgs[1]}), 1);");
                    break;
                case PICATextureCombinerMode.AddSigned:
                    _sb.AppendLine($"\toutput.a = clamp(({alphaArgs[0]}) + ({alphaArgs[1]}) - 0.5, 0, 1);");
                    break;
                case PICATextureCombinerMode.Interpolate:
                    _sb.AppendLine($"\toutput.a = lerp(({alphaArgs[1]}), ({alphaArgs[0]}), ({alphaArgs[2]}));");
                    break;
                case PICATextureCombinerMode.Subtract:
                    _sb.AppendLine($"\toutput.a = max(({alphaArgs[0]}) - ({alphaArgs[1]}), 0);");
                    break;
                case PICATextureCombinerMode.DotProduct3Rgb:
                    _sb.AppendLine($"\toutput.a = min(dot(float3({alphaArgs[0]}), float3({alphaArgs[1]})), 1);");
                    break;
                case PICATextureCombinerMode.DotProduct3Rgba:
                    _sb.AppendLine($"\toutput.a = min(dot(float4({alphaArgs[0]}), float4({alphaArgs[1]})), 1);");
                    break;
                case PICATextureCombinerMode.MultAdd:
                    _sb.AppendLine($"\toutput.a = min(({alphaArgs[0]}) * ({alphaArgs[1]}) + ({alphaArgs[2]}), 1);");
                    break;
                case PICATextureCombinerMode.AddMult:
                    _sb.AppendLine($"\toutput.a = min(({alphaArgs[0]}) + ({alphaArgs[1]}), 1) * ({alphaArgs[2]});");
                    break;
            }
        }

        private static string GetCombinerSource(PICATextureCombinerSource source, string constant)
        {
            switch (source)
            {
                default:
                case PICATextureCombinerSource.PrimaryColor: return $"{ShaderOutputRegName.Color}";
                case PICATextureCombinerSource.FragmentPrimaryColor: return "fragPriColor";
                case PICATextureCombinerSource.FragmentSecondaryColor: return "fragSecColor";
                case PICATextureCombinerSource.Texture0: return "color0";
                case PICATextureCombinerSource.Texture1: return "color1";
                case PICATextureCombinerSource.Texture2: return "color2";
                case PICATextureCombinerSource.PreviousBuffer: return "combBuffer";
                case PICATextureCombinerSource.Constant: return constant;
                case PICATextureCombinerSource.Previous: return "previous";
            }
        }

        private static string GetFloat4(RGBA color) => string.Format(CultureInfo.InvariantCulture, "float4({0}, {1}, {2}, {3})", color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
    }
}
