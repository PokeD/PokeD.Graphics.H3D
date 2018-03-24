using System;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

using PokeD.Graphics.H3D;
using PokeD.Graphics.Vertices;

namespace PokeD.Graphics.Content.ContentReaders
{
    public class H3DAnimatedDynamicVertexBufferReader : ContentTypeReader<H3DAnimatedDynamicVertexBuffer>
    {
        protected override H3DAnimatedDynamicVertexBuffer Read(ContentReader input, H3DAnimatedDynamicVertexBuffer buffer)
        {
            var graphicsDeviceService = (IGraphicsDeviceService)input.ContentManager.ServiceProvider.GetService(typeof(IGraphicsDeviceService));
            var device = graphicsDeviceService.GraphicsDevice;

            // read standard VertexBuffer
            var declaration = input.ReadRawObject<VertexDeclaration>();
            var vertexCount = (int) input.ReadUInt32();

            //read data                      
            var channels = declaration.GetVertexElements();
            var cpuVertices = new H3DCPUVertex[vertexCount];
            var gpuVertices = new H3DGPUVertex[vertexCount];

            for (var i = 0; i < vertexCount; i++)
            {
                foreach (var channel in channels)
                {
                    switch (channel.VertexElementUsage)
                    {
                        case VertexElementUsage.Position:
                            System.Diagnostics.Debug.Assert(channel.VertexElementFormat == VertexElementFormat.Vector3);
                            var pos = input.ReadVector3();
                            cpuVertices[i].Position = pos;
                            gpuVertices[i].Position = pos;
                            break;

                        case VertexElementUsage.Normal:
                            System.Diagnostics.Debug.Assert(channel.VertexElementFormat == VertexElementFormat.Vector3);
                            var nor = input.ReadVector3();
                            cpuVertices[i].Normal = nor;
                            gpuVertices[i].Normal = nor;
                            break;

                        case VertexElementUsage.Color:
                            System.Diagnostics.Debug.Assert(channel.VertexElementFormat == VertexElementFormat.Color);
                            var col = input.ReadColor();
                            gpuVertices[i].Color = col;
                            break;

                        case VertexElementUsage.TextureCoordinate:
                            System.Diagnostics.Debug.Assert(channel.VertexElementFormat == VertexElementFormat.Vector2);
                            var tex = input.ReadVector2();
                            switch (channel.UsageIndex)
                            {
                                case 0:
                                    cpuVertices[i].TextureCoord0 = tex;
                                    gpuVertices[i].TextureCoord0 = tex;
                                    break;
                                case 1:
                                    cpuVertices[i].TextureCoord1 = tex;
                                    gpuVertices[i].TextureCoord1 = tex;
                                    break;
                                case 2:
                                    cpuVertices[i].TextureCoord2 = tex;
                                    gpuVertices[i].TextureCoord2 = tex;
                                    break;
                            }
                            break;

                        case VertexElementUsage.BlendWeight:
                            System.Diagnostics.Debug.Assert(channel.VertexElementFormat == VertexElementFormat.Vector4);
                            var wei = input.ReadVector4();
                            cpuVertices[i].BlendWeights = wei;
                            break;

                        case VertexElementUsage.BlendIndices:
                            System.Diagnostics.Debug.Assert(channel.VertexElementFormat == VertexElementFormat.Byte4);
                            var ind = new Byte4(input.ReadByte(), input.ReadByte(), input.ReadByte(), input.ReadByte());
                            cpuVertices[i].BlendIndices = ind;
                            break;

                        default:
                            throw new Exception();
                    }
                }
            }

            // read extras
            var isWriteOnly = input.ReadBoolean();

            if (buffer == null)
            {
                var usage = isWriteOnly ? BufferUsage.WriteOnly : BufferUsage.None;
                buffer = new H3DAnimatedDynamicVertexBuffer(device, H3DGPUVertex.VertexDeclaration, vertexCount, usage);
            }

            buffer.SetData(gpuVertices, 0, vertexCount);
            buffer.SetGPUVertices(gpuVertices);
            buffer.SetCPUVertices(cpuVertices);

            return buffer;
        }
    }
}