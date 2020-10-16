using UnityEngine;
using DeltaDNA;
using System.Collections.Generic;
#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif

namespace View.Control
{
	/// <summary>
	/// The main game controller.
	/// </summary>
	public class GameController : MonoBehaviour
	{
        private void RecordEvent(string event, Dictionary<string, object> params)
        {
            //DDNA.Instance.RecordEvent (event, params);
            //DDNA.Instance.Upload();
        }
	    void Start()
        {
            DDNA.Instance.Settings.BackgroundEventUploadRepeatRateSeconds = 5;
            DDNA.Instance.Settings.BackgroundEventUpload = false;
            // Configure the SDK
            DDNA.Instance.SetLoggingLevel(DeltaDNA.Logger.Level.DEBUG);
            DDNA.Instance.ClientVersion = "1.0.0";
            
            DDNA.Instance.StartSDK();
            RecordEvent ("start");
#if UNITY_ANDROID
            var channel = new AndroidNotificationChannel()
            {
                Id = "channel_id",
                Name = "Default Channel",
                Importance = Importance.Default,
                Description = "Generic notifications",
            };
            AndroidNotificationCenter.RegisterNotificationChannel(channel);
            var notification = new AndroidNotification();
            notification.Title = "Your Title";
            notification.Text = "Your Text";
            notification.FireTime = System.DateTime.Now.AddSeconds(5);

            AndroidNotificationCenter.SendNotification(notification, "channel_id");
            AndroidNotificationCenter.OnNotificationReceived += OnNotificationReceived;
        }
        public void OnNotificationReceived(AndroidNotificationIntentData data)
        {
            RecordEvent ("notification-received", GetNotificationIntentData(data));
        }
        Dictionary<string, object> GetNotificationIntentData(AndroidNotificationIntentData data)
        {
            return new Dictionary<string, object>
            {
                ["notification-id"] = data.Id,
                ["notification-channel"] = data.Channel,
                ["notification-title"] = data.Notification.Title,
                ["notification-text"] = data.Notification.Text,
                ["notification-firetime"] = data.Notification.FireTime,
            };
#endif
        }
		private void Update() 
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
                RecordEvent ("start-quit");
                DDNA.Instance.StopSDK();
				Application.Quit();
			}
		}
	    void OnApplicationPause(bool pauseStatus)
        {
            var data = AndroidNotificationCenter.GetLastNotificationIntent();
            RecordEvent (pauseStatus ? "pause" : "unpause", GetNotificationIntentData(data));
        }
        void OnApplicationQuit()
        {
            RecordEvent ("quit");
        }
	}
}
