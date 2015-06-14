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

		protected AbstractFragmentShaderStage( params string[] shaderResourceIDs )
			: base ( shaderResourceIDs )
		{

		}

		override protected ShaderType GetShaderType()
		{
			return ShaderType.FragmentShader;
		}
	}
}

