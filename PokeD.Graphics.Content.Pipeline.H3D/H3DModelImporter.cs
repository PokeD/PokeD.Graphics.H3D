using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

using PokeD.Graphics.Content.Pipeline.Extensions;
using PokeD.Graphics.Content.Pipeline.H3D;
using PokeD.Graphics.Content.Pipeline.MaterialAnimation;

using SPICA.Formats.CtrH3D;
using SPICA.Formats.CtrH3D.Animation;
using SPICA.Formats.CtrH3D.Model;
using SPICA.Formats.CtrH3D.Model.Material;
using SPICA.Formats.CtrH3D.Model.Mesh;
using SPICA.PICA.Commands;
using SPICA.PICA.Converters;
using SPICA.WinForms.Formats;

namespace PokeD.Graphics.Content.Pipeline
{
    [ContentImporter(".bin",
                    ".bch", ".mbn",
                    ".tex",
                    ".mod",
                    ".mfx",
                    ".cgfx",
                    ".smd",
                    ".obj",
                DisplayName = "H3D Model Importer - PokeD.Graphics", DefaultProcessor = "GPUAnimatedH3DModelProcessor")]
    public class H3DModelImporter : ContentImporter<NodeContent>
    {
        private ContentIdentity _identity;

        private NodeContent _rootNode;

