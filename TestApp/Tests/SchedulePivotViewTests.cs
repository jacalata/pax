using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using PAX7.View;
using PAX7.ViewModel;

//list<string>
//observablecollection

namespace PAX7.Tests
{
    [TestClass]
    public class SchedulePivotViewTests : SilverlightTest
    {

        private ScheduleViewModel MockViewModel;


        [TestCleanup]
        public void _clearSchedule()
        {
            MockViewModel = null;
        }


          [TestMethod]
        [TestProperty("TestCategory", "Constructors")]
        public void createSchedulePivotView()
        {
            string pivotType = "byDay";
            Fakes.FakeSchedule fakeSchedule = new Fakes.FakeSchedule(false);
            SchedulePivotView pivotView = new SchedulePivotView(pivotType, fakeSchedule);
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
