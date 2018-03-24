using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

using SPICA.Formats.CtrH3D.Model.Material;
using SPICA.PICA.Commands;
using SPICA.PICA.Converters;

namespace PokeD.Graphics.Content.Pipeline.Extensions
{
    internal static class NumericsExtension
    {
        public static Matrix GetMatrix(this H3DTextureCoord coord)
        {
            return Matrix.CreateScale(new Vector3(coord.Scale.ToXNA(), 1)) * Matrix.CreateTranslation(new Vector3(coord.Translation.ToXNA(), 1));
        }

        public static TextureAddressMode ToXNAWrap(this PICATextureWrap wrap)
        {
            switch (wrap)
            {
                default:
                case PICATextureWrap.ClampToEdge: return TextureAddressMode.Clamp;
                case PICATextureWrap.ClampToBorder: return TextureAddressMode.Border;
                case PICATextureWrap.Repeat: return TextureAddressMode.Wrap;
                case PICATextureWrap.Mirror: return TextureAddressMode.Mirror;
            }
        }

        public static Matrix ToXNA(this SPICA.Math3D.Matrix3x4 m)
        {
            return new Matrix(
                m.M11, m.M12, m.M13, 0f,
                m.M21, m.M22, m.M23, 0f,
                m.M31, m.M32, m.M33, 0f,
                m.M41, m.M42, m.M43, 1f);
        }

        public static Matrix ToXNA(this System.Numerics.Matrix4x4 matrix) =>
            new Matrix(
                matrix.M11, matrix.M12, matrix.M13, matrix.M14,
                matrix.M21, matrix.M22, matrix.M23, matrix.M24,
                matrix.M31, matrix.M32, matrix.M33, matrix.M34,
                matrix.M41, matrix.M42, matrix.M43, matrix.M44);

        public static Color ToXNA(this SPICA.Math3D.RGBA rgba) => new Color(rgba.R, rgba.G, rgba.B, rgba.A);

        public static Vector2 ToXNA(this System.Numerics.Vector2 vector2) => new Vector2(vector2.X, vector2.Y);
        public static Vector3 ToXNA(this System.Numerics.Vector3 vector3) => new Vector3(vector3.X, vector3.Y, vector3.Z);
        public static Vector4 ToXNA(this System.Numerics.Vector4 vector4) => new Vector4(vector4.X, vector4.Y, vector4.Z, vector4.W);

        public static Vector3 ToVector3(this System.Numerics.Vector4 vector4) => new Vector3(vector4.X, vector4.Y, vector4.Z);
        public static Vector2 ToVector2(this System.Numerics.Vector4 vector4) => new Vector2(vector4.X, vector4.Y);
        public static Color ToColor(this System.Numerics.Vector4 vector4) => new Color(vector4.X, vector4.Y, vector4.Z, vector4.W);

        public static Vector2 ToUV(this Vector2 textcoord) => new Vector2(textcoord.X, textcoord.Y);

        public static Quaternion ToXNA(this System.Numerics.Quaternion quaternion) => new Quaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);

        public static Byte4 ToByte4(this BoneIndices boneIndices) => new Byte4(boneIndices.b0, boneIndices.b1, boneIndices.b2, boneIndices.b3);
        public static Vector4 ToVector4(this BoneWeights boneWeights) => new Vector4(boneWeights.w0, boneWeights.w1, boneWeights.w2, boneWeights.w3);
    }
}