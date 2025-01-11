using Raylib_CsLo;
using System.Collections.Generic;

namespace Tetris
{

    public class SoundManager
    {
        public static Sound backgroundMusicGame;
        public static Sound blockplace;
        public static Sound lineclear;
        public static Sound levelup;
        public static Sound lose;

        public static List<Sound> music = new List<Sound>();
        public static List<Sound> sfx = new List<Sound>();

        public static void LoadSound()
        {
            blockplace = Raylib.LoadSound("audio/blockplace.wav");
            lineclear = Raylib.LoadSound("audio/lineclear.wav");
            levelup = Raylib.LoadSound("audio/levelup.wav");
            lose = Raylib.LoadSound("audio/lose.wav");

            backgroundMusicGame = Raylib.LoadSound("audio/Theme.mp3");

            music.Add(backgroundMusicGame);
            sfx.Add(blockplace);
            sfx.Add(lineclear);
            sfx.Add(levelup);
            sfx.Add(lose);
        }

        public static void LoopAudio(Sound sound)
        {
            if (!Raylib.IsSoundPlaying(sound))
            {
                Raylib.PlaySound(sound);
            }
        }

        public static void ChangeVolumeMusic(float musicVolume)
        {
            foreach (Sound sound in music)
            {
                Raylib.SetSoundVolume(sound, musicVolume);
            }
        }       

        public static void ChangeSFXVolume(float sfxVolume)
        {
            foreach (Sound sound in sfx)
            {
                Raylib.SetSoundVolume(sound, sfxVolume);
            }
        }

    }
}