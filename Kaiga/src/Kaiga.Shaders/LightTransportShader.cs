using OpenTK.Graphics.OpenGL4;
using Kaiga.Core;
using Kaiga.Geom;
using Kaiga.Shaders.Vertex;
using Kaiga.Shaders.Fragment;
using Kaiga.Textures;
using OpenTK.Input;
using System.Diagnostics;
using System;
using System.Timers;

namespace Kaiga.Shaders
{
	public class LightTransportShader : AbstractShader<ScreenQuadVertexShader, LightTransportFragShader>
	{
		readonly ScreenQuadGeometry screenQuadGeom;
		readonly DepthAwareBlurShader blurShader;

		public LightTransportShader()
		{
			screenQuadGeom = new ScreenQuadGeometry();
			blurShader = new DepthAwareBlurShader();
		}

		override public void Dispose()
		{
			base.Dispose();
			screenQuadGeom.Dispose();
			blurShader.Dispose();
		}

		public void Render()
		{
			RenderParams.LightTransportRenderTarget.SwapRadiosityAndAOTextures();
			RenderParams.LightTransportRenderTarget.BindForLightTransport();

			// Perform light transport shader
			BindPerPass();
			vertexShader.BindPerPass();
			fragmentShader.BindPerPass();
			screenQuadGeom.Bind();
			screenQuadGeom.Draw();



			RenderParams.LightTransportRenderTarget.BindForRadiosityAndAOBlurX();
			blurShader.Render( 
				RenderParams.LightTransportRenderTarget.GetCurrentRadiosityAndAOTexture().Texture, 
				RenderParams.RenderTarget.PositionBuffer.Texture, 
				RenderParams.RenderTarget.NormalBuffer.Texture,
				1.0f, 0.0f, 0 );
			RenderParams.LightTransportRenderTarget.BindForRadiosityAndAOBlurY();
			blurShader.Render( 
				RenderParams.LightTransportRenderTarget.BlurBufferTextureRect.Texture, 
				RenderParams.RenderTarget.PositionBuffer.Texture, 
				RenderParams.RenderTarget.NormalBuffer.Texture,
				0.0f, 1.0f, 0 );
			



			RenderParams.LightTransportRenderTarget.BindForReflectionBlurX();
			blurShader.Render( 
				RenderParams.LightTransportRenderTarget.GetCurrentReflectionTexture().Texture, 
				RenderParams.RenderTarget.PositionBuffer.Texture, 
				RenderParams.RenderTarget.NormalBuffer.Texture,
				1.0f, 0.0f, 0 );
			RenderParams.LightTransportRenderTarget.BindForReflectionBlurY();
			blurShader.Render( 
				RenderParams.LightTransportRenderTarget.BlurBufferTextureRect.Texture, 
				RenderParams.RenderTarget.PositionBuffer.Texture, 
				RenderParams.RenderTarget.NormalBuffer.Texture,
				0.0f, 1.0f, 0 );
			
		}
	}

	public class LightTransportFragShader : AbstractFragmentShaderStage
	{
		readonly RandomDirectionTexture randomTexture;

		long frameCounter = 0;

		public LightTransportFragShader() : base( "LightingLib.frag", "LightTransportShader.frag" )
		{
			randomTexture = new RandomDirectionTexture();
			randomTexture.Width = 32;
			randomTexture.Height = 32;
		}

