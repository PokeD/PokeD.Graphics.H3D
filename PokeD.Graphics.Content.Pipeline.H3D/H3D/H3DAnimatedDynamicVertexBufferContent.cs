using Microsoft.Xna.Framework.Content.Pipeline.Processors;

using tainicom.Aether.Content.Pipeline.Graphics;

namespace PokeD.Graphics.Content.Pipeline.H3D
{
    public class H3DAnimatedDynamicVertexBufferContent : DynamicVertexBufferContent
    {
        public H3DAnimatedDynamicVertexBufferContent(VertexBufferContent source, int size = 0) : base(source, size) { }
    }
}