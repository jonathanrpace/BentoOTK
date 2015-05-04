﻿using System;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Geom;
using Kaiga.Core;
using Kaiga.Shaders.Vertex;
using Kaiga.Shaders.Fragment;

namespace Kaiga.Shaders
{
	public class ScreenQuadTextureRectShader : AbstractShader<ScreenQuadVertexShader, RectangleTextureFragShader>
	{
		private readonly ScreenQuadGeometry screenQuadGeom;

		public ScreenQuadTextureRectShader()
		{
			screenQuadGeom = new ScreenQuadGeometry();
		}

		override public void Dispose()
		{
			base.Dispose();
			screenQuadGeom.Dispose();
		}
		
		public void Render( RenderParams renderParams, int source )
		{
			BindPerPass( renderParams );

			GL.ActiveShaderProgram( pipeline, fragmentShader.ShaderProgram );
			fragmentShader.SetTexture( source );
			
			screenQuadGeom.Bind();

			GL.DrawElements( PrimitiveType.Triangles, screenQuadGeom.NumIndices, DrawElementsType.UnsignedInt, IntPtr.Zero ); 


			UnbindPerPass();
		}
	}
}

