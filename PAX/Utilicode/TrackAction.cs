using System;
using System.ComponentModel.Composition;
using Microsoft.WebAnalytics;

namespace PAX7.Utilicode
{
    public class AnalyticsTracker
    {
        public AnalyticsTracker()
        {
            CompositionInitializer.SatisfyImports(this);
        }

        [Import("Log")]
        public Action<AnalyticsEvent> Log { get; set; }

        public void Track(string category, string name)
        {
            Track(category, name, null);
        }

        public void Track(string category, string name, string actionValue)
        {
            Log(new AnalyticsEvent { Category = category, Name = name, ObjectName = actionValue });
        }
    }
}
