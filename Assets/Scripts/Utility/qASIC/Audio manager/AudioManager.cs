﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using qASIC.FileManagement;

namespace qASIC.AudioManagment
{
    public class AudioManager : MonoBehaviour
    {
        public AudioMixer mixer;
        public bool roundValue = true;

        [Header("Saving")]
        public TextAsset saveFilePreset;
        public string userSavePath = "qASIC/Audio.txt";
        public string editorUserSavePath = "qASIC/Audio-editor.txt";
        private static string _config = string.Empty;

        public static int ChannelCount 
        {
            get
            {
                if (singleton == null) return 0;
                return singleton.channels.Count;
            }
        }

        #region Singleton
        public static AudioManager singleton;

        private void Awake() => AssignSingleton();

        public static bool Paused = false;

        private static void CheckSingleton()
        {
            if (singleton != null) return;
            qDebug.LogError("Audio manager does not exist! Instantiating...");
            new GameObject("Audio Manager - autogenerated").AddComponent<AudioManager>().AssignSingleton();
        }

        private void AssignSingleton()
        {
            if (singleton == null)
            {
                singleton = this;
                DontDestroyOnLoad(gameObject);
                return;
            }
            if(singleton != this) Destroy(this);
        }
        #endregion

        #region Saving
        private void Start()
        {
            if (singleton == this) LoadSettings();
        }

        public static void LoadSettings()
        {
            CheckSingleton();
            string path = singleton.userSavePath;
#if UNITY_EDITOR
            path = singleton.editorUserSavePath;
#endif
            if(singleton.saveFilePreset != null) ConfigController.Repair($"{Application.persistentDataPath}/{path}", singleton.saveFilePreset.text);
            if(!FileManager.TryLoadFileWriter($"{Application.persistentDataPath}/{path}", out _config)) return;
            List<string> sets = ConfigController.CreateOptionList(_config);

            for (int i = 0; i < sets.Count; i++)
            {
                string[] values = sets[i].Split(':');
                if (values.Length != 2) continue;
                if (!float.TryParse(values[1], out float result)) continue;
                SetFloat(values[0], result, false);
            }
        }
        #endregion

        #region Parameters
        public static bool GetFloat(string name, out float value)
        {
            CheckSingleton();
            value = 0f;
            if (singleton.mixer == null) return false;
            return singleton.mixer.GetFloat(name, out value);
        }

        public static void SetFloat(string name, float value, bool preview = true)
        {
            CheckSingleton();
            if (singleton.mixer == null || !singleton.mixer.GetFloat(name, out _))
            {
                if (!preview) qDebug.LogError("Parameter or mixer does not exist! Cannot save or change parameter!");
                return;
            }

            if (singleton.roundValue) value = Mathf.Round(value);
            singleton.mixer.SetFloat(name, value);

            if (!preview || string.IsNullOrWhiteSpace(singleton.userSavePath)) return;

            string path = singleton.userSavePath;
#if UNITY_EDITOR
            path = singleton.editorUserSavePath;
#endif
            _config = ConfigController.SetSetting(_config, name, value.ToString());
            qDebug.Log($"Changed parameter <b>{name}</b> to {value}", "settings");
            FileManager.SaveFileWriter($"{Application.persistentDataPath}/{path}", _config);
        }
        #endregion

        #region Channels
        public Dictionary<string, AudioChannel> channels = new Dictionary<string, AudioChannel>();

        public static AudioChannel GetChannel(string name)
        {
            if (singleton.channels.ContainsKey(name))
            {
                if(singleton.channels[name].Source == null) singleton.channels[name].Source = singleton.gameObject.AddComponent<AudioSource>();
                return singleton.channels[name];
            }

            AudioChannel data = new AudioChannel();
            data.Source = singleton.gameObject.AddComponent<AudioSource>();
            return data;
        }

        public static void SetChannel(string name, AudioChannel channel)
        {
            if (singleton.channels.ContainsKey(name))
            {
                singleton.channels[name] = channel;
                return;
            }
            singleton.channels.Add(name, channel);
        }

