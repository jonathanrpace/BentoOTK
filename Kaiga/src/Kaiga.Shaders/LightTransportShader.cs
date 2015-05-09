using OpenTK.Graphics.OpenGL4;
using Kaiga.Core;
using Kaiga.Geom;
using Kaiga.Shaders.Vertex;
using Kaiga.Shaders.Fragment;
using System.Diagnostics;
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
			renderParams.AORenderTarget.BindForAOPhase();

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
			
			// Blur result to remove noise
			//float radius = (float)Mouse.GetState().X / 200.0f;
			//Debug.WriteLine( radius );
			//radius *= renderParams.LightTransportResolutionScalar;
			float radius = 1.25f * renderParams.LightTransportResolutionScalar;
			for ( var i = 0; i < 6; i++ )
			{
				renderParams.AORenderTarget.BindForBlurA();
				blurShader.Render( renderParams, renderParams.AORenderTarget.AOBuffer.Texture, renderParams.RenderTarget.PositionBuffer.Texture, radius, 0.0f );
				renderParams.AORenderTarget.BindForBlurB();
				blurShader.Render( renderParams, renderParams.AORenderTarget.AOBlurBuffer.Texture, renderParams.RenderTarget.PositionBuffer.Texture, 0.0f, radius );
				radius += 2.25f * renderParams.LightTransportResolutionScalar;
			}
		}
	}
}

