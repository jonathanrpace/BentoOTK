using Kaiga.Core;
using Kaiga.Geom;
using Kaiga.Shaders.Vertex;
using Kaiga.Shaders.Fragment;
using OpenTK.Graphics.OpenGL4;
using OpenTK;
using OpenTK.Input;
using System.Diagnostics;

namespace Kaiga.Shaders
{
	public class DepthAwareBlurShader : AbstractShader<ScreenQuadVertexShader, DepthAwareBlurFragShader>
	{
		readonly ScreenQuadGeometry screenQuadGeom;

		public DepthAwareBlurShader()
		{
			screenQuadGeom = new ScreenQuadGeometry();
		}

		public override void Dispose()
		{
			base.Dispose();
			screenQuadGeom.Dispose();
		}

		public void Render( int texture, int positionTexture, int normalTexture, float directionX, float directionY, int sigmaIndex )
		{
			BindPerPass();
			fragmentShader.Bind( texture, positionTexture, normalTexture, directionX, directionY, sigmaIndex );
			screenQuadGeom.Bind();
			screenQuadGeom.Draw();
		}
	}

	public class DepthAwareBlurFragShader : AbstractFragmentShaderStage
	{
		public void Bind( int texture, int positionTexture, int normalTexture, float directionX, float directionY, int sigmaIndex )
		{
			SetUniform2( "u_direction", new Vector2( directionX, directionY ) );
			SetUniform1( "u_sigmaIndex", sigmaIndex );

			SetTexture( "s_texture", texture, TextureTarget.TextureRectangle );
			SetTexture( "s_positionTexture", positionTexture, TextureTarget.TextureRectangle );
			SetTexture( "s_normalTexture", normalTexture, TextureTarget.TextureRectangle );

			//float depthMax = Mouse.GetState().Y / 500.0f;
			//Debug.WriteLine( "depthMax : " + depthMax.ToString() );
			//SetUniform1( "u_depthMax", depthMax );

			//float colorDiffMax = Mouse.GetState().X / 500.0f;
			//Debug.WriteLine( colorDiffMax.ToString() );
			//SetUniform1( "u_colorDiffMax", colorDiffMax );

			//float normalDiffMax = Mouse.GetState().X / 500.0f;
			//Debug.WriteLine( "normalDiffMax : " + normalDiffMax.ToString() );
			//SetUniform1( "u_normalDiffMax", normalDiffMax );

			//float colorDiffMax = Mouse.GetState().X / 500.0f;
			//Debug.WriteLine( "colorDiffMax: " + colorDiffMax.ToString() );
			//const float colorDiffMax = 0.5f;
			//SetUniform1( "u_colorDiffMax", colorDiffMax );

			SetUniform1( "u_lightTransportResScalar", RenderParams.LightTransportResolutionScalar );
		}

		protected override string GetShaderSource()
		{
			return @"
#version 450 core

// Samplers
uniform sampler2DRect s_texture;
uniform sampler2DRect s_positionTexture;
uniform sampler2DRect s_normalTexture;

// Uniforms
uniform vec2 u_direction;
uniform float u_depthMax = 20.0f;
uniform float u_colorDiffMax = 0.25f;
uniform float u_normalDiffMax = 1.0f;
uniform float u_lightTransportResScalar;
uniform int u_sigmaIndex = 0;

// Outputs
layout( location = 0 ) out vec4 out_fragColor;

// Kernel weights generated from tool here
// http://dev.theomader.com/gaussian-kernel-calculator/

/*
const int NUM_SAMPLES = 17;
const float[] OFFSETS = 
{
	-8, -7, -6, -5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5, 6, 7, 8
};
const float [4][17] WEIGHTS =
{
	// Sigma 16.0
	{ 
		0.054357,	0.055973,	0.057412,	0.058658,	0.059698,	0.06052,	0.061113,	0.061472,
		0.061592,	0.061472,	0.061113,	0.06052,	0.059698,	0.058658,	0.057412,	0.055973,	0.054357
	},
	// Sigma 8.0
	{ 
		0.042481,	0.047756,	0.052854,	0.05759,	0.06178,	0.065248,	0.067844,	0.06945,
		0.069994,	0.06945,	0.067844,	0.065248,	0.06178,	0.05759,	0.052854,	0.047756,	0.042481
	},
	// Sigma 4.0
	{
		0.014076,	0.022439,	0.033613,	0.047318,	0.062595,	0.077812,	0.090898,	0.099783,	
		0.102934,	0.099783,	0.090898,	0.077812,	0.062595,	0.047318,	0.033613,	0.022439,	0.014076

	},
	// Sigma 2.0
	{
		0.000078,	0.000489,	0.002403,	0.009245,	0.027835,	0.065592,	0.12098,	0.17467,	
		0.197417,	0.17467,	0.12098,	0.065592,	0.027835,	0.009245,	0.002403,	0.000489,	0.000078
	}
};
*/

const int NUM_SAMPLES = 9;
const float[] OFFSETS = 
{
	-4, -3, -2, -1, 0, 1, 2, 3, 4
};
const float [][NUM_SAMPLES] WEIGHTS =
{
	// Sigma 8.0
	{ 
		0.103201,	0.108994,	0.11333,	0.116014,	0.116923,	0.116014,	0.11333,	0.108994,	0.103201
	},
};


void main(void)
{
	vec2 uv = gl_FragCoord.xy;

	float denominator = 0.0f;
	vec4 outputColor = vec4(0.0f,0.0f,0.0f,0.0f);
	float fragDepth = texture2DRect( s_positionTexture, uv / u_lightTransportResScalar ).z;
	vec3 fragNormal = texture2DRect( s_normalTexture, uv / u_lightTransportResScalar ).xyz;
	vec4 fragColor = texture2DRect( s_texture, uv );
	
	float angleDepthMaxScalar = 1.0f + tan( (1.0f - fragNormal.z) * 1.57f );

	float[] weights = WEIGHTS[u_sigmaIndex];
	for ( int i = 0; i < NUM_SAMPLES; i++ )
	{
		float offset = OFFSETS[i];
		float weight = weights[i];

		vec2 sampleUV = (uv + u_direction * offset) / u_lightTransportResScalar;

		vec3 normalSample = texture2DRect( s_normalTexture, sampleUV ).xyz;
		float dotProduct = clamp( dot( normalSample, fragNormal ), 0.0f, 1.0f );
		float normalWeight = max( 0.001f, (dotProduct * 3.0) - 2.0f );

		float depthSample = texture2DRect( s_positionTexture, sampleUV ).z;
		float depthDiff = abs(fragDepth - depthSample);
		float depthWeight = 1.0f - min( (depthDiff * u_depthMax) / angleDepthMaxScalar, 1.0f );

		vec4 colorSample = texture2DRect( s_texture, uv + u_direction * offset );
		//vec4 colorDiff = abs(colorSample - fragColor);
		//float maxChannel = max(max(colorDiff.x, colorDiff.y), max(colorDiff.z, colorDiff.w));
		//float colorStep = 1.0f - min( maxChannel / u_colorDiffMax, 1.0f );
		
		float biasedWeight = normalWeight * depthWeight * weight;
		outputColor += colorSample * biasedWeight;
		denominator += biasedWeight;
	}
	outputColor /= denominator;

	out_fragColor = outputColor;
}
";
		}
	}
}

