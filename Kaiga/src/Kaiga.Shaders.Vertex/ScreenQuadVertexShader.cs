using OpenTK.Graphics.OpenGL4;

namespace Kaiga.Shaders.Vertex
{
	public class ScreenQuadVertexShader : AbstractVertexShaderStage
	{
		override protected string GetShaderSource()
		{
			return @"
			#version 450 core

			// Inputs
			layout(location = 0) in vec2 in_Position;
			layout(location = 2) in vec2 in_uv;

			// Outputs
			out gl_PerVertex 
			{
				vec4 gl_Position;
			};

			out Varying
			{
				out vec2 out_uv;
			};

			void main(void)
			{
				gl_Position = vec4( in_Position, 0.0, 1.0 );

				out_uv = in_uv;
			}
			";
		}

	}
}

