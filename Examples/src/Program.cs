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

		public Program()
			: base
			(
				1280, 720,
				new GraphicsMode( 8, 0 ), "Bento", 0,
				DisplayDevice.Default, 4, 5,
				GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug
			)
		{

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

			var rand = new Random();
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
					material.roughness = columnRatio;
					material.reflectivity = rowRatio;
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

			var light0 = CreateLight();
			light0.GetComponentByType<Transform>().TranslateBy( 0.0f, 0.5f, 0.0f );

			var light1 = CreateLight();
			light1.GetComponentByType<Transform>().TranslateBy( 0.0f, -0.25f, 0.5f );
			light1.GetComponentByType<PointLight>().Color = new Vector3( 0.0f, 0.6f, 1.0f );
			light1.GetComponentByType<PointLight>().Intensity = 0.5f;

			/*
			for ( int i = 0; i < 1; i++ )
			{
				var entity = new Entity();
				var transform = new Transform();

				const float positionScale = 1.0f;
				transform.Matrix = Matrix4.Identity * Matrix4.CreateTranslation( 
					(float)(rand.NextDouble()-0.5f)*positionScale, 
					(float)(rand.NextDouble()-0.5f)*positionScale, 
					(float)(rand.NextDouble()-0.5f)*positionScale );

				transform.Matrix = Matrix4.Identity * Matrix4.CreateTranslation( 
					0.0f, 
					0.5f,
					0.0f );
				entity.AddComponent( transform );

				var lightComponent = new PointLight( 
					new Vector3( (float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble() ), 
					2.0f, 1.0f, 1.0f, 10 );

				var lightComponent = new PointLight( 
					new Vector3( 1.0f, 1.0f, 1.0f ), 
					2.0f, 1.0f, 1.0f, 1.0f );

				entity.AddComponent( lightComponent );

				scene.AddEntity( entity );
			}
			*/


			scene.AddProcess( new GraphicsContextDependencyManager() );
			scene.AddProcess( new OrbitCamera() );
		}

		Entity CreateLight()
		{
			var entity = new Entity();
			var transform = new Transform();
			entity.AddComponent( transform );
			var lightComponent = new PointLight();
			entity.AddComponent( lightComponent );

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
			scene.Update( 1 / RenderFrequency );
		}
	}
}

