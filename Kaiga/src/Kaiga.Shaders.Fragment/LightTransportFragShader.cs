using Kaiga.Core;
using OpenTK.Input;
using System.Diagnostics;
using Kaiga.Textures;

namespace Kaiga.Shaders.Fragment
{
	public class LightTransportFragShader : AbstractFragmentShaderStage
	{
		readonly RandomDirectionTexture randomTexture;

		public LightTransportFragShader() : base( "LightTransportShader.frag" )
		{
			randomTexture = new RandomDirectionTexture();
			randomTexture.Width = 8;
			randomTexture.Height = 8;
		}

		override public void BindPerPass( RenderParams renderParams )
		{
			base.BindPerPass( renderParams );
			
			SetTexture2D( "s_positionBuffer", renderParams.PositionTexture2D.Texture );
			SetTexture2D( "s_normalBuffer", renderParams.NormalTexture2D.Texture );
			SetTexture2D( "s_directLightBuffer2D", renderParams.DirectLightTexture2D.Texture );
			SetTexture2D( "s_indirectLightBuffer2D", renderParams.IndirectLightTexture2D.Texture );
			SetTexture2D( "s_randomTexture", randomTexture.Texture );
			SetRectangleTexture( "s_material", renderParams.MaterialTextureRect.Texture );

			SetUniformMatrix4( "u_projectionMatrix", ref renderParams.ProjectionMatrix );

			SetUniform1( "u_maxMip", renderParams.PositionTexture2D.NumMipMaps-4 );

			SetUniform1( "u_lightTransportResolutionScalar", renderParams.LightTransportResolutionScalar );

			//float radius = (float)Mouse.GetState().X / 1000.0f;
			//Debug.WriteLine( radius );
			const float radius = 0.2f;
			SetUniform1( "radius", radius );

			//float bias = (float)Mouse.GetState().Y / 100.0f;
			//Debug.WriteLine( bias );
			const float bias = 0.4f;
			SetUniform1( "bias", bias );

			//float falloffScalar = (float)Mouse.GetState().X / 100.0f;
			//Debug.WriteLine( falloffScalar );
			const float falloffScalar = 10000.0f;
			SetUniform1( "falloffScalar", falloffScalar );

			//float q = (float)Mouse.GetState().X / 50000.0f;
			//Debug.WriteLine( q );
			const float q = 0.006f;
			SetUniform1( "q", q );

			//float epsilon = (float)Mouse.GetState().X / 500.0f;
			//Debug.WriteLine( epsilon );
			const float epsilon= 0.03f;
			SetUniform1( "epsilon", epsilon );

			//float radiosityScalar = (float)Mouse.GetState().X / 10.0f;
			//Debug.WriteLine( radiosityScalar );
			const float radiosityScalar = 2.0f;
			SetUniform1( "u_radiosityScalar", radiosityScalar );

			//float colorBleedingBoost = (float)Mouse.GetState().X / 1000.0f;
			//Debug.WriteLine( colorBleedingBoost );
			const float colorBleedingBoost = 0.25f;
			SetUniform1( "u_colorBleedingBoost", colorBleedingBoost );

			SetUniform1( "u_aspectRatio", renderParams.CameraLens.AspectRatio );


			bool aoEnabled = Mouse.GetState().X > 200.0f;
			bool radiosityEnabled = Mouse.GetState().Y > 100.0f;
			SetUniform1( "u_aoEnabled", aoEnabled );
			SetUniform1( "u_radiosityEnabled", radiosityEnabled );


			//const int numSteps = 32;
			//SetUniform1( "u_numSteps", numSteps );

			//int maxSteps = Mouse.GetState().X / 50;
			//Debug.WriteLine( maxSteps );
			//const int maxSteps =8;
			//SetUniform1( "maxSteps", maxSteps );

			//int numBinarySearchSteps = Mouse.GetState().Y / 50;
			//Debug.WriteLine( numBinarySearchSteps );
			//const int numBinarySearchSteps = 16;
			//SetUniform1( "numBinarySearchSteps", numBinarySearchSteps );

			//float roughnessJitter = (float)Mouse.GetState().Y / 1000.0f;
			//Debug.WriteLine( roughnessJitter );
			const float roughnessJitter = 0.1f;
			SetUniform1( "u_roughnessJitter", roughnessJitter );

			//float zDistanceMin = (float)Mouse.GetState().Y / 2000.0f;
			//Debug.WriteLine( zDistanceMin );
			//const float zDistanceMin = 1.0f;
			//SetUniform1( "u_zDistanceMin", zDistanceMin );

			Debug.WriteLine( "" );
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
	}
}