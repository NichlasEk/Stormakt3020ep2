using System;
using System.Collections.Generic;
using System.Numerics;

namespace Atland;

public static class Navigation
{
    public static bool Contains(Vector2[] polygon,Vector2 p)
    {
        bool inside=false;
        for(int i=0,j=polygon.Length-1;i<polygon.Length;j=i++)
        {var a=polygon[i];var b=polygon[j];if((a.Y>p.Y)!=(b.Y>p.Y)&&p.X<(b.X-a.X)*(p.Y-a.Y)/(b.Y-a.Y)+a.X)inside=!inside;}
        return inside;
    }
    private static Vector2 Edge(Vector2[] poly,Vector2 p)
    {
        float best=float.MaxValue;var point=poly[0];
        for(int i=0;i<poly.Length;i++)
        {var a=poly[i];var d=poly[(i+1)%poly.Length]-a;var q=a+d*Math.Clamp(Vector2.Dot(p-a,d)/d.LengthSquared(),0,1);float distance=Vector2.DistanceSquared(q,p);if(distance<best){best=distance;point=q;}}
        return point;
    }
    public static Vector2[] Expand(Vector2[] polygon,float amount)
    {
        var result=new Vector2[polygon.Length];var center=Vector2.Zero;
        foreach(var p in polygon)center+=p;center/=polygon.Length;
        for(int i=0;i<polygon.Length;i++)result[i]=polygon[i]+Vector2.Normalize(polygon[i]-center)*amount;
        return result;
    }
    public static Vector2 Clamp(Vector2[] ground,Vector2[] obstacle,Vector2 p)
    {
        if(!Contains(ground,p))p=Vector2.Lerp(Edge(ground,p),new Vector2(768,620),.003f);
        if(obstacle.Length>0&&Contains(obstacle,p))
        {var center=Vector2.Zero;foreach(var vertex in obstacle)center+=vertex;center/=obstacle.Length;
            var q=Edge(obstacle,p);p=q+Combat.Normal(q-center,Vector2.UnitX)*.6f;}
        return p;
    }
    public static bool Clear(Vector2[] ground,Vector2[] obstacle,Vector2 a,Vector2 b)
    {
        int samples=Math.Max(2,(int)MathF.Ceiling(Vector2.Distance(a,b)/8));
        for(int i=0;i<=samples;i++){var p=Vector2.Lerp(a,b,i/(float)samples);if(!Contains(ground,p)||(obstacle.Length>0&&Contains(obstacle,p)))return false;}
        return true;
    }
    public static Vector2 Next(Vector2[] ground,Vector2[] obstacle,Vector2 from,Vector2 target)
    {
        target=Clamp(ground,obstacle,target);
        if(obstacle.Length==0||Clear(ground,obstacle,from,target))return target;
        // Small deterministic visibility graph around the measured crate island.
        var vertices=new List<Vector2>{from,target};vertices.AddRange(Expand(obstacle,12));
        int count=vertices.Count;var distance=new float[count];Array.Fill(distance,float.MaxValue);
        var previous=new int[count];Array.Fill(previous,-1);var visited=new bool[count];distance[0]=0;
        for(int pass=0;pass<count;pass++)
        {
            int best=-1;for(int i=0;i<count;i++)if(!visited[i]&&(best<0||distance[i]<distance[best]))best=i;
            if(best<0||distance[best]==float.MaxValue)break;visited[best]=true;if(best==1)break;
            for(int j=0;j<count;j++)if(!visited[j]&&Clear(ground,obstacle,vertices[best],vertices[j]))
            {float d=distance[best]+Vector2.Distance(vertices[best],vertices[j]);if(d<distance[j]){distance[j]=d;previous[j]=best;}}
        }
        int next=1;if(previous[next]<0)return from;
        while(previous[next]>0)next=previous[next];return vertices[next];
    }
}
