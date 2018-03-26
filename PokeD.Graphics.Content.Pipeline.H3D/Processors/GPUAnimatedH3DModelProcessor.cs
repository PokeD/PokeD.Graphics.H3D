using System;
using System.ComponentModel;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

using tainicom.Aether.Content.Pipeline.Processors;

namespace PokeD.Graphics.Content.Pipeline.Processors
{
    [ContentProcessor(DisplayName = "GPU Animated H3D Model - PokeD.Graphics")]
    public class GPUAnimatedH3DModelProcessor : GPUAnimatedModelProcessor
    {
        [DisplayName("MaxBones"), DefaultValue(250)]
        public override int MaxBones { get; set; } = 250;

        [Browsable(false)] // Force H3DSkinnedEffect shader
        public sealed override MaterialProcessorDefaultEffect DefaultEffect { get => base.DefaultEffect; set => base.DefaultEffect = value; }

        public override ModelContent Process(NodeContent input, ContentProcessorContext context)
        {
            //if (context.TargetPlatform == TargetPlatform.DesktopGL)
            //    throw new PlatformNotSupportedException("GPU Animation not supported on DesktopGL!");

            context.Logger.LogMessage("Processing GPU Animated H3D Model");
            try
            {
                if (context.TargetPlatform == TargetPlatform.Windows) // DirectX requires to have all vertices
                    FixDirectXGeometry(input);

                return base.Process(input, context);
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

        protected internal static void FixDirectXGeometry(NodeContent nodeContent)
        {
            foreach (var childNode in nodeContent.Children)
            {
                if (childNode is MeshContent meshContent)
                {
                    foreach (var geometry in meshContent.Geometry)
                    {
                        var verticesCount = meshContent.Positions.Count;

                        if (!geometry.Vertices.Channels.Contains(VertexChannelNames.Color(0)))
                            geometry.Vertices.Channels.Add(VertexChannelNames.Color(0), Enumerable.Repeat(Color.White, verticesCount));

                        if (!geometry.Vertices.Channels.Contains(VertexChannelNames.TextureCoordinate(1)))
                            geometry.Vertices.Channels.Add(VertexChannelNames.TextureCoordinate(1), Enumerable.Repeat(Vector2.Zero, verticesCount));

                        if (!geometry.Vertices.Channels.Contains(VertexChannelNames.TextureCoordinate(2)))
                            geometry.Vertices.Channels.Add(VertexChannelNames.TextureCoordinate(2), Enumerable.Repeat(Vector2.Zero, verticesCount));
                    }
                }
            }
        }
    }
}