		override public void BindPerPass()
		{
			base.BindPerPass();

			SetTexture2D( "s_positionBuffer", RenderParams.PositionTexture2D.Texture );
			SetTexture2D( "s_prevPositionBuffer", RenderParams.PrevPositionTexture2D.Texture );
			SetTexture2D( "s_normalBuffer", RenderParams.NormalTexture2D.Texture );
			SetTexture2D( "s_directLightBuffer2D", RenderParams.DirectLightTexture2D.Texture );
			SetTexture2D( "s_indirectLightBuffer2D", RenderParams.IndirectLightTexture2D.Texture );
			SetTexture2D( "s_randomTexture", randomTexture.Texture );
			SetRectangleTexture( "s_material", RenderParams.MaterialTextureRect.Texture );
			SetRectangleTexture( "s_albedo", RenderParams.AlbedoTextureRect.Texture );
			SetRectangleTexture( "s_prevBounceAndAo", RenderParams.LightTransportRenderTarget.GetPreviousRadiosityAndAOTexture().Texture );
			SetRectangleTexture( "s_prevReflection", RenderParams.LightTransportRenderTarget.GetPreviousReflectionTexture().Texture );


			SetUniformMatrix4( "u_projectionMatrix", ref RenderParams.ProjectionMatrix );
			SetUniformMatrix4( "u_invViewMatrix", ref RenderParams.InvViewMatrix );
			SetUniformMatrix4( "u_prevViewProjectionMatrix", ref RenderParams.PrevViewProjectionMatrix );
			SetUniformMatrix4( "u_prevInvViewProjectionMatrix", ref RenderParams.PrevInvViewProjectionMatrix );


			SetUniform1( "u_maxMip", RenderParams.PositionTexture2D.NumMipMaps-4 );

			SetUniform1( "u_lightTransportResolutionScalar", RenderParams.LightTransportResolutionScalar );
			SetUniform1( "u_aspectRatio", RenderParams.CameraLens.AspectRatio );

			float time = (float)frameCounter / 8;
			SetUniform1( "u_time", time  );
			frameCounter++;
			frameCounter = frameCounter > 8 ? 0 : frameCounter;

			//float radius = Math.Abs( (float)Mouse.GetState().Y / 1000.0f );
			//Debug.WriteLine( "radius: " + radius );
			//SetUniform1( "u_radius", radius );

			//float u_aoAttenutationPower = Math.Abs( (float)Mouse.GetState().X / 500.0f );
			//Debug.WriteLine( "u_aoAttenutationPower:" + u_aoAttenutationPower );
			//SetUniform1( "u_aoAttenutationPower", u_aoAttenutationPower );

			//float u_aoAttenutationScale = Math.Abs( (float)Mouse.GetState().Y / 200.0f );
			//Debug.WriteLine( "u_aoAttenutationScale:" + u_aoAttenutationScale );		
			//SetUniform1( "u_aoAttenutationScale", u_aoAttenutationScale );

			//SetUniform1( "u_flag", Mouse.GetState().X > 500  );

			//float radiosityScalar = (float)Mouse.GetState().X / 10.0f;
			//Debug.WriteLine( radiosityScalar );
			//SetUniform1( "u_radiosityScalar", radiosityScalar );

			//float sampleMixrate = (float)Mouse.GetState().X / 1000.0f;
			//Debug.WriteLine( sampleMixrate );
			//SetUniform1( "SAMPLE_MIX_RATE", sampleMixrate );

			//float colorBleedingBoost = (float)Mouse.GetState().X / 1000.0f;
			//Debug.WriteLine( colorBleedingBoost );
			//SetUniform1( "u_colorBleedingBoost", colorBleedingBoost );

			//int numBinarySearchSteps = Mouse.GetState().Y / 50;
			//Debug.WriteLine( numBinarySearchSteps );
			//const int numBinarySearchSteps = 16;
			//SetUniform1( "NUM_BINARY_SERACH_STEPS", numBinarySearchSteps );

			//float bias = 1.0f + (float)Mouse.GetState().X / 2000.0f;
			//Debug.WriteLine( "bias : " + bias );
			//SetUniform1( "BIAS", bias );

			//float rayStepScalar = 1.0f + (float)Mouse.GetState().X / 2000.0f;
			//Debug.WriteLine( rayStepScalar );
			//const int numBinarySearchSteps = 16;
			//SetUniform1( "RAY_STEP_SCALAR", rayStepScalar );

			//float roughnessJitter = (float)Mouse.GetState().Y / 5000.0f;
			//Debug.WriteLine( roughnessJitter );
			//SetUniform1( "u_roughnessJitter", roughnessJitter );

			//float maxReflectDepthDiff = (float)Mouse.GetState().Y / 2000.0f;
			//Debug.WriteLine( maxReflectDepthDiff );
			//const float maxReflectDepthDiff = 0.01f;
			//SetUniform1( "u_maxReflectDepthDiff", maxReflectDepthDiff );

			//float nominalReflectDepthDiff = (float)Mouse.GetState().X / 1000.0f;
			//Debug.WriteLine( nominalReflectDepthDiff );
			//SetUniform1( "u_nominalReflectDepthDiff", nominalReflectDepthDiff );
		}
	}
}

