using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Silverlight.Testing.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PAX7.Model;
using PAX7.ViewModel;
using PAX7.View;

namespace PAX7.Tests.Fakes
{
    public partial class FakeSchedulePivotView : Microsoft.Phone.Controls.PhoneApplicationPage
    {
    }

    public partial class FakeSchedulePivotView
    {
        public FakeSchedulePivotView()
        {
            pivotString = "Day";
            searchString = "";
        }

        private ScheduleViewModel vm;        
        public string pivotString{  get;  set;}   //default = "Day"
        public string searchString{ get; set;}  
      //  private bool doEvents;// = false;

        public void OnLoadComplete()
        {
           // if (doEvents)
           //     schedulePivot.ItemsSource = vm.EventSlices; //this is slow because I need to do the whole list load first?
        }

   
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            vm = new ScheduleViewModel(new SchedulePivotView(), pivotString, searchString);
            vm.LoadSchedule();
        }
    }
}
