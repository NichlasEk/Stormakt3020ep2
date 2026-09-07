using Godot;
using System;
using System.Text.Json;

namespace Atland;

public sealed record FilmCaption(float Start,float End,string Speaker,string Text);
public sealed record FilmDefinition(string Id,string Title,string File,float Duration,string Description,FilmCaption[] Captions)
{
    public FilmCaption? CaptionAt(double seconds)=>Array.Find(Captions,c=>seconds>=c.Start&&seconds<c.End);
}
public static class FilmLibrary
{
    public static FilmDefinition[] Load()
    {
        var films=JsonSerializer.Deserialize<FilmDefinition[]>(FileAccess.GetFileAsString("res://assets/story/films.json"),new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!;
        foreach(var film in films)
        {
            if(string.IsNullOrWhiteSpace(film.Id)||film.Duration<=0||film.Captions is null)throw new InvalidOperationException("Ogiltig filmdefinition");
            float end=0;
            foreach(var caption in film.Captions)
            {if(caption.Start<end||caption.End<=caption.Start||caption.End>film.Duration)throw new InvalidOperationException("Ogiltig textning");end=caption.End;}
        }
        return films;
    }
}
