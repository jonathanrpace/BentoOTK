using OpenTK.Graphics.OpenGL4;
using Kaiga.Core;
using Kaiga.Geom;
using Kaiga.Shaders.Vertex;
using Kaiga.Shaders.Fragment;

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
			BindPipeline( renderParams );
			GL.ActiveShaderProgram( pipeline, vertexShader.ShaderProgram );
			vertexShader.BindPerPass( renderParams );
			GL.ActiveShaderProgram( pipeline, fragmentShader.ShaderProgram );
			fragmentShader.BindPerPass( renderParams );
			screenQuadGeom.Bind();
			screenQuadGeom.Draw();
			screenQuadGeom.Unbind();
			UnbindPipeline();
			
			renderParams.LightTransportRenderTarget.BindForBlurA();
			blurShader.Render( renderParams, renderParams.LightTransportRenderTarget.RadiosityAndAOTextureRect.Texture, renderParams.RenderTarget.PositionBuffer.Texture, 1.0f, 0.0f );
			renderParams.LightTransportRenderTarget.BindForBlurB();
			blurShader.Render( renderParams, renderParams.LightTransportRenderTarget.BlurBufferTextureRect.Texture, renderParams.RenderTarget.PositionBuffer.Texture, 0.0f, 1.0f );

		}
	}
}

