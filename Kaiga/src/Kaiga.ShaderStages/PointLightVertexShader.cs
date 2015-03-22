using Kaiga.Core;
using OpenTK.Graphics.OpenGL4;

namespace Kaiga.ShaderStages
{
	public class PointLightVertexShader : AbstractVertexShaderStage
	{
		public void BindPerLight( RenderParams renderParams )
		{
			SetUniformMatrix4( "MVPMatrix", ref renderParams.ModelViewProjectionMatrix );
		}

		override protected string GetShaderSource()
		{
			return @"
			#version 440 core

			// Uniforms
			uniform mat4 MVPMatrix;

			// Inputs
			layout(location = 0) in vec3 in_Position;

			// Outputs
			out gl_PerVertex 
			{
				vec4 gl_Position;
			};

			void main(void)
			{
				gl_Position = MVPMatrix * vec4(in_Position, 1);
			} 
			";
		}
	}
}

