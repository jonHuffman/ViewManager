namespace Copper.ViewManager.UnitTests
{
    using System;
    using System.Collections;
    using System.Reflection;
    using NUnit.Framework;
    using UnityEngine.SceneManagement;
    using Code;
    using UnityEngine;
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

            Assert.IsFalse(ViewManager.Instance.IsViewActive(View.MainMenu));
            Assert.IsFalse(ViewManager.Instance.IsViewActive(View.SettingsView));
            Assert.IsFalse(ViewManager.Instance.IsViewActive(View.ConfirmationDialog));
        }

        [Test]
        public static void RemoveAllViewsExceptLayer()
        {
            //ViewManager.Instance.AddView(View.MainMenu);
            //ViewManager.Instance.AddView(View.SettingsView);
            //ViewManager.Instance.AddView(View.ConfirmationDialog);
            //ViewManager.Instance.RemoveAllViews(exemptLayers);

            //Assert.IsFalse(ViewManager.Instance.IsViewActive(View.MainMenu));
            //Assert.IsFalse(ViewManager.Instance.IsViewActive(View.ConfirmationDialog));
            //Assert.IsTrue(ViewManager.Instance.IsViewActive(View.SettingsView));
            throw new NotImplementedException("I realized that RemoveAllViews with exempt layers isn't valuable without having a way to easily get Layer IDs (it used to be easier). To be implemented.'");
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
            //ViewManager.Instance.AddView(View.MainMenu);
            //ViewManager.Instance.GetViewIdOnLayer();
            throw new NotImplementedException("I realized that GetViewIdOnLayer isn't valuable without having a way to easily get Layer IDs (it used to be easier). To be implemented.'");
        }
    }
}
