using UnityEngine;

[CreateAssetMenu(menuName = "Loot/LootTable")]
public class LootTable : ScriptableObject
{
    [System.Serializable]
    public struct LootEntry
    {
        public GameObject prefab;
        [Range(0f, 1f)]
        public float chance;
    }

    public LootEntry[] entries;

    [Header("Scatter")]
    // How far items spread from the drop point
    public float scatterRadius = 0.8f;
    // Upward force on spawn — makes items pop up before landing
    public float scatterUpForce = 3f;
    // Outward force away from drop center
    public float scatterOutForce = 2f;

    public void Drop(Vector3 position)
    {
        foreach (var entry in entries)
        {
            if (Random.value <= entry.chance)
                SpawnScattered(entry.prefab, position);
        }
    }

    private void SpawnScattered(GameObject prefab, Vector3 position)
    {
        // Pick a random horizontal direction for this item
        Vector2 randomCircle = Random.insideUnitCircle.normalized;
        Vector3 outDir = new Vector3(randomCircle.x, 0, randomCircle.y);

        // Spawn slightly above position so the arc is visible
        Vector3 spawnPos = position + Vector3.up * 0.5f;
        GameObject item = Instantiate(prefab, spawnPos, Quaternion.identity);

        // If the item has a Rigidbody, launch it — otherwise just place it
        // offset from the drop point
        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 force = (outDir * scatterOutForce) + (Vector3.up * scatterUpForce);
            rb.AddForce(force, ForceMode.Impulse);
        }
        else
        {
            // No Rigidbody — just place it at a random offset so items
            // don't all stack on the same spot
            Vector2 offset = Random.insideUnitCircle * scatterRadius;
            item.transform.position = position + new Vector3(offset.x, 0, offset.y);
        }
    }
}