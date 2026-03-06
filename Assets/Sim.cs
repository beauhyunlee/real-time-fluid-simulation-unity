using UnityEngine;

public class Sim : MonoBehaviour
{
    [Header("Simulation")]
    [Range(0f, 50f)]
    [SerializeField] private float gravity = 9.8f;

    [Header("Bounds")]
    [Range(1f, 20f)]
    [SerializeField] private float boundsWidth = 10f;
    [Range(1f, 20f)]
    [SerializeField] private float boundsHeight= 5f;

    [Header("Particles")]
    [SerializeField] private int numParticles = 100;
    [SerializeField] private float particleSpacing = 0.1f;
    [SerializeField] private float particleRadius = 0.2f;



//    public float gravity = 9.8f;
//    public Vector2 boundsSize = new Vector2(10f, 5f);
    
    private Vector2[] positions;
    private Vector2[] velocities;
   

   void Start()
   {
    positions = new Vector2[numParticles]; // initialize from object
    velocities = new Vector2[numParticles]; // initial push

    int particlesPerRow = Mathf.CeilToInt(Mathf.Sqrt(numParticles));
    int particlesPerCol = (numParticles - 1) / particlesPerRow + 1;

    float spacing = particleRadius * 2 + particleSpacing;

    for (int i = 0; i < numParticles; i++)
    {
        int row = i / particlesPerRow;
        int col = i % particlesPerRow;

        float x = (col - particlesPerRow / 2f) * spacing;
        float y = (row - particlesPerCol / 2f) * spacing;

        positions[i] = new Vector2(x,y);
        velocities[i] = Vector2.zero;
    }
   }

   void Update()
   {
    for (int i = 0; i < numParticles; i++){
        // physics
        velocities[i] += Vector2.down * gravity * Time.deltaTime;
        positions[i] += velocities[i] * Time.deltaTime;
        
        ResolveCollisions(ref positions[i], ref velocities[i]);

    }
    
    
    // // rendering
    // transform.position = position;

    // // visual update
    // transform.localScale = Vector3.one * particleRadius * 2f;
   }

   void ResolveCollisions(ref Vector2 position, ref Vector2 velocity)
   {
    // Position correction through the use of clamping forces to remove penetration entirely.
    float halfWidth = boundsWidth / 2f;
    float halfHeight = boundsHeight / 2f;

    // Vector2 halfBounds = boundsSize / 2f; 
    
    // X 
    if (Mathf.Abs(position.x) + particleRadius > halfWidth)
    {
        position.x = (halfWidth - particleRadius) * Mathf.Sign(position.x); // Position correction
        velocity.x *= -1f; // Conservation of momentum and energy - flip position 
    }
    
    // Y
    if (Mathf.Abs(position.y + particleRadius) > halfHeight)
    {
        position.y = (halfHeight - particleRadius) * Mathf.Sign(position.y); // Position correction
        velocity.y *= -1f; // Conservation of momentum and energy - flip position 
    }
   }
   void OnDrawGizmos()
   {
    if (positions == null) return;

    Gizmos.color = Color.white;
    for (int i = 0; i < positions.Length; i++)
    {
        Gizmos.DrawSphere(positions[i], particleRadius);
    }
    Gizmos.color = Color.white;
    Gizmos.DrawWireCube(Vector2.zero, new Vector2(boundsWidth, boundsHeight));
   }

}
