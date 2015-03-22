using System;
using Kaiga.ShaderStages;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Geom;
using Kaiga.Core;

namespace Kaiga.Shaders
{
	public class TextureOutputShader : AbstractShader
	{
		private new readonly ScreenQuadVertexShader vertexShader;
		private new readonly TextureOutputFragShader fragmentShader;
		private readonly ScreenQuadGeometry screenQuadGeom;

		public TextureOutputShader() : base( new ScreenQuadVertexShader(), new TextureOutputFragShader() )
		{
			vertexShader = (ScreenQuadVertexShader)base.vertexShader;
			fragmentShader = (TextureOutputFragShader)base.fragmentShader;
			screenQuadGeom = new ScreenQuadGeometry();
		}

		#region IGraphicsContextDependant implementation

		override public void CreateGraphicsContextResources()
		{
			base.CreateGraphicsContextResources();
			screenQuadGeom.CreateGraphicsContextResources();
		}

		override public void DisposeGraphicsContextResources()
		{
			base.DisposeGraphicsContextResources();
			screenQuadGeom.DisposeGraphicsContextResources();
		}

		#endregion

		public void Render( RenderParams renderParams, int texture )
		{
			BindPerPass( renderParams );

			GL.ActiveShaderProgram( pipeline, fragmentShader.ShaderProgram );
			fragmentShader.Bind( texture );
			
			screenQuadGeom.Bind();

			GL.DrawElements( PrimitiveType.Triangles, screenQuadGeom.NumIndices, DrawElementsType.UnsignedInt, IntPtr.Zero ); 


			UnbindPerPass();
		}
	}
}

