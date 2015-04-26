using OpenTK.Graphics.OpenGL4;
using OpenTK;
using Kaiga.Core;

namespace Kaiga.Shaders.Fragment
{
	public class BoxBlurFragShader : AbstractFragmentShaderStage
	{
		public void Bind( RenderParams renderParams, int texture, float radiusU, float radiusV )
		{
			SetUniform2( "u_radius", new Vector2( radiusU, radiusV ) );
			SetUniformTexture( 0, "s_texture", texture, TextureTarget.TextureRectangle );

			//float depthCutoff = Mouse.GetState().X / 5000.0f;
			//Debug.WriteLine( depthCutoff.ToString() );
			SetUniform1( "u_depthCutoff", 0.02f );
		}

		protected override string GetShaderSource()
		{
			return @"
#version 450 core

// Samplers
uniform sampler2DRect s_texture;

// Uniforms
uniform vec2 u_radius;
uniform float u_depthCutoff;

// Outputs
layout( location = 0 ) out vec2 out_fragColor;

void main(void)
{
	float accumulator = 0.0f;
	float numSamples = 1.0f;
	//float depthCutoff = 0.00001f;

	// Simply add sampleA to the average.
	vec4 sampleA = texture2DRect( s_texture, gl_FragCoord.xy );
	accumulator += sampleA.x;

	// The next two samples have a check - if their depth is too different from this fragment's depth
	// disregard them.
	vec4 sampleB = texture2DRect( s_texture, gl_FragCoord.xy - u_radius );
	float depthDiffB = abs(sampleA.y - sampleB.y);
	float stepB = step( depthDiffB, u_depthCutoff );
	accumulator += (sampleB.x * stepB);
	numSamples += stepB;

	vec4 sampleC = texture2DRect( s_texture, gl_FragCoord.xy + u_radius );
	float depthDiffC = abs(sampleA.y - sampleC.y);
	float stepC = step( depthDiffC, u_depthCutoff );
	accumulator += (sampleC.x * stepC);
	numSamples += stepC;
	

	out_fragColor = vec2( accumulator / numSamples, sampleA.y );
}

";
		}
	}
}

