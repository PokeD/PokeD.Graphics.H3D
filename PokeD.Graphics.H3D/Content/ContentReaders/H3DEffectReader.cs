using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using PokeD.Graphics.Effects;
using PokeD.Graphics.H3D;

namespace PokeD.Graphics.Content.ContentReaders
{
    public class H3DEffectReader : ContentTypeReader<IH3DEffect>
    {
        protected override IH3DEffect Read(ContentReader reader, IH3DEffect existingInstance)
        {
            var graphicsDeviceService = (IGraphicsDeviceService) reader.ContentManager.ServiceProvider.GetService(typeof(IGraphicsDeviceService));
            var device = graphicsDeviceService.GraphicsDevice;

            var isSkinned = reader.ReadBoolean();
            var effectByteCodeLength = reader.ReadInt32();
            var effectByteCode = reader.ReadBytes(effectByteCodeLength);
            var effect = isSkinned ? new H3DSkinningEffect(device, effectByteCode) : new H3DEffect(device, effectByteCode);
            effect.Material = reader.ReadString();

            var textureSamplerSettingsCount = reader.ReadInt32();
            effect.TextureSamplerSettings = new TextureSamplerSettings[textureSamplerSettingsCount];
            for (var i = 0; i < textureSamplerSettingsCount; i++)
            {
                effect.TextureSamplerSettings[i] = new TextureSamplerSettings(
                    (TextureSamplerSettings.TextureMinFilter) reader.ReadByte(),
                    (TextureSamplerSettings.TextureMagFilter) reader.ReadByte(),
                    (TextureAddressMode) reader.ReadByte(),
                    (TextureAddressMode) reader.ReadByte());
            }

            var textureCount = reader.ReadInt32();
            var textureNames = new string[textureCount];
            for (var i = 0; i < textureCount; i++)
                textureNames[i] = reader.ReadString();
            for (var i = 0; i < textureCount; i++)
            {
                var index = i;
                reader.ReadSharedResource(delegate (Texture2D texture)
                {
                    texture.Name = textureNames[index];

                    if (effect.Texture0 == null)
                        effect.Texture0 = texture;
                    else if (effect.Texture1 == null)
                        effect.Texture1 = texture;
                    else if (effect.Texture2 == null)
                        effect.Texture2 = texture;
                });
            }

            effect.CullMode = (CullMode) reader.ReadByte();

            effect.EmissionColor  = reader.ReadColor().ToVector4();
            effect.AmbientColor   = reader.ReadColor().ToVector4();
            effect.DiffuseColor   = reader.ReadColor().ToVector4();
            effect.Specular0Color = reader.ReadColor().ToVector4();
            effect.Specular1Color = reader.ReadColor().ToVector4();
            effect.Constant0Color = reader.ReadColor().ToVector4();
            effect.Constant1Color = reader.ReadColor().ToVector4();
            effect.Constant2Color = reader.ReadColor().ToVector4();
            effect.Constant3Color = reader.ReadColor().ToVector4();
            effect.Constant4Color = reader.ReadColor().ToVector4();
            effect.Constant5Color = reader.ReadColor().ToVector4();
            var blendColor = reader.ReadColor().ToVector4();

            effect.DepthBufferRead = reader.ReadBoolean();
            effect.DepthBufferWrite = reader.ReadBoolean();

            effect.StencilBufferRead = reader.ReadBoolean();
            effect.StencilBufferWrite = reader.ReadBoolean();

            return effect;
        }
    }
}