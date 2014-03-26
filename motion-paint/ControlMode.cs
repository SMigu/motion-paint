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
    class ControlMode
    {
        private int id { get; set; }
        private string name { get; set; }
        private bool isActive { get; set; }

        public ControlMode(ControlManager controlManager)
        {
            id = controlManager.getNextAvailableId();
            name = "default" + id.ToString();
            isActive = false;
        }

        public bool isInteractionActive(HandPointer hands)
        {
            return false;
        }

    }
}
