using Kaiga.Core;
using Kaiga.Geom;
using Kaiga.Shaders.Vertex;
using Kaiga.Shaders.Fragment;
using OpenTK.Graphics.OpenGL4;
using OpenTK;

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

		public void Render( int texture, int positionTexture, float radiusU, float radiusV )
		{
			BindPerPass();
			fragmentShader.Bind( texture, positionTexture, radiusU, radiusV );
			screenQuadGeom.Bind();
			screenQuadGeom.Draw();
		}
	}

	public class DepthAwareBlurFragShader : AbstractFragmentShaderStage
	{
		public void Bind( int texture, int positionTexture, float radiusU, float radiusV )
		{
			SetUniform2( "u_direction", new Vector2( radiusU, radiusV ) );

			SetTexture( "s_texture", texture, TextureTarget.TextureRectangle );
			SetTexture( "s_positionTexture", positionTexture, TextureTarget.TextureRectangle );

			float depthMax = Mouse.GetState().Y / 5000.0f;
			Debug.WriteLine( depthMax.ToString() );
			const float depthMax = 0.015f;
			SetUniform1( "u_depthMax", depthMax );

			//float colorDiffMax = Mouse.GetState().X / 500.0f;
			//Debug.WriteLine( colorDiffMax.ToString() );
			const float colorDiffMax = 1.0f;
			SetUniform1( "u_colorDiffMax", colorDiffMax );

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

/*
const float[] WEIGHTS =
{
	0.014076,	0.022439,	0.033613,	0.047318,	0.062595,	0.077812,	0.090898,	0.099783,	
	0.102934,	0.099783,	0.090898,	0.077812,	0.062595,	0.047318,	0.033613,	0.022439,	0.014076

};
*/

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

		float depthSample = texture2DRect( s_positionTexture, (uv + u_direction * offset) / u_lightTransportResScalar ).z;
		float depthDiff = abs(fragDepth - depthSample);
		float depthmax = u_depthMax;// + (dFdx(depthSample) * u_direction.x + dFdy(depthSample) * u_direction.y) * 20.0f;
		float depthStep = 1.0f - min( depthDiff / depthmax, 1.0f );
	
		vec4 colorSample = texture2DRect( s_texture, uv + u_direction * offset );
		vec4 colorDiff = abs(colorSample - fragColor);
		float maxChannel = max(max(colorDiff.x, colorDiff.y), max(colorDiff.z, colorDiff.w));
		float colorStep = 1.0f - min( maxChannel / u_colorDiffMax, 1.0f );
		
		outputColor += colorSample * weight * depthStep;// * colorStep;
		denominator += weight * depthStep ;//* colorStep;
	}
	outputColor /= denominator;

	out_fragColor = outputColor;
}
";
		}
	}
}

