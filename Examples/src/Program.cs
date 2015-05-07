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
using Kaiga.Textures;

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

		float random()
		{
			return (float)rand.NextDouble();
		}

		protected override void OnLoad( System.EventArgs e )
		{
			VSync = VSyncMode.Off;
			Context.ErrorChecking = true;

			scene = new Scene( this );

			var renderer = new DeferredRenderer();
			scene.AddProcess( renderer );
			renderer.AddRenderPass( new GPass() );
			renderer.AddRenderPass( new PointLightRenderPass() );
			renderer.AddRenderPass( new AmbientLightRenderPass() );
			renderer.AddRenderPass( new ImageLightRenderPass() );
			//renderer.AddRenderPass( new SkyboxRenderPass() );

			const int numColumns = 10;
			const int numRows = 10;
			const float spacing = 0.25f;
			const float radius = 0.1f;

			var sphereGeom = new SphereGeometry();
			sphereGeom.SubDivisions = 32;
			for ( int i = 0; i < numColumns; i++ )
			{
				var columnRatio = (float)i / ( numColumns - 1 );
				for ( int j = 0; j < numRows; j++ )
				{
					var rowRatio = (float)j / ( numRows - 1 );

					var entity = new Entity();
					entity.AddComponent( sphereGeom );

					var material = new StandardMaterial();
					material.Roughness = 0.3f + (1.0f-columnRatio) * 0.4f;
					entity.AddComponent( material );

					var transform = new Transform();
					transform.Scale( radius );
					transform.Translate
					(
						( i - numColumns * 0.5f ) * spacing, 
						radius + random() * 0.0f,
						( j - numRows * 0.5f ) * spacing
					);
					entity.AddComponent( transform );

					if ( random() < 0.25f )
					{
						material.Diffuse = new Vector3( random(), random(), random() );
						entity.AddComponent( new EmissivePulser( 1.0f, 0.0f, 1.0f, random() * (float)Math.PI ) );
					}
					scene.AddEntity( entity );
				}
			}

			const float border = 0.5f;
			const float planeWidth = numColumns * spacing + border * 2.0f;
			const float planeHeight = numRows * spacing + border * 2.0f;
			// Floor
			{
				var entity = new Entity();

				var geom = new PlaneGeometry();
				geom.Width = planeWidth;
				geom.Height = planeHeight;
				entity.AddComponent( geom );

				var transform = new Transform();
				transform.RotateX( (float)Math.PI * 0.5f );
				entity.AddComponent( transform );

				var material = new StandardMaterial();
				material.Roughness = 0.9f;
				material.Diffuse = new Vector3( 1.0f, 1.0f, 1.0f );
				entity.AddComponent( material );

				//entity.AddComponent( new EmissivePulser( 1.32f, 0.0f, 0.1f, 0.75f ) );

				scene.AddEntity( entity );
			}

			// Green wall
			{
				var entity = new Entity();

				var geom = new PlaneGeometry();
				geom.Width = planeWidth;
				geom.Height = planeHeight;
				entity.AddComponent( geom );

				var transform = new Transform();
				transform.RotateY( (float)Math.PI );
				transform.Translate( 0.0f, planeWidth * 0.5f, -planeHeight * 0.5f );
				entity.AddComponent( transform );

				var material = new StandardMaterial();
				material.Roughness = 0.9f;
				material.Diffuse = new Vector3( 0.0f, 1.0f, 0.0f );
				entity.AddComponent( material );

				entity.AddComponent( new EmissivePulser( 1.12f, 0.1f, 1.0f, 0.2f ) );

				scene.AddEntity( entity );
			}

			// Red wall
			{
				var entity = new Entity();

				var geom = new PlaneGeometry();
				geom.Width = planeWidth;
				geom.Height = planeHeight;
				entity.AddComponent( geom );

				var transform = new Transform();
				transform.RotateY( -(float)Math.PI * 0.5f );
				transform.Translate( -planeWidth * 0.5f, planeHeight * 0.5f, 0.0f );
				entity.AddComponent( transform );

				var material = new StandardMaterial();
				material.Roughness = 0.9f;
				material.Diffuse = new Vector3( 1.0f, 0.0f, 0.0f );
				entity.AddComponent( material );

				entity.AddComponent( new EmissivePulser( 1.0f, 0.1f, 1.0f, 3.1f ) );

				scene.AddEntity( entity );
			}

			for ( int i = 0; i < 20; i++ )
			{
				CreateLight();
			}

			CreateImageLight();
			//CreateAmbientLight();

			scene.AddProcess( new OrbitCamera() );
			scene.AddProcess( new SwarmProcess() );
			scene.AddProcess( new EmissivePulseProcess() );
		}

		Entity CreateLight()
		{
			var entity = new Entity();
			var transform = new Transform();
			entity.AddComponent( transform );

			var radius = 0.05f + random() * 0.05f;
			var color = new Vector3( random(), random(), random() );

			var pointLight = new PointLight();
			pointLight.Radius = radius;
			pointLight.Intensity = 200.0f;
			pointLight.AttenuationRadius = radius * 3.0f;
			//pointLight.Color = color;
			//entity.AddComponent( pointLight );

			var sphereGeom = new SphereGeometry();
			sphereGeom.Radius = pointLight.Radius * 2.0f;
			entity.AddComponent( sphereGeom );

			var material = new StandardMaterial();

			if ( random() < 0.25 )
			{
				material.Diffuse = color;
				entity.AddComponent( new EmissivePulser( 1.0f, 0.0f, 1.0f, random() * (float)Math.PI ) );
			}

			material.Roughness = random();
			entity.AddComponent( material );

			var swarmMember = new SwarmMember
			( 
				new Vector3( random() * 2.0f, random() * 2.0f, random() * 2.0f ),
				new Vector3( random()* 0.5f, (float)rand.NextDouble() * 0.5f, random() * 0.5f ) 
			);
			entity.AddComponent( swarmMember );

			scene.AddEntity( entity );

			return entity;
		}

		void CreateImageLight()
		{
			var imageLight = new Entity();
			var light = new ImageLight( 0.2f );
			light.Texture = new ExternalCubeTexture();
			imageLight.AddComponent( light, -1 );
			scene.AddEntity( imageLight );
		}

		void CreateAmbientLight()
		{
			var skybox = new Entity();
			var material = new SkyboxMaterial();
			material.Texture = new ExternalCubeTexture();
			skybox.AddComponent( material );
			scene.AddEntity( skybox );

			var entity = new Entity();

			var ambientLight = new AmbientLight( 1.0f, 1.0f, 1.0f, 0.2f );
			entity.AddComponent( ambientLight );

			scene.AddEntity( entity );
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

