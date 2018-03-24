# PokeD.Graphics.H3D

Portion of code taken from [SPICA](https://github.com/gdkchan/SPICA)
  
  
## Importers
* [H3D Model](https://github.com/PokeD/PokeD.Graphics.Animation/blob/master/PokeD.Graphics.Content.Pipeline.Animation/Processors/CPUAnimatedModelProcessor.cs) - Import an animated H3D/BCH [Model](https://github.com/MonoGame/MonoGame/blob/master/MonoGame.Framework/Graphics/Model.cs).
  
  
## Processors
* [GPU Animated H3D Model](https://github.com/PokeD/PokeD.Graphics.H3D/blob/master/PokeD.Graphics.Content.Pipeline.H3D/Processors/GPUAnimatedH3DModelProcessor.cs) - Import an animated [Model](https://github.com/MonoGame/MonoGame/blob/master/MonoGame.Framework/Graphics/Model.cs).
* [CPU Animated H3D Model](https://github.com/PokeD/PokeD.Graphics.H3D/blob/master/PokeD.Graphics.Content.Pipeline.H3D/Processors/CPUAnimatedH3DModelProcessor.cs) - Import an animated [Model](https://github.com/MonoGame/MonoGame/blob/master/MonoGame.Framework/Graphics/Model.cs) to be animated by the CPU. Based on [DynamicModelProcessor](https://github.com/PokeD/PokeD.Graphics.Animation/blob/master/PokeD.Graphics.Content.Pipeline.Animation/Processors/DynamicModelProcessor.cs), the imported asset is of type [Microsoft.Xna.Framework.Graphics.Model](https://github.com/MonoGame/MonoGame/blob/master/MonoGame.Framework/Graphics/Model.cs) where the [VertexBuffer](https://github.com/MonoGame/MonoGame/blob/master/MonoGame.Framework/Graphics/Vertices/VertexBuffer.cs) is replaced by a [DefaultAnimatedDynamicVertexBuffer](https://github.com/PokeD/PokeD.Graphics.Animation/blob/master/PokeD.Graphics.Animation/SkeletalAnimation/DefaultAnimatedDynamicVertexBuffer.cs), it inherits from [DynamicVertexBuffer](https://github.com/MonoGame/MonoGame/blob/master/MonoGame.Framework/Graphics/Vertices/DynamicVertexBuffer.cs).
  
  
## Example
See [PokeD.Graphics.Animation](https://github.com/PokeD/PokeD.Graphics.Animation)
  
  
## Notes
Both H3D Model processors include shaders in the output. There are two versions of the techniques - one with *_Basic* prefix and one with *_Custom*.  
*_Basic* is a very simple shader that will most likely render complex model wrong. 
*_Custom* is generated based on *SPICA*'s shader generator. It's still incomplete.  
Both do not process light atm.
