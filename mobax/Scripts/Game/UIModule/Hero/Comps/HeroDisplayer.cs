using System;
using System.Collections.Generic;
using System.Linq;


public static class HeroDisplayer
{
    public static HeroFilter Filter { get; }

    public static List<HeroInfo> List => Filter.DisplayList;
    
    public static HeroInfo Hero => Filter.DisplayHero;

    public static HeroInfo Default => Filter.DefaultHero;

    static HeroDisplayer()
    {
        Filter = new HeroFilter();
    }
}