using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PokeD.Graphics.Extensions
{
    public static class EffectParameterCollectionExtensions
    {
        public static EffectParameter TryGet(this EffectParameterCollection parameters, string name) =>
            parameters.Any(parameter => parameter.Name == name) ? parameters[name] : null;

        public static void TrySet(this EffectParameterCollection parameters, string name, float value)
        {
            if (parameters.Any(parameter => parameter.Name == name))
                parameters[name].SetValue(value);
        }

        public static void TrySet(this EffectParameterCollection parameters, string name, Vector4 value)
        {
            if (parameters.Any(parameter => parameter.Name == name))
                parameters[name].SetValue(value);
        }

        public static void TrySet(this EffectParameterCollection parameters, string name, Vector3 value)
        {
            if (parameters.Any(parameter => parameter.Name == name))
                parameters[name].SetValue(value);
        }

        public static void TrySet(this EffectParameterCollection parameters, string name, Vector2 value)
        {
            if (parameters.Any(parameter => parameter.Name == name))
                parameters[name].SetValue(value);
        }

        public static void TrySet(this EffectParameterCollection parameters, string name, Texture2D value)
        {
            if (parameters.Any(parameter => parameter.Name == name))
                parameters[name].SetValue(value);
        }

        public static void TrySet(this EffectParameterCollection parameters, string name, Matrix value)
        {
            if (parameters.Any(parameter => parameter.Name == name))
                parameters[name].SetValue(value);
        }

        public static void TrySet(this EffectParameterCollection parameters, string name, bool value)
        {
            if (parameters.Any(parameter => parameter.Name == name))
                parameters[name].SetValue(value);
        }
    }
}
