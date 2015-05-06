using Kaiga.Core;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using System.Diagnostics;
using Kaiga.Textures;

namespace Kaiga.Shaders.Fragment
{
	public class LightResolveFragShader : AbstractFragmentShaderStage
	{
		RandomAngleTexture randomTexture;

		public LightResolveFragShader() : base( "LightResolveShader.frag" )
		{
			randomTexture = new RandomAngleTexture();
			randomTexture.Width = 64;
			randomTexture.Height = 64;
		}

		override public void BindPerPass( RenderParams renderParams )
		{
			base.BindPerPass( renderParams );

			SetRectangleTexture( "s_directLightBuffer", renderParams.RenderTarget.DirectLightBuffer.Texture );
			SetRectangleTexture( "s_indirectLightBuffer", renderParams.RenderTarget.IndirectLightBuffer.Texture );
			SetRectangleTexture( "s_materialBuffer", renderParams.RenderTarget.MaterialBuffer.Texture );
			//SetRectangleTexture( "s_aoBuffer", renderParams.AORenderTarget.AOBuffer.Texture );

			SetTexture2D( "s_positionBuffer", renderParams.PositionBufferMippedTexture.Texture );
			SetTexture2D( "s_normalBuffer", renderParams.NormalBufferMippedTexture.Texture );
			SetTexture2D( "s_directLightBuffer2D", renderParams.DirectLightBufferMippedTexture.Texture );
			SetTexture2D( "s_indirectLightBuffer2D", renderParams.IndirectLightBufferMippedTexture.Texture );
			SetTexture2D( "s_randomTexture", randomTexture.Texture );

			SetUniform1( "u_maxMip", renderParams.PositionBufferMippedTexture.NumMipMaps-3 );

			//float radius = (float)Mouse.GetState().X / 1000.0f;
			//Debug.WriteLine( radius );
			const float radius = 0.5f;
			SetUniform1( "radius", radius );

			//float bias = (float)Mouse.GetState().Y / 1000.0f;
			//Debug.WriteLine( bias );
			const float bias = 0.1f;
			SetUniform1( "bias", bias );

			//float falloffScalar = (float)Mouse.GetState().X / 100.0f;
			//Debug.WriteLine( falloffScalar );
			const float falloffScalar = 5.0f;
			SetUniform1( "falloffScalar", falloffScalar );

			//float q = (float)Mouse.GetState().X / 50.0f;
			//Debug.WriteLine( q );
			const float q = 60.0f;
			SetUniform1( "q", q );

			//float radiosityScalar = (float)Mouse.GetState().X / 10.0f;
			//Debug.WriteLine( radiosityScalar );
			const float radiosityScalar = 10.0f;
			SetUniform1( "u_radiosityScalar", radiosityScalar );

			//float colorBleedingBoost = (float)Mouse.GetState().X / 1000.0f;
			//Debug.WriteLine( colorBleedingBoost );
			const float colorBleedingBoost = 0.9f;
			SetUniform1( "u_colorBleedingBoost", colorBleedingBoost );

			SetUniform1( "u_aspectRatio", renderParams.CameraLens.AspectRatio );


			bool aoEnabled = Mouse.GetState().X > 200.0f;
			bool radiosityEnabled = Mouse.GetState().Y > 100.0f;
			SetUniform1( "u_aoEnabled", aoEnabled );
			SetUniform1( "u_radiosityEnabled", radiosityEnabled );

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