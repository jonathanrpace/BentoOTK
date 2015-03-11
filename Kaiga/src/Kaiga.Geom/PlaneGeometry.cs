using System;
using OpenTK.Graphics.OpenGL4;
using Ramen;
using Kaiga.Core;

namespace Kaiga.Geom
{
	public class PlaneGeometry : Geometry, IMultiTypeObject
	{
		private float _width = 1;
		public float Width 
		{ 
			get { return _width; } 
			set
			{
				_width = value;
				invalid = true;
			}
		}

		private float _height = 1;
		public float Height
		{ 
			get { return _height; } 
			set
			{
				_height = value;
				invalid = true;
			}
		}

		private int _numDivisionsX = 0;
		public int NumDivisionsX
		{ 
			get { return _numDivisionsX; } 
			set
			{
				_numDivisionsX = value;
				invalid = true;
			}
		}

		private int _numDivisionsY = 0;
		public int NumDivisionsY
		{
			get { return _numDivisionsY; } 
			set
			{
				_numDivisionsY = value;
				invalid = true;
			}
		}
		

		override public void CreateGraphicsContextResources()
		{
			base.CreateGraphicsContextResources();

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
		}

		protected override void Validate()
		{
			int numVerticesX = 2 + _numDivisionsX;
			int numVerticesY = 2 + _numDivisionsY;
			NumVertices = numVerticesX * numVerticesY;
			NumTriangles = ( numVerticesX - 1 ) * ( numVerticesY - 1 ) * 2;
			NumIndices = NumTriangles * 3;

			var positions = new float[NumVertices * 3];
			var normals = new float[NumVertices * 3];
			var uvs = new float[NumVertices * 2];
			var colors = new float[NumVertices * 4];
			var indices = new int[NumIndices];

			var rand = new Random();

			var indicesIndex = 0;
			var vertexIndex = 0;
			for ( int i = 0; i < numVerticesY; i++ )
			{
				float yRatio = i / ( numVerticesY - 1 );
				for (int j = 0; j < numVerticesX; j++) 
				{
					float xRatio = j / ( numVerticesX - 1 );
					int float2Index = (i * numVerticesY + j) * 2;
					int float3Index = (i * numVerticesY + j) * 3;
					int float4Index = (i * numVerticesY + j) * 4;

					positions[ float3Index ] = ( xRatio * _width ) - _width * 0.5f;
					positions[ float3Index + 1 ] = ( yRatio * _height ) - _height * 0.5f;
					positions[ float3Index + 2 ] = 0.0f;

					normals[ float3Index ] = 0;
					normals[ float3Index + 1] = 0;
					normals[ float3Index + 2] = 1;

					uvs[float2Index] = xRatio;
					uvs[float2Index + 1] = 1-yRatio;

					colors[ float4Index ] = xRatio;
					colors[ float4Index + 1 ] = yRatio;
					colors[ float4Index + 2 ] = 0;
					colors[ float4Index + 3 ] = 0;

					if ( i < numVerticesY-1 && j < numVerticesX-1 )
					{
						indices[ indicesIndex ] = vertexIndex;
						indices[ indicesIndex + 1 ] = vertexIndex + numVerticesX;
						indices[ indicesIndex + 2 ] = vertexIndex + 1;
						indicesIndex += 3;

						indices[ indicesIndex ] = vertexIndex + 1;
						indices[ indicesIndex + 1 ] = vertexIndex + numVerticesX;
						indices[ indicesIndex + 2 ] = vertexIndex + numVerticesX + 1;
						indicesIndex += 3;
					}

					vertexIndex++;
				}
			}

			BufferVertexData( Attribute.Position, ref positions, sizeof(float) );
			BufferVertexData( Attribute.Normal, ref normals, sizeof(float) );
			BufferVertexData( Attribute.Uv, ref uvs, sizeof(float) );
			BufferVertexData( Attribute.Color, ref colors, sizeof(float) );
			BufferIndexData( 0, ref indices );
		}

		#region IMultiTypeObject implementation

	 	static readonly Type[] types = { typeof(Geometry), typeof(PlaneGeometry), typeof(IGraphicsContextDependant) };
		public Type[] Types { get { return types; } }

		#endregion
	}
}

