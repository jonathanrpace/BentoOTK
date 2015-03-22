using OpenTK.Graphics.OpenGL4;

namespace Kaiga.ShaderStages
{
	abstract public class AbstractFragmentShaderStage : AbstractShaderStage
	{
		override protected ShaderType GetShaderType()
		{
			return ShaderType.FragmentShader;
		}
	}
}

