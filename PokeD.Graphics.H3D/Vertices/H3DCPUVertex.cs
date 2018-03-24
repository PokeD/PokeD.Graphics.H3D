using System.Runtime.InteropServices;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace PokeD.Graphics.Vertices
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct H3DCPUVertex : IVertexType
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TextureCoord0;
        public Vector2 TextureCoord1;
        public Vector2 TextureCoord2;
        public Byte4 BlendIndices;
        public Vector4 BlendWeights;

        #region IVertexType Members
        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
            new VertexElement(00, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(32, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1),
            new VertexElement(40, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 2),
            new VertexElement(48, VertexElementFormat.Byte4,   VertexElementUsage.BlendIndices, 0),
            new VertexElement(52, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0));
        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
        #endregion
        
        public H3DCPUVertex(Vector3 position, Vector3 normal, Vector2 textureCoord0, Vector2 textureCoord1, Vector2 textureCoord2, Byte4 blendIndices, Vector4 blendWeights)
        {
            Position = position;
            Normal = normal;
            TextureCoord0 = textureCoord0;
            TextureCoord1 = textureCoord1;
            TextureCoord2 = textureCoord2;
            BlendIndices = blendIndices;
            BlendWeights = blendWeights;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Position.GetHashCode() * 397) ^
                       Normal.GetHashCode() ^
                       TextureCoord0.GetHashCode() ^
                       TextureCoord1.GetHashCode() ^
                       TextureCoord2.GetHashCode() ^
                       BlendIndices.GetHashCode() ^
                       BlendWeights.GetHashCode();
            }
        }

        public override string ToString() => 
            $"{{Position:{Position} Normal:{Normal} TextureCoord0:{TextureCoord0} TextureCoord1:{TextureCoord1} TextureCoord2:{TextureCoord2} BlendIndices:{BlendIndices} BlendWeights:{BlendWeights}}}";

        public static bool operator ==(H3DCPUVertex left, H3DCPUVertex right) => 
            left.Position == right.Position &&
            left.Normal == right.Normal &&
            left.TextureCoord0 == right.TextureCoord0 &&
            left.TextureCoord1 == right.TextureCoord1 &&
            left.TextureCoord2 == right.TextureCoord2 &&
            left.BlendIndices == right.BlendIndices &&
            left.BlendWeights == right.BlendWeights;
        public static bool operator !=(H3DCPUVertex left, H3DCPUVertex right) => !(left == right);

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj.GetType() != GetType())
                return false;
            return this == (H3DCPUVertex) obj;
        }
    }
}