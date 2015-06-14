using OpenTK.Graphics.OpenGL4;
using Kaiga.Core;
using Kaiga.Geom;
using Kaiga.Shaders.Vertex;
using Kaiga.Shaders.Fragment;
using Kaiga.Textures;
using OpenTK.Input;

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

		public void Render( RenderParams renderParams )
		{
			renderParams.LightTransportRenderTarget.BindForLightTransport();

			// Perform light transport shader
			BindPerPass( renderParams );
			GL.ActiveShaderProgram( pipeline, vertexShader.ShaderProgram );
			vertexShader.BindPerPass( renderParams );
			GL.ActiveShaderProgram( pipeline, fragmentShader.ShaderProgram );
			fragmentShader.BindPerPass( renderParams );
			screenQuadGeom.Bind();
			screenQuadGeom.Draw();
			screenQuadGeom.Unbind();
			UnbindPerPass();
			
			renderParams.LightTransportRenderTarget.BindForBlurA();
			blurShader.Render( renderParams, renderParams.LightTransportRenderTarget.RadiosityAndAOTextureRect.Texture, renderParams.RenderTarget.PositionBuffer.Texture, 1.0f, 0.0f );
			renderParams.LightTransportRenderTarget.BindForBlurB();
			blurShader.Render( renderParams, renderParams.LightTransportRenderTarget.BlurBufferTextureRect.Texture, renderParams.RenderTarget.PositionBuffer.Texture, 0.0f, 1.0f );

		}
	}

	public class LightTransportFragShader : AbstractFragmentShaderStage
	{
		readonly RandomDirectionTexture randomTexture;

		public LightTransportFragShader() : base( "LightTransportShader.frag" )
		{
			randomTexture = new RandomDirectionTexture();
			randomTexture.Width = 128;
			randomTexture.Height = 128;
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
			SetUniform1( "u_radius", radius );

			//float falloffScalar = (float)Mouse.GetState().X / 100.0f;
			//Debug.WriteLine( falloffScalar );
			const float aoFalloffScalar = 1.0f;
			SetUniform1( "u_aoFalloffScalar", aoFalloffScalar );

			const float bounceFalloffScalar = 20.0f;
			SetUniform1( "u_bounceFalloffScalar", bounceFalloffScalar );

			SetUniform1( "u_flag", Mouse.GetState().X > 500  );

			//float radiosityScalar = (float)Mouse.GetState().X / 10.0f;
			//Debug.WriteLine( radiosityScalar );
			const float radiosityScalar = 2.0f;
			SetUniform1( "u_radiosityScalar", radiosityScalar );

			//float colorBleedingBoost = (float)Mouse.GetState().X / 1000.0f;
			//Debug.WriteLine( colorBleedingBoost );
			const float colorBleedingBoost = 0.25f;
			SetUniform1( "u_colorBleedingBoost", colorBleedingBoost );

			SetUniform1( "u_aspectRatio", renderParams.CameraLens.AspectRatio );



			//int numBinarySearchSteps = Mouse.GetState().Y / 50;
			//Debug.WriteLine( numBinarySearchSteps );
			//const int numBinarySearchSteps = 16;
			//SetUniform1( "numBinarySearchSteps", numBinarySearchSteps );

			//float roughnessJitter = (float)Mouse.GetState().Y / 5000.0f;
			//Debug.WriteLine( roughnessJitter );
			const float roughnessJitter = 0.0f;
			SetUniform1( "u_roughnessJitter", roughnessJitter );


			//float maxReflectDepthDiff = (float)Mouse.GetState().Y / 2000.0f;
			//Debug.WriteLine( maxReflectDepthDiff );
			const float maxReflectDepthDiff = 0.1f;
			SetUniform1( "u_maxReflectDepthDiff", maxReflectDepthDiff );

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

