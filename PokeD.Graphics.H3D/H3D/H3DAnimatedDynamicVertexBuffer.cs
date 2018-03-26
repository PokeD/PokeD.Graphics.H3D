using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using PokeD.Graphics.Vertices;

using tainicom.Aether.Animation;

namespace PokeD.Graphics.H3D
{
    public class H3DAnimatedDynamicVertexBuffer : AnimatedDynamicVertexBuffer<H3DCPUVertex, H3DGPUVertex>
    {
        public H3DAnimatedDynamicVertexBuffer(GraphicsDevice graphicsDevice, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage bufferUsage) :
            base(graphicsDevice, vertexDeclaration, vertexCount, bufferUsage) { }

        public override void UpdateVertices(int startIndex, int elementCount, Matrix[] boneTransforms = null, Matrix[] materialTransform = null)
        {
            var transformSum = Matrix.Identity;
            for (var i = startIndex; i < startIndex + elementCount; i++)
            {
                if (boneTransforms != null)
                {
                    var w1 = CPUVertices[i].BlendWeights.X;
                    var w2 = CPUVertices[i].BlendWeights.Y;
                    var w3 = CPUVertices[i].BlendWeights.Z;
                    var w4 = CPUVertices[i].BlendWeights.W;

                    var indices = CPUVertices[i].BlendIndices.ToVector4();

                    var m1 = boneTransforms[(int) indices.X];
                    var m2 = boneTransforms[(int) indices.Y];
                    var m3 = boneTransforms[(int) indices.Z];
                    var m4 = boneTransforms[(int) indices.W];

                    transformSum.M11 = m1.M11 * w1 + m2.M11 * w2 + m3.M11 * w3 + m4.M11 * w4;
                    transformSum.M12 = m1.M12 * w1 + m2.M12 * w2 + m3.M12 * w3 + m4.M12 * w4;
                    transformSum.M13 = m1.M13 * w1 + m2.M13 * w2 + m3.M13 * w3 + m4.M13 * w4;

                    transformSum.M21 = m1.M21 * w1 + m2.M21 * w2 + m3.M21 * w3 + m4.M21 * w4;
                    transformSum.M22 = m1.M22 * w1 + m2.M22 * w2 + m3.M22 * w3 + m4.M22 * w4;
                    transformSum.M23 = m1.M23 * w1 + m2.M23 * w2 + m3.M23 * w3 + m4.M23 * w4;

                    transformSum.M31 = m1.M31 * w1 + m2.M31 * w2 + m3.M31 * w3 + m4.M31 * w4;
                    transformSum.M32 = m1.M32 * w1 + m2.M32 * w2 + m3.M32 * w3 + m4.M32 * w4;
                    transformSum.M33 = m1.M33 * w1 + m2.M33 * w2 + m3.M33 * w3 + m4.M33 * w4;

                    transformSum.M41 = m1.M41 * w1 + m2.M41 * w2 + m3.M41 * w3 + m4.M41 * w4;
                    transformSum.M42 = m1.M42 * w1 + m2.M42 * w2 + m3.M42 * w3 + m4.M42 * w4;
                    transformSum.M43 = m1.M43 * w1 + m2.M43 * w2 + m3.M43 * w3 + m4.M43 * w4;

                    // Support the 4 Bone Influences - Position then Normal
                    Vector3.Transform(ref CPUVertices[i].Position, ref transformSum, out GPUVertices[i].Position);
                    Vector3.TransformNormal(ref CPUVertices[i].Normal, ref transformSum, out GPUVertices[i].Normal);
                }

                if (materialTransform != null)
                {
                    Vector2.Transform(ref CPUVertices[i].TextureCoord0, ref materialTransform[0], out GPUVertices[i].TextureCoord0);
                    Vector2.Transform(ref CPUVertices[i].TextureCoord1, ref materialTransform[1], out GPUVertices[i].TextureCoord1);
                    Vector2.Transform(ref CPUVertices[i].TextureCoord2, ref materialTransform[2], out GPUVertices[i].TextureCoord2);
                }
            }

            // put the vertices into our vertex buffer
            SetData(GPUVertices, 0, VertexCount, SetDataOptions.NoOverwrite);
        }
    }
}