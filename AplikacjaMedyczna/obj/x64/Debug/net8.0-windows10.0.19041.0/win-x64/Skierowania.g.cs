﻿#pragma checksum "C:\Users\dinod\source\repos\AplikacjaMedyczna\AplikacjaMedyczna\AplikacjaMedyczna\Skierowania.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "664B1D18D1749F7273F529D1914013FC3C1B092B40480A59CD35C6FB12341D9E"
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
    partial class Skierowania : 
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
            case 2: // Skierowania.xaml line 13
                {
                    this.splitView = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.SplitView>(target);
                }
                break;
            case 3: // Skierowania.xaml line 57
                {
                    this.SkierowanieDetailDialog = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.ContentDialog>(target);
                }
                break;
            case 4: // Skierowania.xaml line 76
                {
                    this.NavbarToggleButton = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Primitives.ToggleButton>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Primitives.ToggleButton)this.NavbarToggleButton).Checked += this.NavbarToggleButton_Checked;
                    ((global::Microsoft.UI.Xaml.Controls.Primitives.ToggleButton)this.NavbarToggleButton).Unchecked += this.NavbarToggleButton_Unchecked;
                }
                break;
            case 5: // Skierowania.xaml line 25
                {
                    this.WpisyButton = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)this.WpisyButton).Click += this.NavButton_Click;
                }
                break;
            case 6: // Skierowania.xaml line 26
                {
                    this.ReceptyButton = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)this.ReceptyButton).Click += this.NavButton_Click;
                }
                break;
            case 7: // Skierowania.xaml line 27
                {
                    this.SkierowaniaButton = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)this.SkierowaniaButton).Click += this.NavButton_Click;
                }
                break;
            case 8: // Skierowania.xaml line 28
                {
                    this.WynikiButton = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)this.WynikiButton).Click += this.NavButton_Click;
                }
                break;
            case 9: // Skierowania.xaml line 29
                {
                    this.BackButton = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)this.BackButton).Click += this.NavButton_Click;
                }
                break;
            case 10: // Skierowania.xaml line 32
                {
                    this.FilteredListView = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.ListView>(target);
                    ((global::Microsoft.UI.Xaml.Controls.ListView)this.FilteredListView).ItemClick += this.FilteredListView_ItemClick;
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

