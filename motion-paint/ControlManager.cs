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
            return modesList.Count();
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

        public bool isDrawActive(UserInfo[] userInfos, Skeleton[] skeletons) 
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
            double smallest = 100000.0;
            int UserIdOfClosest = 0;

            foreach (KeyValuePair<int, double> entry in userDistanceList)
            {
                if (entry.Value < smallest) {
                    smallest = entry.Value;
                    UserIdOfClosest = entry.Key;
                }
            }
            if (UserIdOfClosest == 0) 
            {
                return false;
            }
            	        
		     // check if the closest users action is triggered 
            foreach (var userInfo in userInfos){
                if (userInfo.SkeletonTrackingId == UserIdOfClosest)
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

        public double calculateSpeed(Dictionary<Point3D, DateTime> lastLocationData, Dictionary<Point3D, DateTime> currentLocationData)
        {
            var currentPoint = currentLocationData.ElementAt(0).Key;
            var currentTime = currentLocationData.ElementAt(0).Value;
            var lastPoint = lastLocationData.ElementAt(0).Key;
            var lastTime = lastLocationData.ElementAt(0).Value;

            double distance = (double)Math.Sqrt(Math.Pow(lastPoint.X - currentPoint.X, 2) + Math.Pow(lastPoint.Y - currentPoint.Y, 2) + Math.Pow(lastPoint.Z - currentPoint.Z, 2));
            TimeSpan timeDiff = currentTime - lastTime;
            double deltaTime = (double) timeDiff.TotalMilliseconds;

            return distance / deltaTime;
        }
    }
}
