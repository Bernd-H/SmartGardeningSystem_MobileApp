using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using MobileApp.Common.Specifications;
using UIKit;

namespace MobileApp.iOS.Renders {
    public class CalculateTextWidthiOS : ITextWidth {
        public double CalculateTextWidth(string text) {
            var uiLabel = new UILabel();
            uiLabel.Text = text;
            var length = uiLabel.Text.StringSize(uiLabel.Font);
            return length.Width;
        }
    }
}
