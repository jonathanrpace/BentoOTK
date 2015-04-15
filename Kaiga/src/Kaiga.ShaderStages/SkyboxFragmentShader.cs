using System;
using OpenTK.Graphics.OpenGL4;
using Ramen;
using OpenTK.Input;

namespace Kaiga.ShaderStages
{
	public class SkyboxFragmentShader : AbstractFragmentShaderStage
	{
		public void SetTexture( int texture )
		{
			SetUniformTexture( 0, "tex", texture, TextureTarget.TextureCubeMap );
			SetUniform1( "mipRatio", Mouse.GetState().X / 500.0f );
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

			void main()
			{ 
				ivec2 texSize = textureSize( tex, 0 );
				int numMipMaps = textureQueryLevels( tex );

				float mipmapIndex = mipRatio * numMipMaps;
				vec3 normal = in_normal;
				float scale = 1 - exp2(mipmapIndex) / texSize.x; // CubemapSize is the size of the base mipmap
				vec3 absNormal = abs( normal );
				float M = max(max(absNormal.x, absNormal.y), absNormal.z);
				if (absNormal.x != M) normal.x *= scale;
				if (absNormal.y != M) normal.y *= scale;
				if (absNormal.z != M) normal.z *= scale;

				out_fragColor = textureLod( tex, normal, mipmapIndex );
			}
";
		}
	}
}

