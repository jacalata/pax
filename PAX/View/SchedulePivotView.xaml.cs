using System.Windows;
using System.Windows.Input;
using Microsoft.Phone.Tasks;
using PAX7.Model;
using PAX7.ViewModel;


namespace PAX7.View
{
    public partial class SchedulePivotView
    {
        internal ScheduleViewModel vm = null;
        internal string pivotString = null;
        #region helper text strings
        private string emptyPersonalScheduleResults = "Browse the full schedule and select events to appear here in your filtered personal schedule.";
        private string emptySearchResults = "No events matched your search term.";
        private string baconResults = "The bacon is a lie.";
        #endregion

        public bool isStarView { get; set; }

        public SchedulePivotView() { } //empty constructor 

        /// <summary>
        /// This constructor is only called in test code - pass in a pivotString. 
        /// Regular code will call the empty constructor and then OnNavigatedTo, which will
        /// generate the UI features. This will only get you the actual schedule data.
        /// </summary>
        /// <param name="pivotString"></param>
        internal SchedulePivotView(string pivotString)
        {
            this.pivotString = pivotString;
            LoadSchedule();
        }

        /// <summary>
        /// This page hosts the listing of events for search, personal schedule and generic schedule
        /// React to the navigation parameters we received as the page was opened to decide which view
        /// we are showing and generate the appropriate UI
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            InitializeComponent();

            //read parameters and populate the pivot type
            if (this.NavigationContext.QueryString.ContainsKey("PivotOn"))
                this.pivotString = this.NavigationContext.QueryString["PivotOn"];
            else 
                this.pivotString = "Day";
            
            base.OnNavigatedTo(e);
            isStarView = (this.pivotString == ScheduleViewModel.PivotView.Stars.ToString());
            ApplicationBar.IsVisible = isStarView;

            if (this.pivotString == ScheduleViewModel.PivotView.Search.ToString())
            {
                searchHeader.Visibility = Visibility.Visible;
                schedulePivot.Margin = new Thickness(20, 100, 20, 20); //setting the margin to give space for the search box
                _searchText.Focus(); //focus in textbox
                //now wait for a search to be entered
            }
            else
            {
                //not a search, so just load the schedule view immediately
                startProgressBar();
                searchHeader.Visibility = Visibility.Collapsed;
                LoadSchedule();
            }
        }

        /// <summary>
        /// Begin loading the schedule view we want
        /// If it's a search, this will kick off the database search
        /// </summary>
        /// <param name="inputString">If populated, search the schedule for this string</param>
        internal void LoadSchedule(string inputString=null)
        {
            if ((inputString == null) && (this.pivotString == ScheduleViewModel.PivotView.Search.ToString()))
            {
                // this could be caused by the test constructor?
                return;
            }
            vm = new ScheduleViewModel(this, this.pivotString, inputString);
            vm.LoadSchedule();
        }

        /// <summary>
        /// When triggered by the user, begin a search of the schedule with the user-given text.
        /// Make sure to show a nice responsive loading ui while we search in the background. 
        /// </summary>
        private void search()
        {
            // instant ui reaction
            startProgressBar();
            // now begin actual search
            LoadSchedule(_searchText.Text);
        }

        #region event reactions
        /// <summary>
        /// React to the 'finished loading event listing' event to stop displaying progress/waiting and 
        /// show either the plain list of events, or helper text if we got an empty list
        /// </summary>
        public void OnLoadComplete()
        {
            if (LayoutRoot == null) 
                return; // hack to avoid UI stuff during test
            stopProgressBar();

            schedulePivot.ItemsSource = vm.EventSlices; //this is slow: because I need to do the whole list load first?


            //check if there were any results, and which pivot header they are under 
            bool empty = true;
            int firstResult = 0;
            foreach (ScheduleSlice slice in vm.EventSlices)
            {
                if (slice.events.Count > 0)
                {
                    empty = false;
                    break;
                }
                firstResult++;
            }

            //if there are no elements in 'my schedule' or for search results, show explanatory text
            if (empty == true)
            {
                if (pivotString == ScheduleViewModel.PivotView.Search.ToString())
                {
                    if (_searchText.Text.Contains("bacon")) //added for Russell
                    {
                        explanatoryText.Text = baconResults;
                    }
                    else
                    {
                        explanatoryText.Text = emptySearchResults;
                    }
                }
                else if (pivotString == ScheduleViewModel.PivotView.Stars.ToString())
                {
                    explanatoryText.Text = emptyPersonalScheduleResults;
                }
                explanatoryText.Visibility = Visibility.Visible;
            }
            else
            {
                // swoop the user to the first populated pivot
                schedulePivot.SelectedIndex = firstResult;
            }

        }


        /// <summary>
        /// React to user tapping the search button
        /// </summary>
        /// <param name="sender">search button</param>
        /// <param name="e">event args</param>
        private void searchButtonClick(object sender, RoutedEventArgs e)
        {
            search();
        }

        /// <summary>
        /// React to user hitting 'enter' in the search field
        /// </summary>
        /// <param name="sender">search text field</param>
        /// <param name="e">event args</param>
        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                search();
            }
        }

        private void ScheduleEmail_Click(object sender, System.EventArgs e)
        {
            EmailComposeTask emailComposeTask = new EmailComposeTask();
            emailComposeTask.Subject = "My PAX Schedule";
            emailComposeTask.Body = "Here's a list of the PAX events I've picked out to see this year!\n\n";


            foreach (ScheduleSlice slice in vm.EventSlices)
            {
                if (slice.events.Count > 0)
                {
                    emailComposeTask.Body += slice.slicePageTitle + "\n";
                    foreach (Event evt in slice.events)
                    {
                        emailComposeTask.Body += evt.friendlyStartTime + ", " + evt.Location + ": " +
                                                 evt.Name + "\n";
                    }
                    emailComposeTask.Body += "\n";
                }
                
            }
            emailComposeTask.Body +=
                "\ncreated with PAX Digital Assistant: http://www.windowsphone.com/en-us/store/app/pax-digital-assistant/6e56d005-983a-e011-854c-00237de2db9e";
            emailComposeTask.Show();
        }
        #endregion

        #region UI management
        /// <summary>
        /// Initialise the progress bar and make all the elements of it visible
        /// Make sure the helper text is hidden
        /// </summary>
        private void startProgressBar()
        { //why is the progress bar not starting until we get to the onloaded point? 
            explanatoryText.Visibility = Visibility.Collapsed; //could be searching again after a failed search
            LoadingProgressBar.IsIndeterminate = true;
            LoadingProgressBar.Visibility = Visibility.Visible;
            loadingText.Visibility = Visibility.Visible; //should be controlled with the parent ProgressBar?
            progressBar.Visibility = Visibility.Visible; //covers loadingprogressbar and loadingtext?
        }

        /// <summary>
        /// Turn off the progress bar and hide all elements related to it
        /// Make sure the helper text is hidden (although that shouldn't be necessary)
        /// </summary>
        private void stopProgressBar()
        {
            //explanatoryText.Visibility = Visibility.Collapsed; 
            LoadingProgressBar.IsIndeterminate = false; //for performance, I think?
            progressBar.Visibility = Visibility.Collapsed;
            loadingText.Visibility = Visibility.Collapsed;
            LoadingProgressBar.Visibility = Visibility.Collapsed;
        }
        #endregion

    
    }
}