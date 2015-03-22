using System;
using Ramen;
using OpenTK;
using OpenTK.Input;
using Kaiga.Core;
using Kaiga.Components;

namespace Kaiga.Processes
{
	public class OrbitCamera : NamedObject, IProcess
	{
		public float Dolly { get; set; }
		public float DollyTarget { get; set; }
		public float DollySpeed { get; set; }
		public float DollyEase { get; set; }
		public float MinDolly { get; set; }
		public float MaxDolly { get; set; }

		public Vector3 Position { get; set; }
		public Vector3 PositionTarget { get; set; }
		public Vector3 PositionSpeed { get; set; }
		public Vector3 PositionEase { get; set; }

		public Vector2 Rotation { get; set; }
		public Vector2 RotationTarget { get; set; }
		public Vector2 RotationSpeed { get; set; }
		public Vector2 RotationEase { get; set; }

		Matrix4 matrix;
		Vector2 storedRotation;
		Scene scene;
		Vector2 mouseDownPos;
		bool mouseIsDown = false;

		public OrbitCamera()
		{
			Dolly = DollyTarget = 2.0f;
			DollySpeed = 0.1f;
			DollyEase = 0.1f;
			MinDolly = 0.1f;
			MaxDolly = 5.0f;

			Position = PositionTarget = new Vector3( 0.0f, 0.0f, 0.0f );
			PositionSpeed = new Vector3( 0.1f, 0.1f, 0.1f );
			PositionEase = new Vector3( 0.2f, 0.2f, 0.2f );

			Rotation = RotationTarget = new Vector2( 0.0f, 0.0f );
			RotationEase = new Vector2( 0.2f, 0.2f );
			RotationSpeed = new Vector2( 3.0f, 3.0f );
		}

		public void OnAddedToScene( Scene scene )
		{
			this.scene = scene;

			scene.GameWindow.MouseDown += HandleMouseDown;
			scene.GameWindow.MouseUp += HandleMouseUp;
			scene.GameWindow.MouseWheel += HandleMouseWheel;
		}

		public void OnRemovedFromScene( Scene scene )
		{
			scene.GameWindow.MouseDown -= HandleMouseDown;
			scene.GameWindow.MouseUp -= HandleMouseUp;
			scene.GameWindow.MouseWheel -= HandleMouseWheel;

			this.scene = null;
		}

		public void Update( double dt )
		{
			ProcessInput();
			ApplyEase();
			UpdateMatrix();
		}

		void ProcessInput()
		{
			if ( mouseIsDown )
			{
				MouseState mouseState = Mouse.GetCursorState();
				Vector2 mouseDelta = new Vector2( mouseState.X, mouseState.Y ) - mouseDownPos;
				mouseDelta.X /= scene.GameWindow.Width;
				mouseDelta.Y /= scene.GameWindow.Height;


				RotationTarget = storedRotation + mouseDelta * RotationSpeed;
			}

			KeyboardState keyboardState = Keyboard.GetState();

			var translation = new Vector3();
			if ( keyboardState.IsKeyDown( Key.W ) )
			{
				translation.Z += PositionSpeed.Z;
			}
			if ( keyboardState.IsKeyDown( Key.S ) )
			{
				translation.Z -= PositionSpeed.Z;
			}
			if ( keyboardState.IsKeyDown( Key.A ) )
			{
				translation.X += PositionSpeed.X;
			}
			if ( keyboardState.IsKeyDown( Key.D ) )
			{
				translation.X -= PositionSpeed.X;
			}
		
			PositionTarget += translation;
		}

		void ApplyEase()
		{
			Position += ( PositionTarget - Position ) * PositionEase;
			Rotation += ( RotationTarget - Rotation ) * RotationEase;
			DollyTarget = MathHelper.Clamp( DollyTarget, MinDolly, MaxDolly );
			Dolly += ( DollyTarget - Dolly ) * DollyEase;
		}

		void UpdateMatrix()
		{
			matrix = Matrix4.Identity;


			matrix *= Matrix4.CreateRotationY( Rotation.X );
			matrix *= Matrix4.CreateRotationX( Rotation.Y );
			matrix *= Matrix4.CreateTranslation( 0, 0, -Dolly );
			matrix *= Matrix4.CreateTranslation( Position );

			var renderer = scene.GetProcessByType<IRenderer>();
			if ( renderer == null ) return;

			// Apply the resultant matrix to the renderer's camera's transform.
			renderer.Camera.GetComponentByType<Transform>().Matrix = matrix;
		}
		
		void HandleMouseDown (object sender, MouseButtonEventArgs e)
		{
			mouseIsDown = true;

			MouseState mouseState = Mouse.GetCursorState();
			mouseDownPos = new Vector2( mouseState.X, mouseState.Y );

			storedRotation = RotationTarget;
		}

		void HandleMouseUp (object sender, MouseButtonEventArgs e)
		{
			mouseIsDown = false;
		}

		void HandleMouseWheel (object sender, MouseWheelEventArgs e)
		{
			DollyTarget -= e.DeltaPrecise * DollySpeed;
		}
	}
}

