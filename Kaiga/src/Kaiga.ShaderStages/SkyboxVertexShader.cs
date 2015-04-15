using Kaiga.ShaderStages;
using Kaiga.Core;
using OpenTK;

namespace Kaiga.ShaderStages
{
	public class SkyboxVertexShader : AbstractVertexShaderStage
	{
		override public void BindPerPass( RenderParams renderParams )
		{
			base.BindPerPass( renderParams );
			
			SetUniformMatrix4( "u_viewProjectionMatrix", ref renderParams.NormalViewProjectionMatrix );
		}

		protected override string GetShaderSource()
		{
			return @"
			#version 450 core

			// Inputs
			layout(location = 0) in vec3 in_position;

			uniform mat4 u_viewProjectionMatrix;

			// Outputs
			out gl_PerVertex 
			{
				vec4 gl_Position;
			};

			out vec3 out_normal;

			void main(void)
			{
				vec4 out_position = u_viewProjectionMatrix * vec4( in_position, 1.0 );
				out_position.z = 0.999f * out_position.w;
				gl_Position = out_position;
				out_normal = in_position;
			}
";
		}
	}
}

