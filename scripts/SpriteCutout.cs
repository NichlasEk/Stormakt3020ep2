using Godot;
using System;

/// Removes the connected pale backdrop BEFORE texture filtering, and unmixes
/// backdrop colour from silhouette pixels. Enclosed steel highlights stay opaque.
public static class SpriteCutout
{
    public static ImageTexture Load(string path, string? diagnosticPath=null, Color? chromaKey=null)
    {
        using var source=GD.Load<Texture2D>(path);
        using var image=source.GetImage();
        if(image.IsCompressed())image.Decompress();
        image.Convert(Image.Format.Rgba8);
        int width=image.GetWidth(),height=image.GetHeight(),count=width*height;
        byte[] pixels=image.GetData(),result=(byte[])pixels.Clone();
        var outside=new bool[count];var queue=new int[count];int head=0,tail=0;
        var backdrop=chromaKey??new Color(.96f,.96f,.96f);
        float[] background={backdrop.R,backdrop.G,backdrop.B};
        bool Pale(int i)
        {
            int p=i*4,r=pixels[p],g=pixels[p+1],b=pixels[p+2];
            if(chromaKey!=null)return r>150&&b>150&&g<120&&Math.Min(r,b)-g>80;
            return Math.Min(r,Math.Min(g,b))>194 && Math.Max(r,Math.Max(g,b))-Math.Min(r,Math.Min(g,b))<35;
        }
        void Visit(int i)
        {
            if(outside[i]||!Pale(i))return;
            outside[i]=true;queue[tail++]=i;
        }
        for(int x=0;x<width;x++){Visit(x);Visit((height-1)*width+x);}
        for(int y=0;y<height;y++){Visit(y*width);Visit(y*width+width-1);}
        // Chroma colour is forbidden in the costume, so enclosed limb gaps are
        // background too. The legacy white path keeps enclosed metal highlights.
        if(chromaKey!=null)for(int i=0;i<count;i++)Visit(i);
        while(head<tail)
        {
            int i=queue[head++],x=i%width,y=i/width;
            if(x>0)Visit(i-1);if(x+1<width)Visit(i+1);
            if(y>0)Visit(i-width);if(y+1<height)Visit(i+width);
        }
        var interior=new bool[count];
        for(int y=1;y<height-1;y++)for(int x=1;x<width-1;x++)
        {
            int i=y*width+x;
            interior[i]=!outside[i]&&!outside[i-1]&&!outside[i+1]&&!outside[i-width]&&!outside[i+width];
        }
        for(int y=0;y<height;y++)for(int x=0;x<width;x++)
        {
            int i=y*width+x,p=i*4;
            if(interior[i])continue;
            result[p+3]=outside[i]?(byte)0:(byte)255;
            // Find a nearby uncontaminated foreground colour. Also bleed this
            // into transparent texels so linear filtering cannot sample white.
            int best=-1,bestDistance=100;
            for(int dy=-3;dy<=3;dy++)for(int dx=-3;dx<=3;dx++)
            {
                int nx=x+dx,ny=y+dy,d=dx*dx+dy*dy;
                if(nx<0||nx>=width||ny<0||ny>=height||d>=bestDistance)continue;
                int n=ny*width+nx;
                if(interior[n]){best=n;bestDistance=d;}
            }
            if(best<0){if(outside[i])result[p]=result[p+1]=result[p+2]=0;continue;}
            int q=best*4;
            if(outside[i]){for(int c=0;c<3;c++)result[p+c]=pixels[q+c];continue;}
            // Least-squares coverage for observed = alpha*foreground + (1-alpha)*backdrop.
            float numerator=0,denominator=0;
            for(int c=0;c<3;c++)
            {
                float f=pixels[q+c]/255f,b=background[c],o=pixels[p+c]/255f;
                numerator+=(o-b)*(f-b);denominator+=(f-b)*(f-b);
            }
            float alpha=denominator>.001f?Math.Clamp(numerator/denominator,0,1):1;
            result[p+3]=(byte)MathF.Round(alpha*255);
            // Use the trusted foreground colour rather than dividing noisy
            // edge RGB by low coverage, which amplifies green/magenta fringes.
            for(int c=0;c<3;c++)result[p+c]=pixels[q+c];
        }
        if(chromaKey!=null)for(int i=0;i<count;i++)
        {
            int p=i*4,spill=Math.Min(result[p],result[p+2])-result[p+1];
            if(spill>10){result[p]=(byte)(result[p]-spill);result[p+2]=(byte)(result[p+2]-spill);}
        }
        using var cutout=Image.CreateFromData(width,height,false,Image.Format.Rgba8,result);
        if(diagnosticPath!=null)
        {
            var err=cutout.SavePng(diagnosticPath);
            if(err!=Error.Ok)throw new InvalidOperationException($"Cannot save cutout: {err}");
        }
        cutout.GenerateMipmaps();
        return ImageTexture.CreateFromImage(cutout);
    }
}
