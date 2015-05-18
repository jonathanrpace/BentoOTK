using System;
using Kaiga.Core;
using System.Diagnostics;
using OpenTK.Input;
using Kaiga.Textures;

namespace Kaiga.Shaders.Fragment
{
	public class ScreenSpaceReflectionFragShader : AbstractFragmentShaderStage
	{
		readonly RandomDirectionTexture randomTexture;

		public ScreenSpaceReflectionFragShader() : base( "ScreenSpaceReflectionShader.frag" )
		{
			randomTexture = new RandomDirectionTexture();
			randomTexture.Width = 32;
			randomTexture.Height = 32;
		}

		public override void Dispose()
		{
			base.Dispose();
			randomTexture.Dispose();
		}

		public override void BindPerPass(RenderParams renderParams)
		{
			SetTexture2D( "s_positionBuffer", renderParams.PositionTexture2D.Texture );
			SetTexture2D( "s_normalBuffer", renderParams.NormalTexture2D.Texture );
			SetTexture2D( "s_directLightBuffer2D", renderParams.DirectLightTexture2D.Texture );
			SetTexture2D( "s_indirectLightBuffer2D", renderParams.IndirectLightTexture2D.Texture );
			SetTexture2D( "s_randomTexture", randomTexture.Texture );
			SetRectangleTexture( "s_material", renderParams.MaterialTextureRect.Texture );

			SetUniformMatrix4( "u_projectionMatrix", ref renderParams.ProjectionMatrix );

			SetUniform1( "u_lightTransportResolutionScalar", renderParams.LightTransportResolutionScalar );

			//float maxReflectDepthDiff = (float)Mouse.GetState().X / 500.0f;
			//Debug.WriteLine( maxReflectDepthDiff );
			const float maxReflectDepthDiff = 0.1f;
			SetUniform1( "u_maxReflectDepthDiff", maxReflectDepthDiff );

			//int numBinarySearchSteps = Mouse.GetState().Y / 50;
			//Debug.WriteLine( numBinarySearchSteps );
			//const int numBinarySearchSteps = 16;
			//SetUniform1( "numBinarySearchSteps", numBinarySearchSteps );

			//float roughnessJitter = (float)Mouse.GetState().Y / 1000.0f;
			//Debug.WriteLine( roughnessJitter );
			const float roughnessJitter = 0.0f;
			SetUniform1( "u_roughnessJitter", roughnessJitter );

			//float rayStep = (float)Mouse.GetState().X / 10000.0f;
			//Debug.WriteLine( rayStep );
			//SetUniform1( "rayStep", rayStep );
		}
	}
}

