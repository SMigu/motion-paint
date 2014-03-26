using System;
using System.IO;

public class SettingsManager
{
    private int controlModeId {get; set;}
    private int screenWidth {get; set;}
    private int screenHeight {get; set;}
    private bool fullscreen {get; set;}


    public SettingsManager()
	{
        string path = "conf.txt";
        string line;
        char[] delimiterChars = {' '};
        
        controlModeId = 0;
        screenWidth = 800;
        screenHeight = 600;
        fullscreen = false;

        try
        {
            if (File.Exists(path)) 
            {
                StreamReader file = new StreamReader(path);
                while((line = file.ReadLine()) != null)
                {   
                    string[] configLine = line.Split(delimiterChars);
                    switch (configLine[0])
	                {
                        case "controlModeId":
                            controlModeId = int.Parse(configLine[1]);
                            break;
                        case "screenWidth":
                            screenWidth = int.Parse(configLine[1]);
                            break;
                        case "screenHeight":
                            screenHeight = int.Parse(configLine[1]);
                            break;
                        case "fullscreen":
                            if(configLine[1] == "true")
                            {
                                fullscreen = true;
                            }
                            else
                            {
                                fullscreen = false;
                            }
                            break;
		                default:
                            Console.WriteLine("");
                            break;
	                }
                }
            }
            else
            {
                string[] lines = 
                    { 
                        "controlModeId " + controlModeId.ToString(),
                        "screenWidth " + screenWidth.ToString(),
                        "screenHeight " + screenHeight.ToString(),
                        "fullscreen " + fullscreen.ToString() 
                    };
                System.IO.File.WriteAllLines(path, lines);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("The process failed: {0}", e.ToString());
        }
	}
}
