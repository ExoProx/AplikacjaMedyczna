﻿#pragma checksum "C:\Users\dinod\source\repos\AplikacjaMedyczna\AplikacjaMedyczna\AplikacjaMedyczna\Wyniki.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "C423E29F984C7E77986BC28C3C79C361C55503AE9510B3DB665045F1B18C399B"
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
    partial class Wyniki : 
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
            case 2: // Wyniki.xaml line 14
                {
                    this.splitView = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.SplitView>(target);
                }
                break;
            case 3: // Wyniki.xaml line 35
                {
                    this.NavbarToggleButton = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Primitives.ToggleButton>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Primitives.ToggleButton)this.NavbarToggleButton).Checked += this.NavbarToggleButton_Checked;
                    ((global::Microsoft.UI.Xaml.Controls.Primitives.ToggleButton)this.NavbarToggleButton).Unchecked += this.NavbarToggleButton_Unchecked;
                }
                break;
            case 4: // Wyniki.xaml line 26
                {
                    this.WpisyButton = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)this.WpisyButton).Click += this.NavButton_Click;
                }
                break;
            case 5: // Wyniki.xaml line 27
                {
                    this.ReceptyButton = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)this.ReceptyButton).Click += this.NavButton_Click;
                }
                break;
            case 6: // Wyniki.xaml line 28
                {
                    this.SkierowaniaButton = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)this.SkierowaniaButton).Click += this.NavButton_Click;
                }
                break;
            case 7: // Wyniki.xaml line 29
                {
                    this.WynikiButton = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)this.WynikiButton).Click += this.NavButton_Click;
                }
                break;
            case 8: // Wyniki.xaml line 30
                {
                    this.BackButton = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)this.BackButton).Click += this.NavButton_Click;
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

