﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PokeD.Graphics.Content.Pipeline.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("PokeD.Graphics.Content.Pipeline.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to //-----------------------------------------------------------------------------
        ///// Macros.fxh
        /////
        ///// Microsoft XNA Community Game Platform
        ///// Copyright (C) Microsoft Corporation. All rights reserved.
        /////-----------------------------------------------------------------------------
        ///
        ///#ifdef SM4
        ///
        ///// Macros for targetting shader model 4.0 (DX11)
        ///
        ///#define TECHNIQUE(name, vsname, psname) \
        ///	technique name { pass { VertexShader = compile vs_4_0 vsname(); PixelShader = compile ps_4_0 psname(); } }
        ///
        ///#defi [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Macros {
            get {
                return ResourceManager.GetString("Macros", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to float4 PSBasic(PSInput input) : SV_TARGET0
        ///{
        ///	float4 output;
        ///
        ///	float4 tex0 	= SAMPLE_TEXTURE(Texture0, input.TexCoord0);
        ///	float4 tex1 	= SAMPLE_TEXTURE(Texture1, input.TexCoord1);
        ///	float4 tex2 	= SAMPLE_TEXTURE(Texture2, input.TexCoord2);
        ///	
        ///	float3 color0	= tex0.rgb * Constant0Color.rgb;
        ///	float3 color1	= tex1.rgb * Constant1Color.rgb;
        ///	float3 color2	= tex2.rgb * Constant2Color.rgb;
        ///	
        ///	output			= float4( ((color0 + color1 + color2) * Constant5Color.rgb), tex0.a );
        ///	AlphaTest(output);
        ///	return ou [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string PixelShader {
            get {
                return ResourceManager.GetString("PixelShader", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #ifndef SET_CPU_RENDER
        ///	#define SET_CPU_RENDER 0
        ///#endif
        ///
        ///DECLARE_TEXTURE(Texture0, 0);
        ///DECLARE_TEXTURE(Texture1, 1);
        ///DECLARE_TEXTURE(Texture2, 2);
        ///
        ///DECLARE_TEXTURE(LUT0, 3);
        ///DECLARE_TEXTURE(LUT1, 4);
        ///DECLARE_TEXTURE(LUT2, 5);
        ///DECLARE_TEXTURE(LUT3, 6);
        ///DECLARE_TEXTURE(LUT4, 7);
        ///DECLARE_TEXTURE(LUT5, 8);
        ///DECLARE_TEXTURE(LUT6, 9);
        ///
        ///DECLARE_TEXTURE(LightDistanceLUT0, 10);
        ///DECLARE_TEXTURE(LightDistanceLUT1, 11);
        ///DECLARE_TEXTURE(LightDistanceLUT2, 12);
        ///
        ///DECLARE_TEXTURE(LightAngleLUT0, 13);
        ///DE [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string VertexShaderBase {
            get {
                return ResourceManager.GetString("VertexShaderBase", resourceCulture);
            }
        }
    }
}