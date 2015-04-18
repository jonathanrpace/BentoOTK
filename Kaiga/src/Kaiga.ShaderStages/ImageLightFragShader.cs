using OpenTK.Graphics.OpenGL4;
using Kaiga.Core;
using Kaiga.Lights;
using OpenTK;

namespace Kaiga.ShaderStages
{
	public class ImageLightFragShader : AbstractFragmentShaderStage
	{
		override public void BindPerPass( RenderParams renderParams )
		{
			SetUniformTexture( 0, "s_positionBuffer", renderParams.RenderTarget.PositionBuffer.Texture, TextureTarget.TextureRectangle );
			SetUniformTexture( 1, "s_normalBuffer", renderParams.RenderTarget.NormalBuffer.Texture, TextureTarget.TextureRectangle );
			SetUniformTexture( 2, "s_materialBuffer", renderParams.RenderTarget.MaterialBuffer.Texture, TextureTarget.TextureRectangle );
			SetUniformTexture( 3, "s_albedoBuffer", renderParams.RenderTarget.AlbedoBuffer.Texture, TextureTarget.TextureRectangle );
			SetUniformTexture( 4, "s_aoBuffer", renderParams.AORenderTarget.AOBuffer.Texture, TextureTarget.TextureRectangle );

			SetUniformMatrix3( "normalInvViewMatrix", ref renderParams.NormalInvViewMatrix, true );
		}

		public void BindPerLight( RenderParams renderParams, ImageLight light )
		{
			SetUniformTexture( 5, "s_envMap", light.Texture.Texture, TextureTarget.TextureCubeMap );
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
uniform sampler2DRect s_aoBuffer;
uniform samplerCube s_envMap;

// Outputs
layout( location = 0 ) out vec4 out_color;

uniform mat3 normalInvViewMatrix;

float G1V(float dotNV, float k)
{
	return 1.0f/(dotNV*(1.0f-k)+k);
}

float angularDot( vec3 A, vec3 B, in float angularSize )
{
	float dotValue = clamp( dot( A, B ), 0.0f, 1.0f );
	float pi = 3.14159f;

	float angle = 1.0f - acos(dotValue) * (2.0f / pi);
	angle = min( angle, angularSize );
	angle *= rcp( angularSize );

	float ret = sin( angle * pi * 0.5f );
	
	return ret;
}

float LightingFuncGGX_REF( vec3 N, vec3 V, vec3 L, float roughness, float F0, float angularSize )
{
	float alpha = roughness*roughness;

	vec3 H = normalize(V+L);

	float dotNL = angularDot(N,L,angularSize);
	float dotNV = clamp(dot(N,V), 0.0f, 1.0f);
	float dotNH = angularDot(N,H,angularSize);
	float dotLH = angularDot(L,H,angularSize);

	// D
	float alphaSqr = alpha*alpha;
	float pi = 3.14159f;
	float denom = dotNH * dotNH *(alphaSqr-1.0) + 1.0f;
	float D = alphaSqr/(pi * denom * denom);

	// F
	float dotLH5 = pow(1.0f-dotLH,5);
	float F = F0 + (1.0-F0)*(dotLH5);

	// V
	float k = alpha/2.0f;
	float vis = G1V(dotNL,k)*G1V(dotNV,k);

	float numerator = D * F * vis;
    float denominator = dotNL;
    float rs = numerator/ denominator;

	return dotNL * D * F * vis;
}

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
	float ao = texture2DRect( s_aoBuffer, gl_FragCoord.xy * 0.5 ).x;

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

	//vec3 light = vec3( LightingFuncGGX_REF( normal, viewDir, lightDir, roughness, reflectivity, 0.999f ) );
	
	light.xyz *= ao;//(ao + (1.0f-roughness));
	light.xyz *= reflectivity;
	light.xyz += emissive;
	light.xyz *= albedo;
	light.a = 1.0;

	out_color = light;
}
";
		}

	}
}
