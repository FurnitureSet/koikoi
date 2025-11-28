using UnityEngine;

public class ScoreData {
    public int Total => (Plain + Poetry + Blue + Text + Ribbon +
                         Animal + InoShikaCho + Brights + Tsukimi +
                         Hanami + Month) * (Over7 ? 2 : 1) * (KoiKoi ? 2 : 1);

    // Plain Score
    public int Plain = 0;
    
    // Ribbon Scores
    public int Poetry = 0;
    public int Blue = 0;
    public int Text = 0;
    public int Ribbon = 0;
    
    // Animal Scores
    public int Animal = 0;
    public int InoShikaCho = 0;
    
    // Bright Scores
    public int Brights = 0;
    public int Tsukimi = 0;
    public int Hanami = 0;
    
    // Special Scores
    public bool Over7 = false;
    public bool KoiKoi = false;
    public int Month = 0;
}
