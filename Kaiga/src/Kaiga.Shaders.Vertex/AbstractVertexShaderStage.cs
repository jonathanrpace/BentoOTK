using OpenTK.Graphics.OpenGL4;
using Kaiga.Shaders;

namespace Kaiga.Shaders.Vertex
{
	abstract public class AbstractVertexShaderStage : AbstractShaderStage
	{
		override protected ShaderType GetShaderType()
		{
			return ShaderType.VertexShader;
		}
	}
}

