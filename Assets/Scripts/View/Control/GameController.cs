using UnityEngine;
using DeltaDNA;
using System;
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
        string DictionaryToString(Dictionary < string, object > dictionary) {  
            string dictionaryString = "{";  
            foreach(KeyValuePair < string, object > keyValues in dictionary) {  
                dictionaryString += keyValues.Key + " : " + keyValues.Value + ", ";  
            }  
            return dictionaryString.TrimEnd(',', ' ') + "}";  
        }
        float[] Vector3ToFloat(Vector3 vec) {
            return new float[]{vec.x, vec.y, vec.z};
        }
        void RecordEvent(string eventName, Dictionary<string, object> eventParams = null)
        {
            Debug.Log("RecordEvent: " + eventName + " eventParams: " + (eventParams != null ? DictionaryToString(eventParams) : null));
            if (eventParams == null)
            {
                DDNA.Instance.RecordEvent (eventName);
            }
            else
            {
                DDNA.Instance.RecordEvent (eventName, eventParams);
            }
            DDNA.Instance.Upload();
        }
        Dictionary<string, object> GetInputData() {
            return new Dictionary<string, object>{
                ["orientation"] = Input.deviceOrientation,
                ["compass"] = new Dictionary<string, object>{
                    ["headingAccuracy"] = Input.compass.headingAccuracy,
                    ["rawVector"] = Vector3ToFloat(Input.compass.rawVector),
                    ["timestamp"] = Input.compass.timestamp,
                    ["magneticHeading"] = Input.compass.magneticHeading,
                },
                ["gyro"] = new Dictionary<string, object>{
                    ["gravity"] = Vector3ToFloat(Input.gyro.gravity),
                    ["userAcceleration"] = Vector3ToFloat(Input.gyro.userAcceleration),
                    ["attitude"] = Vector3ToFloat(Input.gyro.attitude.eulerAngles),
                }
            };
        }
        void Start()
        {
            RecordEvent ("start", new Dictionary<string, object>{["input"] = GetInputData()});
        }
	    void Awake()
        {
            DDNA.Instance.Settings.BackgroundEventUploadRepeatRateSeconds = 5;
            DDNA.Instance.Settings.BackgroundEventUpload = false;
            // Configure the SDK
            DDNA.Instance.SetLoggingLevel(DeltaDNA.Logger.Level.DEBUG);
            DDNA.Instance.ClientVersion = "1.0.0";
            
            DDNA.Instance.StartSDK();
            RecordEvent ("awake");
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
            Input.compass.enabled = true;
            Input.gyro.enabled = true;
        }
        public void OnNotificationReceived(AndroidNotificationIntentData data)
        {
            RecordEvent ("notification-received", new Dictionary<string, object>{["notification"] = GetNotificationIntentData(data)});
        }
        Dictionary<string, object> GetNotificationIntentData(AndroidNotificationIntentData data)
        {
            return data == null ? null : new Dictionary<string, object>
            {
                ["id"] = data.Id,
                ["channel"] = data.Channel,
                ["title"] = data.Notification.Title,
                ["text"] = data.Notification.Text,
                ["firetime"] = data.Notification.FireTime,
            };
        }
	    void OnApplicationPause(bool pauseStatus)
        {
            var data = new Dictionary<string, object>{
                ["notification"] = GetNotificationIntentData(AndroidNotificationCenter.GetLastNotificationIntent()),
                ["input"] = GetInputData(),
            };
            RecordEvent (pauseStatus ? "pause" : "unpause", data);
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
        void OnApplicationQuit()
        {
            RecordEvent ("quit");
        }
        void OnApplicationFocus(bool hasFocus)
        {
            RecordEvent (hasFocus ? "focus" : "unfocus");
        }
	}
}
