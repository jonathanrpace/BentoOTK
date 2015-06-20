using Kaiga.Shaders.Vertex;
using Kaiga.Shaders.Fragment;
using Kaiga.Core;
using OpenTK.Graphics.OpenGL4;
using System;
using OpenTK.Input;

namespace Kaiga.Shaders
{
	public class SkyboxShader : AbstractShader<SkyboxVertexShader, SkyboxFragmentShader>
	{
		public void SetTexture( int texture )
		{
			fragmentShader.SetTexture( texture );
		}
	}

	public class SkyboxVertexShader : AbstractVertexShaderStage
	{
		override public void BindPerPass()
		{
			base.BindPerPass();

			SetUniformMatrix4( "u_viewProjectionMatrix", ref RenderParams.NormalViewProjectionMatrix );
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

	public class SkyboxFragmentShader : AbstractFragmentShaderStage
	{
		public void SetTexture( int texture )
		{
			SetTexture( "tex", texture, TextureTarget.TextureCubeMap );
			SetUniform1( "mipRatio", Math.Max( Math.Min( Mouse.GetState().X / 500.0f, 1.0f ), 0.0f ) );
		}

		protected override string GetShaderSource()
		{
			return @"
			#version 450 core

			// Samplers
			uniform samplerCube tex;

			uniform float mipRatio;

			// Inputs
			in Varying
			{
				vec3 in_normal;
			};

			// Outputs
			layout( location = 0 ) out vec4 out_fragColor;

			vec3 getEdgeFixedCubeMapNormal( in vec3 normal, float mipBias, int baseSize )
			{
				vec3 out_normal = normal;
				float scale = 1.0f - exp2(mipBias) / baseSize;
				vec3 absNormal = abs( normal );
				float M = max(max(absNormal.x, absNormal.y), absNormal.z);
				if (absNormal.x != M) out_normal.x *= scale;
				if (absNormal.y != M) out_normal.y *= scale;
				if (absNormal.z != M) out_normal.z *= scale;
				return out_normal;
			}

			void main()
			{ 
				ivec2 texSize = textureSize( tex, 0 );
				int numMipMaps = textureQueryLevels( tex )-1;
				float mipBias = 0.0f * numMipMaps;

				vec3 cubeNormal = getEdgeFixedCubeMapNormal( in_normal, mipBias, texSize.x );
				out_fragColor = pow( textureLod( tex, cubeNormal, mipBias ), vec4( 2.2 ) );
			}

			
";
		}
	}
}

