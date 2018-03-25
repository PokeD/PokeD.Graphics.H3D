using System.Globalization;
using System.Text;

using PokeD.Graphics.Content.Pipeline.Properties;

using SPICA.Formats.CtrH3D.Model.Material;
using SPICA.Math3D;
using SPICA.PICA.Commands;
using SPICA.PICA.Shader;

namespace PokeD.Graphics.Content.Pipeline.H3D
{
    internal class ShaderGenerator
    {
        public const string EmissionUniform = "EmissionColor";
        public const string AmbientUniform = "AmbientColor";
        public const string DiffuseUniform = "DiffuseColor";
        public const string Specular0Uniform = "Specular0Color";
        public const string Specular1Uniform = "Specular1Color";
        public const string Constant0Uniform = "Constant0Color";
        public const string Constant1Uniform = "Constant1Color";
        public const string Constant2Uniform = "Constant2Color";
        public const string Constant3Uniform = "Constant3Color";
        public const string Constant4Uniform = "Constant4Color";
        public const string Constant5Uniform = "Constant5Color";

        private H3DMaterialParams Params { get; }
        public int BoneCount { get; set; } = 0;

        private StringBuilder _sb;

        private bool[] _hasTexColor;

        public ShaderGenerator(H3DMaterialParams @params) => Params = @params;

        public string GetFragShader()
        {
            _sb = new StringBuilder();
            _sb.AppendLine(Resources.Macros);
            _sb.AppendLine($"#define MAX_BONES {BoneCount}");
            _sb.AppendLine(Resources.VertexShaderBase);

            _hasTexColor = new [] { false, false, false };

            var index = 0;

            var hasFragColors = false;

            _sb.AppendLine();
            _sb.AppendLine();
            GenAlphaTest();
            _sb.AppendLine();
            _sb.AppendLine();
            _sb.AppendLine("// PokeD.Graphics/SPICA auto-generated Pixel Shader");
            _sb.AppendLine($"float4 PSCustom(PSInput input) : COLOR0");
            _sb.AppendLine($"{{");
            _sb.AppendLine($"\tfloat4 output;");

            _sb.AppendLine($"\tfloat4 Previous;");

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
                    //Check if the Fragment lighting colors are used
                    if (!hasFragColors && (
                        stage.Source.Color[param] == PICATextureCombinerSource.FragmentPrimaryColor ||
                        stage.Source.Alpha[param] == PICATextureCombinerSource.FragmentPrimaryColor ||
                        stage.Source.Color[param] == PICATextureCombinerSource.FragmentSecondaryColor ||
                        stage.Source.Alpha[param] == PICATextureCombinerSource.FragmentSecondaryColor))
                    {
                        GenFragColors();
                        hasFragColors = true;
                    }

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

                    if (((int)stage.Operand.Color[param] & 1) != 0)
                        colorArg = $"1 - {colorArg}";

                    if (((int)stage.Operand.Alpha[param] & 1) != 0)
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
                    _sb.AppendLine("\tCombBuffer.rgb = Previous.rgb;");

                if (stage.UpdateAlphaBuffer)
                    _sb.AppendLine("\tCombBuffer.a = Previous.a;");

                if (index < 6 && (hasColor || hasAlpha))
                    _sb.AppendLine("\tPrevious = output;");
            }

            _sb.AppendLine($"\tAlphaTest(output);");

            _sb.AppendLine($"\treturn output;");
            _sb.AppendLine($"}}");
            _sb.AppendLine($"// End of auto-generated code");
            _sb.AppendLine();
            _sb.AppendLine();
            _sb.AppendLine(Resources.PixelShader);


            return _sb.ToString();
        }

        private void GenAlphaTest()
        {
            _sb.AppendLine($"void AlphaTest(float4 output)");
            _sb.AppendLine($"{{");
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
            _sb.AppendLine($"}}");
        }

