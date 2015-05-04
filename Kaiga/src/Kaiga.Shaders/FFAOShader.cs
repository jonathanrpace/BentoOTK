using System;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Core;
using Kaiga.Geom;
using Ramen;
using OpenTK.Input;
using System.Diagnostics;
using Kaiga.Shaders.Vertex;
using Kaiga.Shaders.Fragment;

namespace Kaiga.Shaders
{
	public class FFAOShader : AbstractShader<ScreenQuadVertexShader, FFAOFragShader>
	{
		readonly ScreenQuadGeometry screenQuadGeom;

		readonly BoxBlurShader boxBlurShader;

		public FFAOShader()
		{
			screenQuadGeom = new ScreenQuadGeometry();
			boxBlurShader = new BoxBlurShader();
		}

		override public void Dispose()
		{
			base.Dispose();
			screenQuadGeom.Dispose();
			boxBlurShader.Dispose();
		}

		public void Render( RenderParams renderParams )
		{
			BindPerPass( renderParams );

			GL.ActiveShaderProgram( pipeline, vertexShader.ShaderProgram );
			//vertexShader.BindPerPass( renderParams );

			GL.ActiveShaderProgram( pipeline, fragmentShader.ShaderProgram );
			fragmentShader.BindPerPass( renderParams );

			screenQuadGeom.Bind();
			GL.DrawElements( PrimitiveType.Triangles, screenQuadGeom.NumIndices, DrawElementsType.UnsignedInt, IntPtr.Zero ); 
			screenQuadGeom.Unbind();

			UnbindPerPass();
			
			//float radius = Mouse.GetState().X / 200.0f;
			//Debug.WriteLine( radius.ToString() );
			for ( var i = 0; i < 1; i++ )
			{
				float radius = 2.5f;

				renderParams.AORenderTarget.BindForBlurA();
				boxBlurShader.Render( renderParams, renderParams.AORenderTarget.AOBuffer.Texture, radius, 0.0f );
				renderParams.AORenderTarget.BindForBlurB();
				boxBlurShader.Render( renderParams, renderParams.AORenderTarget.AOBlurBuffer.Texture, 0.0f, radius );
			}
			renderParams.AORenderTarget.BindForAOPhase();
		}
	}
}

