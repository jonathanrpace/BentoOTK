using Kaiga.Core;
using OpenTK.Graphics.OpenGL4;

namespace Kaiga.Shaders.Fragment
{
	public class LightResolveFragShader : AbstractFragmentShaderStage
	{
		override public void BindPerPass( RenderParams renderParams )
		{
			base.BindPerPass( renderParams );

			SetUniformTexture( "s_directLightBuffer", renderParams.RenderTarget.DirectLightBuffer.Texture, 
				TextureTarget.TextureRectangle );
			SetUniformTexture( "s_indirectLightBuffer", renderParams.RenderTarget.IndirectLightBuffer.Texture, 
				TextureTarget.TextureRectangle );
			SetUniformTexture( "s_materialBuffer", renderParams.RenderTarget.MaterialBuffer.Texture, 
				TextureTarget.TextureRectangle );
			SetUniformTexture( "s_aoBuffer", renderParams.AORenderTarget.AOBuffer.Texture, 
				TextureTarget.TextureRectangle );
		}

		/*
		 * Downsample both direct and indirect light buffers into 2D - mipmapped textures
		 * 		Blur as mip decreases (sample nearest 4 blocks)
		 * During resolve
		 * 		Raycast from each pixel along normal.
		 * 		When hit, sample light buffers
		 * 			Choose mip based on roughness
		 * 			Add these together as 'bounce'
		 * 			out = ray_mipped_direct + ray_mipped_indirect + direct
		 * 		When miss
		 * 			out = direct + indirect
		 * 
		*/

		override protected string GetShaderSource()
		{
			return @"
#version 450 core

// Samplers
uniform sampler2DRect s_directLightBuffer;
uniform sampler2DRect s_indirectLightBuffer;
uniform sampler2DRect s_materialBuffer;
uniform sampler2DRect s_aoBuffer;

// Consts
layout( location = 0 ) out vec4 out_fragColor;

void main()
{
	vec4 directLight = texture( s_directLightBuffer, gl_FragCoord.xy );
	vec4 indirectLight = texture( s_indirectLightBuffer, gl_FragCoord.xy );
	vec4 material = texture( s_materialBuffer, gl_FragCoord.xy );
	
	float roughness = material.x;
	float reflectivity = material.y;
	float emissive = material.z;

	vec4 ao = texture2DRect( s_aoBuffer, vec2( gl_FragCoord.xy * 0.5f ) );

	ao.x += (1.0f-ao.x) * (1.0f-roughness);
	out_fragColor = directLight + (indirectLight * ao.x) + vec4(emissive);
}
";
		}
	}
}