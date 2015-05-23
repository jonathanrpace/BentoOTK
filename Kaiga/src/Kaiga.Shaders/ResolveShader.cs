﻿using System;
using Kaiga.Shaders.Vertex;
using Kaiga.Shaders.Fragment;
using Kaiga.Core;
using Kaiga.Geom;

namespace Kaiga.Shaders
{
	public class ResolveShader : AbstractShader<ScreenQuadVertexShader, ResolveFragShader>
	{
		readonly ScreenQuadGeometry screenQuadGeom;

		public ResolveShader()
		{
			screenQuadGeom = new ScreenQuadGeometry();
		}

		public override void Dispose()
		{
			base.Dispose();
			screenQuadGeom.Dispose();
		}

		public void Render( RenderParams renderParams )
		{
			BindPipeline( renderParams );

			BindFragmentShader();
			fragmentShader.BindPerPass( renderParams );
			BindVertexShader();

			screenQuadGeom.Bind();
			screenQuadGeom.Draw();
			screenQuadGeom.Unbind();

			UnbindPipeline();
		}
	}
}
