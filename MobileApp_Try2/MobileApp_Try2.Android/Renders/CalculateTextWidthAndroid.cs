using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MobileApp_Try2.Specifications;
using Xamarin.Forms;

namespace MobileApp_Try2.Droid.Renders {
    public class CalculateTextWidthAndroid : ITextWidth {
        public double CalculateTextWidth(string text) {
            Android.Graphics.Rect bounds = new Android.Graphics.Rect();
            TextView textView = new TextView(Forms.Context);
            textView.Paint.GetTextBounds(text.ToCharArray(), 0, text.Length, bounds);
            var length = bounds.Width();
            return length / Resources.System.DisplayMetrics.ScaledDensity;
        }
    }
}
