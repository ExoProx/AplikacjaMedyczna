﻿#pragma checksum "E:\Programy\Github\AplikacjaMedyczna\AplikacjaMedyczna\PanelGlowny.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "322946C408F493A929A88F34D8794F2FA1901A992D37CF885FC36C87B8E3C767"
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
    partial class PanelGlowny : 
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
            case 2: // PanelGlowny.xaml line 13
                {
                    this.splitView = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.SplitView>(target);
                }
                break;
            case 3: // PanelGlowny.xaml line 37
                {
                    this.NavbarToggleButton = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Primitives.ToggleButton>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Primitives.ToggleButton)this.NavbarToggleButton).Checked += this.NavbarToggleButton_Checked;
                    ((global::Microsoft.UI.Xaml.Controls.Primitives.ToggleButton)this.NavbarToggleButton).Unchecked += this.NavbarToggleButton_Unchecked;
                }
                break;
            case 4: // PanelGlowny.xaml line 26
                {
                    this.PatientChoiceButton = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)this.PatientChoiceButton).Click += this.NavButton_Click;
                }
                break;
            case 5: // PanelGlowny.xaml line 28
                {
                    this.WpisyButton = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)this.WpisyButton).Click += this.NavButton_Click;
                }
                break;
            case 6: // PanelGlowny.xaml line 29
                {
                    this.ReceptyButton = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)this.ReceptyButton).Click += this.NavButton_Click;
                }
                break;
            case 7: // PanelGlowny.xaml line 30
                {
                    this.SkierowaniaButton = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)this.SkierowaniaButton).Click += this.NavButton_Click;
                }
                break;
            case 8: // PanelGlowny.xaml line 31
                {
                    this.WynikiButton = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)this.WynikiButton).Click += this.NavButton_Click;
                }
                break;
            case 9: // PanelGlowny.xaml line 32
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

