using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Core.Enums;
using Game.Core.Extensions;
using Game.Core.MasterData;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game.Core.Services
{
    public class AudioService : GameService
    {
        private GameServiceReference<MasterDataService> _masterDataService;
        private MemoryDatabase MemoryDatabase => _masterDataService.Reference.MemoryDatabase;

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

        public async Task PlayBgmAsync(string assetName)
        {
            var audioClip = await Addressables.LoadAssetAsync<AudioClip>(assetName);
            if (_bgmSource.isPlaying)
                _bgmSource.DOFade(0f, 0.5f).onComplete += () => { PlayBgmCore(); };
            else
                PlayBgmCore();

            return;

            // TODO: マスターボリュームなどを設定できるオプション画面
            void PlayBgmCore()
            {
                _bgmSource.Stop();
                _bgmSource.clip = audioClip;
                _bgmSource.volume = 0f;
                _bgmSource.mute = false;
                _bgmSource.loop = true;
                _bgmSource.Play();
                _bgmSource.DOFade(0.5f, 0.25f); // 一旦うるさいので50%
            }
        }

        public void StopBgm()
        {
            if (_bgmSource.isPlaying)
            {
                _bgmSource.DOFade(0f, 0.25f).onComplete += () => { _bgmSource.Stop(); };
            }
        }

        public async Task PlayVoiceAsync(string assetName, CancellationToken token = default)
        {
            var audioClip = await Addressables.LoadAssetAsync<AudioClip>(assetName);

            // Memo: 今のところ、前回のボイスはフェードアウトして再生するとしているが
            // →再生リクエスト間隔が短いとうるさくなるため、一括で再生間隔を設定する方法を検討
            if (_voiceSource.isPlaying)
                _voiceSource.DOFade(0f, 0.1f).onComplete += () => { PlayVoiceCore(); };
            else
                PlayVoiceCore();

            await Task.Delay(TimeSpan.FromSeconds(audioClip.length), token);
            return;

            // Memo: ループなどの制御を検討
            void PlayVoiceCore()
            {
                _voiceSource.Stop();
                _voiceSource.volume = 1f;
                _voiceSource.mute = false;
                _voiceSource.loop = false;
                _voiceSource.PlayOneShot(audioClip);
            }
        }

        public async Task PlaySoundEffectAsync(string assetName, CancellationToken token = default)
        {
            var audioClip = await Addressables.LoadAssetAsync<AudioClip>(assetName);

            if (_sfxSource.isPlaying)
                _sfxSource.DOFade(0f, 0.1f).onComplete += () => { PlaySoundEffectCore(); };
            else
                PlaySoundEffectCore();

            await Task.Delay(TimeSpan.FromSeconds(audioClip.length), token);
            return;

            // Memo: ループなどの制御を検討
            void PlaySoundEffectCore()
            {
                _sfxSource.Stop();
                _sfxSource.volume = 1f;
                _sfxSource.mute = false;
                _sfxSource.loop = false;
                _sfxSource.PlayOneShot(audioClip);
            }
        }

        public async Task PlayRandomAsync(AudioCategory audioCategory, AudioPlayTag audioPlayTag, CancellationToken token = default)
        {
            var cueNames = MemoryDatabase.AudioPlayTagsMasterTable.FindByAudioPlayTag((int)audioPlayTag)
                .Select(x =>
                {
                    if (!MemoryDatabase.AudioMasterTable.TryFindById(x.AudioId, out var audioMaster))
                        return null;

                    if (audioMaster.AudioCategory != (int)audioCategory)
                        return null;

                    return audioMaster.AssetName;
                })
                .Where(x => x != null)
                .ToArray();
            if (cueNames.Length <= 0)
                return;

            var cueIndex = UnityEngine.Random.Range(0, cueNames.Length);
            var cueName = cueNames[cueIndex];

            switch (audioCategory)
            {
                case AudioCategory.Bgm:
                    await PlayBgmAsync(cueName);
                    break;
                case AudioCategory.Voice:
                {
                    await PlayVoiceAsync(cueName, token);
                    break;
                }
                case AudioCategory.SoundEffect:
                    await PlaySoundEffectAsync(cueName, token);
                    break;
            }
        }
    }
}