Rate My App
===========

The Rate My App component is used to create prompts that appear at set intervals and allow the user to provide feedback and rate the application in the Windows Phone Store.

When the application with Rate My App component is started for the 5th time, a dialog for reviewing the app is shown to the user. If the user declines to review the app, he/she will be given the option to provide direct feedback to the developer. On the 10th run of the app, if a review was not already collected, the user will be prompted one more time to rate the app.

This solution consists of Rate My App component and several demo applications demonstrating how Rate My App component can be integrated to various kinds of Windows Phone applications like XAML, Panorama, XAML/XNA, XAML/Direct3D apps.

This solution is hosted in GitHub:
https://github.com/nokia-developer/rate-my-app

Rate My App component is also available through NuGet Package Manager. Search NuGet repositories for "RateMyApp", install it on your application, and follow the instructions in Rate My App Guide to easily integrate review and feedback functionality to your app. 

Developed with Microsoft Visual Studio Express for Windows Phone 2012.

Compatible with:

 * Windows Phone 7
 * Windows Phone 8

Tested to work on:

 * Nokia Lumia 925
 

Instructions
------------

Make sure you have the following installed:

 * Windows 8
 * Windows Phone SDK (min. WP 7.1)

To build and run the sample:

 * Open the SLN file
   * File > Open Project, select the file RateMyApp.sln
 * Right-click RateMyXamlAppWP8 in the Solution Explorer and select "Set as StartUp Project"   
 * Select the target 'Emulator WXGA'.
 * Press F5 to build the project and run it on the Windows Phone Emulator.

To deploy the sample on Windows Phone 8 device:
 * See the official documentation for deploying and testing applications on Windows Phone devices at http://msdn.microsoft.com/en-us/library/windowsphone/develop/ff402565(v=vs.105).aspx

More detailed information can be found in the \Doc\RateMyAppGuide.pdf file. 


About the implementation
------------------------

Important folders:

| Folder | Description |
| ------ | ----------- |
| / | Contains the project file, the license information and this file (README.md) |
| RateMyApp | Root folder for the implementation files of RateMyApp component. |
| RateMyApp/Controls | FeedbackOverlay user control of RateMyApp component. |
| RateMyApp/Helpers | Helper classes for RateMyApp component. |
| RateMyApp/Properties | RateMyApp component properties. |
| RateMyApp/Resources | RateMyApp component localization resources. |
| RateMyAppDemos | Root folder for the demo applications using RateMyApp component. |
| RateMyAppDemos/WP7 | Root folder for WP7 demo applications. |
| RateMyAppDemos/WP7/RateMyXamlAppWP7 | Root folder for RateMyXamlAppWP7 application. |
| RateMyAppDemos/WP7/RateMyXamlXnaApp | Root folder for RateMyXamlXnaApp application. |
| RateMyAppDemos/WP8 | Root folder for WP8 demo applications. |
| RateMyAppDemos/WP8/RateMyDirect3DApp | Root folder for RateMyDirect3DApp application. |
| RateMyAppDemos/WP8/RateMyPanoramaApp | Root folder for RateMyPanoramaApp application. |
| RateMyAppDemos/WP8/RateMyXamlAppWP8 | Root folder for RateMyXamlAppWP8 application. |

Important files:

| File | Description |
| ---- | ----------- |
| FeedbackOverlay.xaml | Rating/Feedback dialog user control. |
| FeedbackOverlay.xaml.cs | Code behind file for Rating/Feedback dialog user control. |
| FeedbackHelper.cs | Helper class to control FeedbackOverlay behaviour. |
| StorageHelper.cs | Helper class to handle RateMyApp component settings. |
| MainPage.xaml | Page that contains FeedbackOverlay control. |
| MainPage.xaml.cs | Handles ApplicationBar visibility. |

Important classes:

| Class | Description |
| ----- | ----------- |
| FeedbackOverlay | The user control to access RateMyApp functionality. |


Localisation
------------

In this first release the component includes strings localized in the following languages: Arabic, German, English US, English UK, Spanish, Finnish, French, Hebrew, Croatian, Hungarian, Italian, Lithuanian, Dutch, Polish, Portuguese BR, Portuguese PT, Romanian, Slovenian and Chinese Simplified.

You are welcome to contribute your own translations (or improve the existing ones) at http://www.getlocalization.com/ratemyapp/


Known issues
------------

No known issues.


License
-------

See the license text file delivered with this project: https://github.com/nokia-developer/rate-my-app/blob/master/License.txt
  

Version history
---------------

 * 1.0 First public release of the current implementation. You can find an earlier version of the project at http://developer.nokia.com/Community/Wiki/File:WP8_Rate_My_App_Flow.zip


