using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

using PokeD.Graphics.Content.Pipeline.H3D;

namespace PokeD.Graphics.Content.Pipeline.Serialization
{
    [ContentTypeWriter]
    public class H3DEffectWriter : ContentTypeWriter<H3DMaterialContent>
    {
        protected override void Write(ContentWriter output, H3DMaterialContent value)
        {
            output.Write(value.IsSkinned);

            var effectByteCode = value.EffectCompiled.GetEffectCode();
            output.Write(effectByteCode.Length);
            output.Write(effectByteCode);

            output.Write(value.Material);

            output.Write(value.TextureSamplerSettings.Length);
            foreach (var textureHandler in value.TextureSamplerSettings)
            {
                output.Write((byte) textureHandler.MinFilter);
                output.Write((byte) textureHandler.MagFilter);
                output.Write((byte) textureHandler.WrapU);
                output.Write((byte) textureHandler.WrapV);
            }

            output.Write(value.TextureList.Length);
            foreach (var textureContent in value.TextureList)
                output.Write(textureContent.Name);
            foreach (var textureContent in value.TextureList)
                output.WriteSharedResource(textureContent);

            output.Write((byte) value.FaceCulling.GetValueOrDefault());

            output.Write(value.EmissionColor.GetValueOrDefault(Color.White));
            output.Write(value.AmbientColor.GetValueOrDefault(Color.White));
            output.Write(value.DiffuseColor.GetValueOrDefault(Color.White));
            output.Write(value.Specular0Color.GetValueOrDefault(Color.White));
            output.Write(value.Specular1Color.GetValueOrDefault(Color.White));
            output.Write(value.Constant0Color.GetValueOrDefault(Color.White));
            output.Write(value.Constant1Color.GetValueOrDefault(Color.White));
            output.Write(value.Constant2Color.GetValueOrDefault(Color.White));
            output.Write(value.Constant3Color.GetValueOrDefault(Color.White));
            output.Write(value.Constant4Color.GetValueOrDefault(Color.White));
            output.Write(value.Constant5Color.GetValueOrDefault(Color.White));
            output.Write(value.BlendColor.GetValueOrDefault(Color.White));

            output.Write(value.DepthBufferRead.GetValueOrDefault());
            output.Write(value.DepthBufferWrite.GetValueOrDefault());

            output.Write(value.StencilBufferRead.GetValueOrDefault());
            output.Write(value.StencilBufferWrite.GetValueOrDefault());
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform) => "PokeD.Graphics.Content.ContentReaders.H3DEffectReader, PokeD.Graphics.H3D";
    }
}