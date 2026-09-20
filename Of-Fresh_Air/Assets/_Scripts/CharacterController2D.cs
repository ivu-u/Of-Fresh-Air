using UnityEngine;

// TODO: note: Auto Sync transforms is on, but not sure if we can find a better alternative
[RequireComponent(typeof(BoxCollider2D))]
public class CharacterController2D : MonoBehaviour
{
    [SerializeField] private float speed = 10; 
    [SerializeField] private float walkAcceleration = 75;       // TODO: possibly add accel/deccel curves
    [SerializeField] private float groundDeceleration = 70;
    [SerializeField] private float airAcceleration = 30; 
    [SerializeField] private float airDeceleration = 20; 
    [SerializeField] private float jumpHeight = 4;
    [SerializeField] private float jumpTime = 1;    // in seconds

    private Transform _t;
    private BoxCollider2D _collider;
    private Vector2 _velocity;
    private bool _isGrounded;

    private float _gravity => jumpHeight / (2 * Mathf.Pow(jumpTime, 2f));
    private float _jumpVelocity => Mathf.Sqrt(2 * jumpHeight * _gravity);


    void Awake() {
        _t = transform;
        _collider = GetComponent<BoxCollider2D>();
    }

    void Update() {
        Jump();
        ApplyGravity();

        UpdateVelocity();
        ApplyVelocity();

        HandleCollisions();
    }

    private void ApplyVelocity() { 
        transform.Translate(_velocity * Time.deltaTime); 
    }


    private void UpdateVelocity() {
        float moveInput = Input.GetAxisRaw("Horizontal");       // TODO: refactor input system

        float acceleration = _isGrounded ? walkAcceleration : airAcceleration;
        float decceleration = _isGrounded ? groundDeceleration : airDeceleration;

        if (moveInput != 0) {
            _velocity.x = Mathf.MoveTowards(_velocity.x, speed * moveInput, acceleration * Time.deltaTime);
        } else {
            _velocity.x = Mathf.MoveTowards(_velocity.x, speed * moveInput, decceleration * Time.deltaTime);
        }
    }

    // !!TODO!!: BUG --> isGrounded bugs tf out
    // TODO: currently it's possible for our player to be pushed out of one collider
    //          and into another. 
    // TODO: seperate grounded checks   | currently grounded == surface with an angle
    //          with less than 90 degrees with respect to world up
    private void HandleCollisions() {
        _isGrounded = false;
        Collider2D[] hits = Physics2D.OverlapBoxAll(_t.position, _collider.size, 0);

        foreach (Collider2D hit in hits) {
            if (hit == _collider) continue;

            ColliderDistance2D colliderDistance = hit.Distance(_collider);

            if (colliderDistance.isOverlapped) {
                _t.Translate(colliderDistance.pointA - colliderDistance.pointB);
            }

            if (Vector2.Angle(colliderDistance.normal, Vector2.up) < 90 && _velocity.y < 0) {
                _isGrounded = true;
            }
        }
    }

    private void Jump() {           // y = -0.5gt² + v't (v' = initial jump velocity)
        if (_isGrounded) {
            _velocity.y = 0;

            if (Input.GetButtonDown("Jump")) {
                _velocity.y = _jumpVelocity;
            }
        }
    }

    private void ApplyGravity() { 
        if (!_isGrounded) {
            _velocity.y -= _gravity * Time.deltaTime;
        }
    }
}
