using OpenTK.Graphics.OpenGL4;
using Kaiga.Shaders;

namespace Kaiga.Shaders.Fragment
{
	abstract public class AbstractFragmentShaderStage : AbstractShaderStage
	{
		override protected ShaderType GetShaderType()
		{
			return ShaderType.FragmentShader;
		}
	}
}

