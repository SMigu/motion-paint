using System;
using System.Configuration;
using System.IO;

public class SettingsManager : ApplicationSettingsBase
{
    [UserScopedSetting()]
    [DefaultSettingValue("0")]
    public int controlModeId
    {
        get
        {
            return ((int)this["controlModeId"]);
        }
        set
        {
            this["controlModeId"] = (int)value;
        }
    }

    [UserScopedSetting()]
    [DefaultSettingValue("push")]
    public string selectionMode 
    {
        get 
        {
            return ((string)this["selectionMode"]);
        }
        set 
        {
            switch (value)
            {
                case "hover":
                   break;
                case "push":
                   break;
                default:
                   throw new ArgumentException();
            }
        }
    }

    [UserScopedSetting()]
    [DefaultSettingValue("1280")]
    public int screenWidth 
    {
        get 
        {
            return ((int)this["screenWidth"]);       
        }
        set 
        {
            if ((int)value > 1280)
                this["screenWidth"] = (int)value;
            else
                throw new ArgumentOutOfRangeException();
        }
    }

    [UserScopedSetting()]
    [DefaultSettingValue("720")]
    public int screenHeight 
    {
        get 
        {
            return ((int)this["screenHeight"]);
        }
        set 
        {
            if ((int)value > 720)
                this["screenHeight"] = (int)value;
            else
                throw new ArgumentOutOfRangeException();
        }
    }

    [UserScopedSetting()]
    [DefaultSettingValue("true")]
    public bool fullscreen 
    {
        get 
        {
            return ((bool)this["fullscreen"]);
        }
        set 
        {
            this["fullscreen"] = (bool)value;
        }
    }

}
