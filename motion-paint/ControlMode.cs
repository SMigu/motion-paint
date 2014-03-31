using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Interaction;
using Microsoft.Kinect.Toolkit.Controls;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media.Media3D;

namespace motion_paint
{
    class ControlMode
    {
        protected int id { get; set; }
        protected string name { get; set; }
        protected bool isActive { get; set; }

        public ControlMode(ControlManager controlManager)
        {
            id = controlManager.getNextAvailableId();
            name = "default" + id.ToString();
            isActive = false;
        }

        public ControlMode(ControlManager controlManager, string modeName) 
        {
            id = controlManager.getNextAvailableId();
            name = modeName;
            isActive = false;
        }

        public virtual bool isInteractionActive(ReadOnlyCollection<InteractionHandPointer> hands)
        {
            return false;
        }

        public virtual Point getCursorLocation(KinectRegion region)
        {
            return new Point();
        }

        public virtual Point3D getHandLocation()
        {
            return new Point3D();
        }

    }
}
