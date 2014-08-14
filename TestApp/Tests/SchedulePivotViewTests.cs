using System;
using System.IO.IsolatedStorage;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Silverlight.Testing;
using Microsoft.Silverlight.Testing.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using PAX7.View;
using PAX7.ViewModel;

using System.Collections.Generic; //list<string>
using System.Collections.ObjectModel; //observablecollection

namespace PAX7.Tests
{
    [TestClass]
    public class SchedulePivotViewTests : SilverlightTest
    {

        private ScheduleViewModel MockViewModel;


        [TestInitialize]
        [TestCleanup]
        public void _clearSchedule()
        {
            MockViewModel = null;
            IsolatedStorageSettings.ApplicationSettings.Clear();
        }


          [TestMethod]
        [TestProperty("TestCategory", "Constructors")]
        public void createSchedulePivotView()
        {
            string pivotType = "byDay";
            Assert.IsTrue(IsolatedStorageSettings.ApplicationSettings.Count == 0);
            SchedulePivotView pivotView = new SchedulePivotView(pivotType);
            Assert.IsNotNull(pivotView);
        }

        /*
       * These tests need the View itself, as the error messages are handled entirely by it. 
       * 
       [TestMethod]
      [Asynchronous]
      [Tag("search")]
      public void VerifyEmptyResult()
      {
          //read the explanatoryText and verify it says 'there were no results'
      }
      
       [TestMethod]
      [Asynchronous]
      [Tag("search")]
       public void VerifyBaconTrickResult()
       {
         // read the explanatoryText and verify it says something something bacon
       }
         
         [TestMethod]
        [Asynchronous]
        [Tag("search")]
        public void VerifyEmptyMyScheduleText()
        {
            // read the explanatoryText and verify it says something something add to my schedule
        }
         */

    }
}
