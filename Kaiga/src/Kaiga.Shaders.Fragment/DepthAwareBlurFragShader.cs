using OpenTK.Graphics.OpenGL4;
using OpenTK;
using Kaiga.Core;
using OpenTK.Input;
using System.Diagnostics;

namespace Kaiga.Shaders.Fragment
{
	public class DepthAwareBlurFragShader : AbstractFragmentShaderStage
	{
		public void Bind( RenderParams renderParams, int texture, int positionTexture, float radiusU, float radiusV )
		{
			SetUniform2( "u_direction", new Vector2( radiusU, radiusV ) );

			SetTexture( "s_texture", texture, TextureTarget.TextureRectangle );
			SetTexture( "s_positionTexture", positionTexture, TextureTarget.TextureRectangle );

			//float depthMax = Mouse.GetState().X / 5000.0f;
			//Debug.WriteLine( depthMax.ToString() );
			SetUniform1( "u_depthMax", 0.15f );

			//float colorDiffMax = Mouse.GetState().X / 500.0f;
			//Debug.WriteLine( colorDiffMax.ToString() );
			const float colorDiffMax = 0.5f;
			SetUniform1( "u_colorDiffMax", colorDiffMax );

			//float colorDiffMax = Mouse.GetState().X / 500.0f;
			//Debug.WriteLine( "colorDiffMax: " + colorDiffMax.ToString() );
			//const float colorDiffMax = 0.5f;
			//SetUniform1( "u_colorDiffMax", colorDiffMax );

			SetUniform1( "u_lightTransportResScalar", renderParams.LightTransportResolutionScalar );
		}

		protected override string GetShaderSource()
		{
			return @"
#version 450 core

// Samplers
uniform sampler2DRect s_texture;
uniform sampler2DRect s_positionTexture;

// Uniforms
uniform vec2 u_direction;
uniform float u_depthMax;
uniform float u_colorDiffMax;
uniform float u_lightTransportResScalar;

// Outputs
layout( location = 0 ) out vec4 out_fragColor;

const int NUM_SAMPLES = 17;
const float[] OFFSETS = 
{
	-8, -7, -6, -5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5, 6, 7, 8
};
const float[] WEIGHTS = 
{ 
	0.042481,	0.047756,	0.052854,	0.05759,	0.06178,	0.065248,	0.067844,	0.06945,
	0.069994,	0.06945,	0.067844,	0.065248,	0.06178,	0.05759,	0.052854,	0.047756,	0.042481

};


void main(void)
{
	vec2 uv = gl_FragCoord.xy;

	float denominator = 0.0f;
	vec4 outputColor = vec4(0.0f,0.0f,0.0f,0.0f);
	float fragDepth = texture2DRect( s_positionTexture, uv / u_lightTransportResScalar ).z;
	vec4 fragColor = texture2DRect( s_texture, uv );

	for ( int i = 0; i < NUM_SAMPLES; i++ )
	{
		float offset = OFFSETS[i];
		float weight = WEIGHTS[i];

		vec4 colorSample = texture2DRect( s_texture, uv + u_direction * offset );
		float depthSample = texture2DRect( s_positionTexture, (uv + u_direction * offset) / u_lightTransportResScalar ).z;

		float depthDiff = fragDepth - depthSample;
		float depthMax = u_depthMax * ( 1.0f );// + ddx + ddy );
		float depthStep = 1.0f - smoothstep( 0.0f, depthMax, depthDiff );

		vec4 colorDiff = abs(colorSample - fragColor);
		float colorStep = 1.0f - smoothstep( 0.0f, u_colorDiffMax, max(max(colorDiff.x, colorDiff.y), max(colorDiff.z, colorDiff.w)) );

		outputColor += colorSample * weight * depthStep * colorStep;
		denominator += weight * depthStep * colorStep;
	}
	outputColor /= denominator;

	out_fragColor = outputColor;
}
";
		}
	}
}

