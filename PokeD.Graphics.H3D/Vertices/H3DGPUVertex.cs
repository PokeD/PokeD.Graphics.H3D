using System.Runtime.InteropServices;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PokeD.Graphics.Vertices
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct H3DGPUVertex : IVertexType
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Color Color;
        public Vector2 TextureCoord0;
        public Vector2 TextureCoord1;
        public Vector2 TextureCoord2;

        #region IVertexType Members

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
            new VertexElement(00, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(24, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(28, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(36, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1),
            new VertexElement(44, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 2));

        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
        #endregion

        public H3DGPUVertex(Vector3 position, Vector3 normal, Color color, Vector2 textureCoord0, Vector2 textureCoord1, Vector2 textureCoord2)
        {
            Position = position;
            Normal = normal;
            Color = color;
            TextureCoord0 = textureCoord0;
            TextureCoord1 = textureCoord1;
            TextureCoord2 = textureCoord2;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Position.GetHashCode() * 397) ^
                       Normal.GetHashCode() ^
                       Color.GetHashCode() ^
                       TextureCoord0.GetHashCode() ^
                       TextureCoord1.GetHashCode() ^
                       TextureCoord2.GetHashCode();
            }
        }

        public override string ToString() => 
            $"{{Position:{Position} Normal:{Normal} Color:{Color} TextureCoord0:{TextureCoord0} TextureCoord1:{TextureCoord1} TextureCoord2:{TextureCoord2}}}";

        public static bool operator ==(H3DGPUVertex left, H3DGPUVertex right) =>
            left.Position == right.Position &&
            left.Normal == right.Normal &&
            left.Color == right.Color &&
            left.TextureCoord0 == right.TextureCoord0 &&
            left.TextureCoord1 == right.TextureCoord1 &&
            left.TextureCoord2 == right.TextureCoord2;
        public static bool operator !=(H3DGPUVertex left, H3DGPUVertex right) => !(left == right);

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj.GetType() != GetType())
                return false;
            return this == (H3DGPUVertex)obj;
        }
    }
}