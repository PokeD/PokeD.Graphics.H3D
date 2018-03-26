using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using PokeD.Graphics.Extensions;
using PokeD.Graphics.H3D;

namespace PokeD.Graphics.Effects
{
    public class H3DEffect : Effect, IH3DEffect, IEffectMaterialAnimation
    {
        private static float[] ToArray(Matrix[] array) => array.SelectMany(ToArray).ToArray();
        private static float[] ToArray(Matrix matrix)
        {
            return new []
            {
                /*
                matrix.M11, matrix.M21, matrix.M31, matrix.M41,
                matrix.M12, matrix.M22, matrix.M32, matrix.M42,
                matrix.M13, matrix.M23, matrix.M33, matrix.M43,
                matrix.M14, matrix.M24, matrix.M34, matrix.M44
                */
                matrix.M11, matrix.M12, matrix.M13, //matrix.M14,
                matrix.M21, matrix.M22, matrix.M23, //matrix.M24,
                matrix.M31, matrix.M32, matrix.M33, //matrix.M34,
                //matrix.M41, matrix.M42, matrix.M43, matrix.M44
            };
        }

        #region Parameters
        private EffectParameter _eyePositionParameter;
        private EffectParameter _worldParameter;
        private EffectParameter _worldInverseTransposeParameter;
        private EffectParameter _worldViewProjectionParameter;

        private EffectParameter _textureTransformsParameter;

        private EffectParameter _texture0Parameter;
        private EffectParameter _texture1Parameter;
        private EffectParameter _texture2Parameter;

        private EffectParameter _lut0Parameter;
        private EffectParameter _lut1Parameter;
        private EffectParameter _lut2Parameter;
        private EffectParameter _lut3Parameter;
        private EffectParameter _lut4Parameter;
        private EffectParameter _lut5Parameter;

        private EffectParameter _lightDistanceLUT0Parameter;
        private EffectParameter _lightDistanceLUT1Parameter;
        private EffectParameter _lightDistanceLUT2Parameter;

        private EffectParameter _lightAngleLUT0Parameter;
        private EffectParameter _lightAngleLUT1Parameter;
        private EffectParameter _lightAngleLUT2Parameter;

        private EffectParameter _textureCube0Parameter;

        private EffectParameter _emissionColorParameter;
        private EffectParameter _ambientColorParameter;
        private EffectParameter _diffuseColorParameter;
        private EffectParameter _specular0ColorParameter;
        private EffectParameter _specular1ColorParameter;
        private EffectParameter _constant0ColorParameter;
        private EffectParameter _constant1ColorParameter;
        private EffectParameter _constant2ColorParameter;
        private EffectParameter _constant3ColorParameter;
        private EffectParameter _constant4ColorParameter;
        private EffectParameter _constant5ColorParameter;
        #endregion Parameters

        #region Properties

        public string Material { get; set; }

        public Matrix World
        {
            get => _world;
            set
            {
                _world = value;
                DirtyFlags |= H3DEffectDirtyFlags.World | H3DEffectDirtyFlags.WorldViewProj;
            }
        }
        private Matrix _world = Matrix.Identity;

        public Matrix View
        {
            get => _view;
            set
            {
                _view = value;
                DirtyFlags |= H3DEffectDirtyFlags.WorldViewProj | H3DEffectDirtyFlags.EyePosition;
            }
        }
        private Matrix _view = Matrix.Identity;

        public Matrix Projection
        {
            get => _projection;
            set
            {
                _projection = value;
                DirtyFlags |= H3DEffectDirtyFlags.WorldViewProj;
            }
        }
        private Matrix _projection = Matrix.Identity;

        private Matrix _worldView;

        public Matrix[] TextureTransforms { set => _textureTransformsParameter?.SetValue((value)); }

        public Texture2D Texture0 { get => _texture0Parameter?.GetValueTexture2D(); set => _texture0Parameter?.SetValue(value); }
        public Texture2D Texture1 { get => _texture1Parameter?.GetValueTexture2D(); set => _texture1Parameter?.SetValue(value); }
        public Texture2D Texture2 { get => _texture2Parameter?.GetValueTexture2D(); set => _texture2Parameter?.SetValue(value); }

        public Texture2D LUT0 { get => _lut0Parameter?.GetValueTexture2D(); set => _lut0Parameter?.SetValue(value); }
        public Texture2D LUT1 { get => _lut1Parameter?.GetValueTexture2D(); set => _lut1Parameter?.SetValue(value); }
        public Texture2D LUT2 { get => _lut2Parameter?.GetValueTexture2D(); set => _lut2Parameter?.SetValue(value); }
        public Texture2D LUT3 { get => _lut3Parameter?.GetValueTexture2D(); set => _lut3Parameter?.SetValue(value); }
        public Texture2D LUT4 { get => _lut4Parameter?.GetValueTexture2D(); set => _lut4Parameter?.SetValue(value); }
        public Texture2D LUT5 { get => _lut5Parameter?.GetValueTexture2D(); set => _lut5Parameter?.SetValue(value); }

