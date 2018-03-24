using System;
using System.IO;

using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

using PokeD.Graphics.Content.Pipeline.H3D;

namespace PokeD.Graphics.Content.Pipeline.Processors
{
    [ContentProcessor(DisplayName = "H3D Material - PokeD.Graphics")]
    public class H3DMaterialProcessor : ContentProcessor<H3DMaterialContent, H3DMaterialContent>
    {
        public override H3DMaterialContent Process(H3DMaterialContent input, ContentProcessorContext context)
        {
            var tmpPath = Path.GetTempFileName();
            try
            {
                context.Logger.LogMessage("Processing H3D Material");

                input.Effect.Identity = new ContentIdentity(tmpPath);
                File.WriteAllText(tmpPath, input.Effect.EffectCode);
                var effectProcessor = new EffectProcessor();
                input.EffectCompiled = effectProcessor.Process(input.Effect, context);
                
                // We don't need to do anything - the texture is imported from the binary file
                return input;
            }
            catch (Exception ex)
            {
                context.Logger.LogMessage("Error {0}", ex);
                throw;
            }
            finally { File.Delete(tmpPath); }
        }
    }
}