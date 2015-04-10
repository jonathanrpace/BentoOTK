using OpenTK.Graphics.OpenGL4;
using System;
using Kaiga.Core;

namespace Kaiga.Geom
{
	public static class Attribute
	{
		public const int Position = 0;
		public const int Normal = 1;
		public const int Uv = 2;
		public const int Color = 3;
	}

	public abstract class Geometry : AbstractValidatable, IGeometry
	{
		protected int vertexArrayBuffer;
		protected int[] vertexBuffers;
		protected int[] indexBuffers;

		public int NumVertices { get; protected set; }
		public int NumTriangles { get; protected set; }
		public int NumIndices { get; protected set; }

		override protected void onInvalidate()
		{
			if ( GL.IsVertexArray( vertexArrayBuffer ) )
			{
				GL.DeleteVertexArray( vertexArrayBuffer );
				GL.DeleteBuffers( vertexBuffers.Length, vertexBuffers );
				GL.DeleteBuffers( indexBuffers.Length, indexBuffers );
			}
		}
		
		public void Bind()
		{
			validate();

			GL.BindVertexArray( vertexArrayBuffer );
			GL.BindBuffer( BufferTarget.ElementArrayBuffer, indexBuffers[ 0 ] );
		}

		public void Unbind()
		{ 
			GL.BindVertexArray( 0 );
			GL.BindBuffer( BufferTarget.ElementArrayBuffer, 0 );
		}

		protected void BufferVertexData<T>( int attributeIndex, ref T[] data, int dataSize ) where T : struct
		{
			GL.BindBuffer( BufferTarget.ArrayBuffer, vertexBuffers[ attributeIndex ] );
			GL.BufferData<T>
			(
				BufferTarget.ArrayBuffer, 
				new IntPtr( dataSize * data.Length ), 
				data, 
				BufferUsageHint.StaticDraw 
			);
			GL.BindBuffer( BufferTarget.ArrayBuffer, 0 );
		}

		protected void BufferIndexData( int index, ref int[] indices )
		{
			GL.BindBuffer( BufferTarget.ElementArrayBuffer, indexBuffers[ index ] );
			GL.BufferData<int>
			(
				BufferTarget.ElementArrayBuffer,
				new IntPtr( indices.Length * sizeof(int) ),
				indices,
				BufferUsageHint.StaticDraw
			);
			GL.BindBuffer( BufferTarget.ElementArrayBuffer, 0 );
		}
	}
}

