﻿#pragma checksum "D:\Upwork History\2017\01\RPI-ForkLift\Backup\20170128\IoTCoreDefaultApp\Views\OOBENetwork.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "BDADD2A768D8385098014CF5F1DC370D"
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
    partial class OOBENetwork : 
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
                    this.rootPage = (global::Windows.UI.Xaml.Controls.Page)(target);
                }
                break;
            case 2:
                {
                    this.WifiInitialState = (global::Windows.UI.Xaml.DataTemplate)(target);
                }
                break;
            case 3:
                {
                    this.WifiConnectState = (global::Windows.UI.Xaml.DataTemplate)(target);
                }
                break;
            case 4:
                {
                    this.WifiPasswordState = (global::Windows.UI.Xaml.DataTemplate)(target);
                }
                break;
            case 5:
                {
                    this.WifiConnectingState = (global::Windows.UI.Xaml.DataTemplate)(target);
                }
                break;
            case 6:
                {
                    global::Windows.UI.Xaml.Controls.Button element6 = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 120 "..\..\..\Views\OOBENetwork.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)element6).Click += this.CancelButton_Clicked;
                    #line default
                }
                break;
            case 7:
                {
                    global::Windows.UI.Xaml.Controls.Button element7 = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 94 "..\..\..\Views\OOBENetwork.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)element7).Click += this.NextButton_Clicked;
                    #line default
                }
                break;
            case 8:
                {
                    global::Windows.UI.Xaml.Controls.Button element8 = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 95 "..\..\..\Views\OOBENetwork.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)element8).Click += this.CancelButton_Clicked;
                    #line default
                }
                break;
            case 9:
                {
                    global::Windows.UI.Xaml.Controls.PasswordBox element9 = (global::Windows.UI.Xaml.Controls.PasswordBox)(target);
                    #line 91 "..\..\..\Views\OOBENetwork.xaml"
                    ((global::Windows.UI.Xaml.Controls.PasswordBox)element9).PasswordChanged += this.WifiPasswordBox_PasswordChanged;
                    #line 91 "..\..\..\Views\OOBENetwork.xaml"
                    ((global::Windows.UI.Xaml.Controls.PasswordBox)element9).Loaded += this.WifiPasswordBox_Loaded;
                    #line default
                }
                break;
            case 10:
                {
                    global::Windows.UI.Xaml.Controls.CheckBox element10 = (global::Windows.UI.Xaml.Controls.CheckBox)(target);
                    #line 66 "..\..\..\Views\OOBENetwork.xaml"
                    ((global::Windows.UI.Xaml.Controls.CheckBox)element10).Checked += this.ConnectAutomaticallyCheckBox_Changed;
                    #line 66 "..\..\..\Views\OOBENetwork.xaml"
                    ((global::Windows.UI.Xaml.Controls.CheckBox)element10).Unchecked += this.ConnectAutomaticallyCheckBox_Changed;
                    #line default
                }
                break;
            case 11:
                {
                    global::Windows.UI.Xaml.Controls.Button element11 = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 69 "..\..\..\Views\OOBENetwork.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)element11).Click += this.ConnectButton_Clicked;
                    #line default
                }
                break;
            case 12:
                {
                    this.SkipButton = (global::Windows.UI.Xaml.Controls.HyperlinkButton)(target);
                    #line 169 "..\..\..\Views\OOBENetwork.xaml"
                    ((global::Windows.UI.Xaml.Controls.HyperlinkButton)this.SkipButton).Click += this.SkipButton_Clicked;
                    #line default
                }
                break;
            case 13:
                {
                    this.NoneFoundText = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 14:
                {
                    this.DirectConnectionStackPanel = (global::Windows.UI.Xaml.Controls.StackPanel)(target);
                }
                break;
            case 15:
                {
                    this.WifiListView = (global::Windows.UI.Xaml.Controls.ListView)(target);
                    #line 164 "..\..\..\Views\OOBENetwork.xaml"
                    ((global::Windows.UI.Xaml.Controls.ListView)this.WifiListView).SelectionChanged += this.WifiListView_SelectionChanged;
                    #line default
                }
                break;
            case 16:
                {
                    this.NoWifiFoundText = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 17:
                {
                    this.RefreshButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 162 "..\..\..\Views\OOBENetwork.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)this.RefreshButton).Click += this.RefreshButton_Click;
                    #line default
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
