using System;
using OpenTK;
using OpenTK.Graphics;
using Ramen;
using Kaiga.Core;
using Kaiga.RenderPasses;
using Kaiga.Geom;
using Kaiga.Processes;
using Kaiga.Components;
using Kaiga.Materials;
using Kaiga.Lights;
using System.Diagnostics;

namespace Examples
{
	public class Program : GameWindow
	{
		[STAThread]
		public static void Main()
		{
			using (Program program = new Program())
			{
				program.Run(60);
			}
		}

		private Scene scene;
		Random rand;

		//private Entity MainLight;
		//private double elapsed = 0.0f;

		public Program()
			: base
			(
				1280, 720,
				new GraphicsMode( 8, 0 ), "Bento", 0,
				DisplayDevice.Default, 4, 5,
				GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug
			)
		{
			rand = new Random();
		}

		protected override void OnLoad( System.EventArgs e )
		{
			VSync = VSyncMode.On;
			Context.ErrorChecking = true;

			scene = new Scene( this );

			var renderer = new DeferredRenderer();
			scene.AddProcess( renderer );
			renderer.AddRenderPass( new TestRenderPass() );
			renderer.AddRenderPass( new PointLightRenderPass() );
			
			const int numColumns = 10;
			const int numRows = 10;
			const float spacing = 0.2f;
			const float radius = 0.1f;

			for ( int i = 0; i < numColumns; i++ )
			{
				var columnRatio = (float)i / ( numColumns - 1 );
				for ( int j = 0; j < numRows; j++ )
				{
					var rowRatio = (float)j / ( numRows - 1 );

					var entity = new Entity();
					
					var geom = new SphereGeometry();
					geom.Radius = radius;
					geom.SubDivisions = 32;
					entity.AddComponent( geom );

					var material = new StandardMaterial();
					material.Roughness = columnRatio;
					material.Reflectivity = rowRatio;
					entity.AddComponent( material );

					var transform = new Transform();
					transform.Matrix = Matrix4.Identity * Matrix4.CreateTranslation( 
						(i-numColumns*0.5f) * spacing, 
						0.0f, 
						(j-numRows*0.5f) * spacing );
					entity.AddComponent( transform );

					scene.AddEntity( entity );
				}
			}

			for ( int i = 0; i < 20; i++ )
			{
				CreateLight();
			}

			//MainLight = CreateLight();
			//MainLight.GetComponentByType<Transform>().TranslateBy( 0.0f, 0.5f, 0.0f );

			//var light1 = CreateLight();
			//light1.GetComponentByType<Transform>().TranslateBy( 0.0f, -0.25f, 0.5f );
			//light1.GetComponentByType<PointLight>().Color = new Vector3( 0.0f, 0.6f, 1.0f );
			//light1.GetComponentByType<PointLight>().Intensity = 0.5f;

			scene.AddProcess( new GraphicsContextDependencyManager() );
			scene.AddProcess( new OrbitCamera() );
			scene.AddProcess( new SwarmProcess() );
		}

		Entity CreateLight()
		{
			var entity = new Entity();
			var transform = new Transform();
			entity.AddComponent( transform );

			var radius = 0.01f + (float)rand.NextDouble() * 0.05f;
			var color = new Vector3( (float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble() );

			var pointLight = new PointLight();
			pointLight.Radius = radius;
			pointLight.Intensity = 10.0f;
			pointLight.Color = color;
			entity.AddComponent( pointLight );

			var sphereGeom = new SphereGeometry();
			sphereGeom.Radius = pointLight.Radius;
			entity.AddComponent( sphereGeom );

			var material = new StandardMaterial();
			material.Diffuse = color;
			material.Emissive = 0.1f;
			entity.AddComponent( material );

			var swarmMember = new SwarmMember( 
				                  	new Vector3( (float)rand.NextDouble() * 2.0f, (float)rand.NextDouble() * 0.5f, (float)rand.NextDouble() * 2.0f ),
									new Vector3( (float)rand.NextDouble() * 0.5f, (float)rand.NextDouble() * 0.5f, (float)rand.NextDouble() * 0.5f ) 
			                  );
			entity.AddComponent( swarmMember );

			scene.AddEntity( entity );

			return entity;
		}

		protected override void OnUnload( System.EventArgs e )
		{
			scene.Clear();
			scene = null;

			base.OnUnload(e);
		}

		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			//Matrix4 rotation = Matrix4.CreateRotationY((float)e.Time);
			//Matrix4.Mult(ref rotation, ref modelviewMatrix, out modelviewMatrix);
			//GL.UniformMatrix4(modelviewMatrixLocation, false, ref modelviewMatrix);

			var keyboard = OpenTK.Input.Keyboard.GetState();
			if (keyboard[OpenTK.Input.Key.Escape])
				Exit();
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			/*
			elapsed += e.Time;
			var radius = 0.01f + ((float)Math.Sin( elapsed )+1.0f) * 0.1f;
			MainLight.GetComponentByType<PointLight>().Radius = radius;
			MainLight.GetComponentByType<SphereGeometry>().Radius = radius;

			const double speed = 0.01;
			const float translateRadius = 1.0f;
			var x = (float)Math.Sin( elapsed * 17 * speed ) * translateRadius;
			var y = (float)Math.Cos( elapsed * 19 * speed ) * translateRadius;
			var z = (float)Math.Sin( elapsed * 23 * speed ) * translateRadius * 0.25f;
			
			MainLight.GetComponentByType<Transform>().Matrix = Matrix4.Identity * Matrix4.CreateTranslation( x, y, z );
			*/
			scene.Update( 1 / RenderFrequency );
		}
	}
}

