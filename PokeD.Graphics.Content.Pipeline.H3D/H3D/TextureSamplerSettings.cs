using Microsoft.Xna.Framework.Graphics;

namespace PokeD.Graphics.Content.Pipeline.H3D
{
    public struct TextureSamplerSettings
    {
        public enum TextureMagFilter { Point, Linear }
        public enum TextureMinFilter { Point, PointMipmapPoint, PointMipmapLinear, Linear, LinearMipmapPoint, LinearMipmapLinear }

        public TextureAddressMode WrapU { get; set; }
        public TextureAddressMode WrapV { get; set; }
        public TextureMagFilter MagFilter { get; set; }
        public TextureMinFilter MinFilter { get; set; }
    }
}