        private void GenFragColors()
        {
            //_sb.AppendLine($"\tfloat4 FragPriColor = float4((EmissionColor + AmbientColor * SAmbient).rgb, 1);");
            _sb.AppendLine($"\tfloat4 FragPriColor = float4(1, 1, 1, 1);");
            _sb.AppendLine($"\tfloat4 FragSecColor = float4(0, 0, 0, 1);");
            return;

            //See Model and Mesh class for the LUT mappings
            var dist0 = GetLUTInput(Params.LUTInputSelection.Dist0, Params.LUTInputScale.Dist0, 0);
            var dist1 = GetLUTInput(Params.LUTInputSelection.Dist1, Params.LUTInputScale.Dist1, 1);
            var fresnel = GetLUTInput(Params.LUTInputSelection.Fresnel, Params.LUTInputScale.Fresnel, 2);
            var reflecR = GetLUTInput(Params.LUTInputSelection.ReflecR, Params.LUTInputScale.ReflecR, 3);
            var reflecG = GetLUTInput(Params.LUTInputSelection.ReflecG, Params.LUTInputScale.ReflecG, 4);
            var reflecB = GetLUTInput(Params.LUTInputSelection.ReflecB, Params.LUTInputScale.ReflecB, 5);

            var color = $"{EmissionUniform} + {AmbientUniform} * SAmbient";

            _sb.AppendLine($"\tfloat4 FragPriColor = float4(({color}).rgb, 1);");
            _sb.AppendLine("\tfloat4 FragSecColor = float4(0, 0, 0, 1);");

            if (Params.BumpMode == H3DBumpMode.AsBump ||
                Params.BumpMode == H3DBumpMode.AsTangent)
            {
                if (!_hasTexColor[Params.BumpTexture])
                {
                    GenTexColor(Params.BumpTexture);
                    _hasTexColor[Params.BumpTexture] = true;
                }
            }

            var bumpColor = $"Color{Params.BumpTexture}.xyz * 2 - 1";

            switch (Params.BumpMode)
            {
                case H3DBumpMode.AsBump:
                    _sb.AppendLine($"\tfloat3 SurfNormal = {bumpColor};");
                    _sb.AppendLine("\tfloat3 SurfTangent = float3(1, 0, 0);");
                    break;

                case H3DBumpMode.AsTangent:
                    _sb.AppendLine("\tfloat3 SurfNormal = float3(0, 0, 1);");
                    _sb.AppendLine($"\tfloat3 SurfTangent = {bumpColor};");
                    break;

                default: /* NotUsed */
                    _sb.AppendLine("\tfloat3 SurfNormal = float3(0, 0, 1);");
                    _sb.AppendLine("\tfloat3 SurfTangent = float3(1, 0, 0);");
                    break;
            }

            //Recalculates the Z axis on the normal to give more precision.
            //For Tangent it was reported that the 3DS doesn't recalculate Z
            //(or maybe it just doesn't matter for the final formula where it is used).
            if ((Params.FragmentFlags & H3DFragmentFlags.IsBumpRenormalizeEnabled) != 0)
            {
                _sb.AppendLine("\tSurfNormal.z = sqrt(max(1 - dot(SurfNormal.xy, SurfNormal.xy), 0));");
            }

            var halfProj = "Half - Normal / dot(Normal, Normal) * dot(Normal, Half)";

            var quatNormal = $"{ShaderOutputRegName.QuatNormal}";
            var view = $"{ShaderOutputRegName.View}";

            _sb.AppendLine($"\tfloat4 NormQuat = normalize({quatNormal});");
            _sb.AppendLine($"\tfloat3 Normal = QuatRotate(NormQuat, SurfNormal);");
            _sb.AppendLine($"\tfloat3 Tangent = QuatRotate(NormQuat, SurfTangent);");


            //Lights loop start
            _sb.AppendLine();
            _sb.AppendLine("\tfor (int i = 0; i < LightsCount; i++) {");

            _sb.AppendLine("\t\tfloat3 Light = (Lights[i].Directional != 0)" +
                 " ? normalize(Lights[i].Position)" +
                $" : normalize(Lights[i].Position + {view}.xyz);");

            _sb.AppendLine($"\t\tfloat3 Half = normalize(normalize({view}.xyz) + Light);");
            _sb.AppendLine("\t\tfloat CosNormalHalf = dot(Normal, Half);");
            _sb.AppendLine($"\t\tfloat CosViewHalf = dot(normalize({view}.xyz), Half);");
            _sb.AppendLine($"\t\tfloat CosNormalView = dot(Normal, normalize({view}.xyz));");
            _sb.AppendLine("\t\tfloat CosLightNormal = dot(Light, Normal);");
            _sb.AppendLine("\t\tfloat CosLightSpot = dot(Light, Lights[i].Direction);");
            _sb.AppendLine($"\t\tfloat CosPhi = dot({halfProj}, Tangent);");

            _sb.AppendLine("\t\tfloat ln = (Lights[i].TwoSidedDiff != 0)" +
                " ? abs(CosLightNormal)" +
                " : max(CosLightNormal, 0);");

            _sb.AppendLine("\t\tfloat SpotAtt = 1;");
            _sb.AppendLine();

            _sb.AppendLine("\t\tif (Lights[i].SpotAttEnb != 0) {");
            _sb.AppendLine("\t\t\tfloat SpotIndex;");
            _sb.AppendLine();

            _sb.AppendLine("\t\t\tswitch (Lights[i].AngleLUTInput) {");
            _sb.AppendLine("\t\t\tcase 0: SpotIndex = CosNormalHalf; break;");
            _sb.AppendLine("\t\t\tcase 1: SpotIndex = CosViewHalf; break;");
            _sb.AppendLine("\t\t\tcase 2: SpotIndex = CosNormalView; break;");
            _sb.AppendLine("\t\t\tcase 3: SpotIndex = CosLightNormal; break;");
            _sb.AppendLine("\t\t\tcase 4: SpotIndex = CosLightSpot; break;");
            _sb.AppendLine("\t\t\tcase 5: SpotIndex = CosPhi; break;");
            _sb.AppendLine("\t\t\t}");
            _sb.AppendLine();

            _sb.AppendLine("\t\t\tSpotAtt = SampleLUT(" +
                "6 + i * 2, SpotIndex, Lights[i].AngleLUTScale);");

            _sb.AppendLine("\t\t}");
            _sb.AppendLine();

            _sb.AppendLine("\t\tfloat DistAtt = 1;");
            _sb.AppendLine();

            _sb.AppendLine("\t\tif (Lights[i].DistAttEnb != 0) {");

            string distAttIdx;

            distAttIdx = $"length(-{view}.xyz - Lights[i].Position) * Lights[i].AttScale";
            distAttIdx = $"clamp({distAttIdx} + Lights[i].AttBias, 0, 1)";

            _sb.AppendLine($"\t\t\tDistAtt = SampleLUT(7 + i * 2, {distAttIdx}, 1);");
            _sb.AppendLine("\t\t}");
            _sb.AppendLine();

            var clampHighLight = string.Empty;

            var specular0Color = Specular0Uniform;
            var specular1Color = Specular1Uniform;

            if ((Params.FragmentFlags & H3DFragmentFlags.IsClampHighLightEnabled) != 0)
            {
                clampHighLight = " * fi";

                _sb.AppendLine("\t\tfloat fi = (CosLightNormal < 0) ? 0 : 1;");
            }

            if ((Params.FragmentFlags & H3DFragmentFlags.IsLUTDist0Enabled) != 0)
            {
                specular0Color += " * d0";

                _sb.AppendLine($"\t\tfloat d0 = {dist0};");
            }

            if ((Params.FragmentFlags & H3DFragmentFlags.IsLUTReflectionEnabled) != 0)
            {
                specular1Color = "r";

                _sb.AppendLine("\t\tfloat4 r = float4(");
                _sb.AppendLine($"\t\t\t{reflecR},");
                _sb.AppendLine($"\t\t\t{reflecG},");
                _sb.AppendLine($"\t\t\t{reflecB}, 1);");
            }

            if ((Params.FragmentFlags & H3DFragmentFlags.IsLUTDist1Enabled) != 0)
            {
                specular1Color += " * d1";

                _sb.AppendLine($"\t\tfloat d1 = {dist1};");
            }

            if ((Params.FragmentFlags & H3DFragmentFlags.IsLUTGeoFactor0Enabled) != 0)
                specular0Color += " * g";

            if ((Params.FragmentFlags & H3DFragmentFlags.IsLUTGeoFactor1Enabled) != 0)
                specular1Color += " * g";

            if ((Params.FragmentFlags & H3DFragmentFlags.IsLUTGeoFactorEnabled) != 0)
                _sb.AppendLine("\t\tfloat g = ln / abs(dot(Half, Half));");

            _sb.AppendLine("\t\tfloat4 Diffuse =");
            _sb.AppendLine($"\t\t\t{AmbientUniform} * Lights[i].Ambient +");
            _sb.AppendLine($"\t\t\t{DiffuseUniform} * Lights[i].Diffuse * clamp(ln, 0, 1);");
            _sb.AppendLine($"\t\tfloat4 Specular = " +
                $"{specular0Color} * Lights[i].Specular0 + " +
                $"{specular1Color} * Lights[i].Specular1;");
            _sb.AppendLine($"\t\tFragPriColor.rgb += Diffuse.rgb * SpotAtt * DistAtt;");
            _sb.AppendLine($"\t\tFragSecColor.rgb += Specular.rgb * SpotAtt * DistAtt{clampHighLight};");

            if ((Params.FresnelSelector & H3DFresnelSelector.Pri) != 0)
                _sb.AppendLine($"\t\tFragPriColor.a = {fresnel};");

            if ((Params.FresnelSelector & H3DFresnelSelector.Sec) != 0)
                _sb.AppendLine($"\t\tFragSecColor.a = {fresnel};");

            //Lights loop end
            _sb.AppendLine("\t}");
            _sb.AppendLine("\tFragPriColor = clamp(FragPriColor, 0, 1);");
            _sb.AppendLine("\tFragSecColor = clamp(FragSecColor, 0, 1);");
            _sb.AppendLine();
        }

