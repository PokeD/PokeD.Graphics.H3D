using Microsoft.Xna.Framework.Graphics;

namespace PokeD.Graphics.H3D
{
    public class TextureSamplerSettings
    {
        public enum TextureMagFilter
        {
            Point,
            Linear
        }
        public enum TextureMinFilter
        {
            Point,
            PointMipmapPoint,
            PointMipmapLinear,
            Linear,
            LinearMipmapPoint,
            LinearMipmapLinear
        }

        public TextureAddressMode WrapU { get; }
        public TextureAddressMode WrapV { get; }
        public TextureMagFilter MagFilter { get; }
        public TextureMinFilter MinFilter { get; }

        public TextureFilter GetFilter()
        {
            if (MagFilter == TextureMagFilter.Point)
            {
                switch (MinFilter)
                {
                    case TextureMinFilter.Point:
                        return TextureFilter.Point;
                    case TextureMinFilter.PointMipmapPoint:
                        return TextureFilter.Point;
                    case TextureMinFilter.PointMipmapLinear:
                        return TextureFilter.MinLinearMagPointMipLinear;
                    case TextureMinFilter.Linear:
                        return TextureFilter.MinLinearMagPointMipLinear;
                    case TextureMinFilter.LinearMipmapPoint:
                        return TextureFilter.MinLinearMagPointMipPoint;
                    case TextureMinFilter.LinearMipmapLinear:
                        return TextureFilter.MinLinearMagPointMipLinear;
                }
            }
            else
            {
                switch (MinFilter)
                {
                    case TextureMinFilter.Point:
                        return TextureFilter.MinPointMagLinearMipPoint;
                    case TextureMinFilter.PointMipmapPoint:
                        return TextureFilter.MinPointMagLinearMipPoint;
                    case TextureMinFilter.PointMipmapLinear:
                        return TextureFilter.MinPointMagLinearMipLinear;
                    case TextureMinFilter.Linear:
                        return TextureFilter.Linear;
                    case TextureMinFilter.LinearMipmapPoint:
                        return TextureFilter.MinPointMagLinearMipLinear; //Dunno
                    case TextureMinFilter.LinearMipmapLinear:
                        return TextureFilter.Linear;
                }
            }

            return TextureFilter.Point;
        }

        public SamplerState SamplerState { get; }

        public TextureSamplerSettings(TextureMinFilter minFilter, TextureMagFilter magFilter, TextureAddressMode wrapU, TextureAddressMode wrapV )
        {
            MinFilter = minFilter;
            MagFilter = magFilter;
            WrapU = wrapU;
            WrapV = wrapV;

            SamplerState = new SamplerState()
            {
                Filter = GetFilter(),
                AddressU = WrapU,
                AddressV = WrapV,
            };
        }
    }
}