using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MobileApp_Try2.Specifications;
using MobileApp_Try2.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileApp_Try2.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SGModuleDetailPage : ContentPage {
		public SGModuleDetailPage()
		{
			InitializeComponent ();
            BindingContext = new SGModuleDetailViewModel();

            slider.BindingContext = this;
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
        }

        private void BuildSlider() {

            // slider, help from https://ahorasomos.izertis.com/solidgear/en/xamarin-diaries-stepped-slider/
            slider.Maximum = 4;
            slider.Minimum = 0;

            stack.Children.Clear();
            var values = new List<string>();
            values.Add("1");
            values.Add("2");
            values.Add("3");
            values.Add("4");
            values.Add("5");

            var ballSize = slider.Height;
            var labelWidth = (slider.Width - ballSize) / (values.Count - 1);
            var textService = DependencyService.Get<ITextWidth>();

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
    }
}