        public override NodeContent Import(string filename, ContentImporterContext context)
        {
            context.Logger.LogMessage("Importing H3D file: {0}", filename);

            _identity = new ContentIdentity(filename, GetType().Name);
            _rootNode = new NodeContent() { Identity = _identity, Name = "RootNode" };

            var scene = FormatIdentifier.IdentifyAndOpen(filename);
            var model = scene.Models[0];
            
            if (!scene.Textures.Any())
            {
                var path = Path.Combine(Path.GetDirectoryName(filename), $"{Path.GetFileNameWithoutExtension(filename)}@Textures{Path.GetExtension(filename)}");
                if (File.Exists(path))
                {
                    context.Logger.LogMessage($"Found texture file {path}. Loading data...");
                    scene.Merge(FormatIdentifier.IdentifyAndOpen(path, model.Skeleton));
                }
                else
                    context.Logger.LogMessage($"Couldn't find texture file {path}!");
            }

            // Textures
            var textures = new Dictionary<string, Texture2DContent>();
            foreach (var texture in scene.Textures)
            {
                var bitmapContent = new PixelBitmapContent<Color>(texture.Width, texture.Height)
                {
                    Identity = _identity,
                    Name = texture.Name
                };
                bitmapContent.SetPixelData(texture.ToRGBA());

                var textureContent = new Texture2DContent()
                {
                    Identity = _identity,
                    Name = texture.Name
                };
                textureContent.Faces[0].Add(bitmapContent);
                textures.Add(textureContent.Name, textureContent);
            }

            // Materials
            var materials = new Dictionary<string, H3DMaterialContent>();
            foreach (var material in model.Materials)
            {
#if DEBUG
                var hlslCode = new HLSLShaderGenerator(material.MaterialParams) { BoneCount = model.Skeleton.Count }.GetShader();
                var glslCode = new GLSLFragmentShaderGenerator(material.MaterialParams).GetFragShader();
#endif
                var materialContent = new H3DMaterialContent()
                {
                    Identity = _identity,
                    Name = material.Name,

                    Effect = new EffectContent
                    {
                        Identity = _identity,
                        Name = "H3DEffect",
                        EffectCode = new HLSLShaderGenerator(material.MaterialParams) { BoneCount = model.Skeleton.Count}.GetShader()
                    },
                    Material = material.Name,

                    FaceCulling = (H3DFaceCulling?) material.MaterialParams.FaceCulling,

                    EmissionColor = material.MaterialParams.EmissionColor.ToXNA(),
                    AmbientColor = material.MaterialParams.AmbientColor.ToXNA(),
                    DiffuseColor = material.MaterialParams.DiffuseColor.ToXNA(),
                    Specular0Color = material.MaterialParams.Specular0Color.ToXNA(),
                    Specular1Color = material.MaterialParams.Specular1Color.ToXNA(),
                    Constant0Color = material.MaterialParams.Constant0Color.ToXNA(),
                    Constant1Color = material.MaterialParams.Constant1Color.ToXNA(),
                    Constant2Color = material.MaterialParams.Constant2Color.ToXNA(),
                    Constant3Color = material.MaterialParams.Constant3Color.ToXNA(),
                    Constant4Color = material.MaterialParams.Constant4Color.ToXNA(),
                    Constant5Color = material.MaterialParams.Constant5Color.ToXNA(),
                    BlendColor = material.MaterialParams.BlendColor.ToXNA(),
                    DepthBufferRead = material.MaterialParams.DepthBufferRead,
                    DepthBufferWrite = material.MaterialParams.DepthBufferWrite,
                    StencilBufferRead = material.MaterialParams.StencilBufferRead,
                    StencilBufferWrite = material.MaterialParams.StencilBufferWrite,
                };

                var texCount = 0;
                if (material.EnabledTextures[0])
                    texCount++;
                if (material.EnabledTextures[1])
                    texCount++;
                if (material.EnabledTextures[2])
                    texCount++;
                materialContent.TextureList = new Texture2DContent[texCount];
                if (material.EnabledTextures[0])
                    materialContent.TextureList[0] = textures[material.Texture0Name];
                if (material.EnabledTextures[1])
                    materialContent.TextureList[1] = textures[material.Texture1Name];
                if (material.EnabledTextures[2])
                    materialContent.TextureList[2] = textures[material.Texture2Name];

                materialContent.TextureSamplerSettings = material.TextureMappers.Select(tm => new TextureSamplerSettings()
                {
                    WrapU = tm.WrapU.ToXNAWrap(),
                    WrapV = tm.WrapV.ToXNAWrap(),
                    MagFilter = (TextureSamplerSettings.TextureMagFilter) tm.MagFilter,
                    MinFilter = (TextureSamplerSettings.TextureMinFilter) tm.MinFilter
                }).ToArray();

                materials.Add(material.Name, materialContent);
            }

            // Geometry
            var meshes = new List<MeshContent>();
            for (var i = 0; i < model.Meshes.Count; i++)
            {
                var modelMesh = model.Meshes[i];

                if (modelMesh.Type == H3DMeshType.Silhouette)
                    continue;

                var mesh = new MeshContent()
                {
                    Identity = _identity,
                    Name = $"{model.Materials[modelMesh.MaterialIndex].Name}_node{i}",
                };
                var geometry = new GeometryContent
                {
                    Identity = _identity,
                    Material = materials[model.Materials[modelMesh.MaterialIndex].Name]
                };
                var vertices = GetWorldSpaceVertices(model.Skeleton, modelMesh);
                var baseVertex = mesh.Positions.Count;
                foreach (var vertex in vertices)
                    mesh.Positions.Add(vertex.Position.ToVector3());
                geometry.Vertices.AddRange(Enumerable.Range(baseVertex, vertices.Length));

                foreach (var attribute in modelMesh.Attributes)
                {
                    if (attribute.Name >= PICAAttributeName.BoneIndex)
                        continue;

                    switch (attribute.Name)
                    {
                        case PICAAttributeName.Position: break; // Already added
                        case PICAAttributeName.Normal:
                            geometry.Vertices.Channels.Add(VertexChannelNames.Normal(0), vertices.Select(vertex => vertex.Normal.ToVector3()));
                            break;
                        case PICAAttributeName.Tangent:
                            geometry.Vertices.Channels.Add(VertexChannelNames.Tangent(0), vertices.Select(vertex => vertex.Tangent.ToVector3()));
                            break;
                        case PICAAttributeName.Color:
                            geometry.Vertices.Channels.Add(VertexChannelNames.Color(0), vertices.Select(vertex => vertex.Color.ToColor()));
                            break;
                        case PICAAttributeName.TexCoord0:
                            geometry.Vertices.Channels.Add(VertexChannelNames.TextureCoordinate(0), vertices.Select(vertex => vertex.TexCoord0.ToVector2().ToUV()));
                            break;
                        case PICAAttributeName.TexCoord1:
                            geometry.Vertices.Channels.Add(VertexChannelNames.TextureCoordinate(1), vertices.Select(vertex => vertex.TexCoord1.ToVector2().ToUV()));
                            break;
                        case PICAAttributeName.TexCoord2:
                            geometry.Vertices.Channels.Add(VertexChannelNames.TextureCoordinate(2), vertices.Select(vertex => vertex.TexCoord2.ToVector2().ToUV()));
                            break;
                    }
                }

                var vertexOffset = 0;
                var xnaWeights = new List<BoneWeightCollection>();
                foreach (var modelSubMesh in modelMesh.SubMeshes)
                {
                    geometry.Indices.AddRange(modelSubMesh.Indices.Select(index => (int) index));

                    var vertexCount = modelSubMesh.MaxIndex + 1 - vertexOffset;
                    var subMeshVertices = vertices.Skip(vertexOffset).Take(vertexCount).ToList();
                    
                    if (modelSubMesh.Skinning == H3DSubMeshSkinning.Smooth)
                    {
                        foreach (var vertex in subMeshVertices)
                        {
                            var list = new BoneWeightCollection();
                            for (var index = 0; index < 4; index++)
                            {
                                var bIndex = vertex.Indices[index];
                                var weight = vertex.Weights[index];

                                if (weight == 0) break;

                                if (bIndex < modelSubMesh.BoneIndicesCount && bIndex > -1)
                                    bIndex = modelSubMesh.BoneIndices[bIndex];
                                else
                                    bIndex = 0;

                                list.Add(new BoneWeight(model.Skeleton[bIndex].Name, weight));
                            }
                            xnaWeights.Add(list);
                        }
                    }
                    else
                    {
                        foreach (var vertex in vertices)
                        {
                            var bIndex = vertex.Indices[0];

                            if (bIndex < modelSubMesh.BoneIndices.Length && bIndex > -1)
                                bIndex = modelSubMesh.BoneIndices[bIndex];
                            else
                                bIndex = 0;

                            xnaWeights.Add(new BoneWeightCollection() { new BoneWeight(model.Skeleton[bIndex].Name, 0) });
                        }
                    }
                    vertexOffset += vertexCount;
                }
                geometry.Vertices.Channels.Add(VertexChannelNames.Weights(0), xnaWeights);
                mesh.Geometry.Add(geometry);
                meshes.Add(mesh);
            }

            foreach (var mesh in meshes)
                _rootNode.Children.Add(mesh);

            var rootBone = ImportBones(model);
            _rootNode.Children.Add(rootBone);

            if (!scene.SkeletalAnimations.Any())
            {
                var path = Path.Combine(Path.GetDirectoryName(filename), $"{Path.GetFileNameWithoutExtension(filename)}@Animations{Path.GetExtension(filename)}");
                if(File.Exists(path))
                {
                    context.Logger.LogMessage($"Found animation file {path}. Loading data...");
                    scene.Merge(FormatIdentifier.IdentifyAndOpen(path, model.Skeleton));
                }
                else
                    context.Logger.LogMessage($"Couldn't find animation file {path}!");
            }

            foreach (var animation in ImportSkeletalAnimations(scene))
                rootBone.Animations.Add(animation.Name, animation);

            foreach (var animation in ImportMaterialAnimations(scene))
                _rootNode.Children.Add(animation);

            return _rootNode;
        }

