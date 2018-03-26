using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using PokeD.Graphics.H3D;

namespace PokeD.Graphics.Effects
{
    public interface IH3DEffect : IEffectMatrices
    {
        string Material { get; }

        Texture2D Texture0 { get; set; }
        Texture2D Texture1 { get; set; }
        Texture2D Texture2 { get; set; }

        TextureSamplerSettings[] TextureSamplerSettings { get; set; }

        Color BlendColor { get; }

        CullMode CullMode { get; set; }

        bool DepthBufferRead { get; set; }
        bool DepthBufferWrite { get; set; }

        bool StencilBufferRead { get; set; }
        bool StencilBufferWrite { get; set; }
    }
}