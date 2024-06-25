<!-- default badges list -->
[![](https://img.shields.io/badge/Open_in_DevExpress_Support_Center-FF7200?style=flat-square&logo=DevExpress&logoColor=white)](https://supportcenter.devexpress.com/ticket/details/T1085817)
[![](https://img.shields.io/badge/📖_How_to_use_DevExpress_Examples-e9f6fc?style=flat-square)](https://docs.devexpress.com/GeneralInformation/403183)
[![](https://img.shields.io/badge/💬_Leave_Feedback-feecdd?style=flat-square)](#does-this-example-address-your-development-requirementsobjectives)
<!-- default badges end -->
# Dashboard for WinForms - Inspect the Dashboard Performance

The following example shows how to integrate the Dashboard Diagnostic Tool's functionality in the Dashboard Designer to allow users to inspect the dashboard's performance.

## Overview

The **Inspect** button is inserted into the custom Ribbon's **Performance Diagnostics** page group. The button click starts a new diagnostic session. The Dashboard Diagnostic Tool monitors user actions to collects actions for each event of the event session. When you uncheck the button, the session ends and the resulting report is automatically saved to the XML file. You can open the report in the Diagnostic Tool UI.

![Dashboard Diagnostic Tool integrated into the Dashboard Designer](./images/dashboardMain.png)


## Files to Look at

[DesignerForm1.cs](./CS/DashboardDiagnostis/DesignerForm1.cs) ([DesignerForm1.vb](./VB/DashboardDiagnostis/DesignerForm1.vb))

## Inspect the Dashboard Performance in the Dashboard Designer

1. Download the [Dashboard Diagnostic Tool](https://github.com/DevExpress-Examples/bi-dashboard-diagnosic-tool). 

2. Reference `DiagnosticTool.dll` and install the [Microsoft.Diagnostics.Tracing.TraceEvent](https://www.nuget.org/packages/Microsoft.Diagnostics.Tracing.TraceEvent/) package in your dashboard project. 

3. Create a custom button and insert it in the Ribbon.


4. Create a `DiagnosticController` object. 

5. Call the controller's Start() and Stop() methods on a button click to run and finish the Dashboard Diagnostic Tool's session.


6. Implement the `IFileController` interface and specify the output file path in the `TrySaveFile` method. Pass a new class instance that implements `IFileController` in the controller's constructor. 


7. To save the resulting report to the specified output path, call the controller's `Save()` method.

## Documentation

- [Ribbon](https://docs.devexpress.com/Dashboard/15732/winforms-dashboard/winforms-designer/ui-elements-and-customization/ui-elements/ribbon#configure-ribbon-at-runtime)
- [BI Dashboard Diagnostic Tool](https://docs.devexpress.com/Dashboard/403867/basic-concepts-and-terminology/bi-dashboard-performance/bi-dashboard-diagnostic-tool)
<!-- feedback -->
## Does this example address your development requirements/objectives?

[<img src="https://www.devexpress.com/support/examples/i/yes-button.svg"/>](https://www.devexpress.com/support/examples/survey.xml?utm_source=github&utm_campaign=dashboard-for-winforms-diagnose-performance&~~~was_helpful=yes) [<img src="https://www.devexpress.com/support/examples/i/no-button.svg"/>](https://www.devexpress.com/support/examples/survey.xml?utm_source=github&utm_campaign=dashboard-for-winforms-diagnose-performance&~~~was_helpful=no)

(you will be redirected to DevExpress.com to submit your response)
<!-- feedback end -->
