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

		public Program()
			: base
			(
				1920, 1080,
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
			renderer.AddRenderPass( new AORenderPass() );

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

			var floor = new Entity();
			var floorGeom = new PlaneGeometry();
			floorGeom.Width = numColumns * spacing;
			floorGeom.Height = numRows * spacing;
			floor.AddComponent( floorGeom );
			floor.AddComponent( new Transform( Matrix4.Identity * Matrix4.CreateRotationX( (float)Math.PI * 0.5f )) );
			floor.AddComponent( new StandardMaterial() );
			scene.AddEntity( floor );

			for ( int i = 0; i < 20; i++ )
			{
				CreateLight();
			}

			scene.AddProcess( new OrbitCamera() );
			scene.AddProcess( new SwarmProcess() );
		}

		Entity CreateLight()
		{
			var entity = new Entity();
			var transform = new Transform();
			entity.AddComponent( transform );

			var radius = 0.01f + (float)rand.NextDouble() * 0.1f;
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
				                  	new Vector3( (float)rand.NextDouble() * 2.0f, (float)rand.NextDouble() * 2.0f, (float)rand.NextDouble() * 2.0f ),
									new Vector3( (float)rand.NextDouble() * 0.5f, (float)rand.NextDouble() * 0.5f, (float)rand.NextDouble() * 0.5f ) 
			                  );
			entity.AddComponent( swarmMember );

			scene.AddEntity( entity );

			return entity;
		}

		protected override void OnClosing( System.ComponentModel.CancelEventArgs e )
		{
			scene.Dispose();
			scene = null;

			base.OnClosing(e);
		}

		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			var keyboard = OpenTK.Input.Keyboard.GetState();
			if (keyboard[OpenTK.Input.Key.Escape])
				Exit();
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			scene.Update( 1 / RenderFrequency );
		}
	}
}

