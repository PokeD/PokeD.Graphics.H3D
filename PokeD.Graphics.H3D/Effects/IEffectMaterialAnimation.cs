using Microsoft.Xna.Framework;

namespace PokeD.Graphics.Effects
{
    public interface IEffectMaterialAnimation
    {
        string Material { get; }

        Matrix[] TextureTransforms { set; }
    }
}