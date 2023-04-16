// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Windowing;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.Diagnostics.Contracts;
using System.Reflection.Metadata;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BoardShare
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BlankWindow1 : Window
    {
        public BlankWindow1()
        {
            this.InitializeComponent();

            //SetTitleBar()
            GetAppWindowAndPresenter();
            _apw.IsShownInSwitchers = false;
            _presenter.SetBorderAndTitleBar(false, false);

        }

        public void GetAppWindowAndPresenter()
        {
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WindowId myWndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            _apw = AppWindow.GetFromWindowId(myWndId);
            _presenter = _apw.Presenter as OverlappedPresenter;
        }
        private AppWindow _apw;
        private OverlappedPresenter _presenter;


        public Side Side{ set; get; }
        public int ScreenIndex { set; get; }

        public bool IsVisible { get; private set; } = true;
        public void Show()
        {
            if (!IsVisible)
            {
                IsVisible = true;
                _apw.Show();
            }
        }
        public void Hide()
        {
            if (IsVisible)
            {
                IsVisible = false;
                _apw.Hide();
            }
        }
    }
}