        private BoneContent ImportBones(H3DModel model)
        {
            var rootNode = new BoneContent();

            var childBones = new Queue<Tuple<H3DBone, BoneContent>>();

            childBones.Enqueue(Tuple.Create(model.Skeleton[0], rootNode));

            while (childBones.Count > 0)
            {
                var boneNode = childBones.Dequeue();

                var bone = boneNode.Item1;

                boneNode.Item2.Identity = _identity;
                boneNode.Item2.Name = bone.Name;
                boneNode.Item2.Transform = bone.Transform.ToXNA();

                foreach (var b in model.Skeleton)
                {
                    if (b.ParentIndex == -1)
                        continue;

                    var parentBone = model.Skeleton[b.ParentIndex];

                    if (parentBone == bone)
                    {
                        var nnode = new BoneContent();

                        childBones.Enqueue(Tuple.Create(b, nnode));

                        boneNode.Item2.Children.Add(nnode);
                    }
                }
            }

            return rootNode;
        }
        private static PICAVertex[] GetWorldSpaceVertices(H3DDict<H3DBone> skeleton, H3DMesh mesh)
        {
            var vertices = mesh.GetVertices();

            var transformedVertices = new bool[vertices.Length];

            //Smooth meshes are already in World Space, so we don't need to do anything.
            if (mesh.Skinning != H3DMeshSkinning.Smooth)
            {
                foreach (var sm in mesh.SubMeshes)
                {
                    foreach (var i in sm.Indices)
                    {
                        if (transformedVertices[i]) continue;

                        transformedVertices[i] = true;

                        var v = vertices[i];

                        if (skeleton != null && skeleton.Count > 0 && sm.Skinning != H3DSubMeshSkinning.Smooth)
                        {
                            int b = sm.BoneIndices[v.Indices[0]];

                            var transform = skeleton[b].GetWorldTransform(skeleton);

                            v.Position = System.Numerics.Vector4.Transform(new System.Numerics.Vector3(v.Position.X, v.Position.Y, v.Position.Z), transform);

                            v.Normal.W = 0;

                            v.Normal = System.Numerics.Vector4.Transform(v.Normal, transform);
                            v.Normal = System.Numerics.Vector4.Normalize(v.Normal);
                        }

                        vertices[i] = v;
                    }
                }
            }

            return vertices;
        }

