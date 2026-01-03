using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Core.Extensions;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game.Core.Services
{
    public class AudioService : GameService
    {
        // BGM/SE/Voiceの再生管理などをやる場所

        // BGMはクロスフェード
        // SE/Voiceは上書き
        private GameObject _audioService;
        private AudioSource _bgmSource;
        private AudioSource _voiceSource;
        private AudioSource _sfxSource;

        protected internal override void Startup()
        {
            _audioService = new GameObject(nameof(AudioService));
            _bgmSource = new GameObject("BgmSource").AddComponent<AudioSource>();
            _voiceSource = new GameObject("VoiceSource").AddComponent<AudioSource>();
            _sfxSource = new GameObject("SfxSource").AddComponent<AudioSource>();

            _bgmSource.transform.SetParent(_audioService.transform);
            _voiceSource.transform.SetParent(_audioService.transform);
            _sfxSource.transform.SetParent(_audioService.transform);

            UnityEngine.Object.DontDestroyOnLoad(_audioService);

            base.Startup();
        }

        protected internal override void Shutdown()
        {
            _bgmSource.SafeDestroy();
            _bgmSource = null;
            _voiceSource.SafeDestroy();
            _voiceSource = null;
            _sfxSource.SafeDestroy();
            _sfxSource = null;
            _audioService.SafeDestroy();
            _audioService = null;

            base.Shutdown();
        }

        protected internal override bool AllowResidentOnMemory => true;

        public async Task PlayBgmAsync(string cueName)
        {
            var audioClip = await Addressables.LoadAssetAsync<AudioClip>(cueName);
            _bgmSource.DOFade(0f, 1f).onComplete += () =>
            {
                _bgmSource.Stop();
                _bgmSource.clip = audioClip;
                _bgmSource.volume = 0f;
                _bgmSource.mute = false;
                _bgmSource.loop = true;
                _bgmSource.Play();
                _bgmSource.DOFade(1f, 1f);
            };
        }

        public async Task PlayVoiceAsync(string cueName)
        {
            var audioClip = await Addressables.LoadAssetAsync<AudioClip>(cueName);
            _voiceSource.volume = 1f;
            _voiceSource.mute = false;
            _voiceSource.loop = false;
            _voiceSource.PlayOneShot(audioClip);
        }

        public async Task PlaySfxAsync(string cueName)
        {
            var audioClip = await Addressables.LoadAssetAsync<AudioClip>(cueName);
            _sfxSource.volume = 1f;
            _sfxSource.mute = false;
            _sfxSource.loop = false;
            _sfxSource.PlayOneShot(audioClip);
        }
    }
}