using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class IntroScreen : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI introText;
    [SerializeField] private string nextSceneName = "SampleScene";
    [SerializeField] private float autoSkipTime = 8f; // 几秒后自动跳过

    private float timer = 0f;
    private bool skipped = false;

    private readonly string content = 
    "达摩克利斯之剑 · The Sword of Damocles\n\n" +
    "古老传说中，君主头顶悬挂一把利剑，以一根细丝维系。\n" +
    "An ancient blade hung above a king — suspended by a single thread.\n\n" +
    "权力与荣耀之下，是随时降临的毁灭。\n" +
    "Beneath all glory lies the ever-present threat of ruin.\n\n" +
    "黑桃五 · Spade Five——\n" +
    "每一步都是赌注，每一次选择都在收紧那根细丝。\n" +
    "Every move is a wager. Every choice pulls the thread tighter.\n\n" +
    "五步之内，找到出路。否则，剑落。\n" +
    "Five steps. Find the way out. Or the sword falls.\n\n" +
    "[ 按任意键开始 · Press any key to begin ]";

    private void Start()
    {
        if (introText != null)
            introText.text = content;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (!skipped && (Input.anyKeyDown || timer >= autoSkipTime))
        {
            skipped = true;
            SceneManager.LoadScene(nextSceneName);
        }
    }
}