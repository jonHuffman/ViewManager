namespace Copper.ViewManager.UnitTests
{
    using Code;
    using NUnit.Framework;
    using System;
    using System.Collections;
    using System.Reflection;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.TestTools;

    public class ViewManagerTests
    {
        [UnitySetUp]
        public static IEnumerator SetUp()
        {
            //yield return new EnterPlayMode();
            // There should only be one scene in the project
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(0);

            // Wait until the asynchronous scene fully loads
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            ViewManager.Instance.RemoveAllViews();
        }

        [Test]
        public static void AddBasicView()
        {
            ViewManager.Instance.AddView(View.MainMenu);
            Assert.IsTrue(ViewManager.Instance.IsViewActive(View.MainMenu));
        }

        [Test]
        public static void AddViewWithData()
        {
            const int HEALTH_VALUE = 75;

            ViewManager.Instance.AddView(View.GameHUD, new GameHUD.GameHUDData(HEALTH_VALUE));
            Assert.IsTrue(ViewManager.Instance.IsViewActive(View.GameHUD));

            GameHUD gameHUD = GameObject.FindObjectOfType<GameHUD>();
            FieldInfo gameHUDDataInfo = gameHUD.GetType().GetField("gameHudData", BindingFlags.Instance | BindingFlags.NonPublic);
            GameHUD.GameHUDData internalDataObject = gameHUDDataInfo.GetValue(gameHUD) as GameHUD.GameHUDData;
            Assert.AreEqual(HEALTH_VALUE, internalDataObject.health);
        }

        [Test]
        public static void RemoveView()
        {
            ViewManager.Instance.AddView(View.MainMenu);
            ViewManager.Instance.RemoveView(View.MainMenu);

            Assert.IsFalse(ViewManager.Instance.IsViewActive(View.MainMenu));
        }

        [Test]
        public static void RemoveAllViews()
        {
            ViewManager.Instance.AddView(View.MainMenu);
            ViewManager.Instance.AddView(View.SettingsView);
            ViewManager.Instance.AddView(View.ConfirmationDialog);
            ViewManager.Instance.RemoveAllViews();

            Assert.IsFalse(ViewManager.Instance.IsViewActive(View.MainMenu), "MainMenu View is still active");
            Assert.IsFalse(ViewManager.Instance.IsViewActive(View.SettingsView), "Settings View is still active");
            Assert.IsTrue(ViewManager.Instance.IsViewActive(View.ConfirmationDialog), "Dialog is not active");
        }

        [Test]
        public static void RemoveAllViewsExceptLayer()
        {
            ViewManager.Instance.AddView(View.MainMenu);
            ViewManager.Instance.AddView(View.SettingsView);
            ViewManager.Instance.AddView(View.ConfirmationDialog);
            ViewManager.Instance.RemoveAllViews(new[] { Layer.Popup });

            Assert.IsFalse(ViewManager.Instance.IsViewActive(View.MainMenu), "MainMenu View is still active");
            Assert.IsTrue(ViewManager.Instance.IsViewActive(View.SettingsView), "Settings View is not active");
            Assert.IsTrue(ViewManager.Instance.IsViewActive(View.ConfirmationDialog), "Dialog is not active");
        }

        [Test]
        public static void RemoveAllDialogs()
        {
            ViewManager.Instance.AddView(View.MainMenu);
            ViewManager.Instance.AddView(View.SettingsView);
            ViewManager.Instance.AddView(View.ConfirmationDialog);
            ViewManager.Instance.RemoveAllDialogs();

            Assert.IsTrue(ViewManager.Instance.IsViewActive(View.MainMenu), "MainMenu View is not active");
            Assert.IsTrue(ViewManager.Instance.IsViewActive(View.SettingsView), "Settings View is not active");
            Assert.IsFalse(ViewManager.Instance.IsViewActive(View.ConfirmationDialog), "Dialog is still active");
        }

        [Test]
        public static void AddingViewRemovesViewOnSameLayer()
        {
            ViewManager.Instance.AddView(View.MainMenu);
            ViewManager.Instance.AddView(View.GameHUD, new GameHUD.GameHUDData(100));
            Assert.IsFalse(ViewManager.Instance.IsViewActive(View.MainMenu));
        }

        [Test]
        public static void LayerIsOccupied()
        {
            ViewManager.Instance.AddView(View.SettingsView);
            int viewOnLayer = ViewManager.Instance.GetViewIdOnLayer(Layer.Popup);
            Assert.AreEqual(View.SettingsView, viewOnLayer);
        }

        [Test]
        public static void SetGreyoutAlpha()
        {
            const float GREYOUT_ALPHA = 0.15f;
            ViewManager.Instance.SetGreyoutAlpha(GREYOUT_ALPHA);

            Assembly viewManagerAssembly = typeof(ViewManager).Assembly;
            Type greyoutLayerType = viewManagerAssembly.GetType("Copper.ViewManager.Code.Layers.GreyoutLayer");

            FieldInfo greyoutLayerInfo = typeof(ViewManager).GetField("greyoutLayer", BindingFlags.Instance | BindingFlags.NonPublic);
            object internalDataObject = greyoutLayerInfo.GetValue(ViewManager.Instance);

            FieldInfo greyoutLayerColorInfo = greyoutLayerType.GetField("greyoutColor", BindingFlags.Instance | BindingFlags.NonPublic);
            Color greyoutColor;
            if (greyoutLayerColorInfo.GetValue(internalDataObject) is Color)
            {
                greyoutColor = (Color)greyoutLayerColorInfo.GetValue(internalDataObject);
            }
            else
            {
                throw new NullReferenceException("Could not find Color on GreyoutLayer");
            }

            Assert.AreEqual(GREYOUT_ALPHA, greyoutColor.a);
        }
    }
}
