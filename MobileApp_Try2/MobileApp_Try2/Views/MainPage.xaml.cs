using Microcharts;
using MobileApp_Try2.Models;
using MobileApp_Try2.ViewModels;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileApp_Try2.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            this.BindingContext = new MainPageViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();


            var entries = new[]
 {
     new ChartEntry(212)
     {
         Label = "UWP",
         ValueLabel = "212",
     },
     new ChartEntry(248)
     {
         Label = "Android",
         ValueLabel = "248",
     },
     new ChartEntry(128)
     {
         Label = "iOS",
         ValueLabel = "128",
     },
     new ChartEntry(514)
     {
         Label = "Shared",
         ValueLabel = "514",
} };

            var chart = new LineChart() { Entries = entries };

            chartView.Chart = chart;
        }
    }
}