        public Texture2D LightDistanceLUT0 { get => _lightDistanceLUT0Parameter?.GetValueTexture2D(); set => _lightDistanceLUT0Parameter?.SetValue(value); }
        public Texture2D LightDistanceLUT1 { get => _lightDistanceLUT1Parameter?.GetValueTexture2D(); set => _lightDistanceLUT1Parameter?.SetValue(value); }
        public Texture2D LightDistanceLUT2 { get => _lightDistanceLUT2Parameter?.GetValueTexture2D(); set => _lightDistanceLUT2Parameter?.SetValue(value); }

        public Texture2D LightAngleLUT0 { get => _lightAngleLUT0Parameter?.GetValueTexture2D(); set => _lightAngleLUT0Parameter?.SetValue(value); }
        public Texture2D LightAngleLUT1 { get => _lightAngleLUT1Parameter?.GetValueTexture2D(); set => _lightAngleLUT1Parameter?.SetValue(value); }
        public Texture2D LightAngleLUT2 { get => _lightAngleLUT2Parameter?.GetValueTexture2D(); set => _lightAngleLUT2Parameter?.SetValue(value); }

        public Texture2D TextureCube0 { get => _textureCube0Parameter?.GetValueTexture2D(); set => _textureCube0Parameter?.SetValue(value); }

        public Vector4? EmissionColor { get => _emissionColorParameter?.GetValueVector4(); set => _emissionColorParameter?.SetValue(value.GetValueOrDefault()); }
        public Vector4? AmbientColor { get => _ambientColorParameter?.GetValueVector4(); set => _ambientColorParameter?.SetValue(value.GetValueOrDefault()); }
        public Vector4? DiffuseColor { get => _diffuseColorParameter?.GetValueVector4(); set => _diffuseColorParameter?.SetValue(value.GetValueOrDefault()); }
        public Vector4? Specular0Color { get => _specular0ColorParameter?.GetValueVector4(); set => _specular0ColorParameter?.SetValue(value.GetValueOrDefault()); }
        public Vector4? Specular1Color { get => _specular1ColorParameter?.GetValueVector4(); set => _specular1ColorParameter?.SetValue(value.GetValueOrDefault()); }
        public Vector4? Constant0Color { get => _constant0ColorParameter?.GetValueVector4(); set => _constant0ColorParameter?.SetValue(value.GetValueOrDefault()); }
        public Vector4? Constant1Color { get => _constant1ColorParameter?.GetValueVector4(); set => _constant1ColorParameter?.SetValue(value.GetValueOrDefault()); }
        public Vector4? Constant2Color { get => _constant2ColorParameter?.GetValueVector4(); set => _constant2ColorParameter?.SetValue(value.GetValueOrDefault()); }
        public Vector4? Constant3Color { get => _constant3ColorParameter?.GetValueVector4(); set => _constant3ColorParameter?.SetValue(value.GetValueOrDefault()); }
        public Vector4? Constant4Color { get => _constant4ColorParameter?.GetValueVector4(); set => _constant4ColorParameter?.SetValue(value.GetValueOrDefault()); }
        public Vector4? Constant5Color { get => _constant5ColorParameter?.GetValueVector4(); set => _constant5ColorParameter?.SetValue(value.GetValueOrDefault()); }

        public Color BlendColor { get; set; }

        public TextureSamplerSettings[] TextureSamplerSettings { get; set; }

        public CullMode CullMode { get; set; }

        public bool DepthBufferRead { get; set; }
        public bool DepthBufferWrite { get; set; }

        public bool StencilBufferRead { get; set; }
        public bool StencilBufferWrite { get; set; }

        public bool UseCustom
        {
            get => _useCustom;
            set
            {
                _useCustom = value;
                DirtyFlags |= H3DEffectDirtyFlags.ShaderIndex;
            }
        }
        private bool _useCustom = false;

        protected H3DEffectDirtyFlags DirtyFlags = H3DEffectDirtyFlags.ShaderIndex;
        
        #endregion

