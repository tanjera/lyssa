/* Ibi Keller
 * Audio_Handler.cs
 *
 * Manages audio for player and acts as a utility for other gameobjects
 */

using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class __Audio : MonoBehaviour {
	
	GameObject _Player;
	AudioSource _Audio;
	__Game _Game;
	__Menu _Caption_Display;
	
	// Gameobject to play error sounds through (e.g. a preceptor, coworker)
	public AudioSource _Error_Audio;
	
	public enum Clip_Flags {
		NULL,
		PAUSE,
	
		HUM,
		BEEP,
		RING,
		SIREN,
		ALARM,
		PHONE,
		LIGHT,
		SQUIRT,
		FLUID,
		
		ERROR,
		ERROR_REACH,
		
		SUCCESS,
		FAILURE,
		
		MALE,
		FEMALE,
		VOICE
	}	
	public class _AudioClip {
		public AudioClip _Clip;
		public string _Caption = "";
		public List<Clip_Flags> _Flags = new List<Clip_Flags>();
		
		public _AudioClip (AudioClip Clip) {
			_Clip = Clip;
		}
		public string Name {
			get { return _Clip.name; }
		}
		public bool Caption {
			get { return _Caption != String.Empty; }
		}
	}
	public class _AudioTrigger {
		public _AudioClip _Clip;
		public AudioSource _Source;
		
		public _AudioTrigger (_AudioClip Clip, AudioSource Source) {
			_Clip = Clip;
			_Source = Source;
		}
	}
	
	public List<_AudioClip> _Clips = new List<_AudioClip>();
	public List<_AudioTrigger> _Triggers = new List<_AudioTrigger>();
	
	
	void Awake () {
		_Player = GameObject.Find("__Player");
		_Audio = _Player.AddComponent<AudioSource>();
		_Game = GameObject.Find("__Game Controller").GetComponent<__Game>();
        _Caption_Display = (Instantiate(GameObject.Find("__Audio Captions GUI")) as GameObject).GetComponent<__Menu>().Instantiate(this.gameObject);
		
		if (_Error_Audio == null)
			_Error_Audio = _Audio;
		
		/* Import and sort the audio clips */
		UnityEngine.Object[] Import_Audio = Resources.LoadAll("Audio", typeof(AudioClip));
		
		foreach (UnityEngine.Object eachObject in Import_Audio){
			if (eachObject is AudioClip) {
				string fileName = eachObject.name.ToUpper();
				_AudioClip eachClip = new _AudioClip(eachObject as AudioClip);
				
				foreach (string eachEnum in Enum.GetNames(typeof(Clip_Flags)))
					if (fileName.Contains(eachEnum.ToUpper()))
						eachClip._Flags.Add((Clip_Flags)(Enum.Parse(typeof(Clip_Flags), eachEnum)));
				
				_Clips.Add(eachClip);
			}
		}
		
		/* Import and sort captions */
		int i = -1, j = -1;


        //FIXME can't use streamreader in webplayer


		string Captions = new StreamReader(String.Format("{0}/Resources/Audio/__Captions.txt", Application.dataPath)).ReadToEnd();
		
		foreach (_AudioClip eachClip in _Clips) {
			if ((i = Captions.IndexOf(eachClip.Name)) < 0)
				continue;
			
			eachClip._Caption = ((j = Captions.IndexOf(System.Environment.NewLine, i)) < 0
					? Captions.Substring(i)
					: Captions.Substring(i, j - i))
				.Replace(eachClip.Name, String.Empty).Trim();
		}
		
		// List the audio clips?
		// _Clips.ForEach((obj) => Debug.Log(obj.Name));
	}
	
	IEnumerator Caption__Show(_AudioClip _Clip) {
		if (!_Game._Option__Captions || !_Clip.Caption)
			yield break;
		
		_Caption_Display.Text__Set("_Caption__Text", _Clip._Caption, "black");
		_Caption_Display.Show();
		yield return new WaitForSeconds(_Clip._Clip.length);
		_Caption_Display.Hide();
		_Caption_Display.Text__Clear("_Caption__Text");
		
		yield break;
	}
	public void Play__Once (Clip_Flags Flag, bool Randomly = true) { 
		Play__Once(_Audio, new Clip_Flags[] { Flag }, Randomly); 
	}
	public void Play__Once (Clip_Flags[] Flags, bool Randomly = true) { 
		Play__Once(_Audio, Flags, Randomly); 
	}
	public void Play__Once (AudioSource Source, Clip_Flags Flag, bool Randomly = true) { 
		Play__Once(Source, new Clip_Flags[] { Flag }, Randomly); 
	}
	public void Play__Once (AudioSource Source, Clip_Flags[] Flags, bool Randomly = true) { 
		if (!Source.isPlaying) {
			_AudioClip _Clip;
			if (!Find_Clip(out _Clip, Flags, Randomly))
				return;
			Source.clip = _Clip._Clip;
			Source.loop = false;
			Source.Play();
			StartCoroutine(Caption__Show(_Clip));
		}
	}
	public void Play__Once (string Name, bool Randomly = true) { 
		Play__Once(_Audio, Name, Randomly);
	}
	public void Play__Once (AudioSource Source, string Name, bool Randomly = true) { 
		if (!Source.isPlaying) {
			_AudioClip _Clip;
			if (!Find_Clip(out _Clip, Name, Randomly))
				return;
			Source.clip = _Clip._Clip;
			Source.loop = false;
			Source.Play();
			StartCoroutine(Caption__Show(_Clip));
		}
	}
	public void Play__Loop (Clip_Flags[] Flags, bool Randomly = true) {
		Play__Loop(_Audio, Flags, Randomly);
	}
	public void Play__Loop (AudioSource Source, Clip_Flags[] Flags, bool Randomly = true) {
		_AudioClip _Clip;
		if (!Find_Clip(out _Clip, Flags, Randomly))
			return;
		Source.clip = _Clip._Clip;
		Source.loop = true;
		Source.Play();
		StartCoroutine(Caption__Show(_Clip));
	}
	public void Trigger__Loop (_AudioClip Clip) {
		Trigger__Loop(_Audio, Clip);
	}
	public void Trigger__Loop (AudioSource Source, Clip_Flags Flag, bool Randomly = false) 
		{ Trigger__Loop (Source, new Clip_Flags[] { Flag }, Randomly); }
	public void Trigger__Loop (AudioSource Source, Clip_Flags[] Flags, bool Randomly = false) {
		_AudioClip _Clip;
		if (!Find_Clip(out _Clip, Flags, Randomly))
			return;
		Trigger__Loop (Source, _Clip);
	}
	public void Trigger__Loop (AudioSource Source, _AudioClip Clip) {
		if (Find_Trigger(Source) != null
				&& Find_Trigger(Source)._Clip == Clip)
			return;
		
		_AudioTrigger _Out = new _AudioTrigger(Clip, Source);
		Source.loop = true;
		Source.clip = Clip._Clip;
		Source.Play();
		_Triggers.Add(_Out);
	}
	public void Trigger__Clear (AudioSource Source) {
		if (Find_Trigger(Source) == null)
			return;
		
		Source.Stop();
		_Triggers.Remove(Find_Trigger(Source));
	}
	
	_AudioTrigger Find_Trigger (AudioSource Source) {
		return _Triggers.Find(delegate(_AudioTrigger obj) { return obj._Source == Source; });
	}
	
	public bool Find_Clip (out _AudioClip Clip, string Name, bool Randomly = true) {
		_AudioClip[] Collect = _Clips.FindAll(delegate(_AudioClip obj) { return obj.Name == Name; }).ToArray();
		
		if (Collect.Length == 0) {
			Clip = null;
			return false;
		} else if (!Randomly)
			Clip = Collect[0];
		else
			Clip = Collect[UnityEngine.Random.Range(0, Collect.Length)];
		return true;
	}
	public bool Find_Clip (out _AudioClip Clip, Clip_Flags Flag, bool Randomly = true) {
		return Find_Clip(out Clip, new Clip_Flags[] { Flag });
	}
	public bool Find_Clip (out _AudioClip Clip, Clip_Flags[] Flags, bool Randomly = true) {
		_AudioClip[] Collect = _Clips.FindAll(delegate(_AudioClip obj) {
					foreach (Clip_Flags eachFlag in Flags)
						if (!obj._Flags.Contains(eachFlag))
							return false;
					return true;
			}).ToArray();
		
		if (Collect.Length == 0) {
			Clip = null;
			return false;
		} else if (!Randomly)
			Clip = Collect[0];
		else
			Clip = Collect[UnityEngine.Random.Range(0, Collect.Length)];
		return true;
	}
}
