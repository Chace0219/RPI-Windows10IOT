﻿#pragma checksum "D:\Upwork History\2017\01\RPI-ForkLift\Backup\20170128\IoTCoreDefaultApp\Views\OOBEWelcome.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "415DFBE7F27EA221602148D7608F19D3"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IoTCoreDefaultApp
{
    partial class OOBEWelcome : 
        global::Windows.UI.Xaml.Controls.Page, 
        global::Windows.UI.Xaml.Markup.IComponentConnector,
        global::Windows.UI.Xaml.Markup.IComponentConnector2
    {
        /// <summary>
        /// Connect()
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 14.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1:
                {
                    this.LanguagesListView = (global::Windows.UI.Xaml.Controls.ListView)(target);
                    #line 54 "..\..\..\Views\OOBEWelcome.xaml"
                    ((global::Windows.UI.Xaml.Controls.ListView)this.LanguagesListView).SelectionChanged += this.LanguagesListBox_SelectionChanged;
                    #line default
                }
                break;
            case 2:
                {
                    this.ChooseDefaultLanguage = (global::Windows.UI.Xaml.Controls.StackPanel)(target);
                }
                break;
            case 3:
                {
                    this.CancelButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 86 "..\..\..\Views\OOBEWelcome.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)this.CancelButton).Click += this.CancelButton_Clicked;
                    #line default
                }
                break;
            case 4:
                {
                    this.NextButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 87 "..\..\..\Views\OOBEWelcome.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)this.NextButton).Click += this.NextButton_Clicked;
                    #line default
                }
                break;
            case 5:
                {
                    this.DefaultLanguageProgress = (global::Windows.UI.Xaml.Controls.ProgressBar)(target);
                }
                break;
            case 6:
                {
                    this.DeviceName = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 7:
                {
                    this.IPAddressCaption = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 8:
                {
                    this.IPAddress1 = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 9:
                {
                    this.OSVersionCaption = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 10:
                {
                    this.OSVersion = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            default:
                break;
            }
            this._contentLoaded = true;
        }

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 14.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::Windows.UI.Xaml.Markup.IComponentConnector GetBindingConnector(int connectionId, object target)
        {
            global::Windows.UI.Xaml.Markup.IComponentConnector returnValue = null;
            return returnValue;
        }
    }
}

