﻿#pragma checksum "E:\Programy\Github\AplikacjaMedyczna\AplikacjaMedyczna\StaffLoginPage.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "68C7E7E4E0A51AD90F74D86EB0B7332EE64012411137C7A358AACE8A5C5C5351"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AplikacjaMedyczna
{
    partial class StaffLoginPage : 
        global::Microsoft.UI.Xaml.Controls.Page, 
        global::Microsoft.UI.Xaml.Markup.IComponentConnector
    {

        /// <summary>
        /// Connect()
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.UI.Xaml.Markup.Compiler"," 3.0.0.2411")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 2: // StaffLoginPage.xaml line 38
                {
                    global::Microsoft.UI.Xaml.Controls.Button element2 = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)element2).Click += this.MoveToLogin;
                }
                break;
            case 3: // StaffLoginPage.xaml line 15
                {
                    this.Pesel = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.TextBox>(target);
                    ((global::Microsoft.UI.Xaml.Controls.TextBox)this.Pesel).KeyDown += this.Input_KeyDown;
                    ((global::Microsoft.UI.Xaml.Controls.TextBox)this.Pesel).TextChanged += this.Pesel_TextChanged;
                }
                break;
            case 4: // StaffLoginPage.xaml line 16
                {
                    this.Password = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.PasswordBox>(target);
                    ((global::Microsoft.UI.Xaml.Controls.PasswordBox)this.Password).KeyDown += this.Input_KeyDown;
                }
                break;
            case 5: // StaffLoginPage.xaml line 17
                {
                    global::Microsoft.UI.Xaml.Controls.Button element5 = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)element5).Click += this.LoginButton_Click;
                }
                break;
            case 6: // StaffLoginPage.xaml line 18
                {
                    this.ErrorPESEL = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.TextBlock>(target);
                }
                break;
            case 7: // StaffLoginPage.xaml line 24
                {
                    this.ErrorPassword = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.TextBlock>(target);
                }
                break;
            case 8: // StaffLoginPage.xaml line 30
                {
                    this.ErrorDatabase = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.TextBlock>(target);
                }
                break;
            case 9: // StaffLoginPage.xaml line 36
                {
                    this.ResultTextBlock = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.TextBlock>(target);
                }
                break;
            default:
                break;
            }
            this._contentLoaded = true;
        }


        /// <summary>
        /// GetBindingConnector(int connectionId, object target)
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.UI.Xaml.Markup.Compiler"," 3.0.0.2411")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::Microsoft.UI.Xaml.Markup.IComponentConnector GetBindingConnector(int connectionId, object target)
        {
            global::Microsoft.UI.Xaml.Markup.IComponentConnector returnValue = null;
            return returnValue;
        }
    }
}

