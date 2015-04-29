using OpenTK.Graphics.OpenGL4;
using Kaiga.Shaders;

namespace Kaiga.Shaders.Fragment
{
	abstract public class AbstractFragmentShaderStage : AbstractShaderStage
	{
		protected AbstractFragmentShaderStage()
		{
		}

		protected AbstractFragmentShaderStage( string shaderResourceID )
			: base ( shaderResourceID )
		{
			
		}

		override protected ShaderType GetShaderType()
		{
			return ShaderType.FragmentShader;
		}
	}
}

