using Kaiga.Core;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Textures;
using System.Text;
using System;

namespace Kaiga.ShaderStages
{
	public class FFAOFragShader : AbstractFragmentShaderStage
	{
		RandomDirectionTexture randomTexture;

		const int NUM_SAMPLES = 32;

		public FFAOFragShader()
		{
			randomTexture = new RandomDirectionTexture();
		}
		
		override public void Dispose()
		{
			base.Dispose();
			randomTexture.Dispose();
		}

		override public void BindPerPass( RenderParams renderParams )
		{
			SetUniformTexture( 0, "s_positionBuffer", renderParams.RenderTarget.PositionBuffer.Texture, 
								TextureTarget.TextureRectangle );
			SetUniformTexture( 1, "s_normalBuffer", renderParams.RenderTarget.NormalBuffer.Texture, 
				TextureTarget.TextureRectangle );

			SetUniformTexture( 2, "s_randomTexture", randomTexture.Texture, TextureTarget.Texture2D );

			SetUniformMatrix4( "projectionMatrix", ref renderParams.ProjectionMatrix );
		}

		override protected string GetShaderSource()
		{
			var str = new StringBuilder();
			str.Append( @"
#version 450 core

// Samplers
uniform sampler2DRect s_positionBuffer;
uniform sampler2DRect s_normalBuffer;
uniform sampler2D s_randomTexture;

// Matrices
uniform mat4 projectionMatrix;

// Consts
" );

			str.Append( "const int NUM_SAMPLES = " + NUM_SAMPLES + ";\r\n" );
			str.Append( "uniform vec3 SAMPLES[ " + NUM_SAMPLES + " ] = { \r\n" );

			var rand = new Random();
			for ( var i = 0; i < NUM_SAMPLES; i++ )
			{
				double x = rand.NextDouble();
				double y = rand.NextDouble();
				double z = rand.NextDouble();

				double length = Math.Sqrt( x * x + y * y + z * z );
				x /= length;
				y /= length;
				z /= length;

				length *= rand.NextDouble();

				x *= length;
				y *= length;
				z *= length;


				str.Append( "vec3(" );
				str.Append( x.ToString() + "," );
				str.Append( y.ToString() + "," );
				str.Append( z.ToString() );
				str.Append( ")" );

				if ( i < NUM_SAMPLES - 1 )
				{
					str.Append( "," );
				}
			}
			str.Append( "};\r\n" );

			str.Append( @"
// Outputs
layout( location = 0 ) out float out_ao;

void main()
{
	vec2 sampleCoord = vec2( gl_FragCoord * 2.0f );

	vec4 positionBufferSample = texture2DRect( s_positionBuffer, sampleCoord );
	vec3 position = positionBufferSample.xyz;
	
	ivec2 positionBufferSize = textureSize( s_positionBuffer, 0 );
	
	vec4 normalBufferSample = texture2DRect( s_normalBuffer, sampleCoord );
	vec3 normal = normalBufferSample.xyz;
	//normal.z = -normal.z;

	ivec2 randomTextureSize = textureSize( s_randomTexture, 0 );
	vec4 randomSample = texture2D( s_randomTexture, gl_FragCoord.xy / randomTextureSize );
	
	vec3 randomDirection = normalize( randomSample.xyz );
	
	float cutoff = 0.03;
	float radius = 0.08;
	float depthRadius = cutoff + radius * 2.0f;
	
	float radiusOverDepth = radius / position.z * position.z;
	
	float occlusion = 0.0;
	
	for ( int i = 0; i < NUM_SAMPLES; i++ )
	{
		vec3 ray = radiusOverDepth * reflect( SAMPLES[i], randomDirection );

		// Flip direction of rays pointing towards surface
		ray *= sign( dot( ray, normal.xyz ) );

		vec4 hemiRay = vec4( position + ray, 1.0 );
	
		//hemiRay = vec4( position + normal * radiusOverDepth, 1.0 );

		// Convert to screen-space UV coord
		vec4 screenCoord = projectionMatrix * hemiRay;
		screenCoord /= screenCoord.w;
		screenCoord += 1.0;
		screenCoord *= 0.5;
		screenCoord.xy *= positionBufferSize;
		vec4 occPositionSample = texture2DRect( s_positionBuffer, screenCoord.xy );
		
		//float difference = ((-position.z) - (-occPositionSample.z));
		float difference = occPositionSample.z - position.z;
		occlusion += step(cutoff, difference) * (1.0 - smoothstep(cutoff, depthRadius, difference));
	}
	
	occlusion /= NUM_SAMPLES;
	occlusion *= 2.0f;
	occlusion = 1.0 - occlusion;
	
	out_ao = occlusion;
}
" );

			return str.ToString();
		}
	}
}