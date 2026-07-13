using System.Collections.Generic;
using KnightRun.World;
using UnityEngine;

namespace KnightRun.Player
{
    public class MagicBookVisual : MonoBehaviour
    {
        static readonly Vector3 CoverBaseScale = new Vector3(0.28f, 0.34f, 0.08f);
        static readonly Vector3 PagesBaseScale = new Vector3(0.24f, 0.3f, 0.05f);

        Transform orbitRoot;
        readonly List<Transform> books = new List<Transform>();

        public IReadOnlyList<Transform> Books => books;

        public void Build(Transform parent)
        {
            var orbitGo = new GameObject("MagicBookOrbit");
            orbitGo.transform.SetParent(parent, false);
            orbitGo.transform.localPosition = Vector3.zero;
            orbitRoot = orbitGo.transform;
            SetVisible(false);
        }

        public void SetVisible(bool visible)
        {
            if (orbitRoot != null)
                orbitRoot.gameObject.SetActive(visible);
        }

        public void RebuildBooks(int count, float scaleMultiplier)
        {
            if (orbitRoot == null)
                return;

            count = Mathf.Max(0, count);
            scaleMultiplier = Mathf.Max(0.5f, scaleMultiplier);

            while (books.Count > count)
            {
                int last = books.Count - 1;
                if (books[last] != null)
                    Object.Destroy(books[last].gameObject);
                books.RemoveAt(last);
            }

            while (books.Count < count)
                books.Add(CreateBook($"MagicBook_{books.Count}"));

            for (int i = 0; i < books.Count; i++)
            {
                Transform book = books[i];
                if (book == null)
                    continue;

                book.localScale = Vector3.one * scaleMultiplier;
                float angle = books.Count > 0 ? (360f / books.Count) * i : 0f;
                book.localRotation = Quaternion.Euler(0f, angle, 0f);
            }
        }

        public void UpdateOrbit(float radius, float height, float spinDegreesPerSecond)
        {
            if (orbitRoot == null || !orbitRoot.gameObject.activeSelf || books.Count == 0)
                return;

            orbitRoot.Rotate(Vector3.up, spinDegreesPerSecond * Time.deltaTime, Space.Self);

            for (int i = 0; i < books.Count; i++)
            {
                Transform book = books[i];
                if (book == null)
                    continue;

                float angleRad = (360f / books.Count) * i * Mathf.Deg2Rad;
                book.localPosition = new Vector3(
                    Mathf.Cos(angleRad) * radius,
                    height,
                    Mathf.Sin(angleRad) * radius);

                float bob = Mathf.Sin(Time.time * 4f + i) * 0.04f;
                book.localPosition += Vector3.up * bob;
                book.Rotate(Vector3.up, 120f * Time.deltaTime, Space.Self);
            }
        }

        Transform CreateBook(string name)
        {
            var bookGo = new GameObject(name);
            bookGo.transform.SetParent(orbitRoot, false);

            var cover = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cover.name = "BookCover";
            cover.transform.SetParent(bookGo.transform, false);
            cover.transform.localScale = CoverBaseScale;
            cover.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.KnightArmor);
            Object.Destroy(cover.GetComponent<Collider>());

            var pages = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pages.name = "BookPages";
            pages.transform.SetParent(bookGo.transform, false);
            pages.transform.localScale = PagesBaseScale;
            pages.transform.localPosition = new Vector3(0f, 0f, 0.03f);
            pages.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.Coin);
            Object.Destroy(pages.GetComponent<Collider>());

            return bookGo.transform;
        }
    }
}
