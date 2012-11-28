using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic; //List<>

using PAX7.Model;
using Microsoft.Silverlight.Testing;
using WindowsPhoneEssentials.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PAX7.Tests
{
    [TestClass]
    public class EventUnitTests
    {

        [TestInitialize] //note, this will run before every test: I can't just run it for particular ones
        public void TestInit()
        {
        }


        // constructor
        [TestMethod]
        [TestProperty("TestCategory", "Constructors")]
        public void CreateNewEvent()
        {
            var testEvent = new Event();
            Assert.IsNotNull(testEvent);
        }

        /// <summary>
        /// assert that the property changed event was raised when each possible property was changed
        /// </summary>
        [TestMethod]
        public void ClassProperties_WhenSet_PropertyChangedEventFires()
        {
            var testEvent = new Event();
            List<string> receivedEvents = new List<string>();
            testEvent.PropertyChanged += ((sender, e) => receivedEvents.Add(e.PropertyName));
            
            testEvent.Name = "Session Title";
            Assert.AreEqual(1, receivedEvents.Count);
            Assert.AreEqual("Name", receivedEvents[0]);

            testEvent.Kind = "Session type";
            Assert.AreEqual(2, receivedEvents.Count); 
            Assert.AreEqual("Kind", receivedEvents[1]);

            testEvent.Star = true;
            Assert.AreEqual(3, receivedEvents.Count);
            Assert.AreEqual("Star", receivedEvents[2]);

            testEvent.StartTime = new DateTime(1999, 9, 8);
            Assert.AreEqual(4, receivedEvents.Count);
            Assert.AreEqual("StartTime", receivedEvents[3]);

            testEvent.Location = "Place";
            Assert.AreEqual(5, receivedEvents.Count);
            Assert.AreEqual("Location", receivedEvents[4]);
            
            testEvent.EndTime = new DateTime(1999, 9, 9);
            Assert.AreEqual(6, receivedEvents.Count);
            Assert.AreEqual("EndTime", receivedEvents[5]);

            testEvent.UserNotes = "user details";
            Assert.AreEqual(7, receivedEvents.Count);
            Assert.AreEqual("UserNotes", receivedEvents[6]);

            testEvent.UserNotes = "user details were edited";
            Assert.AreEqual(8, receivedEvents.Count);
            Assert.AreEqual("UserNotes", receivedEvents[7]);

            testEvent.UserNotes = "";
            Assert.AreEqual(9, receivedEvents.Count);
            Assert.AreEqual("UserNotes", receivedEvents[8]);

            testEvent.Star = false;
            Assert.AreEqual(10, receivedEvents.Count);
            Assert.AreEqual("Star", receivedEvents[9]);
        }

        /// <Summary>
        /// test that all the friendly time methods return the correct value
        /// friendlystarttime, time, day, hoursduration
        /// </Summary>
        [TestMethod]
        public void VerifyFriendlyStartTime()
        {
            var testEvent = new Event();
            testEvent.StartTime = DateTime.Parse("1/1/2000 09:15:00 PM");
            Assert.AreEqual("Saturday 9:15 PM", testEvent.friendlyStartTime);
        }

        [TestMethod]
        public void VerifyTime()
        {
            var testEvent = new Event();
            testEvent.StartTime = DateTime.Parse("1/1/2000 09:15:00 PM");
            Assert.AreEqual("9:15 PM", testEvent.time);
        }

        [TestMethod]
        public void VerifyDay()
        {
            var testEvent = new Event();
            testEvent.StartTime = DateTime.Parse("1/1/2000 09:15:00 PM");
            Assert.AreEqual("Saturday", testEvent.day);
        }

        [TestMethod]
        public void VerifyHoursDuration()
        {
            var testEvent = new Event();
            testEvent.StartTime = DateTime.Parse("1/1/2000 09:15:00 PM");
            testEvent.EndTime = DateTime.Parse("1/1/2000 10:15:00 PM");
            Assert.AreEqual(1, testEvent.hoursDuration);
        }

        /// <summary>
        /// test that GetCopy really does have the same values as the event we just created
        /// </summary>
        [TestMethod]
        public void VerifyGetCopy()
        {
            var testEvent = new Event();
            testEvent.Star = true;
            testEvent.Name = "testing";
            testEvent.Location = "testLoc";
            testEvent.Kind = "testKind";
            testEvent.StartTime = DateTime.Parse("1/1/2000 09:15:00 PM");
            testEvent.EndTime = DateTime.Parse("1/1/2000 10:15:00 PM");
            testEvent.Details = "test details";
            testEvent.UserNotes = "test user notes";

            var copyEvent = testEvent.GetCopy();
            Assert.AreEqual("testing", copyEvent.Name);
            Assert.AreEqual(true, copyEvent.Star);
            Assert.AreEqual("testLoc", copyEvent.Location);
            Assert.AreEqual("testKind", copyEvent.Kind);
            Assert.AreEqual(DateTime.Parse("1/1/2000 09:15:00 PM"), copyEvent.StartTime);
            Assert.AreEqual(DateTime.Parse("1/1/2000 10:15:00 PM"), copyEvent.EndTime);
            Assert.AreEqual("test details", copyEvent.Details);
            Assert.AreEqual("test user notes", copyEvent.UserNotes);

        }



    }
}
