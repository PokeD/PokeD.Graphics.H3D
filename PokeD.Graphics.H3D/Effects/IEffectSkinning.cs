using Microsoft.Xna.Framework;

namespace PokeD.Graphics.Effects
{
    public interface IEffectSkeletalAnimation
    {
        Matrix[] Bones { set; }

        int WeightsPerVertex { get; set; }
    }
}