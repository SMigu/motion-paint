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
            return (this.controlModeId);
        }
        set
        {
            this.controlModeId = (int)value;
        }
    }

    [UserScopedSetting()]
    [DefaultSettingValue("push")]
    public string selectionMode 
    {
        get 
        {
            return this.selectionMode;
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
            return this.screenWidth;       
        }
        set 
        {
            if ((int)value > 1280)
                this.screenWidth = (int)value;
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
            return this.screenHeight;
        }
        set 
        {
            if ((int)value > 720)
                this.screenHeight = (int)value;
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
            return this.fullscreen;
        }
        set 
        {
            fullscreen = (bool)value;
        }
    }

}
