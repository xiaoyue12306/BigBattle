using System;
using System.Runtime.InteropServices;
using BigBattle;
using UnityEngine;

// Engine-independent portions can be checked against the installed assemblies
// without loading an Editor. These checks never replace a real player run.
public static class SourceChecks
{
    static int passed;
    static void Check(bool value,string message)
    {
        if(!value)throw new Exception("FAIL "+message);
        Console.WriteLine("PASS "+message);passed++;
    }
    public static int Main()
    {
        for(int count=1;count<=5;count++)
        {
            Rect[] rects=SplitScreenLayout.ForPlayers(count);
            Check(rects.Length==count,"Viewport count "+count);
            float area=0;
            for(int i=0;i<count;i++)
            {
                var rect=rects[i];area+=rect.width*rect.height;
                Check(rect.xMin>=0 && rect.yMin>=0 && rect.xMax<=1.00001f && rect.yMax<=1.00001f,"Viewport bounds "+count+":"+i);
                for(int j=i+1;j<count;j++)Check(!rect.Overlaps(rects[j]),"Viewport non-overlap "+count+":"+i+":"+j);
            }
            Check(Math.Abs(area-1)<.00001f,"Full viewport coverage "+count);
        }
        bool invalid=false;try{SplitScreenLayout.ForPlayers(0);}catch(ArgumentOutOfRangeException){invalid=true;}Check(invalid,"Reject zero players");
        invalid=false;try{SplitScreenLayout.ForPlayers(6);}catch(ArgumentOutOfRangeException){invalid=true;}Check(invalid,"Reject six players");
        Check(LocalInput.Deadzone(new Vector2(.1f,.1f)).sqrMagnitude==0,"Controller drift is ignored");
        Check(LocalInput.Deadzone(Vector2.zero).sqrMagnitude==0,"Neutral stick stays neutral");
        Check(Math.Abs(LocalInput.Deadzone(new Vector2(1,0)).magnitude-1)<.0001f,"Full stick speed preserved");
        Check(LocalInput.Deadzone(new Vector2(1,1)).magnitude<=1.0001f,"Diagonal stick speed capped");
        Check(LocalInput.Deadzone(new Vector2(.21f,0)).magnitude<.02f,"Smooth deadzone edge");
        Check(Marshal.SizeOf(typeof(XInput.Pad))==12 && Marshal.SizeOf(typeof(XInput.State))==16,"Native XInput ABI structure sizes");
        for(int slot=0;slot<4;slot++)
        {
            XInput.State state;Console.WriteLine("INFO XInput slot "+slot+" connected="+XInput.TryRead(slot,out state));
        }
        Console.WriteLine("SOURCE_LOGIC_PASS "+passed+" checks; hardware gameplay and rendering not tested");
        return 0;
    }
}