        private List<AnimationContent> ImportSkeletalAnimations(SPICA.Formats.CtrH3D.H3D scene) =>  
            scene.SkeletalAnimations.Select(sa => ImportSkeletalAnimation(scene.Models[0], sa)).ToList();
        private AnimationContent ImportSkeletalAnimation(H3DModel model, H3DAnimation animation)
        {
            var framesCount = (int) animation.FramesCount + 1;

            var animationNode = new AnimationContent
            {
                Name = animation.Name,
                Identity = _identity,
                Duration = TimeSpan.FromSeconds((float) 1 / (float) 30 * (float) framesCount)
            };

            
            foreach (var elem in animation.Elements)
            {
                if (elem.PrimitiveType != H3DPrimitiveType.Transform && elem.PrimitiveType != H3DPrimitiveType.QuatTransform)
                    continue;

                var sklBone = model.Skeleton.FirstOrDefault(x => x.Name == elem.Name);
                var parent = sklBone != null && sklBone.ParentIndex != -1 ? model.Skeleton[sklBone.ParentIndex] : null;

                var channel = new AnimationChannel();
                for (var frame = 0; frame < framesCount; frame++)
                {
                    var translation = Matrix.Identity;
                    if (elem.Content is H3DAnimTransform transform0)
                    {
                        if (!transform0.TranslationExists)
                            continue;

                        translation = Matrix.CreateTranslation(new Vector3(
                            transform0.TranslationX.Exists ? transform0.TranslationX.GetFrameValue(frame) : sklBone.Translation.X,
                            transform0.TranslationY.Exists ? transform0.TranslationY.GetFrameValue(frame) : sklBone.Translation.Y,
                            transform0.TranslationZ.Exists ? transform0.TranslationZ.GetFrameValue(frame) : sklBone.Translation.Z));
                    }
                    else if (elem.Content is H3DAnimQuatTransform quatTransform)
                    {
                        if (!quatTransform.HasTranslation)
                            continue;

                        translation = Matrix.CreateTranslation(quatTransform.GetTranslationValue(frame).ToXNA());
                    }

                    var rotation = Matrix.Identity;
                    if (elem.Content is H3DAnimTransform transform1)
                    {
                        if (!(transform1.RotationX.Exists || transform1.RotationY.Exists ||
                              transform1.RotationZ.Exists))
                            continue;

                        rotation = Matrix.CreateRotationX(transform1.RotationX.GetFrameValue(frame)) *
                                   Matrix.CreateRotationY(transform1.RotationY.GetFrameValue(frame)) *
                                   Matrix.CreateRotationZ(transform1.RotationZ.GetFrameValue(frame));
                    }
                    else if (elem.Content is H3DAnimQuatTransform quatTransform)
                    {
                        if (!quatTransform.HasRotation)
                            continue;

                        rotation = Matrix.CreateFromQuaternion(quatTransform.GetRotationValue(frame).ToXNA());
                    }

                    var scale = Matrix.Identity;
                    var invScale = System.Numerics.Vector3.One;
                    var pElem = animation.Elements.FirstOrDefault(x => x.Name == parent?.Name);
                    if (elem.Content is H3DAnimTransform transform2)
                    {
                        if (!transform2.ScaleExists)
                            continue;

                        //Compensate parent bone scale (basically, don't inherit scales)
                        if (parent != null && (sklBone.Flags & H3DBoneFlags.IsSegmentScaleCompensate) != 0)
                        {
                            if (pElem != null)
                            {
                                var pTrans = (H3DAnimTransform) pElem.Content;

                                invScale /= new System.Numerics.Vector3(
                                    pTrans.ScaleX.Exists ? pTrans.ScaleX.GetFrameValue(frame) : parent.Scale.X,
                                    pTrans.ScaleY.Exists ? pTrans.ScaleY.GetFrameValue(frame) : parent.Scale.Y,
                                    pTrans.ScaleZ.Exists ? pTrans.ScaleZ.GetFrameValue(frame) : parent.Scale.Z);
                            }
                            else
                                invScale /= parent.Scale;
                        }

                        scale = Matrix.CreateScale((invScale * new System.Numerics.Vector3(
                                                        transform2.ScaleX.Exists ? transform2.ScaleX.GetFrameValue(frame) : sklBone.Scale.X,
                                                        transform2.ScaleY.Exists ? transform2.ScaleY.GetFrameValue(frame) : sklBone.Scale.Y,
                                                        transform2.ScaleZ.Exists ? transform2.ScaleZ.GetFrameValue(frame) : sklBone.Scale.Z)).ToXNA());
                    }
                    else if (elem.Content is H3DAnimQuatTransform quatTransform)
                    {
                        if (!quatTransform.HasScale)
                            continue;

                        //Compensate parent bone scale (basically, don't inherit scales)
                        if (parent != null && (sklBone.Flags & H3DBoneFlags.IsSegmentScaleCompensate) != 0)
                        {
                            if (pElem != null)
                                invScale /= ((H3DAnimQuatTransform) pElem.Content).GetScaleValue(frame);
                            else
                                invScale /= parent.Scale;
                        }

                        scale = Matrix.CreateScale((invScale * quatTransform.GetScaleValue(frame)).ToXNA());
                    }

                    channel.Add(new AnimationKeyframe(TimeSpan.FromSeconds((float) frame / 30f), scale * rotation * translation));
                }

                animationNode.Channels[elem.Name] = channel;
            }

            return animationNode;
        }

