﻿using System;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Geom;
using Kaiga.Core;
using Kaiga.Shaders.Vertex;
using Kaiga.Shaders.Fragment;

namespace Kaiga.Shaders
{
	public class TextureRectToScreenShader : AbstractShader<ScreenQuadVertexShader, RectangleTextureFragShader>
	{
		private readonly ScreenQuadGeometry screenQuadGeom;

		public TextureRectToScreenShader()
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

			ActivateFragmentShader();
			fragmentShader.SetTexture( source );
			
			screenQuadGeom.Bind();
			screenQuadGeom.Draw();
			screenQuadGeom.Unbind();
			
			UnbindPerPass();
		}
	}
}

