using System;
using System.Runtime.CompilerServices;
using UIKit;

namespace FishAngler.CalendarBar.iOS
{
    public class UIFontHelper
    {
        public bool IsBold(UIFont font)
        {
            return font.FontDescriptor.SymbolicTraits.HasFlag(UIFontDescriptorSymbolicTraits.Bold);
        }

        public UIFont CreateBoldFontFrom(UIFont font)
        {
            if(!IsBold(font))            
            {
                var fontAtrAry = font.FontDescriptor.SymbolicTraits;

                fontAtrAry = fontAtrAry | UIFontDescriptorSymbolicTraits.Bold;
                var fontAtrDetails = font.FontDescriptor.CreateWithTraits(fontAtrAry);
                return UIFont.FromDescriptor(fontAtrDetails, font.PointSize);
            }

            return font;
        }

        public UIFont CreateNormalFontFrom(UIFont font)
        {
        	if (IsBold(font))
        	{
        		var fontAtrAry = font.FontDescriptor.SymbolicTraits;

        		fontAtrAry = fontAtrAry & ~ UIFontDescriptorSymbolicTraits.Bold;
        		var fontAtrDetails = font.FontDescriptor.CreateWithTraits(fontAtrAry);
        		return UIFont.FromDescriptor(fontAtrDetails, font.PointSize);
        	}

            return font;
        }
    }
}