        private static string GetLUTInput(PICALUTInput input, PICALUTScale scale, int lut)
        {
            //TODO: CosLightSpot and CosPhi
            string inputStr;

            switch (input)
            {
                default:
                case PICALUTInput.CosNormalHalf: inputStr = "CosNormalHalf"; break;
                case PICALUTInput.CosViewHalf: inputStr = "CosViewHalf"; break;
                case PICALUTInput.CosNormalView: inputStr = "CosNormalView"; break;
                case PICALUTInput.CosLightNormal: inputStr = "CosLightNormal"; break;
                case PICALUTInput.CosLightSpot: inputStr = "CosLightSpot"; break;
                case PICALUTInput.CosPhi: inputStr = "CosPhi"; break;
            }

            var output = $"SAMPLE_TEXTURE(LUT{lut}, float2(({inputStr} + 1) * 0.5, 0)).r";

            if (scale != PICALUTScale.One)
            {
                var scaleStr = scale.ToSingle().ToString(CultureInfo.InvariantCulture);

                output = $"min({output} * {scaleStr}, 1)";
            }

            return output;
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

            _sb.AppendLine($"\tfloat4 Color{index} = {texture};");
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
                    _sb.AppendLine($"\toutput.rgb = saturate(({colorArgs[0]}).rgb + ({colorArgs[1]}).rgb - 0.5, 0, 1);");
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
                    _sb.AppendLine($"\toutput.a = saturate(({alphaArgs[0]}) + ({alphaArgs[1]}) - 0.5, 0, 1);");
                    break;
                case PICATextureCombinerMode.Interpolate:
                    _sb.AppendLine($"\toutput.a = lerp(({alphaArgs[1]}), ({alphaArgs[0]}), ({alphaArgs[2]}));");
                    break;
                case PICATextureCombinerMode.Subtract:
                    _sb.AppendLine($"\toutput.a = max(({alphaArgs[0]}) - ({alphaArgs[1]}), 0);");
                    break;
                case PICATextureCombinerMode.DotProduct3Rgb:
                    _sb.AppendLine($"\toutput.a = min(dot(vec3({alphaArgs[0]}), vec3({alphaArgs[1]})), 1);");
                    break;
                case PICATextureCombinerMode.DotProduct3Rgba:
                    _sb.AppendLine($"\toutput.a = min(dot(vec4({alphaArgs[0]}), vec4({alphaArgs[1]})), 1);");
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
                case PICATextureCombinerSource.FragmentPrimaryColor: return "FragPriColor";
                case PICATextureCombinerSource.FragmentSecondaryColor: return "FragSecColor";
                case PICATextureCombinerSource.Texture0: return "Color0";
                case PICATextureCombinerSource.Texture1: return "Color1";
                case PICATextureCombinerSource.Texture2: return "Color2";
                case PICATextureCombinerSource.PreviousBuffer: return "CombBuffer";
                case PICATextureCombinerSource.Constant: return constant;
                case PICATextureCombinerSource.Previous: return "Previous";
            }
        }

        private static string GetFloat4(RGBA color) => string.Format(CultureInfo.InvariantCulture, "float4({0}, {1}, {2}, {3})", color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
    }
}
