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
using System.Windows.Media.Media3D;

namespace motion_paint
{
    class ControlManager
    {
        List<ControlMode> modesList = new List<ControlMode>();
        int currentModeId { get; set; }

        public ControlManager() 
        {
            currentModeId = 0;
        }

        public int getNextAvailableId()
        {
            return modesList.Count() + 1;
        }

        public void addControlMode(ControlMode mode) 
        {
            modesList.Add(mode);
        }

        public void changeCurrentControlMode(int id) 
        {
            if(id <= modesList.Count())
            currentModeId = id;
        }

        public bool isActionActive(UserInfo[] userInfos, Skeleton[] skeletons) 
        {
            Dictionary<int, double> userDistanceList = new Dictionary<int, double>();

            foreach (var userInfo in userInfos)
            {
                int userID = userInfo.SkeletonTrackingId;
                if (userID == 0)
                {
                    continue;
                }

                foreach (var skel in skeletons) 
                {
                    double skeletonPosition;
                    if (skel.TrackingId == userID)
                    {
                        skeletonPosition = skel.Position.Z;
                        userDistanceList[userID] = skeletonPosition;
                    }
                    else 
                    {
                        continue;
                    }
                }
            }
            if (userDistanceList.Count() == 0)
                return false;
            	        
		    // get id of the closest user
            double largest = 0.0;
            int UserIdOfLargest = 0;

            foreach (KeyValuePair<int, double> entry in userDistanceList)
            {
                if (entry.Value > largest) {
                    largest = entry.Value;
                    UserIdOfLargest = entry.Key;
                }
            }
            if (UserIdOfLargest == 0) 
            {
                return false;
            }
            	        
		     // check if the closest users action is triggered 
            foreach (var userInfo in userInfos){
                if (userInfo.SkeletonTrackingId == UserIdOfLargest)
                {
                    return modesList.ElementAt(currentModeId).isInteractionActive(userInfo.HandPointers);   
                }
                else
                {
                    continue;
                }
            }
            return false;  
        }

        public Point getCursorLocation(KinectRegion region)
        {
            return modesList.ElementAt(currentModeId).getCursorLocation(region);
        }

        public Point3D getHandLocation()
        {
            return modesList.ElementAt(currentModeId).getHandLocation();
        }
    }
}
