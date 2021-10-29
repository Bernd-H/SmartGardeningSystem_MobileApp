using System;
using System.Collections.Generic;
using MobileApp.BusinessLogic.ViewModels;
using MobileApp.Common.Configuration;
using MobileApp.Common.Specifications;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileApp.Views {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddModulePage : ContentPage {
        public AddModulePage() {
            InitializeComponent();
            Shell.SetTabBarIsVisible(this, false);

            this.BindingContext = IoC.Get<AddModuleViewModel>();

            slider.BindingContext = this.BindingContext;
            slider.PropertyChanged += (sender, e) => {
                var slider = (Slider)sender;
                if (e.PropertyName == "Height") {
                    BuildSlider();
                }
                else if (e.PropertyName == "Width" && slider.Height > 0) {
                    //Screen rotation
                    BuildSlider();
                }
            };

            wateringMethodPicker.BindingContext = this.BindingContext;
        }

        #region slider calculations

        private void BuildSlider() {

            // slider, help from https://ahorasomos.izertis.com/solidgear/en/xamarin-diaries-stepped-slider/
            slider.Maximum = 2;
            slider.Minimum = 0;

            stack.Children.Clear();
            var values = new List<string>();
            values.Add("1");
            values.Add("2");
            values.Add("3");

            var ballSize = slider.Height;
            var labelWidth = (slider.Width - ballSize) / (values.Count - 1);
            var textService = IoC.Get<ITextWidth>();

            for (int i = 0; i < values.Count; i++) {
                var textwidth = textService.CalculateTextWidth(values[i]);
                var margin = (ballSize / 2) - (textwidth / 2);
                margin = margin > 0 ? margin : 0;
                stack.Children.Add(new Label() {
                    Text = values[i],
                    WidthRequest = i == values.Count - 1 ? ballSize - margin : labelWidth - margin,
                    HorizontalTextAlignment = i == values.Count - 1 ? TextAlignment.End : TextAlignment.Start,
                    Margin = i == values.Count - 1 ? new Thickness(0, 0, margin, 0) : new Thickness(margin, 0, 0, 0),
                    LineBreakMode = LineBreakMode.NoWrap
                });
            }
        }

        void Slider_ValueChanged(object sender, Xamarin.Forms.ValueChangedEventArgs e) {
            var newStep = Math.Round(e.NewValue / 1);

            var slider = (Slider)sender;
            slider.Value = newStep * 1;
        }

        #endregion
    }
}
