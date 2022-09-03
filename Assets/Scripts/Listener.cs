using UnityEngine;
using System.Collections.Generic;
using System;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;

namespace FrostweepGames.WebGLPUNVoice
{
	/// <summary>
	/// Basic listener system for voice chat
	/// </summary>
	public class Listener : MonoBehaviour
	{
		public event Action SpeakersUpdatedEvent;

		public event Action<int> SpeakerLeavedByInactiveEvent;

		private object _lock = new object();

		private bool _listening;

		/// <summary>
		/// Sets if listening of netowrk events should be started at awake
		/// </summary>
		public bool startListenAtAwake;

		/// <summary>
		/// Returns key - value pair : id of a speaker and its object instance
		/// </summary>
		public Dictionary<int, Speaker> Speakers { get; private set; }

		/// <summary>
		/// Returns info about does speakers muted or not
		/// </summary>
		public bool IsSpeakersMuted { get; private set; } = false;

		private void Awake()
		{
			Speakers = new Dictionary<int, Speaker>();

			if(startListenAtAwake)
			{
				StartListen();
			}
		}

		private void OnDestroy()
		{
			StopListen();

			ResetSpeakers();
		}

		private void Update()
		{
			if (!_listening)
				return;

			lock (_lock)
			{
				foreach (var speaker in Speakers)
				{
					speaker.Value.Update();
				}
			}

			CleanInactiveSpeakers();
		}

		/// <summary>
		/// Resets and destroys all active speakers
		/// </summary>
		private void ResetSpeakers()
		{
			lock (_lock)
			{
				foreach (var speaker in Speakers)
				{
					speaker.Value.Dispose();
				}
				Speakers.Clear();
			}
		}

		/// <summary>
		/// cleans inactive speakers
		/// </summary>
		private void CleanInactiveSpeakers()
		{
			lock (_lock)
			{
				List<int> inactive = new List<int>();

				foreach (var speaker in Speakers)
				{
					if(!speaker.Value.IsActive)
					{
						inactive.Add(speaker.Key);
					}
				}

				foreach(int id in inactive)
				{
					Speakers[id].Dispose();
					Speakers.Remove(id);
				}
				inactive.Clear();
			}

			SpeakersUpdatedEvent?.Invoke();
		}

		/// <summary>
		/// Handles data from network connected to specific client id
		/// </summary>
		/// <param name="id">unique id of a remote client</param>
		/// <param name="bytes">array of received data (samples)</param>
		public void HandleRawData(int id, string name, byte[] bytes)
		{
			if (!_listening)
				return;

			lock (_lock)
			{
				byte[] _bytes = new byte[4];
				byte[] _audio = new byte[bytes.Length - 4];
				Array.Copy(bytes, 0, _bytes, 0, 4);
				Array.Copy(bytes, 4, _audio, 0, bytes.Length - 4);
				int senderId = BitConverter.ToInt32(_bytes);
				// Debug.LogError("sender_id : " + senderId + " receiver_id : " + this.GetComponent<PhotonView>().ViewID + " bytes : " + _audio.Length);
				if(senderId == this.GetComponent<PhotonView>().ViewID){
					Speaker speaker;

					if (!Speakers.ContainsKey(id))
					{
						speaker = new Speaker(senderId, name, transform);
						speaker.IsMute = IsSpeakersMuted;

						Speakers.Add(id, speaker);

						SpeakersUpdatedEvent?.Invoke();
					}
					else
					{
						speaker = Speakers[id];
					}
            		speaker._source.volume = 1f;
					speaker._source.spatialBlend = 0;
            		Debug.LogError("volume : " + speaker._source.volume + " 3D : " + speaker._source.spatialBlend);
					speaker.HandleRawData(_audio);
				}
			}
		}

		/// <summary>
		/// PUN event handler of network events
		/// </summary>
		/// <param name="photonEvent"></param>
		private void NetworkEventReceivedHandler(ExitGames.Client.Photon.EventData photonEvent)
		{
			if (photonEvent.Code == Constants.VoiceEventCode)
			{
				// TODO: provide corrent sender name
				// if(this.GetComponent<PhotonView>().ViewID == photonEvent.send){
					HandleRawData(photonEvent.Sender, "Speaker " + photonEvent.Sender, (byte[])photonEvent.CustomData);
				// }
			}
		}

		/// <summary>
		/// Starts listening of network events
		/// </summary>
		public void StartListen()
		{
			if (_listening)
				return;

			// Subscribe on PUN events 
			Photon.Pun.PhotonNetwork.NetworkingClient.EventReceived += NetworkEventReceivedHandler;

			_listening = true;
		}

		/// <summary>
		/// Stops listening of network events
		/// </summary>
		public void StopListen()
		{
			if (!_listening)
				return;

			// Unsubscribe from PUN events 
			Photon.Pun.PhotonNetwork.NetworkingClient.EventReceived -= NetworkEventReceivedHandler;

			_listening = false;
			ResetSpeakers();
		}

		/// <summary>
		/// Disposes speaker by client id
		/// </summary>
		/// <param name="id"></param>
		public void SpeakerLeave(int id)
		{
			if (!_listening)
				return;

			lock (_lock)
			{
				if (Speakers.ContainsKey(id))
				{
					Speakers[id].Dispose();
					Speakers.Remove(id);

					SpeakerLeavedByInactiveEvent?.Invoke(id);
				}
			}

			SpeakersUpdatedEvent?.Invoke();
		}

		/// <summary>
		/// Sets status of mute of all active speakers 
		/// </summary>
		/// <param name="mute"></param>
		public void SetMuteStatus(bool mute)
		{
			IsSpeakersMuted = mute;

			lock (_lock)
			{
				foreach (var speaker in Speakers)
				{
					speaker.Value.IsMute = mute;
				}
			}
		}
	}
}