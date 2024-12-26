using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LittleBit.Modules.IAppModule.Data.ProductWrappers;
using LittleBit.Modules.IAppModule.Data.Purchases;
using LittleBit.Modules.IAppModule.Services;
using MadPixel.InApps;
using UnityEngine;
using UnityEngine.Purchasing;

namespace LittleBit.MPC.Adapter.InApp
{
    public class InAppMPCInitializer : IIAPService, IDisposable
    {
        private readonly MobileInAppPurchaser _inAppPurchaser;
        private readonly List<OfferConfig> _offerConfigs;

        private IAPService.ProductCollections _productCollection;
        
        
        public event Action<bool, string> OnPurchasingRestored;
        public event Action<string> OnPurchasingSuccess;
        public event Action<string> OnPurchasingFailed;
        public event Action OnInitializationComplete;
        public bool IsInitialized { get; private set; }
        public bool PurchaseRestored { get; }

        public InAppMPCInitializer(MobileInAppPurchaser inAppPurchaser, List<OfferConfig> offerConfigs)
        {
            _productCollection = new IAPService.ProductCollections();
            _inAppPurchaser = inAppPurchaser;
            _offerConfigs = offerConfigs;
            
            Init();

            List<OfferConfig> consumableOffers = new List<OfferConfig>();
            List<OfferConfig> nonConsumableOffers = new List<OfferConfig>();
            List<OfferConfig> subscribeOffers = new List<OfferConfig>();
            foreach (var offerConfig in offerConfigs)
            {
                if (offerConfig.ProductType == ProductType.Consumable)
                    consumableOffers.Add(offerConfig);
                else if (offerConfig.ProductType == ProductType.NonConsumable)
                    nonConsumableOffers.Add(offerConfig);
                else if (offerConfig.ProductType == ProductType.Subscription)
                    subscribeOffers.Add(offerConfig);
                else
                    throw new Exception();
            }

            _inAppPurchaser.StartCoroutine(CheckInit());
            _inAppPurchaser.OnPurchaseResult += OnPurchaseResult;
            _inAppPurchaser.Init(
                offerConfigs
                    .Where(v => v.ProductType == ProductType.NonConsumable)
                    .Select(v => v.Id)
                    .ToList(),
                offerConfigs
                    .Where(v => v.ProductType == ProductType.Consumable)
                    .Select(v => v.Id)
                    .ToList(),
                offerConfigs
                    .Where(v => v.ProductType == ProductType.Subscription)
                    .Select(v => v.Id)
                    .ToList());
            
        }

        private void OnPurchaseResult(Product product)
        {
            if(product == null)
                OnPurchasingFailed?.Invoke(null);
            else
            {
                var id = product.definition.id;
#if IAP_DEBUG || UNITY_EDITOR
                (GetProductWrapper(id) as EditorProductWrapper)!.Purchase();
#endif
              
                OnPurchasingSuccess?.Invoke(id);
            }
        }

        private IEnumerator CheckInit()
        {
            while (_inAppPurchaser.IsInitialized() == false)
            {
                yield return null;
            }

            IsInitialized = true;
            OnInitializationComplete?.Invoke();
        }

        private void Init()
        {
            _offerConfigs.ForEach(offer =>
            {
                _productCollection.AddConfig(offer);
            });

#if IAP_DEBUG || UNITY_EDITOR

            OnInitializationComplete?.Invoke();
            IsInitialized = true;
#endif
        }
        

        public void Purchase(string id, bool freePurchase = false)
        {
            
#if IAP_DEBUG || UNITY_EDITOR
            var product = (GetProductWrapper(id) as EditorProductWrapper);

            if (product is null) return;
            
            if (!product.Metadata.CanPurchase) return;
            
          
            product!.Purchase();
            OnPurchasingSuccess?.Invoke(id);

#else 
            var productRuntime = _inAppPurchaser.GetProduct(id);
            
            if (productRuntime is {availableToPurchase: false}) return;

            if (freePurchase)
            {
                OnPurchasingSuccess?.Invoke(id);
                return;
            }

            _inAppPurchaser.BuyProductInner(id);
#endif

            _inAppPurchaser.BuyProductInner(id);
        }

        public void RestorePurchasedProducts()
        {
            _inAppPurchaser.RestorePurchases();
        }

        public IProductWrapper GetProductWrapper(string id)
        {
#if IAP_DEBUG || UNITY_EDITOR
            return GetDebugProductWrapper(id);
#else
            try
            {
                return GetRuntimeProductWrapper(id);
            }
            catch
            {
                Debug.LogError($"Can't create runtime product wrapper with id:{id}");
                return null;
            }
#endif
        }

        private EditorProductWrapper GetDebugProductWrapper(string id) =>
            _productCollection.GetEditorProductWrapper(id);

        private RuntimeProductWrapper GetRuntimeProductWrapper(string id) =>
            new RuntimeProductWrapper(_inAppPurchaser.GetProduct(id));


        public void Dispose()
        {
            _inAppPurchaser.OnPurchaseResult -= OnPurchaseResult;
            _inAppPurchaser.StopCoroutine(CheckInit());
        }
    }
    
}