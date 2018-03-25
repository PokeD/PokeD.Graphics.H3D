using System;
using System.ComponentModel;
using System.Reflection;
using System.Text;

using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

using PokeD.Graphics.Content.Pipeline.H3D;

using tainicom.Aether.Content.Pipeline.Graphics;
using tainicom.Aether.Content.Pipeline.Processors;

namespace PokeD.Graphics.Content.Pipeline.Processors
{
    [ContentProcessor(DisplayName = "CPU Animated H3D Model - PokeD.Graphics")]
    public class CPUAnimatedH3DModelProcessor : CPUAnimatedModelProcessor, IContentProcessor
    {
        [DisplayName("MaxBones"), DefaultValue(250)]
        public override int MaxBones { get; set; } = 250;

        [Browsable(false)] // -- Force H3DEffect shader
        public sealed override MaterialProcessorDefaultEffect DefaultEffect { get => base.DefaultEffect; set => base.DefaultEffect = value; }

        object IContentProcessor.Process(object input, ContentProcessorContext context)
        {
            context.Logger.LogMessage("Processing CPU Animated H3D Model");
            try
            {
                // Because why not
                var methodInfo = GetType().BaseType.GetMethod("Microsoft.Xna.Framework.Content.Pipeline.IContentProcessor.Process", BindingFlags.NonPublic | BindingFlags.Instance);
                return methodInfo.Invoke(this, new [] { input, context });
            }
            catch (Exception ex)
            {
                context.Logger.LogMessage("Error {0}", ex);
                throw;
            }
        }

        protected override MaterialContent ConvertMaterial(MaterialContent material, ContentProcessorContext context)
        {
            try
            {
                // -- We don't use Matrix[] in CPU rendering
                if (material is H3DMaterialContent h3dMaterial)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("#define DISABLE_SKINNED 1");
                    sb.Append(h3dMaterial.Effect.EffectCode);
                    h3dMaterial.Effect.EffectCode = sb.ToString();
                }
                
                var parameters = new OpaqueDataDictionary
                {
                    {"ColorKeyColor", ColorKeyColor},
                    {"ColorKeyEnabled", ColorKeyEnabled},
                    {"GenerateMipmaps", GenerateMipmaps},
                    {"PremultiplyTextureAlpha", PremultiplyTextureAlpha},
                    {"ResizeTexturesToPowerOfTwo", ResizeTexturesToPowerOfTwo},
                    {"TextureFormat", TextureFormat},
                    {"DefaultEffect", DefaultEffect}
                };
                return context.Convert<MaterialContent, MaterialContent>(material, "H3DMaterialProcessor", parameters);
            }
            catch (Exception ex)
            {
                context.Logger.LogMessage("Error {0}", ex);
                throw;
            }
        }

        protected override void ProcessVertexBuffer(DynamicModelContent dynamicModel, ContentProcessorContext context, DynamicModelMeshPartContent part)
        {
            if (VertexBufferType != DynamicModelContent.BufferType.Default)
            {
                // Replace the default VertexBufferContent with CpuAnimatedVertexBufferContent.
                if (!VertexBufferCache.TryGetValue(part.VertexBuffer, out var vb))
                {
                    vb = new H3DAnimatedDynamicVertexBufferContent(part.VertexBuffer) { IsWriteOnly = VertexBufferType == DynamicModelContent.BufferType.DynamicWriteOnly };
                    VertexBufferCache[part.VertexBuffer] = vb;
                }
                part.VertexBuffer = vb;
            }
        }
    }
}