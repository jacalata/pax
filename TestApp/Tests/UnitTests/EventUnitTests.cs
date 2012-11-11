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

namespace TestApp.Tests.UnitTests
{
    public class EventUnitTests
    {
        
        // constructor
        [TestMethod]
        [TestProperty("TestCategory", "Constructors")]
        public void CreateNewEvent()
        {
            var testEvent = new Event();
            Assert.IsNotNull(testEvent);
        }

        // functionality - star/unstar? 
        // need to assert that the property changed event was raised. 
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

            testEvent.UserNotes = "user details";
            Assert.AreEqual(6, receivedEvents.Count);
            Assert.AreEqual("UserNotes", receivedEvents[5]);

            testEvent.EndTime = new DateTime(1999, 9, 9);
            Assert.AreEqual(7, receivedEvents.Count);
            Assert.AreEqual("EndTime", receivedEvents[6]);

        }

        // test that all the friendly time methods return the correct value


        // add note to event
        // edit note on event
        // delete/clear note from event
        // clear star from event

    }
}
