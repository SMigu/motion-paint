using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Interaction;
using Microsoft.Kinect.Toolkit.Controls;
using System.Windows;
using System.Collections.ObjectModel;

namespace motion_paint
{
    class TwoHandMode : ControlMode
    {
        InteractionHandPointer primaryHand;
        InteractionHandPointer secondaryHand;

        public TwoHandMode(ControlManager controlManager) : base(controlManager)
        {
            base.name = "Two Hand Control";
        }

        public bool isInteractionActive(ReadOnlyCollection<InteractionHandPointer> hands)
        {
            // if primary hand is active and secondary hand is active, secondary hand is above certain height -> return true
            
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
                        secondaryHand = hand;
                    }
                }
            }

            if (primaryHand == null || secondaryHand == null || !primaryHand.IsActive)
                return false;

            if (secondaryHand.Y < 1.2)
                return true;
            else
                return false;
        }
    }
}
