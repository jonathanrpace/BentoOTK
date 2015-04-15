using System;
using OpenTK.Graphics.OpenGL;

namespace Kaiga.Geom
{
	public class SkyboxGeometry : Geometry
	{
		protected override void onValidate()
		{
			vertexArrayBuffer = GL.GenVertexArray();
			GL.BindVertexArray( vertexArrayBuffer );

			vertexBuffers = new int[4];
			GL.GenBuffers( vertexBuffers.Length, vertexBuffers );

			GL.EnableVertexAttribArray(Attribute.Position);
			GL.BindBuffer( BufferTarget.ArrayBuffer, vertexBuffers[ Attribute.Position ] );
			GL.VertexAttribPointer( Attribute.Position, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0 );

			GL.BindBuffer( BufferTarget.ArrayBuffer, 0 );
			GL.BindVertexArray( 0 );

			indexBuffers = new int[1];
			GL.GenBuffers( indexBuffers.Length, indexBuffers );

			var positions = new float[] 
			{ 
				-1,  1, -1,
				 1,  1, -1,
				-1, -1, -1,
				 1, -1, -1,
				-1,  1,  1,
				 1,  1,  1,
				-1, -1,  1,
				 1, -1,  1 
			};
			NumVertices = positions.Length / 3;
			BufferVertexData( Attribute.Position, ref positions, sizeof(float) );

			var indices = new int[] 
			{ 
				2, 1, 0,
				3, 1, 2,

				7, 4, 5,
				6, 4, 7,

				6, 0, 4,
				2, 0, 6,

				3, 5, 1,
				7, 5, 3,

				6, 3, 2,
				7, 3, 6,

				0, 5, 4,
				1, 5, 0 
			};
			NumIndices = indices.Length;
			BufferIndexData( 0, ref indices );
		}
	}
}