        private List<MaterialAnimationContent> ImportMaterialAnimations(SPICA.Formats.CtrH3D.H3D scene) => 
            scene.MaterialAnimations.Where(ma => ma.Elements.Any()).Select(ma => ImportMaterialAnimation(scene.Models[0], scene.Materials, ma)).ToList();
        private MaterialAnimationContent ImportMaterialAnimation(H3DModel model, H3DDict<H3DMaterialParams> materials, H3DMaterialAnim animation)
        {
            var framesCount = (int) animation.FramesCount + 1;

            var animationNode = new MaterialAnimationContent
            {
                Name = animation.Name,
                Identity = _identity,
                Duration = TimeSpan.FromSeconds((float) 1 / (float) 30 * (float) framesCount)
            };

            foreach (var elem in animation.Elements)
            {
                var material = model.Materials.Single(mat => elem.Name == mat.Name);
                var materialParams = material.MaterialParams;

                var channel = new MaterialAnimationChannel();
                for (var frame = 0; frame < framesCount; frame++)
                {
                    var keyFrame = new MaterialAnimationKeyframe(TimeSpan.FromSeconds((float) frame / 30f))
                    {
                        Material = material.Name,
                        Transforms = new Matrix[3]
                    };
                    var tc = new [] { materialParams.TextureCoords[0], materialParams.TextureCoords[1], materialParams.TextureCoords[2] };

                    if (elem.PrimitiveType == H3DPrimitiveType.RGBA)
                    {
                        void SetColor(H3DAnimRGBA c, string name)
                        {
                            var color = new Color();
                            if (c.R.Exists) color.R = (byte) c.R.GetFrameValue(frame);
                            if (c.G.Exists) color.G = (byte) c.G.GetFrameValue(frame);
                            if (c.B.Exists) color.B = (byte) c.B.GetFrameValue(frame);
                            if (c.A.Exists) color.A = (byte) c.A.GetFrameValue(frame);
                            keyFrame.OpaqueData.Add(name, color);
                        }

                        var rgba = (H3DAnimRGBA) elem.Content;
                        switch (elem.TargetType)
                        {
                            case H3DTargetType.MaterialEmission:  SetColor(rgba, "EmissionColor"); break;
                            case H3DTargetType.MaterialAmbient:   SetColor(rgba, "AmbientColor"); break;
                            case H3DTargetType.MaterialDiffuse:   SetColor(rgba, "DiffuseColor"); break;
                            case H3DTargetType.MaterialSpecular0: SetColor(rgba, "Specular0Color"); break;
                            case H3DTargetType.MaterialSpecular1: SetColor(rgba, "Specular1Color"); break;
                            case H3DTargetType.MaterialConstant0: SetColor(rgba, "Constant0Color"); break;
                            case H3DTargetType.MaterialConstant1: SetColor(rgba, "Constant1Color"); break;
                            case H3DTargetType.MaterialConstant2: SetColor(rgba, "Constant2Color"); break;
                            case H3DTargetType.MaterialConstant3: SetColor(rgba, "Constant3Color"); break;
                            case H3DTargetType.MaterialConstant4: SetColor(rgba, "Constant4Color"); break;
                            case H3DTargetType.MaterialConstant5: SetColor(rgba, "Constant5Color"); break;
                        }
                    }
                    else if (elem.PrimitiveType == H3DPrimitiveType.Vector2D)
                    {
                        void SetVector2(H3DAnimVector2D v, ref System.Numerics.Vector2 t)
                        {
                            if (v.X.Exists) t.X = v.X.GetFrameValue(frame);
                            if (v.Y.Exists) t.Y = v.Y.GetFrameValue(frame);
                        }

                        var vector = (H3DAnimVector2D) elem.Content;
                        switch (elem.TargetType)
                        {
                            case H3DTargetType.MaterialTexCoord0Scale: SetVector2(vector, ref tc[0].Scale); break;
                            case H3DTargetType.MaterialTexCoord1Scale: SetVector2(vector, ref tc[1].Scale); break;
                            case H3DTargetType.MaterialTexCoord2Scale: SetVector2(vector, ref tc[2].Scale); break;

                            case H3DTargetType.MaterialTexCoord0Trans: SetVector2(vector, ref tc[0].Translation); break;
                            case H3DTargetType.MaterialTexCoord1Trans: SetVector2(vector, ref tc[1].Translation); break;
                            case H3DTargetType.MaterialTexCoord2Trans: SetVector2(vector, ref tc[2].Translation); break;
                        }
                    }
                    else if (elem.PrimitiveType == H3DPrimitiveType.Float)
                    {
                        var @float = ((H3DAnimFloat) elem.Content).Value;
                        if (!@float.Exists) continue;

                        var value = @float.GetFrameValue(frame);
                        switch (elem.TargetType)
                        {
                            case H3DTargetType.MaterialTexCoord0Rot: tc[0].Rotation = value; break;
                            case H3DTargetType.MaterialTexCoord1Rot: tc[1].Rotation = value; break;
                            case H3DTargetType.MaterialTexCoord2Rot: tc[2].Rotation = value; break;
                        }
                    }
                    else if (elem.PrimitiveType == H3DPrimitiveType.Texture)
                    {
                        var @int = ((H3DAnimFloat) elem.Content).Value;
                        if (!@int.Exists) continue;

                        var value = (int) @int.GetFrameValue(frame);
                        var name = materials[value].Name;
                        switch (elem.TargetType)
                        {
                            case H3DTargetType.MaterialMapper0Texture: keyFrame.OpaqueData.Add("Texture0", name); break;
                            case H3DTargetType.MaterialMapper1Texture: keyFrame.OpaqueData.Add("Texture1", name); break;
                            case H3DTargetType.MaterialMapper2Texture: keyFrame.OpaqueData.Add("Texture2", name); break;
                        }
                    }

                    keyFrame.Transforms[0] = tc[0].GetTransform().ToXNA();
                    keyFrame.Transforms[1] = tc[1].GetTransform().ToXNA();
                    keyFrame.Transforms[2] = tc[2].GetTransform().ToXNA();

                    channel.Add(keyFrame);
                }

                animationNode.Channels[elem.Name] = channel;
            }

            return animationNode;
        }
    }
}