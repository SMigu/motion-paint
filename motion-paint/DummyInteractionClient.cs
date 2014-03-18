using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect.Toolkit.Interaction;

namespace motion_paint
{
    class DummyInteractionClient : IInteractionClient
    {
        public InteractionInfo GetInteractionInfoAtLocation( int skeletonTrackingId, InteractionHandType handType, double x, double y)
        {
            var interactionInfo = new InteractionInfo();
            interactionInfo.IsGripTarget = true;
            interactionInfo.IsPressTarget = true;
            interactionInfo.PressAttractionPointX = 0.5;
            interactionInfo.PressAttractionPointY = 0.5;
            interactionInfo.PressTargetControlId = 1;

            return interactionInfo;
        }
    }
}
