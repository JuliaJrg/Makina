using UnityEngine;

[RequireComponent( typeof(Camera) )]
[RequireComponent( typeof(Rigidbody) )]

public class CameraController : MonoBehaviour {
	public float acceleration = 50; // l'accélération de la caméra
	public float accSprintMultiplier = 4; // multiplicateur de vitesse en sprint
	public float lookSensitivity = 1; // sensibilité de la souris
	public float dampingCoefficient = 5; // coefficient d'amortissement
	public bool focusOnEnable = true; // focus la caméra à l'activation

	private Rigidbody rb; // le rigidbody de la caméra
	Vector3 velocity; // vitesse de la caméra (en unités par seconde)

	static bool Focused {
		get => Cursor.lockState == CursorLockMode.Locked;
		set {
			Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
			Cursor.visible = value == false;
		}
	}

    void OnEnable() {
        if (focusOnEnable) Focused = true;
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // désactive la gravité
    }

	void OnDisable() => Focused = false;

	void Update() {
		// Input
		if( Focused )
			UpdateInput();
		else if( Input.GetMouseButtonDown( 0 ) )
			Focused = true;

		// Physics
		velocity = Vector3.Lerp( velocity, Vector3.zero, dampingCoefficient * Time.deltaTime );
		transform.position += velocity * Time.deltaTime;
	}

	void FixedUpdate() {
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

	void UpdateInput() {
		// Position
		velocity += GetAccelerationVector() * Time.deltaTime;

		// Rotation
		Vector2 mouseDelta = lookSensitivity * new Vector2( Input.GetAxis( "Mouse X" ), -Input.GetAxis( "Mouse Y" ) );
		Quaternion rotation = transform.rotation;
		Quaternion horiz = Quaternion.AngleAxis( mouseDelta.x, Vector3.up );
		Quaternion vert = Quaternion.AngleAxis( mouseDelta.y, Vector3.right );
		transform.rotation = horiz * rotation * vert;

		
		if( Input.GetKeyDown( KeyCode.Escape ) )
			Focused = false;
	}

	// Renvoie le vecteur accélération de la caméra
	Vector3 GetAccelerationVector() {
		Vector3 moveInput = default;

		void AddMovement( KeyCode key, Vector3 dir ) {
			if( Input.GetKey( key ) )
				moveInput += dir;
		}

		// Ajoute les vecteurs de mouvement 
		AddMovement( KeyCode.W, Vector3.forward ); // W pour avancer
		AddMovement( KeyCode.S, Vector3.back ); // S pour reculer
		AddMovement( KeyCode.D, Vector3.right ); // D pour aller à droite
		AddMovement( KeyCode.A, Vector3.left ); // A pour aller à gauche
		AddMovement( KeyCode.Space, Vector3.up ); // Espace pour monter
		AddMovement( KeyCode.LeftControl, Vector3.down ); // Ctrl pour descendre
		Vector3 direction = transform.TransformVector( moveInput.normalized ); // direction du mouvement

		if( Input.GetKey( KeyCode.LeftShift ) )
			return direction * ( acceleration * accSprintMultiplier ); // "sprinting"
		return direction * acceleration; // "walking"
	}
}