        public H3DEffect(Effect cloneSource) : base(cloneSource) => CacheEffectParameters(cloneSource);
        public H3DEffect(GraphicsDevice graphicsDevice, byte[] bytecode) : base(graphicsDevice, bytecode) => CacheEffectParameters(this);
        protected virtual void CacheEffectParameters(Effect effect)
        {
            _eyePositionParameter = effect.Parameters["EyePosition"];
            _worldParameter = effect.Parameters["World"];
            _worldInverseTransposeParameter = effect.Parameters["WorldInverseTranspose"];
            _worldViewProjectionParameter = effect.Parameters["WorldViewProjection"];

            _textureTransformsParameter = effect.Parameters["TextureTransforms"];

            _texture0Parameter = effect.Parameters.TryGet("Texture0");
            _texture1Parameter = effect.Parameters.TryGet("Texture1");
            _texture2Parameter = effect.Parameters.TryGet("Texture2");

            _lut0Parameter = effect.Parameters.TryGet("LUT0");
            _lut1Parameter = effect.Parameters.TryGet("LUT1");
            _lut2Parameter = effect.Parameters.TryGet("LUT2");
            _lut3Parameter = effect.Parameters.TryGet("LUT3");
            _lut4Parameter = effect.Parameters.TryGet("LUT4");
            _lut5Parameter = effect.Parameters.TryGet("LUT5");

            _lightDistanceLUT0Parameter = effect.Parameters.TryGet("LightDistanceLUT0");
            _lightDistanceLUT1Parameter = effect.Parameters.TryGet("LightDistanceLUT1");
            _lightDistanceLUT2Parameter = effect.Parameters.TryGet("LightDistanceLUT2");

            _lightAngleLUT0Parameter = effect.Parameters.TryGet("LightAngleLUT0");
            _lightAngleLUT1Parameter = effect.Parameters.TryGet("LightAngleLUT1");
            _lightAngleLUT2Parameter = effect.Parameters.TryGet("LightAngleLUT2");

            _textureCube0Parameter = effect.Parameters.TryGet("TextureCube0");

            _emissionColorParameter = effect.Parameters.TryGet("EmissionColor");
            _ambientColorParameter = effect.Parameters.TryGet("AmbientColor");
            _diffuseColorParameter = effect.Parameters.TryGet("DiffuseColor");
            _specular0ColorParameter = effect.Parameters.TryGet("Specular0Color");
            _specular1ColorParameter = effect.Parameters.TryGet("Specular1Color");
            _constant0ColorParameter = effect.Parameters.TryGet("Constant0Color");
            _constant1ColorParameter = effect.Parameters.TryGet("Constant1Color");
            _constant2ColorParameter = effect.Parameters.TryGet("Constant2Color");
            _constant3ColorParameter = effect.Parameters.TryGet("Constant3Color");
            _constant4ColorParameter = effect.Parameters.TryGet("Constant4Color");
            _constant5ColorParameter = effect.Parameters.TryGet("Constant5Color");


            TextureTransforms = new[] { Matrix.Identity, Matrix.Identity, Matrix.Identity };
        }

        /// <summary>
        /// Note that it starts from 1, not 0.
        /// </summary>
        /// <param name="shaderIndex"></param>
        protected virtual void OnShaderIndex(ref int shaderIndex)
        {
            if (UseCustom)
                shaderIndex *= 2;
        }

        protected override void OnApply()
        {
            DirtyFlags = SetWorldViewProj(DirtyFlags, ref _world, ref _view, ref _projection, ref _worldView, _worldViewProjectionParameter);

            DirtyFlags = SetLightingMatrices(DirtyFlags, ref _world, ref _view, _worldParameter, _worldInverseTransposeParameter, _eyePositionParameter);

            if ((DirtyFlags & H3DEffectDirtyFlags.ShaderIndex) != 0)
            {
                var shaderIndex = 1;
                OnShaderIndex(ref shaderIndex);
                CurrentTechnique = Techniques[shaderIndex - 1];
                DirtyFlags &= ~H3DEffectDirtyFlags.ShaderIndex;
            }

            base.OnApply();
        }

        public override Effect Clone() => new H3DEffect(this);

        /// <summary>
        /// Lazily recomputes the world+view+projection matrix and
        /// fog vector based on the current effect parameter settings.
        /// </summary>
        protected internal static H3DEffectDirtyFlags SetWorldViewProj(H3DEffectDirtyFlags dirtyFlags, ref Matrix world, ref Matrix view, ref Matrix projection, ref Matrix worldView, EffectParameter worldViewProjParam)
        {
            // Recompute the world+view+projection matrix?
            if ((dirtyFlags & H3DEffectDirtyFlags.WorldViewProj) != 0)
            {
                Matrix.Multiply(ref world, ref view, out worldView);
                Matrix.Multiply(ref worldView, ref projection, out var worldViewProj);

                worldViewProjParam.SetValue(worldViewProj);

                dirtyFlags &= ~H3DEffectDirtyFlags.WorldViewProj;
            }

            return dirtyFlags;
        }

        /// <summary>
        /// Lazily recomputes the world inverse transpose matrix and
        /// eye position based on the current effect parameter settings.
        /// </summary>
        protected internal static H3DEffectDirtyFlags SetLightingMatrices(H3DEffectDirtyFlags dirtyFlags, ref Matrix world, ref Matrix view, EffectParameter worldParam, EffectParameter worldInverseTransposeParam, EffectParameter eyePositionParam)
        {
            // Set the world and world inverse transpose matrices.
            if ((dirtyFlags & H3DEffectDirtyFlags.World) != 0)
            {
                Matrix.Invert(ref world, out var worldTranspose);
                Matrix.Transpose(ref worldTranspose, out var worldInverseTranspose);

                worldParam?.SetValue(world);
                worldInverseTransposeParam?.SetValue(worldInverseTranspose);

                dirtyFlags &= ~H3DEffectDirtyFlags.World;
            }

            // Set the eye position.
            if ((dirtyFlags & H3DEffectDirtyFlags.EyePosition) != 0)
            {
                Matrix.Invert(ref view, out var viewInverse);

                eyePositionParam?.SetValue(viewInverse.Translation);

                dirtyFlags &= ~H3DEffectDirtyFlags.EyePosition;
            }

            return dirtyFlags;
        }
    }
}