        static void StartDestroyCoroutine(ref AudioChannel channel)
        {
            if (channel.DestroyEnum != null)
            {
                singleton.StopCoroutine(channel.DestroyEnum);
                channel.DestroyEnum = null;
            }
            channel.DestroyEnum = channel.DestroyOnPlaybackEnd();
            singleton.StartCoroutine(channel.DestroyEnum);
        }

        static void StopDestroyCoroutine(ref AudioChannel channel)
        {
            if (channel.DestroyEnum != null)
            {
                singleton.StopCoroutine(channel.DestroyEnum);
                channel.DestroyEnum = null;
            }
        }
        #endregion

        #region Play
        public static void Play(string channelName, AudioData data)
        {
            CheckSingleton();
            AudioChannel channel = GetChannel(channelName);
            if (!data.replace && channel.Source.isPlaying) return;

            channel.Source.clip = data.clip;
            channel.Source.loop = data.loop;
            channel.useGlobalControlls = data.useGlobalControls;

            if (singleton.mixer != null) channel.Source.outputAudioMixerGroup = data.group;

            channel.Source.Play();
            if (!Paused || !channel.useGlobalControlls) StartDestroyCoroutine(ref channel);
            else channel.Source.Pause();

            SetChannel(channelName, channel);
        }

        public static void Stop(string channelName)
        {
            CheckSingleton();
            if (!singleton.channels.ContainsKey(channelName)) return;
            AudioChannel channel = singleton.channels[channelName];

            if (channel.Source == null) return;
            Destroy(channel.Source);
            channel = new AudioChannel();

            SetChannel(channelName, channel);
        }

        public static void StopAll()
        {
            AudioSourceController.OnStopAll.Invoke();

            CheckSingleton();

            Dictionary<string, AudioChannel> temp = new Dictionary<string, AudioChannel>(singleton.channels);
            foreach (var item in temp)
            {
                if (singleton.channels[item.Key].Source == null || !singleton.channels[item.Key].useGlobalControlls) continue;
                Destroy(singleton.channels[item.Key].Source);
                SetChannel(item.Key, new AudioChannel());
            }
        }

        public static void Pause(string channelName)
        {
            CheckSingleton();
            if (!singleton.channels.ContainsKey(channelName)) return;
            AudioChannel channel = singleton.channels[channelName];

            if (channel.Source == null || !channel.Source.isPlaying) return;
            channel.Source.Pause();
            channel.paused = true;

            StopDestroyCoroutine(ref channel);

            SetChannel(channelName, channel);
        }

        public static void PauseAll()
        {
            AudioSourceController.OnPauseAll.Invoke();

            CheckSingleton();

            Paused = true;
            Dictionary<string, AudioChannel> temp = new Dictionary<string, AudioChannel>(singleton.channels);
            foreach (var item in temp)
            {
                AudioChannel channel = singleton.channels[item.Key];
                if (channel.Source == null || !channel.Source.isPlaying || !channel.useGlobalControlls) continue;
                channel.Source.Pause();

                StopDestroyCoroutine(ref channel);

                SetChannel(item.Key, channel);
            }
        }

        public static void UnPause(string channelName)
        {
            CheckSingleton();
            if (!singleton.channels.ContainsKey(channelName)) return;
            AudioChannel channel = singleton.channels[channelName];

            if (channel.Source == null || channel.Source.isPlaying) return;
            channel.Source.UnPause();
            channel.paused = false;

            StartDestroyCoroutine(ref channel);

            SetChannel(channelName, channel);
        }

        public static void UnPauseAll()
        {
            AudioSourceController.OnUnPauseAll.Invoke();

            CheckSingleton();

            Paused = false;
            Dictionary<string, AudioChannel> temp = new Dictionary<string, AudioChannel>(singleton.channels);
            foreach (var item in temp)
            {
                AudioChannel channel = singleton.channels[item.Key];
                if (channel.Source == null || channel.Source.isPlaying || channel.paused || !channel.useGlobalControlls) continue;
                channel.Source.UnPause();

                StartDestroyCoroutine(ref channel);

                SetChannel(item.Key, channel);
            }
        }
        #endregion
    }
}