using OpenTK.Graphics.OpenGL4;

namespace Kaiga.ShaderStages
{
	public class NormalBufferFragShader : ShaderStage
	{
		override protected ShaderType GetShaderType()
		{
			return ShaderType.FragmentShader;
		}
		
		override protected string GetShaderSource()
		{
			return @"
			#version 440 core

			// Inputs
			in Varying
			{
				vec2 in_Uv;
			};

			// Samplers
			uniform sampler2D tex;

			// Outputs
			layout( location = 0 ) out vec4 fragColor;

			void main(void)
			{ 
				fragColor = texture( tex, in_Uv );
			}
			";
		}

	}
}

