using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using UnityEngine.Networking;


public class CanvasController : MonoBehaviour
{
    private const int COUNT = 100;
    private const float CELLHEIGHT = 65.0f;
    public Text name;
    public Text score;
    public ToggleGroup tg;
    public GameObject startButton;
    public GameObject formPanel;
    public GameObject rankingPanel;
    public GameObject cells;
    public GameObject prefab;
    public Sprite[] sprites;
    float scaleFactor = 1.0f;

    public void OnSendClick()
    {
        StartCoroutine(PostConnect());
    }

    IEnumerator PostConnect()
    {
        Toggle tgl = tg.ActiveToggles().FirstOrDefault();

        WWWForm form = new WWWForm();
        form.AddField("name", name.text);
        form.AddField("score", score.text);
        form.AddField("sex", tgl.name == "Man" ? 0 : 1);
        form.AddField("count", COUNT);
        string url = "http://localhost:8080/ScoreAPI/GetRanking";

        Debug.Log("Confirmation to Link");
        UnityWebRequest uwr = UnityWebRequest.Post(url, form);
        yield return uwr.SendWebRequest();
        if (uwr.isNetworkError)
        {
            Debug.Log(uwr.error);
        }
        else
        {
            var ranking = JsonUtility.FromJson<RankingData>(uwr.downloadHandler.text);

            if (ranking.isRankingIn)
            {
                for(int i = 0; i < ranking.list.Count; i++)
                {
                    GameObject cell = Instantiate(prefab);
                    if (ranking.lastId == ranking.list[i].id)
                    {
                        Image panel = cell.GetComponent<Image>();
                        panel.color = Color.cyan;
                    }
                    cell.transform.SetParent(cells.transform, false);

                    Text rank = cell.transform.Find("Rank").GetComponent<Text>();

                    rank.text = (i + 1).ToString();

                    Image image = cell.transform.Find("Thumb").GetComponent<Image>();
                    image.sprite = ranking.list[i].sex == 0 ? sprites[0] : sprites[1];

                    Text name = cell.transform.Find("Name").GetComponent<Text>();
                    name.text = ranking.list[i].name;

                    Text scoreText = cell.transform.Find("Score").GetComponent<Text>();
                    scoreText.text = ranking.list[i].score.ToString();


                }
                rankingPanel.SetActive(true);

                if (ranking.rank > Screen.height / scaleFactor / CELLHEIGHT)
                {
                    Vector3 vec = cells.transform.localPosition;

                    float distination = ranking.rank * CELLHEIGHT - Mathf.Max(Screen.height / scaleFactor / 2, Screen.height / scaleFactor - (ranking.list.Count - ranking.rank) * CELLHEIGHT);
                    float diff = distination - vec.y;
                    while(diff> 0.1f)
                    {
                        vec = cells.transform.localPosition;
                        vec.y = Mathf.Lerp(vec.y, distination, 0.1f);
                        cells.transform.localPosition = vec;
                        diff = distination - vec.y;
                        yield return new WaitForSeconds(0.01f);


                    }
                }
            }
            formPanel.SetActive(false);
            startButton.SetActive(true);

        }
    }

}
[Serializable]
class RankingData
{
    public int lastId;
    public int rank;
    public List<Score> list;
    public bool isRankingIn;

}
[Serializable]
class Score
{
    public int id;
    public string name;
    public int score;
    public int sex;
}

