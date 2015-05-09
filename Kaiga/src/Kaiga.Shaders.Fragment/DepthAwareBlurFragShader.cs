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
			SetUniform2( "u_radius", new Vector2( radiusU, radiusV ) );
			SetTexture( "s_texture", texture, TextureTarget.TextureRectangle );
			SetTexture( "s_positionTexture", positionTexture, TextureTarget.TextureRectangle );

			//float depthCutoff = Mouse.GetState().X / 5000.0f;
			//Debug.WriteLine( depthCutoff.ToString() );
			SetUniform1( "u_depthMin", 0.00f );

			//float depthMax = Mouse.GetState().X / 2000.0f;
			//Debug.WriteLine( depthMax.ToString() );
			SetUniform1( "u_depthMax", 0.06f );

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
uniform vec2 u_radius;
uniform float u_depthMin;
uniform float u_depthMax;
uniform float u_lightTransportResScalar;

// Outputs
layout( location = 0 ) out vec4 out_fragColor;

void main(void)
{
	vec4 accumulator = vec4( 0.0f, 0.0f, 0.0f, 0.0f );
	float numSamples = 1.0f;

	// Simply add sampleA to the average.
	vec4 sampleA = texture2DRect( s_texture, gl_FragCoord.xy );
	float depthA = texture2DRect( s_positionTexture, gl_FragCoord.xy / u_lightTransportResScalar ).z;
	accumulator += sampleA;

	// The next two samples have a check - if their depth is too different from this fragment's depth
	// disregard them.

	vec4 sampleB = texture2DRect( s_texture, gl_FragCoord.xy - u_radius );
	float depthB = texture2DRect( s_positionTexture, (gl_FragCoord.xy - u_radius) / u_lightTransportResScalar ).z;
	float depthDiffB = abs(depthA - depthB);
	float stepB = smoothstep( u_depthMax, u_depthMin, depthDiffB );
	accumulator += (sampleB * stepB);
	numSamples += stepB;

	vec4 sampleC = texture2DRect( s_texture, gl_FragCoord.xy + u_radius );
	float depthC = texture2DRect( s_positionTexture, (gl_FragCoord.xy + u_radius) / u_lightTransportResScalar ).z;
	float depthDiffC = abs(depthA - depthC);
	float stepC = smoothstep( u_depthMax, u_depthMin, depthDiffC );
	accumulator += (sampleC * stepC);
	numSamples += stepC;
	
	out_fragColor = accumulator / numSamples;
}

";
		}
	}
}

