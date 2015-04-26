﻿using System;
using Kaiga.ShaderStages;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Core;
using Kaiga.Geom;

namespace Kaiga.Shaders
{
	public class LightResolveShader : AbstractShader
	{
		readonly new ScreenQuadVertexShader vertexShader;
		readonly new LightResolveFragShader fragmentShader;
		readonly ScreenQuadGeometry screenQuadGeom;

		public LightResolveShader() : 
		base( new ScreenQuadVertexShader(), new LightResolveFragShader() )
		{
			vertexShader = (ScreenQuadVertexShader)base.vertexShader;
			fragmentShader = (LightResolveFragShader)base.fragmentShader;

			screenQuadGeom = new ScreenQuadGeometry();
		}

		override public void Dispose()
		{
			base.Dispose();
			screenQuadGeom.Dispose();
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
		}
	}
}

