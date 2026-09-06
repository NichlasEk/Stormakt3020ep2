using System;

/// <summary>Reviewed source regions; neighboring weapon tips must belong to one frame only.</summary>
public static class AnimationAtlasLayout
{
    public static (int Left,int Top,int Right,int Bottom) Cell(string role,string action,int row,int col,int width,int height)
    {
        int left=col*width/4,right=(col+1)*width/4,top=row*height/4,bottom=(row+1)*height/4;
        int X(int x)=>(int)MathF.Round(x*width/1254f);
        if(action=="attack"&&role=="guard")
        {
            if(col==2){left=X(new[]{650,640,600,540}[row]);right=X(new[]{1020,1000,930,920}[row]);}
            if(col==3)left=X(1022);
            if(col==1&&row>=2)right=X(row==2?590:535);
            if(col==1&&row==2)bottom=(int)MathF.Round(920*height/1254f);
            if(col==1&&row==3)top=(int)MathF.Round(920*height/1254f);
        }
        if(action=="attack"&&role=="karl")
        {
            if(col==2){left=X(new[]{635,635,590,575}[row]);right=X(new[]{977,987,931,931}[row]);}
            if(col==3)left=X(row<=1?990:row==2?975:950);
            if(col==1&&row>=2)right=X(row==2?590:575);
        }
        if(role=="pikeman"&&action=="walk")
        {
            int[] rows={0,327,633,929,1254};top=(int)MathF.Round(rows[row]*height/1254f);bottom=(int)MathF.Round(rows[row+1]*height/1254f);
        }
        return(left,top,right,bottom);
    }
}
