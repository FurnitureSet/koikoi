using UnityEngine;
using TMPro;

public class WinScreen : MonoBehaviour {
    [SerializeField] private GameObject scoreUI;
    [SerializeField] private TextMeshProUGUI playerNameTextbox;
    [SerializeField] private TextMeshProUGUI scoreTextbox;
    [SerializeField] private TextMeshProUGUI scoreDataTextbox;
    
    public void DisplayScoreMessage(Player player, ScoreData score) {
        scoreUI.SetActive(true);
        playerNameTextbox.text = $"{player.Name} has won!";
        scoreTextbox.text = $"Score: {score.Total}";
        scoreDataTextbox.text = $"Chaff: {score.Plain}\tRibbon: {score.Ribbon}\tAnimal: {score.Animal}\tIno-Shika-Cho: {score.InoShikaCho}\tPoetry: {score.Poetry}\n" +
                              $"Blue: {score.Blue}\tBrights: {score.Brights}\tTsukimi: {score.Tsukimi}\tHanami: {score.Hanami}\n" +
                              $"Over 7: {score.Over7}\tKoiKoi: {score.KoiKoi}";
    }

    public void HideScoreMessage() {
        scoreUI.SetActive(false);
    }
}
