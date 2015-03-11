using OpenTK.Graphics.OpenGL4;

namespace Kaiga.ShaderStages
{
	public class ScreenQuadVertexShader : ShaderStage
	{
		override protected ShaderType GetShaderType()
		{
			return ShaderType.VertexShader;
		}

		override protected string GetShaderSource()
		{
			return @"
			#version 440 core

			// Inputs
			layout(location = 0) in vec3 in_Position;
			layout(location = 2) in vec2 in_Uv;
			
			// Outputs
			out gl_PerVertex 
			{
				vec4 gl_Position;
			};

			out Varying
			{
				vec2 out_Uv;
			};

			void main(void)
			{
				gl_Position = vec4( in_Position, 1.0 );
				out_Uv = in_Uv;
			}
			";
		}

	}
}

