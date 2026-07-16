using System;
using UnityEngine;

namespace KnightRun.Meta
{
    public class MetaProgression : MonoBehaviour
    {
        public static MetaProgression Instance { get; private set; }

        public int TotalCoins { get; private set; }

        public event Action OnMetaChanged;

        const string CoinsKey = "KnightRun_MetaCoins";

        void Awake()
        {
            Instance = this;
            Load();
        }

        public void Load()
        {
            DebugTestMode.Load();
            TotalCoins = PlayerPrefs.GetInt(CoinsKey, 0);
            CharacterUnlockProgress.Load();
            CharacterSelection.Load();
            HighScoreTable.Load();
            OnMetaChanged?.Invoke();
        }

        public void BankRun(int runCoins, int score, float distance, int enemiesDefeated)
        {
            if (runCoins > 0)
                TotalCoins += runCoins;

            PlayerPrefs.SetInt(CoinsKey, TotalCoins);
            HighScoreTable.TryAdd(score, distance, enemiesDefeated);
            PlayerPrefs.Save();
            OnMetaChanged?.Invoke();
        }

        public bool TryPurchase(ShopUpgradeId id)
        {
            if (!ShopCatalog.CanUpgrade(id))
                return false;

            int cost = ShopCatalog.GetCost(id);
            if (TotalCoins < cost)
                return false;

            TotalCoins -= cost;
            ShopCatalog.IncrementLevel(id);
            PlayerPrefs.SetInt(CoinsKey, TotalCoins);
            PlayerPrefs.Save();
            OnMetaChanged?.Invoke();
            return true;
        }

        public bool TryPurchaseCharacter(HeroCharacterId id)
        {
            if (CharacterOwnership.IsOwned(id))
                return false;

            if (!CharacterCatalog.IsUnlockedForPurchase(id))
                return false;

            int cost = CharacterCatalog.Get(id).PurchaseCost;
            if (TotalCoins < cost)
                return false;

            TotalCoins -= cost;
            CharacterOwnership.SetOwned(id);
            PlayerPrefs.SetInt(CoinsKey, TotalCoins);
            PlayerPrefs.Save();
            OnMetaChanged?.Invoke();
            return true;
        }

        public void ResetAllSaveData()
        {
            TotalCoins = 0;
            PlayerPrefs.DeleteKey(CoinsKey);
            ShopCatalog.ResetAll();
            HighScoreTable.Clear();
            CharacterUnlockProgress.Reset();
            CharacterOwnership.Reset();
            CharacterSelection.ResetToDefault();
            PlayerPrefs.Save();
            OnMetaChanged?.Invoke();
        }
    }
}
