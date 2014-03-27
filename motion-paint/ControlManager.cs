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

        public void changeCurrentControlMode(int id) 
        {
            if(id <= modesList.Count())
                currentModeId = id;
        }

        public bool isActionActive(UserInfo[] userInfos, Skeleton[] skeletons) 
        {
            Dictionary<int, double> userDistanceList = new Dictionary<int, double>();
            try
            {
                foreach (var userInfo in userInfos)
                {
                    int userID = userInfo.SkeletonTrackingId;
                    if (userID == 0)
                    {
                        continue;
                    }
                    double skeletonPosition = skeletons[userID].Position.Z;

                    userDistanceList[userID] = skeletonPosition;
                }

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
            catch (Exception)
            {
                return false;
            }
        }
    }
}
