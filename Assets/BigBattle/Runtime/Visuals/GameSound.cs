using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BigBattle
{
    public enum SoundCue { Slash,Thrust,Toss,Dodge,Ultimate,Hurt,Pop,Slip,Win,ImpactLight,ImpactHeavy,Step,SlashBack,SlashFinish,Clash }

    public sealed class GameSound : MonoBehaviour
    {
        bool muted;
        public bool Muted {get=>muted;set{muted=value;if(muted)foreach(var voice in voices)if(voice!=null)voice.Stop();}}
        public int GeneratedClipCount=>clips.Count;
        public int ImpactPlays {get;private set;}
        public int ClashPlays {get;private set;}
        public float MaximumSample {get;private set;}
        public bool SamplesFinite {get;private set;}=true;
        const int Rate=44100,Variants=4;
        static readonly float[] VictoryNotes={523.25f,659.25f,783.99f,1046.5f};
        static readonly float[] SteelModes={1680,2635,3910,5480,7240};
        readonly List<AudioClip> clips=new List<AudioClip>();
        readonly float[] lastPlayed=new float[Enum.GetValues(typeof(SoundCue)).Length];
        // Steel tails have two reserved voices so the next combo cannot cut them off.
        readonly AudioSource[] voices=new AudioSource[12];
        int nextVoice,nextSteel,variation;
        public void Setup()
        {
            for(int i=0;i<voices.Length;i++){voices[i]=gameObject.AddComponent<AudioSource>();voices[i].spatialBlend=0;voices[i].volume=.44f;voices[i].playOnAwake=false;}
            for(int i=0;i<lastPlayed.Length;i++)lastPlayed[i]=-100;
            for(int cue=0;cue<lastPlayed.Length;cue++)for(int variant=0;variant<Variants;variant++)
            {
                SoundCue id=(SoundCue)cue;
                float duration=id==SoundCue.Win?.72f:id==SoundCue.Ultimate?.70f:id==SoundCue.Clash?.48f:id==SoundCue.SlashFinish?.27f:id==SoundCue.Slip?.30f:id==SoundCue.ImpactHeavy?.25f:id==SoundCue.Step?.075f:.20f;
                float[] samples=Synthesize(id,variant,duration);
                foreach(float value in samples){MaximumSample=Mathf.Max(MaximumSample,Mathf.Abs(value));if(float.IsNaN(value)||float.IsInfinity(value))SamplesFinite=false;}
                var clip=AudioClip.Create("Original clean "+id+" "+variant,samples.Length,1,Rate,false);clip.SetData(samples,0);clips.Add(clip);
            }
        }
        static float[] Synthesize(SoundCue cue,int variant,float duration)
        {
            var random=new System.Random(7301+(int)cue*127+variant*313);var data=new float[Mathf.CeilToInt(Rate*duration)];
            float low=0,mid=0,smooth=0,air=0,phase=0,pitch=1+(variant-1.5f)*.018f;
            for(int i=0;i<data.Length;i++)
            {
                float t=i/(float)Rate,u=t/duration;
                float noise=(float)(random.NextDouble()*2-1);low+=.025f*(noise-low);mid+=.22f*(noise-mid);
                float band=mid-low;float attack=Mathf.Min(1,t/.003f);float fade=1-u;float value;
                if(cue==SoundCue.Clash)
                {
                    // Inharmonic, exponentially damped steel resonances: bright
                    // attack, clear "ting" tail, very little broadband noise.
                    value=0;
                    for(int mode=0;mode<SteelModes.Length;mode++)
                        value+=Mathf.Sin(t*SteelModes[mode]*pitch*6.283185f)*Mathf.Exp(-t*(8+mode*6))*(.36f/(1+mode*.65f));
                    value+=Mathf.Sin(t*630*pitch*6.283185f)*Mathf.Exp(-t*54)*.17f;
                    value+=(noise-mid)*Mathf.Exp(-t*550)*.11f;
                    value*=Mathf.Min(1,t/.0008f)*Mathf.Min(1,fade/.16f);
                }
                else if(cue==SoundCue.ImpactLight || cue==SoundCue.ImpactHeavy || cue==SoundCue.Hurt)
                {
                    bool heavy=cue!=SoundCue.ImpactLight;float f=(heavy?170:260)*Mathf.Exp(-t*18)*pitch+58;
                    phase+=f*6.283185f/Rate;
                    float body=Mathf.Sin(phase)*Mathf.Exp(-t*(heavy?25:38))*.80f;
                    float click=Mathf.Sin(t*2100*pitch*6.283185f)*Mathf.Exp(-t*210)*.27f;
                    float punch=band*Mathf.Exp(-t*85)*.45f;
                    value=(body+click+punch)*Mathf.Min(1,t/.0015f)*fade*fade;
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
                    value=(Mathf.Sin(phase)*.52f+band*.15f)*attack*Mathf.Exp(-t*16)*fade;
                }
                else if(cue==SoundCue.Step)value=(low*.7f+band*.12f)*attack*Mathf.Exp(-t*65)*fade;
                else
                {
                    bool finish=cue==SoundCue.SlashFinish || cue==SoundCue.Ultimate;
                    float envelope=Mathf.Pow(Mathf.Sin(Mathf.PI*u),1.8f);
                    // Two cascaded low-pass poles remove the coarse white-noise
                    // rasp. A moving passband gives air without distortion.
                    float cutoff=650+2100*Mathf.Sin(Mathf.PI*u);
                    float coefficient=1-Mathf.Exp(-6.283185f*cutoff/Rate);
                    smooth+=coefficient*(noise-smooth);air+=coefficient*(smooth-air);
                    float breath=(air-low)*1.85f;
                    float frequency=Mathf.Lerp(cue==SoundCue.SlashBack?860:1050,finish?190:330,u)*pitch;
                    phase+=frequency*6.283185f/Rate;
                    float blade=Mathf.Sin(phase)*.055f+Mathf.Sin(phase*1.413f)*.025f;
                    float weight=Mathf.Sin(t*(finish?112:165)*pitch*6.283185f)*.11f;
                    value=(breath+blade+weight)*envelope;
                    if(cue==SoundCue.Ultimate)value*=.70f+.30f*Mathf.Sin(t*27);
                }
                data[i]=value;
            }
            // Linear peak normalization, no saturator harmonics or hard clipping.
            float peak=0;foreach(float sample in data)peak=Mathf.Max(peak,Mathf.Abs(sample));
            float target=cue==SoundCue.Step?.38f:cue==SoundCue.Clash?.76f:.72f;
            if(peak>.0001f)for(int i=0;i<data.Length;i++)data[i]*=target/peak;
            return data;
        }
        public void Play(SoundCue cue,float strength=1)
        {
            if(Muted || clips.Count==0)return;
            int index=(int)cue;float spacing=cue==SoundCue.Step?.13f:cue==SoundCue.Clash?.06f:.025f;
            if(Time.unscaledTime-lastPlayed[index]<spacing)return;
            lastPlayed[index]=Time.unscaledTime;
            AudioSource voice=voices[cue==SoundCue.Clash?10+nextSteel++%2:nextVoice++%10];voice.Stop();voice.pitch=1+(variation%3-1)*.018f;
            voice.clip=clips[index*Variants+(variation++%Variants)];
            voice.volume=.44f*Mathf.Clamp(strength,.1f,1.1f)*(cue==SoundCue.Step?.24f:cue==SoundCue.Ultimate?.72f:cue==SoundCue.Clash?1.16f:1);voice.Play();
            if(cue==SoundCue.ImpactLight || cue==SoundCue.ImpactHeavy)ImpactPlays++;
            if(cue==SoundCue.Clash)ClashPlays++;
        }
        public void Impact(bool heavy){Play(heavy?SoundCue.ImpactHeavy:SoundCue.ImpactLight,heavy?1.08f:.88f);}
        public void ExportDemo(string path)
        {
            var samples=new float[Rate*7];
            SoundCue[] sequence={SoundCue.Slash,SoundCue.ImpactLight,SoundCue.SlashBack,SoundCue.ImpactLight,SoundCue.SlashFinish,SoundCue.ImpactHeavy,SoundCue.Slash,SoundCue.Clash,SoundCue.SlashBack,SoundCue.Clash,SoundCue.Thrust,SoundCue.ImpactHeavy,SoundCue.Ultimate,SoundCue.ImpactHeavy};
            float[] starts={.10f,.18f,.43f,.52f,.80f,.92f,1.90f,1.99f,2.9f,2.99f,4.05f,4.14f,5.20f,5.43f};
            for(int n=0;n<sequence.Length;n++)
            {
                var clip=clips[(int)sequence[n]*Variants+n%Variants];var data=new float[clip.samples];clip.GetData(data,0);int begin=(int)(starts[n]*Rate);
                for(int i=0;i<data.Length && i+begin<samples.Length;i++)samples[i+begin]+=data[i]*.62f;
            }
            WriteWave(path,samples);
        }
        static void WriteWave(string path,float[] samples)
        {
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
