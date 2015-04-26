using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

namespace Kaiga.Shaders.Fragment
{
	public class SkyboxFragmentShader : AbstractFragmentShaderStage
	{
		public void SetTexture( int texture )
		{
			SetUniformTexture( 0, "tex", texture, TextureTarget.TextureCubeMap );
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

