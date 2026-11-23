#if MES_CRYPTO
//using MES.Crypto.Util;
using UnityEngine.UI;
#endif

using TMPro;
using UnityEngine;

namespace MES.Crypto.UI
{
    public class CryptoCurrencyCounter : MonoBehaviour
    {
        public TextMeshProUGUI bitcoinAmountTxt;
        public RectTransform CurrencyIcon;
        public Transform flowStartPos;
        public float offsetRadius = 300f;
        public float coinScale = 1f;
        private float localCoinAmount = 0;
#if MES_CRYPTO
        void OnEnable()
        {
            CryptoCanvasManager.Instance?.Poke(this);
        }
        void Start()
        {
            Button button = GetComponent<Button>();
            if (button)
                button.onClick.AddListener(On_Currency_Click);
        }

        public void SetBitcoinAmount(float currency, bool shouldAdd = false)
        {
            localCoinAmount = shouldAdd ? localCoinAmount + currency : currency;
            bitcoinAmountTxt.text = localCoinAmount.ToString();
        }

        public void On_Currency_Click()
        {
            CryptoCanvasManager.Instance?.On_Currency_Click();
        }
#endif
    }
}
