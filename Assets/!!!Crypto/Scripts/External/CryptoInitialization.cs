using System.Collections;
using UnityEngine;

namespace MES.Crypto
{
    public class CryptoInitialization : MonoBehaviour
    {
        public float delayForInit = 0f;
        IEnumerator Start()
        {
            yield return new WaitForSecondsRealtime(delayForInit);
#if MES_CRYPTO
            MES.Crypto.CryptoManager.Instance.Initialize();
#endif
        }
    }
}
