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
    [Range(1,200)]
    [SerializeField] private int numParticles = 100;

    [Range(0.01f, 0.05f)]
    [SerializeField] private float particleSpacing = 0.1f;

    [Range(0f, 0.5f)]
    [SerializeField] private float particleRadius = 0.05f;

    [Header("Rendering")]
    [SerializeField] private Mesh particleMesh;
    [SerializeField] private Material particleMaterial;

    // TODO: Scaling risks garbage allocations. Change to preallocate batcha arrays.
    private Matrix4x4[] matrices; 

    private Vector2[] positions;
    private Vector2[] velocities;
   
   // particle change cache
   private int lastParticleCount;
   private float lastParticleRadius;
   private float lastParticleSpacing;

   void Start()
   {
    
    InitializeParticles();

    // Initialize particle state
    lastParticleCount = numParticles;
    lastParticleRadius = particleRadius;
    lastParticleSpacing = particleSpacing;

   }

   void InitializeParticles()
   {
    positions = new Vector2[numParticles]; // initialize from object
    velocities = new Vector2[numParticles]; // initial push
    matrices = new Matrix4x4[numParticles];

    // particle positioning
    int particlesPerRow = Mathf.CeilToInt(Mathf.Sqrt(numParticles));
    int particlesPerCol = (numParticles - 1) / particlesPerRow + 1;

    float spacing = particleRadius * 2 + particleSpacing;

    // 
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
    // Rebuilding particles if parameters change
    if (numParticles != lastParticleCount || particleRadius != lastParticleRadius || particleSpacing != lastParticleSpacing) 
    {
        InitializeParticles();

        lastParticleCount = numParticles;
        lastParticleRadius = particleRadius;
        lastParticleSpacing = particleSpacing;
    }
    
    // Physics
    for (int i = 0; i < positions.Length; i++)
    {
        velocities[i] += Vector2.down * gravity * Time.deltaTime;
        positions[i] += velocities[i] * Time.deltaTime;
        
        ResolveCollisions(ref positions[i], ref velocities[i]);

    }
    
    // Build transforms
    for (int i = 0; i < positions.Length; i++)
    {
        Vector3 pos = new Vector3(positions[i].x, positions[i].y, 0f);

        matrices[i] = Matrix4x4.TRS(
            pos,
            Quaternion.identity,
            Vector3.one * particleRadius * 2f
        );
    }

    // Render batches
    const int batchSize = 1023;

    for (int i = 0; i < matrices.Length; i += batchSize)
    {
        int count = Mathf.Min(batchSize, matrices.Length - i);

        Matrix4x4[] batch = new Matrix4x4[count];
        System.Array.Copy(matrices, i, batch, 0, count);

        Graphics.DrawMeshInstanced(
            particleMesh,
            0,
            particleMaterial,
            batch
        );
    }
   }

   void ResolveCollisions(ref Vector2 position, ref Vector2 velocity)
   {
    // Position correction through the use of clamping forces to remove penetration entirely.
    float halfWidth = boundsWidth / 2f;
    float halfHeight = boundsHeight / 2f;
    
    // X 
    if (Mathf.Abs(position.x) + particleRadius > halfWidth)
    {
        position.x = (halfWidth - particleRadius) * Mathf.Sign(position.x); // Position correction
        velocity.x *= -1f; // Conservation of momentum and energy - flip position 
    }
    
    // Y
    if (Mathf.Abs(position.y) + particleRadius > halfHeight)
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
