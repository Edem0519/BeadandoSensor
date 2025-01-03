﻿using System;

namespace Bead_2024 
{
    public class SzenzorLibrary : EventArgs //-TZS
    {
        public int azon { get; set; }
        public int para { get; set; }
        public int hom { get; set; }
        public int folyoszint { get; set; }
        public int tartalyszint { get; set; }
    }
    public class Szenzorok 
    {
        public int azon { get; set; } //tulajdonság függvények-TZS
        public int para { get; set; }
        public int hom { get; set; }
        public int folyoszint { get; set; }
        public int tartalyszint { get; set; }

        public event EventHandler<SzenzorLibrary> Esemeny;

        public Szenzorok(int azon, int para, int hom, int folyoszint, int tartalyszint) //konstruktor -TZS 
        {
            this.azon = azon;
            this.para = para;
            this.hom = hom;
            this.folyoszint = folyoszint;
            this.tartalyszint = tartalyszint;
        }

        public void Adat() //-TZS
        {
            Esemeny?.Invoke(this, new SzenzorLibrary
            {
                azon = azon,
                para = para,
                hom = hom,
                folyoszint = folyoszint,
                tartalyszint = tartalyszint
            });
        }
    }
}

