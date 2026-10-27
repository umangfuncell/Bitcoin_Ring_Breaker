#define BOOMBIT_PURCHASE_ON

using UnityEngine;
using Coredian.InAppPurchases;
using UnityEngine.UI;
using GameAnalyticsSDK;
using System.Linq;

public class InAppManagerUnityIAP_Boombit : InAppsAbstractClass
#if !BOOMBIT_PURCHASE_ON
//  , IStoreListener
#endif
{
    // The store-specific Purchasing subsystems.

    //readonly string[] productIdentifiers = new string[] {
    //    "removeads","vipoffer"
    //};

    string purchasingProduct = "";
    bool IsFromRestore = false;

    public static string COINS_PACK_1 = MyConstants.COINS_PACK_1;
    public static string COINS_PACK_2 = MyConstants.COINS_PACK_2;
    public static string COINS_PACK_3 = MyConstants.COINS_PACK_3;
    public static string INCOME_PACK = MyConstants.INCOME_BUNDLE_PACK;
    public static string SPEED_POWER_BUNDLE_PACK = MyConstants.SPEED_BUNDLE_PACK;
    public static string MEGA_UPGRADE_BUNDLE_PACK = MyConstants.MEGA_UPGRADE_BUNDLE_PACK;

    public static string COINS_PACK_1_PRICE = MyConstants.COINS_PACK_1_PRICE;
    public static string COINS_PACK_2_PRICE = MyConstants.COINS_PACK_2_PRICE;
    public static string COINS_PACK_3_PRICE = MyConstants.COINS_PACK_3_PRICE;
    public static string INCOME_PACK_PRICE = MyConstants.INCOME_BUNDLE_PACK_PRICE;
    public static string SPEED_BUNDLE_PACK_PRICE = MyConstants.SPEED_BUNDLE_PACK_PRICE;
    public static string MEGA_UPGRADE_BUNDLE_PACK_PRICE = MyConstants.MEGA_UPGRADE_BUNDLE_PACK_PRICE;


#if BOOMBIT_PURCHASE_ON

    public override void InitializeInApps()
    {
        //isFromRestore = false;
        Core.GetService<IInAppPurchasesService>().PurchaseUpdated -= OnPurchaseUpdated;
        Core.GetService<IInAppPurchasesService>().ProductsInitializationSucceeded -= OnInitialized;
        Core.GetService<IInAppPurchasesService>().ProductsInitializationFailed -= OnInitializeFailed;
        Core.GetService<IInAppPurchasesService>().RestorePurchasesSucceeded -= OnRestorePurchasesSucceeded;
        Core.GetService<IInAppPurchasesService>().RestorePurchasesFailed -= OnRestorePurchasesFailed;

        print("Initizalise In apps apps");
        Core.GetService<IInAppPurchasesService>().PurchaseUpdated += OnPurchaseUpdated;
        Core.GetService<IInAppPurchasesService>().ProductsInitializationSucceeded += OnInitialized;
        Core.GetService<IInAppPurchasesService>().ProductsInitializationFailed += OnInitializeFailed;
        Core.GetService<IInAppPurchasesService>().RestorePurchasesSucceeded += OnRestorePurchasesSucceeded;
        Core.GetService<IInAppPurchasesService>().RestorePurchasesFailed += OnRestorePurchasesFailed;
        Core.GetService<IInAppPurchasesService>().InitializeProducts();
    }

    private void OnDisable()
    {
        Core.GetService<IInAppPurchasesService>().PurchaseUpdated -= OnPurchaseUpdated;
        Core.GetService<IInAppPurchasesService>().ProductsInitializationSucceeded -= OnInitialized;
        Core.GetService<IInAppPurchasesService>().ProductsInitializationFailed -= OnInitializeFailed;
        Core.GetService<IInAppPurchasesService>().RestorePurchasesSucceeded -= OnRestorePurchasesSucceeded;
        Core.GetService<IInAppPurchasesService>().RestorePurchasesFailed -= OnRestorePurchasesFailed;
    }

    public void OnInitialized(Coredian.InAppPurchases.EventArgs args)
    {
        // Purchasing has succeeded initializing. Collect our Purchasing references.
        Debug.Log("OnInitialized: PASS IAP");
        PrepareShopButtons();
    }

    private void OnInitializeFailed(Coredian.InAppPurchases.EventArgs args)
    {
        // Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
        Debug.Log("OnInitializeFailed IAP:");
    }

    private void PrepareShopButtons()
    {
        var coin_pack1 = Core.GetService<IInAppPurchasesService>().GetProduct(COINS_PACK_1);
        if (coin_pack1 != null)
        {
            Debug.Log("IAP" + $"Product {coin_pack1.Id} costs {coin_pack1.Price}");
            COINS_PACK_1_PRICE = coin_pack1.Price.ToString();
        }
        var coin_pack2 = Core.GetService<IInAppPurchasesService>().GetProduct(COINS_PACK_2);
        if (coin_pack2 != null)
        {
            Debug.Log("IAP" + $"Product {coin_pack2.Id} costs {coin_pack2.Price}");
            COINS_PACK_2_PRICE = coin_pack2.Price.ToString();
        }
        var coin_pack3 = Core.GetService<IInAppPurchasesService>().GetProduct(COINS_PACK_3);
        if (coin_pack3 != null)
        {
            Debug.Log("IAP" + $"Product {coin_pack3.Id} costs {coin_pack3.Price}");
            COINS_PACK_3_PRICE = coin_pack3.Price.ToString();
        }
        var income_bundle_pack = Core.GetService<IInAppPurchasesService>().GetProduct(INCOME_PACK);
        if (income_bundle_pack != null)
        {
            Debug.Log("IAP" + $"Product {income_bundle_pack.Id} costs {income_bundle_pack.Price}");
            INCOME_PACK_PRICE = income_bundle_pack.Price.ToString();
        }
        var speed_bundle_pack = Core.GetService<IInAppPurchasesService>().GetProduct(SPEED_POWER_BUNDLE_PACK);
        if (speed_bundle_pack != null)
        {
            Debug.Log("IAP" + $"Product {speed_bundle_pack.Id} costs {speed_bundle_pack.Price}");
            SPEED_BUNDLE_PACK_PRICE = speed_bundle_pack.Price.ToString();
        }
        var mega_upgrade_pack = Core.GetService<IInAppPurchasesService>().GetProduct(MEGA_UPGRADE_BUNDLE_PACK);
        if (mega_upgrade_pack != null)
        {
            Debug.Log("IAP" + $"Product {mega_upgrade_pack.Id} costs {mega_upgrade_pack.Price}");
            MEGA_UPGRADE_BUNDLE_PACK_PRICE = mega_upgrade_pack.Price.ToString();
        }

        //ApplyPlayerBonuses();
    }

    public override void SetPrice(Text vipOfferNewPrice, Text vipOfferOldPrice, Text noAdsPrice)
    {
    }

    public override void PurchaseProduct(string productId)
    {
        Debug.Log("Buying Product =" + productId);
        //IsFromRestore = false;
        BuyProductID(productId);
    }

    void BuyProductID(string productId)
    {
        Core.GetService<IInAppPurchasesService>().Purchase(productId);
    }

    public override void RestorePurchases()
    {
        IsFromRestore = true;
        Core.GetService<IInAppPurchasesService>().RestorePurchases();
    }

    private void OnPurchaseUpdated(IPurchase purchase)
    {
        if (purchase.State == PurchaseState.Succeeded)
        {
            Debug.Log("IAP :" + $"OnPurchaseUpdated Purchase succeeded. productId: {purchase.Product.Id}");
            PurchaseSuccessfulEvent(purchase);
        }
        else if (purchase.State == PurchaseState.Pending)
        {
            Debug.Log("IAP :" + $"OnPurchaseUpdated Purchase still pending. productId: {purchase.Product.Id}");
        }
        else if (purchase.State == PurchaseState.Failed)
        {
            OnPurchaseFailed();
        }
    }


    void PurchaseSuccessfulEvent(IPurchase args)
    {
        //Retrieve the purchased product
        var productId = args.Product.Id;

        ////Debug.Log("PURCHASE SUCCESS >>>> : " + product.definition.id);
        var price = args.Product.Price;
        var currencyCode = args.Product.CurrencyCode;
        var id = args.Product.StoreId;

        //Debug.LogError("IsFromRestore: " + IsFromRestore + " Product ID: " + productId + " Store ID: " + id);

        if (PlayerPrefs.GetInt(MyConstants.StartFtueCompleted, 0) == 0)
            return;
        //Add the purchased product to the players inventory
        if (productId == COINS_PACK_1)
        {
            OnCoinPackOneSucess();
            //AF_Revenue_Events(currencyCode, price.ToString(), "1", id);
        }
        else if (productId == COINS_PACK_2)
        {
            OnCoinPackTwoSucess();
            // AF_Revenue_Events(currencyCode, price.ToString(), "1", id);
        }
        else if (productId == COINS_PACK_3)
        {
            OnCoinPackThreeSucess();
            //AF_Revenue_Events(currencyCode, price.ToString(), "1", id);
        }
        else if (productId == INCOME_PACK)
        {
            OnIncomePackSuccess();
            //AF_Revenue_Events(currencyCode, price.ToString(), "1", id);
        }
        else if (productId == SPEED_POWER_BUNDLE_PACK)
        {
            OnSpeedAndPowerPackSuccess();
            //AF_Revenue_Events(currencyCode, price.ToString(), "1", id);
        }
        else if (productId == MEGA_UPGRADE_BUNDLE_PACK)
        {
            OnMegaUpgradePackSuccess();
            //AF_Revenue_Events(currencyCode, price.ToString(), "1", id);
        }

        Debug.Log($"Purchase Complete - Product: {productId}");
        //IsFromRestore = false;
    }

    private void OnPurchaseFailed()
    {
        //TODO: set popup for disable
        //InGameNotification.Instance.ShowGenralPopup().DisplayPopup(LocalizationManager.Instance.GetText("Store_13"), LocalizationManager.Instance.GetText("Store_14"), true);
        //InGameNotification.Instance.ShowGenralPopup().DisplayPopup("Failed", "Purchase failed, Check your network connection and try again", true);
        //GenericPopUp.INSTANCE.ShowPopUp(PopUpType.ALERT, "Failed", "Purchase failed, Check your network connection and try again", false, null, null, "OK", "");
        GenericPopUp.Instance.ShowPopUp(PopUpType.Alert, "Failed" + $"\n Purchase Failed", "Ok");
        IsFromRestore = false;

        // A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing
        // this reason with the user to guide their troubleshooting actions.
        Debug.Log("IAP : OnPurchaseFailed: FAIL.");
    }

    public void AF_Revenue_Events(string currency, string revenue, string quantity, string content_id)
    {
        //Debug.LogError("Currency : " + currency + " Revenue : " + revenue + " Quantity: " + quantity + " ID: " + content_id);

        //System.Collections.Generic.Dictionary<string, string> eventParams = new System.Collections.Generic.Dictionary<string, string>();
        //eventParams.Add("af_currency", currency);
        //eventParams.Add("af_revenue", revenue);
        //eventParams.Add("af_quantity", quantity);
        //eventParams.Add("af_content_id", content_id);

        //AppsFlyerSDK.AppsFlyer.sendEvent("af_purchase", eventParams);
    }

    private void OnRestorePurchasesSucceeded(Coredian.InAppPurchases.EventArgs args)
    {
        Debug.Log("OnRestorePurchasesSucceeded");

        ApplyPlayerBonuses();
    }

    public void ApplyPlayerBonuses()
    {
        var income_pack = Core.GetService<IInAppPurchasesService>().Purchases.FirstOrDefault(p => p.Product.Id == INCOME_PACK);
        if (income_pack != null)
        {
            IsFromRestore = true;
            Debug.Log("IAP : " + $"Product {income_pack.Product.Id} was purchased on {income_pack.Date}");
            OnRestore(income_pack);
        }
        var speed_bundle_pack = Core.GetService<IInAppPurchasesService>().Purchases.FirstOrDefault(p => p.Product.Id == SPEED_POWER_BUNDLE_PACK);
        if (speed_bundle_pack != null)
        {
            IsFromRestore = true;
            Debug.Log("IAP : " + $"Product {speed_bundle_pack.Product.Id} was purchased on {speed_bundle_pack.Date}");
            OnRestore(speed_bundle_pack);
        }
        var mega_upgrade_pack = Core.GetService<IInAppPurchasesService>().Purchases.FirstOrDefault(p => p.Product.Id == MEGA_UPGRADE_BUNDLE_PACK);
        if (mega_upgrade_pack != null)
        {
            IsFromRestore = true;
            Debug.Log("IAP : " + $"Product {mega_upgrade_pack.Product.Id} was purchased on {mega_upgrade_pack.Date}");
            OnRestore(mega_upgrade_pack);
        }
    }

    private void OnRestorePurchasesFailed(Coredian.InAppPurchases.EventArgs args)
    {
        Debug.Log("IAP : OnRestorePurchasesFailed");

#if UNITY_IOS
        //GenericPopUp.Instance.ShowPopUp(PopUpType.Alert, $"{Constants.FAILED} \n\n Restore Purchase failed. Please try again later.", Constants.OK);
#endif
        //GenericPopUp.INSTANCE.ShowPopUp(PopUpType.ALERT, "Failed", "Nothing to restore", false, null, null, "OK", "");
    }
#endif
    #region IAP Purchase Call

    public void CoinPack1()
    {
        Debug.Log("Unity Purchase ==> Coin Pack 1 Clicked");
        PurchaseProduct(COINS_PACK_1);
    }

    public void CoinPack2()
    {
        Debug.Log("Unity Purchase ==> Coin Pack 2 Clicked");
        PurchaseProduct(COINS_PACK_2);
    }

    public void CoinPack3()
    {
        Debug.Log("Unity Purchase ==> Coin Pack 3 Clicked");
        PurchaseProduct(COINS_PACK_3);
    }

    public void IncomePack()
    {
        Debug.Log("Unity Purchase ==> Income Pack Clicked");
        PurchaseProduct(INCOME_PACK);
    }

    public void SpeedBundlePack()
    {
        Debug.Log("Unity Purchase ==> Body Speed Power Bundle Clicked");
        PurchaseProduct(SPEED_POWER_BUNDLE_PACK);
    }

    public void MegaBundlePack()
    {
        Debug.Log("Unity Purchase ==> Mega Bundle Clicked");
        PurchaseProduct(MEGA_UPGRADE_BUNDLE_PACK);
    }

    #endregion

    #region IAP Success Event

    void RestorePopup()
    {
        GenericPopUp.Instance.ShowPopUp(PopUpType.Alert, "Successful" + $"\n\n Restored", "Ok");
    }

    void OnIncomePackSuccess()
    {
        //Return popup if already active
        if (PlayerPrefs.GetInt(MyConstants.INCOME_BUNDLE_PURCHASED) == 1)
            return;

        //Debug.LogError("On Income Pack Success");
        //IAPManager.instance.PurchasePack(IAPPackType.IncomePack);
        ShopManager.instance.BuyIncomeBundlePack();

        if (IsFromRestore)
        {
#if UNITY_IOS
            RestorePopup();
            GameAnalytics.NewDesignEvent("iap_purchase:income_pack_restore");
#endif
            return;
        }
        GameAnalytics.NewDesignEvent(MyConstants.GA_INCOME_PACK_SUCCESS);
        GenericPopUp.Instance.ShowPopUp(PopUpType.Alert, "Successful !" + "\n\nIncome Pack purchased successfully", "Ok");
    }

    void OnSpeedAndPowerPackSuccess()
    {
        //Return popup if already active
        if (PlayerPrefs.GetInt(MyConstants.SPEED_POWER_BUNDLE_PURCHASED) == 1)
            return;

        //Debug.LogError("On Speed And Power Pack Success");
        //IAPManager.instance.PurchasePack(IAPPackType.Premiumpack);
        ShopManager.instance.BuySpeedAndPowerPack();

        if (IsFromRestore)
        {
#if UNITY_IOS
            RestorePopup();
            GameAnalytics.NewDesignEvent("iap_purchase:speed_power_pack_restore");
#endif
            return;
        }
        GameAnalytics.NewDesignEvent(MyConstants.GA_SPEED_POWER_PACK_SUCCESS);
        GenericPopUp.Instance.ShowPopUp(PopUpType.Alert, "Successful !" + "\n\nSpeed power purchased successfully", "Ok");
    }

    void OnMegaUpgradePackSuccess()
    {
        //Return popup if already active
        if (PlayerPrefs.GetInt(MyConstants.MEGA_UPGRADE_BUNDEL_PURCHASED) == 1)
            return;

        //Debug.LogError("On Speed And Power Pack Success");
        //IAPManager.instance.PurchasePack(IAPPackType.Premiumpack);
        ShopManager.instance.BuyMegaUpgradePack();

        if (IsFromRestore)
        {
#if UNITY_IOS
            RestorePopup();
            GameAnalytics.NewDesignEvent("iap_purchase:mega_upgrade_pack_restore");
#endif
            return;
        }
        GameAnalytics.NewDesignEvent(MyConstants.GA_MEGA_PACK_SUCCESS);
        GenericPopUp.Instance.ShowPopUp(PopUpType.Alert, "Successful !" + "\n\nMega upgrade pack purchased successfully", "Ok");
    }

    void OnCoinPackOneSucess()
    {
        //Debug.LogError("On Coin Pack 1 Success");
        ShopManager.instance.BuyCoinPack1();

        GameAnalytics.NewDesignEvent(MyConstants.GA_COIN_PACK_1_SUCCESS);
        GenericPopUp.Instance.ShowPopUp(PopUpType.Alert, "Successful !" + "\n\nCoin Pack 1 purchased successfully", "Ok");
    }

    void OnCoinPackTwoSucess()
    {
        //Debug.LogError("On Coin Pack 2 Success");
        ShopManager.instance.BuyCoinPack2();

        GameAnalytics.NewDesignEvent(MyConstants.GA_COIN_PACK_2_SUCCESS);
        GenericPopUp.Instance.ShowPopUp(PopUpType.Alert, "Successful !" + "\n\nCoin Pack 2 purchased successfully", "Ok");
    }

    void OnCoinPackThreeSucess()
    {
        //Debug.LogError("On Coin Pack 3 Success");
        ShopManager.instance.BuyCoinPack3();

        GameAnalytics.NewDesignEvent(MyConstants.GA_COIN_PACK_3_SUCCESS);
        GenericPopUp.Instance.ShowPopUp(PopUpType.Alert, "Successful !" + "\n\nCoin Pack 3 purchased successfully", "Ok");
    }

    #endregion

    #region IAP Restore
    void OnRestore(IPurchase purchase)
    {
        var restoreMessage = "";
        // This does not mean anything was restored,
        // merely that the restoration process succeeded.
        bool restored = false;

        var productStoreId = purchase.Product.StoreId;

        if (PlayerPrefs.GetInt(MyConstants.INCOME_BUNDLE_PURCHASED) == 0 && productStoreId.Equals(INCOME_PACK))
        {
            restored = true;
            OnIncomePackSuccess();
        }
        else if (PlayerPrefs.GetInt(MyConstants.SPEED_POWER_BUNDLE_PURCHASED) == 0 && productStoreId.Equals(SPEED_POWER_BUNDLE_PACK))
        {
            restored = true;
            OnSpeedAndPowerPackSuccess();
        }
        else if (PlayerPrefs.GetInt(MyConstants.MEGA_UPGRADE_BUNDEL_PURCHASED) == 0 && productStoreId.Equals(MEGA_UPGRADE_BUNDLE_PACK))
        {
            restored = true;
            OnSpeedAndPowerPackSuccess();
        }
        restoreMessage = "Restore Successful";

        Debug.Log(restoreMessage);
        //restoreStatusText.text = restoreMessage;
    }
    #endregion
}
