using OpenTK.Graphics.OpenGL4;

namespace Kaiga.Geom
{
	public static class Attribute
	{
		public const int Position = 0;
		public const int Normal = 1;
		public const int Uv = 2;
		public const int Color = 3;
	}

	public abstract class Geometry : IGeometry
	{
		protected bool invalid = true;

		protected int vertexArrayBuffer;
		protected int[] vertexBuffers;
		protected int[] indexBuffers;

		public int NumVertices { get; protected set; }
		public int NumTriangles { get; protected set; }
		public int NumIndices { get; protected set; }
		
		#region IGLContextDependant implementation

		public virtual void CreateGraphicsContextResources()
		{
			invalid = true;
		}

		public virtual void DisposeGraphicsContextResources()
		{
			if ( GL.IsVertexArray( vertexArrayBuffer ) )
			{
				GL.DeleteVertexArray( vertexArrayBuffer );
				GL.DeleteBuffers( vertexBuffers.Length, vertexBuffers );
				GL.DeleteBuffers( indexBuffers.Length, indexBuffers );
			}
			invalid = true;
		}

		#endregion
		
		public void Bind()
		{
			if ( invalid )
			{
				Validate();
				invalid = false;
			}

			GL.BindVertexArray( vertexArrayBuffer );
			GL.BindBuffer( BufferTarget.ElementArrayBuffer, indexBuffers[ 0 ] );
		}

		public void Unbind()
		{
			GL.BindVertexArray( 0 );
			GL.BindBuffer( BufferTarget.ElementArrayBuffer, 0 );
		}
	
		protected abstract void Validate();
	}
}

