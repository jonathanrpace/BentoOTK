using System;
using Ramen;
using OpenTK.Graphics.OpenGL4;
using OpenTK;
using Kaiga.Util;

namespace Kaiga.Geom
{
	public class BoxGeometry : AbstractGeometry, IMultiTypeObject, IGeometry
	{
		public void Draw()
		{
			GL.DrawElements( PrimitiveType.Triangles, NumIndices, DrawElementsType.UnsignedInt, IntPtr.Zero ); 
		}

		private float _width = 1.0f;
		public float Width 
		{ 
			get { return _width; } 
			set
			{
				_width = value;
				invalidate();
			}
		}

		private float _height = 1.0f;
		public float Height 
		{ 
			get { return _height; } 
			set
			{
				_height = value;
				invalidate();
			}
		}

		private float _depth = 1.0f;
		public float Depth 
		{ 
			get { return _depth; } 
			set
			{
				_depth = value;
				invalidate();
			}
		}

		private int _subDivisions = 1;
		public int SubDivisions
		{ 
			get { return _subDivisions; } 
			set
			{
				_subDivisions = value;
				invalidate();
			}
		}
			
		override protected void onValidate()
		{
			vertexArrayBuffer = GL.GenVertexArray();
			GL.BindVertexArray( vertexArrayBuffer );

			vertexBuffers = new int[4];
			GL.GenBuffers( vertexBuffers.Length, vertexBuffers );

			GL.EnableVertexAttribArray(Attribute.Position);
			GL.BindBuffer( BufferTarget.ArrayBuffer, vertexBuffers[ Attribute.Position ] );
			GL.VertexAttribPointer( Attribute.Position, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0 );

			GL.EnableVertexAttribArray(Attribute.Normal);
			GL.BindBuffer( BufferTarget.ArrayBuffer, vertexBuffers[ Attribute.Normal ] );
			GL.VertexAttribPointer( Attribute.Normal, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0 );

			GL.EnableVertexAttribArray(Attribute.Uv);
			GL.BindBuffer( BufferTarget.ArrayBuffer, vertexBuffers[ Attribute.Uv ] );
			GL.VertexAttribPointer( Attribute.Uv, 2, VertexAttribPointerType.Float, false, sizeof(float) * 2, 0 );

			GL.EnableVertexAttribArray(Attribute.Color);
			GL.BindBuffer( BufferTarget.ArrayBuffer, vertexBuffers[ Attribute.Color ] );
			GL.VertexAttribPointer( Attribute.Color, 4, VertexAttribPointerType.Float, false, sizeof(float) * 4, 0 );

			GL.BindBuffer( BufferTarget.ArrayBuffer, 0 );
			GL.BindVertexArray( 0 );

			indexBuffers = new int[1];
			GL.GenBuffers( indexBuffers.Length, indexBuffers );


			int subs = _subDivisions + 2;

			int numVerticesPerPlane = subs*subs;

			NumVertices = numVerticesPerPlane * 6;

			int numFacesPerPlane = (subs-1)*(subs-1)*2;
			int numIndicesPerPlane = numFacesPerPlane*3;
			int numFaces = numFacesPerPlane * 6;

			NumIndices = numFaces * 3;

			var indices		= new int[NumIndices];
			var positions	= new float[NumVertices * 3];
			var uvs			= new float[NumVertices * 2];
			var normals		= new float[NumVertices * 3];
			var colors 		= new float[NumVertices * 4];
			//var tangents	= new float[numVertices * 3];
			//var bitangents	= new float[numVertices * 3];

			const float PI = (float)Math.PI;

			Matrix4 mat = Matrix4.Identity;
			int vertexOffset = 0;
			float halfWidth = _width * 0.5f;
			float halfHeight = _height * 0.5f;
			float halfDepth = _depth * 0.5f;

			// Y+
			mat *= Matrix4.CreateTranslation( -halfWidth, 0, -halfDepth );
			mat *= Matrix4.CreateTranslation( 0, halfHeight, 0 );
			GeomUtil.GeneratePlane( _width, _depth, subs, subs, ref positions, ref uvs, ref indices, ref mat, vertexOffset, 0 );
			vertexOffset += numVerticesPerPlane;
			ColorVertices( ref colors, 0, 0, 0.0f, 1.0f, 0.0f, 1.0f );
			SetNormals( ref normals, 0, numVerticesPerPlane, 0.0f, 1.0f, 0.0f );

			// Y-
			mat *= Matrix4.CreateRotationX( PI );
			GeomUtil.GeneratePlane( _width, _depth, subs, subs, ref positions, ref uvs, ref indices, ref mat, vertexOffset, numIndicesPerPlane );
			vertexOffset += numVerticesPerPlane;
			ColorVertices( ref colors, numVerticesPerPlane * 1, numVerticesPerPlane, 1.0f, 1.0f, 0.0f, 1.0f );
			SetNormals( ref normals, numVerticesPerPlane * 1, numVerticesPerPlane, 0.0f, -1.0f, 0.0f );

			// X+
			mat = Matrix4.Identity;
			mat *= Matrix4.CreateTranslation( -halfWidth, 0, -halfDepth );
			mat *= Matrix4.CreateTranslation( 0, halfHeight, 0 );
			mat *= Matrix4.CreateRotationZ( PI * 0.5f );
			GeomUtil.GeneratePlane( _depth, _height, subs, subs, ref positions, ref uvs, ref indices, ref mat, vertexOffset, numIndicesPerPlane * 2 );
			vertexOffset += numVerticesPerPlane;
			ColorVertices( ref colors, numVerticesPerPlane*2, numVerticesPerPlane, 1.0f, 0.0f, 0.0f, 1.0f );
			SetNormals( ref normals, numVerticesPerPlane * 2, numVerticesPerPlane, -1.0f, 0.0f, 0.0f );

			// X-
			mat *= Matrix4.CreateRotationZ( PI );
			GeomUtil.GeneratePlane( _depth, _height, subs, subs, ref positions, ref uvs, ref indices, ref mat, vertexOffset, numIndicesPerPlane * 3 );
			vertexOffset += numVerticesPerPlane;
			ColorVertices( ref colors, numVerticesPerPlane*3, numVerticesPerPlane, 1.0f, 0.0f, 1.0f, 1.0f );
			SetNormals( ref normals, numVerticesPerPlane * 3, numVerticesPerPlane, 1.0f, 0.0f, 0.0f );

			// Z+
			mat = Matrix4.Identity;
			mat *= Matrix4.CreateTranslation( -halfWidth, 0, -halfDepth );
			mat *= Matrix4.CreateTranslation( 0, halfHeight, 0 );
			mat *= Matrix4.CreateRotationX( PI * 0.5f );
			GeomUtil.GeneratePlane( _width, _depth, subs, subs, ref positions, ref uvs, ref indices, ref mat, vertexOffset, numIndicesPerPlane * 4 );
			vertexOffset += numVerticesPerPlane;
			ColorVertices( ref colors, numVerticesPerPlane*4, numVerticesPerPlane, 0.0f, 0.0f, 1.0f, 1.0f );
			SetNormals( ref normals, numVerticesPerPlane * 4, numVerticesPerPlane, 0.0f, 0.0f, 1.0f );

			// Z-
			mat *= Matrix4.CreateRotationX( PI );
			GeomUtil.GeneratePlane( _width, _depth, subs, subs, ref positions, ref uvs, ref indices, ref mat, vertexOffset, numIndicesPerPlane * 5 );
			vertexOffset += numVerticesPerPlane;
			ColorVertices( ref colors, numVerticesPerPlane*5, numVerticesPerPlane, 0.0f, 1.0f, 1.0f, 1.0f );
			SetNormals( ref normals, numVerticesPerPlane * 5, numVerticesPerPlane, 0.0f, 0.0f, -1.0f );
				

			//GeometryUtil.generateTangents(this);

			BufferVertexData( Attribute.Position, ref positions, sizeof(float) );
			BufferVertexData( Attribute.Normal, ref normals, sizeof(float) );
			BufferVertexData( Attribute.Uv, ref uvs, sizeof(float) );
			BufferVertexData( Attribute.Color, ref colors, sizeof(float) );
			BufferIndexData( 0, ref indices );
		}

		static void ColorVertices( ref float[] colors, int offset, int count, float r, float g, float b, float a )
		{
			for ( int i = offset*4; i < offset*4+count*4; i += 4 )
			{
				colors[ i ] = r;
				colors[ i + 1 ] = g;
				colors[ i + 2 ] = b;
				colors[ i + 3 ] = a;
			}
		}

		static void SetNormals( ref float[] normals, int offset, int count, float x, float y, float z )
		{
			for ( int i = offset*3; i < offset*3+count*3; i += 3 )
			{
				normals[ i ] = x;
				normals[ i + 1 ] = y;
				normals[ i + 2 ] = z;
			}
		}

		#region IMultiTypeObject implementation

		static readonly Type[] types = { typeof(IGeometry), typeof(SphereGeometry) };
		public Type[] Types { get { return types; } }

		#endregion
	}
}

