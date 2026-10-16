using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BigBattle
{
    public enum SoundCue { Slash,Thrust,Toss,Dodge,Ultimate,Hurt,Pop,Slip,Win,ImpactLight,ImpactHeavy,Step }

    public sealed class GameSound : MonoBehaviour
    {
        bool muted;
        public bool Muted {get=>muted;set{muted=value;if(muted)foreach(var voice in voices)if(voice!=null)voice.Stop();}}
        public int GeneratedClipCount=>clips.Count;
        public int ImpactPlays {get;private set;}
        public float MaximumSample {get;private set;}
        public bool SamplesFinite {get;private set;}=true;
        const int Rate=44100,Variants=4;
        static readonly float[] VictoryNotes={523.25f,659.25f,783.99f,1046.5f};
        readonly List<AudioClip> clips=new List<AudioClip>();
        readonly float[] lastPlayed=new float[12];
        readonly AudioSource[] voices=new AudioSource[10];
        int nextVoice,variation;
        public void Setup()
        {
            for(int i=0;i<voices.Length;i++){voices[i]=gameObject.AddComponent<AudioSource>();voices[i].spatialBlend=0;voices[i].volume=.42f;voices[i].playOnAwake=false;}
            for(int i=0;i<lastPlayed.Length;i++)lastPlayed[i]=-100;
            for(int cue=0;cue<12;cue++)for(int variant=0;variant<Variants;variant++)
            {
                float duration=cue==8?.72f:cue==4?.70f:cue==7?.30f:cue==10?.25f:cue==11?.075f:.20f;
                float[] samples=Synthesize((SoundCue)cue,variant,duration);
                foreach(float value in samples){MaximumSample=Mathf.Max(MaximumSample,Mathf.Abs(value));if(float.IsNaN(value)||float.IsInfinity(value))SamplesFinite=false;}
                var clip=AudioClip.Create("Original layered "+(SoundCue)cue+" "+variant,samples.Length,1,Rate,false);clip.SetData(samples,0);clips.Add(clip);
            }
        }
        static float[] Synthesize(SoundCue cue,int variant,float duration)
        {
            var random=new System.Random(7301+(int)cue*127+variant*313);var data=new float[Mathf.CeilToInt(Rate*duration)];
            float low=0,mid=0,phase=0,pitch=1+(variant-1.5f)*.025f;
            for(int i=0;i<data.Length;i++)
            {
                float t=i/(float)Rate,u=t/duration;
                float noise=(float)(random.NextDouble()*2-1);low+=.035f*(noise-low);mid+=.35f*(noise-mid);
                float band=mid-low;float attack=Mathf.Min(1,t/.003f);float fade=1-u;float value;
                if(cue==SoundCue.ImpactLight || cue==SoundCue.ImpactHeavy || cue==SoundCue.Hurt)
                {
                    bool heavy=cue!=SoundCue.ImpactLight;float f=(heavy?120:210)*Mathf.Exp(-t*8)*pitch+45;
                    phase+=f*Mathf.PI*2/Rate;
                    float thump=Mathf.Sin(phase)*Mathf.Exp(-t*(heavy?23:35))*.75f;
                    float snap=(noise-mid)*Mathf.Exp(-t*160)*.65f;
                    float body=(Mathf.Sin(t*720*pitch*Mathf.PI*2)*.15f+band*.65f)*Mathf.Exp(-t*38);
                    value=(thump+snap+body)*attack*fade;
                }
                else if(cue==SoundCue.Win)
                {
                    value=0;
                    for(int note=0;note<4;note++){float age=t-note*.115f;if(age>=0)value+=(Mathf.Sin(age*VictoryNotes[note]*6.28318f)+.18f*Mathf.Sin(age*VictoryNotes[note]*12.56636f))*Mathf.Exp(-age*9)*Mathf.Min(1,age/.005f)*.35f;}
                    value*=fade;
                }
                else if(cue==SoundCue.Pop || cue==SoundCue.Slip)
                {
                    phase+=(cue==SoundCue.Pop?550*Mathf.Exp(-t*12)+90:1000*Mathf.Exp(-t*11)+100)*pitch*6.28318f/Rate;
                    value=(Mathf.Sin(phase)*.52f+band*.35f)*attack*Mathf.Exp(-t*16)*fade;
                }
                else if(cue==SoundCue.Step)value=(low*.7f+band*.25f)*attack*Mathf.Exp(-t*65)*fade;
                else
                {
                    float sweep=Mathf.Sin(Mathf.PI*Mathf.Pow(u,.65f));
                    float bass=Mathf.Sin(t*(cue==SoundCue.Ultimate?95:180)*6.28318f)*.12f;
                    float air=band*(cue==SoundCue.Dodge?1.15f:1.7f)+(noise-mid)*.12f;
                    value=(air+bass)*sweep*fade;
                    if(cue==SoundCue.Ultimate)value*=.65f+.35f*Mathf.Sin(t*36);
                }
                data[i]=(float)Math.Tanh(value*1.7f)*.78f;
            }
            return data;
        }
        public void Play(SoundCue cue,float strength=1)
        {
            if(Muted || clips.Count==0)return;
            int index=(int)cue;float spacing=cue==SoundCue.Step?.13f:.025f;
            if(Time.unscaledTime-lastPlayed[index]<spacing)return;
            lastPlayed[index]=Time.unscaledTime;
            AudioSource voice=voices[nextVoice++%voices.Length];voice.Stop();voice.pitch=1+(variation%3-1)*.025f;
            voice.clip=clips[index*Variants+(variation++%Variants)];
            voice.volume=.42f*Mathf.Clamp(strength,.1f,1.1f)*(cue==SoundCue.Step?.38f:cue==SoundCue.Ultimate?.75f:1);voice.Play();
            if(cue==SoundCue.ImpactLight || cue==SoundCue.ImpactHeavy)ImpactPlays++;
        }
        public void Impact(bool heavy){Play(heavy?SoundCue.ImpactHeavy:SoundCue.ImpactLight,heavy?1:.82f);}
        public void ExportDemo(string path)
        {
            var samples=new float[Rate*4];
            SoundCue[] sequence={SoundCue.Slash,SoundCue.ImpactLight,SoundCue.Slash,SoundCue.ImpactLight,SoundCue.Thrust,SoundCue.ImpactHeavy,SoundCue.Dodge,SoundCue.Slip,SoundCue.Ultimate,SoundCue.ImpactHeavy};
            float[] starts={.10f,.19f,.65f,.74f,1.20f,1.29f,1.75f,2.10f,2.65f,2.84f};
            for(int n=0;n<sequence.Length;n++)
            {
                var clip=clips[(int)sequence[n]*Variants];var data=new float[clip.samples];clip.GetData(data,0);int begin=(int)(starts[n]*Rate);
                for(int i=0;i<data.Length && i+begin<samples.Length;i++)samples[i+begin]+=data[i]*.6f;
            }
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using(var writer=new BinaryWriter(File.Create(path)))
            {
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+samples.Length*2);writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
                writer.Write(16);writer.Write((short)1);writer.Write((short)1);writer.Write(Rate);writer.Write(Rate*2);writer.Write((short)2);writer.Write((short)16);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(samples.Length*2);
                foreach(float value in samples)writer.Write((short)(Mathf.Clamp(value,-.98f,.98f)*32767));
            }
        }
        void OnDestroy(){foreach(var clip in clips)if(clip!=null)Destroy(clip);}
    }
}
