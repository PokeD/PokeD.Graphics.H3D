using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace PokeD.Graphics.Content.Pipeline.H3D
{
    public class H3DMaterialContent : MaterialContent
    {
        public const string EffectKey = "Effect";
        public const string EffectCompiledKey = "EffectCompiled";
        public const string MaterialKey = "Material";
        public const string TexturesKey = "TextureList";
        public const string TextureSamplerSettingsKey = "TextureSamplerSettings";
        public const string FaceCullingKey = "FaceCulling";
        public const string EmissionColorKey = "EmissionColor";
        public const string AmbientColorKey = "AmbientColor";
        public const string DiffuseColorKey = "DiffuseColor";
        public const string Specular0ColorKey = "Specular0Color";
        public const string Specular1ColorKey = "Specular1Color";
        public const string Constant0ColorKey = "Constant0Color";
        public const string Constant1ColorKey = "Constant1Color";
        public const string Constant2ColorKey = "Constant2Color";
        public const string Constant3ColorKey = "Constant3Color";
        public const string Constant4ColorKey = "Constant4Color";
        public const string Constant5ColorKey = "Constant5Color";
        public const string BlendColorKey = "BlendColor";
        public const string DepthBufferReadKey = "DepthBufferRead";
        public const string DepthBufferWriteKey = "DepthBufferWrite";
        public const string StencilBufferReadKey = "StencilBufferRead";
        public const string StencilBufferWriteKey = "StencilBufferWrite";

        public EffectContent Effect { get => GetReferenceTypeProperty<EffectContent>(EffectKey); set => SetProperty(EffectKey, value); }
        public CompiledEffectContent EffectCompiled { get => GetReferenceTypeProperty<CompiledEffectContent>(EffectCompiledKey); set => SetProperty(EffectCompiledKey, value); }

        public string Material { get => GetReferenceTypeProperty<string>(MaterialKey); set => SetProperty(MaterialKey, value); }

        public Texture2DContent[] TextureList { get => GetReferenceTypeProperty<Texture2DContent[]>(TexturesKey); set => SetProperty(TexturesKey, value); }
        public TextureSamplerSettings[] TextureSamplerSettings { get => GetReferenceTypeProperty<TextureSamplerSettings[]>(TextureSamplerSettingsKey); set => SetProperty(TextureSamplerSettingsKey, value); }

        public H3DFaceCulling? FaceCulling { get => GetValueTypeProperty<H3DFaceCulling>(FaceCullingKey); set => SetProperty(FaceCullingKey, value); }

        public Color? EmissionColor { get => GetValueTypeProperty<Color>(EmissionColorKey); set => SetProperty(EmissionColorKey, value); }
        public Color? AmbientColor { get => GetValueTypeProperty<Color>(AmbientColorKey); set => SetProperty(AmbientColorKey, value); }
        public Color? DiffuseColor { get => GetValueTypeProperty<Color>(DiffuseColorKey); set => SetProperty(DiffuseColorKey, value); }
        public Color? Specular0Color { get => GetValueTypeProperty<Color>(Specular0ColorKey); set => SetProperty(Specular0ColorKey, value); }
        public Color? Specular1Color { get => GetValueTypeProperty<Color>(Specular1ColorKey); set => SetProperty(Specular1ColorKey, value); }
        public Color? Constant0Color { get => GetValueTypeProperty<Color>(Constant0ColorKey); set => SetProperty(Constant0ColorKey, value); }
        public Color? Constant1Color { get => GetValueTypeProperty<Color>(Constant1ColorKey); set => SetProperty(Constant1ColorKey, value); }
        public Color? Constant2Color { get => GetValueTypeProperty<Color>(Constant2ColorKey); set => SetProperty(Constant2ColorKey, value); }
        public Color? Constant3Color { get => GetValueTypeProperty<Color>(Constant3ColorKey); set => SetProperty(Constant3ColorKey, value); }
        public Color? Constant4Color { get => GetValueTypeProperty<Color>(Constant4ColorKey); set => SetProperty(Constant4ColorKey, value); }
        public Color? Constant5Color { get => GetValueTypeProperty<Color>(Constant5ColorKey); set => SetProperty(Constant5ColorKey, value); }
        public Color? BlendColor { get => GetValueTypeProperty<Color>(BlendColorKey); set => SetProperty(BlendColorKey, value); }

        public bool? DepthBufferRead { get => GetValueTypeProperty<bool>(DepthBufferReadKey); set => SetProperty(DepthBufferReadKey, value); }
        public bool? DepthBufferWrite { get => GetValueTypeProperty<bool>(DepthBufferWriteKey); set => SetProperty(DepthBufferWriteKey, value); }

        public bool? StencilBufferRead { get => GetValueTypeProperty<bool>(StencilBufferReadKey); set => SetProperty(StencilBufferReadKey, value); }
        public bool? StencilBufferWrite { get => GetValueTypeProperty<bool>(StencilBufferWriteKey); set => SetProperty(StencilBufferWriteKey, value); }

        public bool IsSkinned => !Effect.EffectCode.Contains("DISABLE_SKINNED 1");
    }
}