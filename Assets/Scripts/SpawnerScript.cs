using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    enum SpawnerType { Straight, Spin }

    [Header("Bullet Attributes")]
    public GameObject bullet;
    public float bulletLife = 1f;
    public float speed = 1f;

    [Header("Spawner Attributes")]
    [SerializeField] private SpawnerType spawnerType;
    [SerializeField] private float firingRate = 1f;

    [Header("Sine Pattern (Straight)")]
    [Tooltip("Max swing (in degrees) from the starting rotation.")]
    public float sineAmplitude = 5f;
    [Tooltip("Oscillation speed (cycles per second).")]
    public float sineFrequency = 2f;

    private float timer = 0f;
    private float initialZ;

    void Start()
    {
        // Cache the spawner’s starting Z angle
        initialZ = transform.eulerAngles.z;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (spawnerType == SpawnerType.Spin)
        {
            // your existing spin behavior
            transform.eulerAngles = new Vector3(0f, 0f, transform.eulerAngles.z + 0.5f);
        }
        else if (spawnerType == SpawnerType.Straight)
        {
            // apply a narrow sine oscillation around initialZ
            float z = initialZ + Mathf.Sin(Time.time * sineFrequency * Mathf.PI * 2f) * sineAmplitude;
            transform.rotation = Quaternion.Euler(0f, 0f, z);
        }

        if (timer >= firingRate)
        {
            Fire();
            timer = 0f;
        }
    }

    private void Fire()
    {
        if (bullet == null) return;

        var spawnedBullet = Instantiate(bullet, transform.position, transform.rotation);
        var b = spawnedBullet.GetComponent<Bullet>();
        b.speed = speed;
        b.bulletLife = bulletLife;
    }
}
