using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PokeD.Graphics.Effects
{
    public class H3DSkinningEffect : H3DEffect, IEffectSkeletalAnimation
    {
        private EffectParameter _bonesParameter;

        public Matrix[] Bones { set => _bonesParameter?.SetValue(value); }

        /// <summary>
        /// Gets or sets the number of skinning weights to evaluate for each vertex (1, 2, or 4).
        /// </summary>
        public int WeightsPerVertex
        {
            get => _weightsPerVertex;

            set
            {
                if (value != 0 && value != 1 && value != 2 && value != 4) // 0 - Disabled
                    throw new ArgumentOutOfRangeException(nameof(value));

                _weightsPerVertex = value;
                DirtyFlags |= H3DEffectDirtyFlags.ShaderIndex;
            }
        }
        private int _weightsPerVertex = 4;

        public H3DSkinningEffect(Effect cloneSource) : base(cloneSource) => CacheEffectParameters(cloneSource);
        public H3DSkinningEffect(GraphicsDevice graphicsDevice, byte[] bytecode) : base(graphicsDevice, bytecode) => CacheEffectParameters(this);
        protected override void CacheEffectParameters(Effect effect)
        {
            base.CacheEffectParameters(effect);

            _bonesParameter = effect.Parameters["Bones"];
        }

        protected override void OnShaderIndex(ref int shaderIndex)
        {
            base.OnShaderIndex(ref shaderIndex);

            shaderIndex = 2;
            if (WeightsPerVertex == 1) shaderIndex += 1;
            if (WeightsPerVertex == 2) shaderIndex += 2;
            if (WeightsPerVertex == 4) shaderIndex += 3;
            if (UseCustom) shaderIndex += 3;
        }

        public override Effect Clone() => new H3DSkinningEffect(this);
    }
}