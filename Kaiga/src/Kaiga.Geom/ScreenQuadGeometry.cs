﻿using System;
using OpenTK.Graphics.OpenGL4;

namespace Kaiga.Geom
{
	public class ScreenQuadGeometry : AbstractGeometry, IGeometry
	{
		override protected void onValidate()
		{
			vertexArrayBuffer = GL.GenVertexArray();
			GL.BindVertexArray( vertexArrayBuffer );

			vertexBuffers = new int[4];
			GL.GenBuffers( vertexBuffers.Length, vertexBuffers );

			GL.EnableVertexAttribArray(Attribute.Position);
			GL.BindBuffer( BufferTarget.ArrayBuffer, vertexBuffers[ Attribute.Position ] );
			GL.VertexAttribPointer( Attribute.Position, 2, VertexAttribPointerType.Float, false, sizeof(float) * 2, 0 );

			GL.EnableVertexAttribArray(Attribute.Uv);
			GL.BindBuffer( BufferTarget.ArrayBuffer, vertexBuffers[ Attribute.Uv ] );
			GL.VertexAttribPointer( Attribute.Uv, 2, VertexAttribPointerType.Float, false, sizeof(float) * 2, 0 );

			GL.BindBuffer( BufferTarget.ArrayBuffer, 0 );
			GL.BindVertexArray( 0 );

			indexBuffers = new int[1];
			GL.GenBuffers( indexBuffers.Length, indexBuffers );

			NumVertices = 4;
			NumIndices = 6;

			var positions = new float[] { -1, 1,  -1, -1,  1, -1,  1, 1 };
			var uvs = new float[] { 0, 1,  0, 0,  1, 0,  1, 1 };
			var indices = new int[] { 0, 1, 3, 3, 1, 2 };

			BufferVertexData( Attribute.Position, ref positions, sizeof(float) );
			BufferVertexData( Attribute.Uv, ref uvs, sizeof(float) );
			BufferIndexData( 0, ref indices );
		}

		public void Draw()
		{
			GL.DrawElements( PrimitiveType.Triangles, NumIndices, DrawElementsType.UnsignedInt, IntPtr.Zero ); 
		}
	}
}

