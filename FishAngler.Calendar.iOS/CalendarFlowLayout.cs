using System;
using Foundation;
using UIKit;

namespace FishAngler.Calendar.iOS
{
    public class CalendarFlowLayout : UICollectionViewFlowLayout
    {
        public override UICollectionViewLayoutAttributes[] LayoutAttributesForElementsInRect(CoreGraphics.CGRect rect)
        {
            var attrsArray = base.LayoutAttributesForElementsInRect(rect);
            foreach (var attrs in attrsArray)
            {
                ApplyLayoutAttributes(attrs);
            }
            return attrsArray;
        }

        public override UICollectionViewLayoutAttributes LayoutAttributesForItem(Foundation.NSIndexPath indexPath)
        {
            var attrs = base.LayoutAttributesForItem(indexPath);
            ApplyLayoutAttributes(attrs);
            return attrs;
        }

        void ApplyLayoutAttributes(UICollectionViewLayoutAttributes attributes)
        {
            if (attributes.RepresentedElementKind != null)
            {
                return;
            }

            if (CollectionView != null)
            {
                var stride = ScrollDirection == UICollectionViewScrollDirection.Horizontal
                        ? CollectionView.Frame.Width
                        : CollectionView.Frame.Height;

                var offset = attributes.IndexPath.Section * stride;
                var xCellOffset = attributes.IndexPath.Item % 7 * ItemSize.Width;
                var yCellOffset = attributes.IndexPath.Item / 7 * ItemSize.Height;

                if (ScrollDirection == UICollectionViewScrollDirection.Horizontal)
                {
                    xCellOffset += offset;
                }
                else
                {
                    yCellOffset += offset;
                }

                attributes.Frame = new CoreGraphics.CGRect(xCellOffset, yCellOffset, ItemSize.Width, ItemSize.Height);
            }
        }
    }
}