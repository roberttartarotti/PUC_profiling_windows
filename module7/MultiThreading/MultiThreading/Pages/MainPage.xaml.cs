using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MultiThreading
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void ConcurrencyButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ConcurrencyPage));
        }

        private void DeadlockButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(DeadlockPage));
        }

        private void ContentionButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ContentionPage));
        }
    }
}