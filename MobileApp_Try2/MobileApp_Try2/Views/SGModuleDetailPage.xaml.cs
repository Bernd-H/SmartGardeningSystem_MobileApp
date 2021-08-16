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

            // slider, help from https://ahorasomos.izertis.com/solidgear/en/xamarin-diaries-stepped-slider/
            slider.Maximum = 4;
            slider.Minimum = 0;
            stack.Children.Clear();
            var values = new List<Label>();
            values.Add(new Label() { Text = "test1" });
            values.Add(new Label() { Text = "test2" });
            values.Add(new Label() { Text = "test3" });
            values.Add(new Label() { Text = "test4" });
            values.Add(new Label() { Text = "test5" });

            var ballSize = slider.Height;
            var labelWidth = (slider.Width - ballSize) / (values.Count - 1);
            var textService = DependencyService.Get<ITextWidth>();

            for (int i = 0; i < values.Count; i++)
            {
                var textwidth = textService.CalculateTextWidth(values[i].Text);
                var margin = (ballSize / 2) - (textwidth / 2);
                margin = margin > 0 ? margin : 0;
                stack.Children.Add(new Label() {
                    Text = values[i].Text,
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
