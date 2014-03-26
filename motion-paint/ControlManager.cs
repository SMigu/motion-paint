using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Interaction;
using Microsoft.Kinect.Toolkit.Controls;

namespace motion_paint
{
    class ControlManager
    {
        List<ControlMode> modesList = new List<ControlMode>();
        int currentModeId { get; set; }

        public int getNextAvailableId()
        {
            return modesList.Count() + 1;
        }

        public void addControlMode(ControlMode mode) 
        {
            modesList.Add(mode);
        }

        public bool isActionActive(HandPointer hands) 
        {
            try
            {
               return modesList.ElementAt(currentModeId).isInteractionActive(hands);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
