using Microsoft.Kinect.Toolkit.Controls;
using Microsoft.Kinect.Toolkit.Interaction;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace motion_paint
{
    class OneHandMode : ControlMode
    {
        InteractionHandPointer primaryHand;
        InteractionHandEventType lastHandEvent;

        public OneHandMode(ControlManager controlManager) : base(controlManager)
        {
            base.name = "One Hand Control";
        }

        public override bool isInteractionActive(ReadOnlyCollection<InteractionHandPointer> hands)
        {
            // if primary -> return true     
            foreach (var hand in hands)
            {
                if (hand.HandType != InteractionHandType.None)
                {
                    if (hand.IsPrimaryForUser)
                    {
                        primaryHand = hand;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            if(primaryHand.HandEventType != InteractionHandEventType.None)
                lastHandEvent = primaryHand.HandEventType;

            if (primaryHand == null || !primaryHand.IsActive)
                return false;

            if (lastHandEvent == InteractionHandEventType.Grip)
            {
                return true;
            }
            else 
            {
                return false;
            }
        }

        public override Point getCursorLocation(KinectRegion region) 
        {
            var x = region.ActualWidth * primaryHand.X;
            var y = region.ActualHeight * primaryHand.Y;

            return new Point(x, y);
        }

        public override Point3D getHandLocation()
        {
            var x = primaryHand.RawX;
            var y = primaryHand.RawY;
            var z = primaryHand.RawZ;
            
            return new Point3D(x,y,z);
        }
    }
}
