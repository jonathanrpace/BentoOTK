using OpenTK.Graphics.OpenGL4;

namespace Kaiga.ShaderStages
{
	abstract public class AbstractVertexShaderStage : AbstractShaderStage
	{
		override protected ShaderType GetShaderType()
		{
			return ShaderType.VertexShader;
		}
	}
}

