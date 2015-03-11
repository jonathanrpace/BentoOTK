using System;
using OpenTK;

namespace Kaiga.Util
{
	public class GeomUtil
	{
		/**
		 * @brief Generates a plane who's width lies along the X axis, and it's height along the Y. Pointing towards positive Z.
		 * 		  It's origin will lie in the center.
		 * @param sizeX
		 * @param sizeY
		 * @param numVerticesX
		 * @param numVerticesY
		 * @param geometry		The geometry instance to fill out with the plane data
		 * @param vertexOffset	Optionally, you can provide a vertex offset where this function starts inserting vertex data.
		 * 						Useful if you're injecting multiple planes into a single geometr (a cube for instance).
		 * @param indexOffset	Used in conjunction with the vertex offset to allow control over where the index data
		 * 						is injected.
		 * @param matrix		All positional data will be transform by this matrix if specified.
		 * 
		 */		
		public static void GeneratePlane
		( 
			float sizeX, 
			float sizeZ, 
			int numVerticesX, 
			int numVerticesZ,
			ref float[] positions,
			ref float[] uvs,
			ref int[] indices,
			ref Matrix4 matrix,
			int vertexOffset = 0,
			int indexOffset = 0
		)
		{
			int positionIndex = vertexOffset*3;
			int uvIndex = vertexOffset*2;

			for ( int z = 0; z < numVerticesZ; z++ )
			{
				float zRatio = (float)z / (numVerticesZ-1);
				for ( int x = 0; x < numVerticesX; x++ )
				{
					float xRatio = (float)x / (numVerticesX-1);

					Vector3 vec = Vector3.Transform( new Vector3( xRatio * sizeX, 0, zRatio * sizeZ ), matrix );

					positions[positionIndex++] = vec.X;
					positions[positionIndex++] = vec.Y;
					positions[positionIndex++] = vec.Z;

					uvs[uvIndex++] = xRatio;
					uvs[uvIndex++] = 1-zRatio;

					if ( x != numVerticesX-1 && z != numVerticesZ-1 )
					{
						int i = (z * numVerticesX) + x;
						indices[indexOffset++] = vertexOffset + i;
						indices[indexOffset++] = vertexOffset + i + numVerticesX;
						indices[indexOffset++] = vertexOffset + i + 1;

						indices[indexOffset++] = vertexOffset + i + 1;
						indices[indexOffset++] = vertexOffset + i + numVerticesX;
						indices[indexOffset++] = vertexOffset + i + numVerticesX + 1;
					}
				}
			}
		}
	}
}

