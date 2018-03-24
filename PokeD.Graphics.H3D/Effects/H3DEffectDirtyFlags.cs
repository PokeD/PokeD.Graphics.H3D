using System;

namespace PokeD.Graphics.Effects
{
    [Flags]
    public enum H3DEffectDirtyFlags
    {
        World = 1,
        WorldViewProj = 2,
        EyePosition = 4,
        ShaderIndex = 8,
    }
}