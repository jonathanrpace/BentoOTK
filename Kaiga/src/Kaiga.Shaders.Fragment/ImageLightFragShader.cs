using OpenTK.Graphics.OpenGL4;
using Kaiga.Core;
using Kaiga.Lights;

namespace Kaiga.Shaders.Fragment
{
	public class ImageLightFragShader : AbstractFragmentShaderStage
	{
		override public void BindPerPass( RenderParams renderParams )
		{
			base.BindPerPass( renderParams );

			SetUniformTexture( "s_positionBuffer", renderParams.RenderTarget.PositionBuffer.Texture, TextureTarget.TextureRectangle );
			SetUniformTexture( "s_normalBuffer", renderParams.RenderTarget.NormalBuffer.Texture, TextureTarget.TextureRectangle );
			SetUniformTexture( "s_materialBuffer", renderParams.RenderTarget.MaterialBuffer.Texture, TextureTarget.TextureRectangle );
			SetUniformTexture( "s_albedoBuffer", renderParams.RenderTarget.AlbedoBuffer.Texture, TextureTarget.TextureRectangle );

			SetUniformMatrix3( "normalInvViewMatrix", ref renderParams.NormalInvViewMatrix, true );
		}

		public void BindPerLight( RenderParams renderParams, ImageLight light )
		{
			SetUniformTexture( "s_envMap", light.Texture.Texture, TextureTarget.TextureCubeMap );
		}

		override protected string GetShaderSource()
		{
			return @"
#version 440 core

// Samplers
uniform sampler2DRect s_positionBuffer;
uniform sampler2DRect s_normalBuffer;
uniform sampler2DRect s_materialBuffer;
uniform sampler2DRect s_albedoBuffer;
uniform samplerCube s_envMap;

// Outputs
layout( location = 0 ) out vec4 out_color;

uniform mat3 normalInvViewMatrix;

vec3 getEdgeFixedCubeMapNormal( in vec3 normal, float mipBias, int baseSize )
{
	vec3 out_normal = normal;
	float scale = 1.0f - (exp2(mipBias) / baseSize);

	vec3 absNormal = abs( normal );
	float M = max(max(absNormal.x, absNormal.y), absNormal.z);
	if (absNormal.x != M) out_normal.x *= scale;
	if (absNormal.y != M) out_normal.y *= scale;
	if (absNormal.z != M) out_normal.z *= scale;
	return out_normal;
}

void main()
{
	vec3 position = texture2DRect( s_positionBuffer, gl_FragCoord.xy ).xyz;
	vec3 normal = texture2DRect( s_normalBuffer, gl_FragCoord.xy ).xyz;
	vec4 material = texture2DRect( s_materialBuffer, gl_FragCoord.xy );
	vec3 albedo = texture2DRect( s_albedoBuffer, gl_FragCoord.xy ).xyz;

	float roughness = material.x;
	float reflectivity = material.y;
	float emissive = material.z;

	vec3 viewDir = normalize( position );
	vec3 reflectVec = reflect( viewDir, normal );
	vec3 lightDir = reflectVec * (1.0f - roughness) + roughness * normal;
	lightDir = normalize( lightDir );
	lightDir *= normalInvViewMatrix;

	ivec2 texSize = textureSize( s_envMap, 0 );
	int numMipMaps = textureQueryLevels( s_envMap )-3;

	float mipBias = max( 0.0f, roughness * numMipMaps );

	vec3 cubeNormal = getEdgeFixedCubeMapNormal( lightDir, mipBias, texSize.x );
	vec4 light = pow( textureLod( s_envMap, cubeNormal, mipBias ), vec4( 2.2 ) );
	
	light.xyz *= reflectivity;
	light.xyz *= albedo;
	light.a = 1.0;

	out_color = light;
}
";
		}

	}
}
