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
using Microsoft.Phone.Scheduler; //for the reminders.

namespace PAX7.Tests
{
    [TestClass]
    public class EventUnitTests
    {
        private Event testEvent;

        [TestInitialize] //note, this will run before every test: I can't just run it for particular ones
        public void TestInit()
        {
            testEvent = new Event();
        }

        [TestCleanup]
        public void CleanUp()
        {
            // clear any reminders that have been set
            testEvent.UnsetReminder();
            testEvent = null;
        }

        // constructor
         [TestMethod]
        [TestProperty("TestCategory", "Constructors")]
        [Tag("event")]
        public void CreateNewEvent()
        {
            Assert.IsNotNull(testEvent);
        }

        /// <summary>
        /// assert that the property changed event was raised when each possible property was changed
        /// </summary>
         [TestMethod]
         [Tag("event")]
        public void ClassProperties_WhenSet_PropertyChangedEventFires()
        {
            List<string> receivedEvents = new List<string>();
            testEvent.PropertyChanged += ((sender, e) => receivedEvents.Add(e.PropertyName));
            
            testEvent.Name = "Session Title";
            Assert.AreEqual(1, receivedEvents.Count);
            Assert.AreEqual("Name", receivedEvents[0]);

            testEvent.Kind = "Session type";
            Assert.AreEqual(2, receivedEvents.Count); 
            Assert.AreEqual("Kind", receivedEvents[1]);
             
            testEvent.StartTime = new DateTime(1999, 9, 8);
            Assert.AreEqual(3, receivedEvents.Count);
            Assert.AreEqual("StartTime", receivedEvents[2]);

            testEvent.Location = "Place";
            Assert.AreEqual(4, receivedEvents.Count);
            Assert.AreEqual("Location", receivedEvents[3]);
            
            testEvent.EndTime = new DateTime(1999, 9, 9);
            Assert.AreEqual(5, receivedEvents.Count);
            Assert.AreEqual("EndTime", receivedEvents[4]);

            testEvent.Star = true;
            Assert.AreEqual(6, receivedEvents.Count);
            Assert.AreEqual("Star", receivedEvents[5]);

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
         [Tag("event")]
        public void VerifyFriendlyStartTime()
        {
            testEvent.StartTime = DateTime.Parse("1/1/2000 09:15:00 PM");
            Assert.AreEqual("Saturday 1/1/2000 9:15 PM", testEvent.friendlyStartTime);
        }

         [TestMethod]
         [Tag("event")]
        public void VerifyTime()
        {
            testEvent.StartTime = DateTime.Parse("1/1/2000 09:15:00 PM");
            Assert.AreEqual("9:15 PM", testEvent.time);
        }

         [TestMethod]
         [Tag("event")]
        public void VerifyDay()
        {
            testEvent.StartTime = DateTime.Parse("1/1/2000 09:15:00 PM");
            Assert.AreEqual("Saturday", testEvent.day);
        }

         [TestMethod]
         [Tag("event")]
        public void VerifyHoursDuration()
        {
            testEvent.StartTime = DateTime.Parse("1/1/2000 09:15:00 PM");
            testEvent.EndTime = DateTime.Parse("1/1/2000 10:15:00 PM");
            Assert.AreEqual(1, testEvent.hoursDuration);
        }

        /// <summary>
        /// test that GetCopy really does have the same values as the event we just created
        /// </summary>
         [TestMethod]
         [Tag("event")]
         [Tag("fixed")]
        public void VerifyGetCopy()
        {
            testEvent.Name = "testing";
            testEvent.Location = "testLoc";
            testEvent.Kind = "testKind";
            testEvent.StartTime = DateTime.Parse("1/1/2000 09:15:00 PM");
            testEvent.EndTime = DateTime.Parse("1/1/2000 10:15:00 PM");
            testEvent.Details = "test details";
            testEvent.UserNotes = "test user notes";
            testEvent.Star = true;

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

        /// <summary>
        /// verify that a reminder is created for a new event
         /// </summary>
         [TestMethod]
         [Tag("event")]
         [Tag("reminder")]
         public void VerifyAddReminder()
         {
             testEvent.Name = "testAReminder";
             testEvent.StartTime = DateTime.Now.AddDays(1);
             testEvent.SetReminder(true);
             ScheduledAction reminder = ScheduledActionService.Find(testEvent._reminderName);
             Assert.IsNotNull(reminder);
         }

        // verify no reminder is created for an event with no time
         /// <summary>
         /// verify that a reminder is created for a new event
         /// </summary>
         [TestMethod]
         [Tag("event")]
         [Tag("reminder")]
         public void VerifyNotAddReminder()
         {
             testEvent.Name = "testNoReminder";
             testEvent.SetReminder(true);
             ScheduledAction reminder = ScheduledActionService.Find(testEvent._reminderName);
             Assert.IsNull(reminder);
         }

        /// <summary>
         /// verify that reminder is deleted when the event is unstarred
         /// </summary>
         [TestMethod]
         [Tag("event")]
        [Tag("reminder")]
         public void VerifyRemoveReminder()
         {
             testEvent.Name = "testRemovingReminder";
             testEvent.StartTime = DateTime.Now.AddDays(1);
             testEvent.SetReminder(true);
             ScheduledAction reminder = ScheduledActionService.Find(testEvent._reminderName);
             Assert.IsNotNull(reminder);
             testEvent.UnsetReminder();
             ScheduledAction reminder2 = ScheduledActionService.Find(testEvent._reminderName);
             Assert.IsNull(reminder2);
         }

        // TODO: what should happen if I add an event to my schedule less than an hour before it is set to begin? Why not set a 10 minute alarm?

    }
}
