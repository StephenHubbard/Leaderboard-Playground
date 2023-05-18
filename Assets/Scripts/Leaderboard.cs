using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LootLocker.Requests;
using TMPro;

public class Leaderboard : MonoBehaviour
{
    [SerializeField] private TMP_InputField _playerNameInputField;
    [SerializeField] private TextMeshProUGUI _playerNames;
    [SerializeField] private TextMeshProUGUI _playerScores;
    [SerializeField] private TextMeshProUGUI _scoreText;

    private int _scoreToSubmit = 0;
    private string _leaderBoardID = "globalHighscore";

    private void Start()
    {
        StartCoroutine(SetupRoutine());
    }

    private void Update() {
        _scoreToSubmit += 1;
        _scoreText.text = _scoreToSubmit.ToString();
    }

    public void SubmitButton() {
        StartCoroutine(SubmitButtonRoutine());
    }

    private IEnumerator SubmitButtonRoutine() {
        yield return SetPlayerName();
        yield return SubmitScoreRoutine(_scoreToSubmit);
        yield return FetchTopHighScoresRoutine();
    } 

    private IEnumerator SetupRoutine()
    {
        yield return LoginRoutine();
        yield return FetchTopHighScoresRoutine();
    }

    private IEnumerator SetPlayerName()
    {
        bool done = false;

        LootLockerSDKManager.SetPlayerName(_playerNameInputField.text, (response) =>
        {
            if (response.success)
            {
                Debug.Log("Successfully set player name");
                done = true;
            }
            else
            {
                Debug.Log("Could not set player name" + response.Error);
                done = true;
            }
        });

        yield return new WaitWhile(() => done == false);
    }

    private IEnumerator SubmitScoreRoutine(int scoreToUpload) {
        bool done = false;
        string playerID = _playerNameInputField.text;

        LootLockerSDKManager.SubmitScore(playerID, scoreToUpload, _leaderBoardID, (response) => 
        {
            if (response.success) {
                Debug.Log("Successfully uploaded score");
                done = true;
            } else {
                Debug.Log("Failed" + response.Error);
                done = true;
            }
        });
        
        _scoreToSubmit = 0;
        _playerNameInputField.text = "";
        yield return new WaitWhile(() => done == false);
    }

    private IEnumerator FetchTopHighScoresRoutine() {
        bool done = false;
        LootLockerSDKManager.GetScoreList(_leaderBoardID, 10, 0, (response) => {
            if (response.success) {
                string tempPlayerNames = "Names\n";
                string tempPlayerScores = "Scores\n";

                LootLockerLeaderboardMember[] members = response.items;

                for (int i = 0; i < members.Length; i++)
                {
                    tempPlayerNames += members[i].rank + ". ";

                    if (members[i].member_id != "") {
                        tempPlayerNames += members[i].member_id;
                    } 

                    tempPlayerScores += members[i].score + "\n";
                    tempPlayerNames += "\n";
                }
                done = true;
                _playerNames.text = tempPlayerNames;
                _playerScores.text = tempPlayerScores;
                Debug.Log("Successfully loaded player high scores.");
            } else {
                Debug.Log("Failed" + response.Error);
                done = true;
            }
        });
        yield return new WaitWhile(() => done == false);
    }

    private IEnumerator LoginRoutine()
    {
        bool done = false;
        LootLockerSDKManager.StartGuestSession((response) =>
        {
            if (response.success)
            {
                Debug.Log("Player was logged in");
                done = true;
            }
            else
            {
                Debug.Log("Could not start session");
                done = true;
            }
        });

        yield return new WaitWhile(() => done == false);
    }
}
