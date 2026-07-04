using KnightRun.Core;
using UnityEngine;

namespace KnightRun.Gameplay
{
    public class Collectible : MonoBehaviour
    {
        [SerializeField] int value = 1;

        float spinSpeed = 180f;

        public void BuildPlaceholder()
        {
            var coin = GameObject.CreatePrimitive(PrimitiveType.Quad);
            coin.name = "CoinMesh";
            coin.transform.SetParent(transform, false);
            coin.transform.localScale = Vector3.one * 0.7f;
            coin.transform.localPosition = Vector3.up * 0.45f;
            coin.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            coin.GetComponent<Renderer>().sharedMaterial = World.KnightRunMaterials.Get(World.KnightRunTexture.Coin);
            Destroy(coin.GetComponent<Collider>());

            var collider = gameObject.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 0.5f;
            collider.center = new Vector3(0f, 0.45f, 0f);
        }

        void Update()
        {
            transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            GameManager.Instance?.AddCoin(value);
            Destroy(gameObject);
        }
    }
}
