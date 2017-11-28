// Copyright (c) Microsoft. All rights reserved.

using System;

namespace IoTCoreDefaultApp
{
    public static class Constants
    {
        public static string HasDoneOOBEKey = "DefaultAppHasDoneOOBE";
        public static string HasDoneOOBEValue = "YES";
        public static string EnableScreensaverKey = "DefaultAppEnableScreensaver";
        public const string GUID_DEVINTERFACE_USB_DEVICE = "A5DCBF10-6530-11D2-901F-00C04FB951ED";

        public const int nDeviceNum = 101;
        public static string ServerURL = "http://192.168.1.122/ForkLift/api.php";

        //
        public static string EanbleTiltKey = "EnableTiltFunc";
        public static string EanbleCheckListKey = "EnableCheckList";
        public static string TiltSensitiKey = "TiltSensitivity";
        public static string EnableCamKey= "Camera_Activity";
        public static string WebCamPreviewKey = "isPreviewing";

        public static string LastLoggedUserName = "LastLoggedUserName";

        public static string TiltAlarmActivated = "TiltAlarmActivated";

        public static string TiltAlarmCnt = "TiltAlarmCnt";
        public static string RunningTime = "RunningTime";

        public static string LastDBTime_Year = "LastDBTime_Year";
        public static string LastDBTime_Month = "LastDBTime_Month";
        public static string LastDBTime_Day = "LastDBTime_Day";
        public static string LastDBTime_Hour = "LastDBTime_Hour";
        public static string LastDBTime_Min = "LastDBTime_Min";
        public static string LastDBTime_Sec = "LastDBTime_Sec";

        // CheckList Constant, All Boolean
        public static string CheckList1 = "CheckList1";
        public static string CheckList2 = "CheckList2";
        public static string CheckList3 = "CheckList3"; // 
        public static string CheckList4 = "CheckList4";

        //
        public static string CheckMsg1 = "CheckMsg1";
        public static string CheckMsg2 = "CheckMsg2";
        public static string CheckMsg3 = "CheckMsg3"; // 
        public static string CheckMsg4 = "CheckMsg2";
        public static string StartupMsg = "StartupMsg";

        public static string[] TutorialDocNames = {
            "GetStarted",
            "HelloBlinky",
            "GetConnected",
            "GetCoding"
        };

        public const int ForkRunningPIN = 5; // Physical  29 for Input
        public const int ForkActivePIN = 6; // Physical 31 for Output
        public const int ACCELCSPIN = 17;
        public const int MaxResetCount = 6;
    }

    public static class FlashSettings
    {
        public static string StartupMsg { get; set; }
        public static string CheckListMsg1 { get; set; }
        public static string CheckListMsg2 { get; set; }
        public static string CheckListMsg3 { get; set; }
        public static string CheckListMsg4 { get; set; }
    }

    public enum UserTypes { ADMINISTRATOR, SUPERVISOR, GENERALUSER, Unknown };

    public static class UserInfo
    {
        public static DateTime LastLogTime { get; set; }
        public static string LogonTime { get; set; }
        public static string LogoffTime { get; set; }
        public static string LastUser { get; set; }
        public static string textMsg { get; set; }
        public static string UserID { get; set; }
        public static string UserName { get; set; }
        public static string rfid { get; set; }
        public static int accountType { get; set; }
    }



}

