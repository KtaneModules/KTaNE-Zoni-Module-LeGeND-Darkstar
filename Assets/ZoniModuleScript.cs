using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class ZoniModuleScript : MonoBehaviour
{
    public KMBombInfo BombInfo;
    public KMBombModule BombModule;
    public KMAudio Audio;
    public KMSelectable[] buttons;
    public TextMesh[] buttonLabels;
    public TextMesh textBox;
    public TextMesh stageTextBox;
    public string[] wordlist;

    static int moduleIdCounter = 1;
    private int moduleId;
    private int wordIndex = 0;
    private int solvedStages = 0;
    private bool moduleSolved;

    #region Twitch Plays
#pragma warning disable 414
    private readonly string TwitchHelpMessage = "Type '!{0} press 4', '!{0} submit 4' or '!{0} 4' to press the button at position 4. The buttons are ordered from 0-9.";
#pragma warning restore 414

    public KMSelectable[] ProcessTwitchCommand(string command)
    {
        var m = Regex.Match(command, @"^\s*(?:press|submit|)\s*([0-9])\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        return m.Success ? (new KMSelectable[] { buttons[m.Groups[1].Value[0] - '0'] }) : null;
    }
    #endregion

    #region Startup Voids
    void Awake()
    {
        moduleId = moduleIdCounter++;
    }

    protected void Start()
    {
        // Shuffle the buttons
        var shuffled = Enumerable.Range(0, 10).ToList().Shuffle();

        for (int i = 0; i < 10; i++)
        {
            buttonLabels[i].text = shuffled[i].ToString();
            buttons[i].OnInteract = AnsweredHandler(shuffled[i]);
        }
        PickWord();
    }

    void PickWord()
    {
        wordIndex = Random.Range(0, 54);
        textBox.text = wordlist[wordIndex];
        Debug.LogFormat("[Zoni #{0}] The displayed word is '{1}'.", moduleId, wordlist[wordIndex]);
    }
    #endregion

    #region Answer Voids
    void SolvedModule()
    {
        moduleSolved = true;
        Debug.LogFormat("[Zoni #{0}] Module disarmed! Well done Caretaker!", moduleId);
        GetComponent<KMBombModule>().HandlePass();
        textBox.text = "";
        Audio.PlaySoundAtTransform("solvesound", transform);
    }

    KMSelectable.OnInteractHandler AnsweredHandler(int buttonIndex)
    {
        return delegate
        {
            if (!moduleSolved)
            {
                GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
                buttons[buttonIndex].AddInteractionPunch();

                var correct = false;
                switch (buttonIndex)
                {
                    case 0: correct = wordIndex == 10 || wordIndex == 14 || wordIndex == 18 || wordIndex == 34 || wordIndex == 49; break;
                    case 1: correct = wordIndex == 0 || wordIndex == 8 || wordIndex == 13 || wordIndex == 23 || wordIndex == 26 || wordIndex == 37 || wordIndex == 48; break;
                    case 2: correct = wordIndex == 5 || wordIndex == 21 || wordIndex == 29 || wordIndex == 33 || wordIndex == 43; break;
                    case 3: correct = wordIndex == 9 || wordIndex == 12 || wordIndex == 28 || wordIndex == 35 || wordIndex == 51; break;
                    case 4: correct = wordIndex == 3 || wordIndex == 17 || wordIndex == 27 || wordIndex == 30 || wordIndex == 39 || wordIndex == 47 || wordIndex == 52; break;
                    case 5: correct = wordIndex == 1 || wordIndex == 22 || wordIndex == 41 || wordIndex == 50; break;
                    case 6: correct = wordIndex == 4 || wordIndex == 6 || wordIndex == 15 || wordIndex == 24 || wordIndex == 40 || wordIndex == 42; break;
                    case 7: correct = wordIndex == 2 || wordIndex == 7 || wordIndex == 19 || wordIndex == 36 || wordIndex == 44; break;
                    case 8: correct = wordIndex == 16 || wordIndex == 20 || wordIndex == 31 || wordIndex == 38 || wordIndex == 45 || wordIndex == 53; break;
                    case 9: correct = wordIndex == 11 || wordIndex == 25 || wordIndex == 32 || wordIndex == 46; break;
                }

                if (correct)
                {
                    Debug.LogFormat("[Zoni #{0}] Correctly pressed button {1}!", moduleId, buttonIndex);

                    solvedStages++;
                    stageTextBox.text = solvedStages.ToString();
                    Debug.LogFormat("[Zoni #{0}] Currently solved stages: {1}", moduleId, solvedStages);

                    if (solvedStages < 3)
                    {
                        Audio.PlaySoundAtTransform("stagecomplete", transform);
                        PickWord();
                    }
                    else
                    {
                        Audio.PlaySoundAtTransform("solvesound", transform);
                        SolvedModule();
                    }
                }
                else
                {
                    GetComponent<KMBombModule>().HandleStrike();
                    Debug.LogFormat("[Zoni #{0}] Strike! You pressed button {1}. That was incorrect.", moduleId, buttonIndex);
                    PickWord();
                }
            }
            return false;
        };
    }
    #endregion
}
