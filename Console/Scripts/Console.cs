/* CONSOLE
 * V0.26
 * FMLHT, 28.03.2019
 */

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace FMLHT {

public class Console : MonoBehaviour {

    public static Console a;

    public KeyCode KeyToggle = KeyCode.Tab;
    new public string name = "FMLHT";
    public Canvas canvas;
    public bool searchForCanvas = true;
    public bool visibleFromStart = false;

    private GameObject body;
    private RectTransform bodyQ;
    private RectTransform bodyA;
    private RectTransform bodyB;
    private Text logB;
    private InputField input;
    private Text textA;
    private Text textSuggestion;

    private bool isVisible;
    private bool isAutocomplete;
    private string suggestion;

    private List<Command> commands = new List<Command>();
    private List<Watcher> watchers = new List<Watcher>();
    private List<string> history = new List<string>();
    private List<string> names = new List<string>();
    private int historyMarker = 0;
    private List<string> logList = new List<string>();

    private struct State
    {
        public bool cursorVisible;
        public CursorLockMode cursorMode;
    }
    private State stateSaved;
    private State stateConsole;

    public enum ActionType
    {
        OnShow,
        OnHide
    }
    
    public struct ConsoleAction
    {
        public ActionType type;
        public System.Action action;
    }

    public static List<ConsoleAction> actions = new List<ConsoleAction>();

    private void Awake()
    {
        if (Console.a == null) {
            a = this;

            if (canvas == null)
            {
                if (searchForCanvas)
                {
                    canvas = FindObjectOfType<Canvas>();
                }
                if (canvas == null || !searchForCanvas)
                {
                    GameObject c_ = Instantiate(Resources.Load("ConsoleCanvasEmpty", typeof(GameObject)) as GameObject);
                    canvas = c_.GetComponent<Canvas>();
                }
            }
            GameObject b = Instantiate(Resources.Load("ConsoleBody", typeof(GameObject)) as GameObject,
                canvas.transform);
            b.transform.SetAsLastSibling();
            body = b.transform.Find("Body").gameObject;
            bodyQ = body.transform.Find("Q").GetComponent<RectTransform>();
            textSuggestion = bodyQ.Find("Suggestion").GetComponent<Text>();
            bodyA = body.transform.Find("A").GetComponent<RectTransform>();
            textA = bodyA.Find("AnswerNest").Find("Answer").GetComponent<Text>();
            input = body.GetComponentInChildren<InputField>();
            bodyB = body.transform.Find("B").GetComponent<RectTransform>();
            logB = body.transform.Find("Log").GetComponent<Text>();

            bodyQ.Find("TitleQ").GetComponent<Text>().text = name + ", ";
            bodyA.Find("TitleA").GetComponent<Text>().text = name + ":";

            body.SetActive(false);
            bodyA.gameObject.SetActive(false);
            isVisible = false;
            isAutocomplete = true;

            this.transform.SetParent(null);

            RegisterDefaults();
        }
    }

    void Start () {
        if (visibleFromStart)
            StartCoroutine(DoAfter(() =>
            {
                Show();
            }));
	}
	
	void Update () {
		if (Input.GetKeyUp(KeyToggle))
        {
            isVisible = !isVisible;
            if (isVisible)
            {
                Show();
            } else
            {
                input.text = "";
                Hide();
            }
        }

        if (isVisible) {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (history.Count > 0)
                {
                    if (historyMarker <= history.Count - 1)
                    {
                        historyMarker++;
                        GoToHistoryMarker();
                    }
                }
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (history.Count > 0)
                {
                    if (historyMarker >= 2)
                    {
                        historyMarker--;
                        GoToHistoryMarker();
                    }
                }
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                if (isAutocomplete)
                {
                    if (suggestion != "")
                    {
                        string _s = suggestion + " ";
                        suggestion = FindCommandByName(suggestion).argumentsText;
                        isAutocomplete = false;
                        StartCoroutine(DoAfter(() =>
                        {
                            input.text = _s;
                            ShowSuggestion();
                            input.caretPosition = input.text.Length;
                        }));
                    }
                }
            }
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    if (isAutocomplete && suggestion != "")
                    {
                        input.text = suggestion;
                    }
                }
                if (input.text.Length > 0)
                {
                    string text_ = Regex.Replace(input.text, @"\t\n\r", "");
                    if (history.Count == 0 || text_ != history.Last())
                        history.Add(text_);
                    historyMarker = 0;
                    Process(text_);
                    input.text = "";
                    suggestion = "";
                    ShowSuggestion();
                    isAutocomplete = true;
                }
            }
            else if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Delete))
            {
                isAutocomplete = true;
                OnInputChange();
            }
            else if (Input.anyKeyDown)
            {
                OnInputChange();
            }
        }
        
        logList.Clear();
        foreach (var w in watchers)
        {
            if (!w.isHidden)
                logList.Add(w.ToString(true));
        }
        logB.text = string.Join("\n", logList.ToArray());
    }

    public void OnInputChange()
    {
        if (isAutocomplete)
        {
            suggestion = "";
            if (input.text.Length > 0)
            {
                var s_ = names.FindAll((s) => { return s.StartsWith(input.text); });
                if (s_.Count > 0)
                {
                    suggestion = s_[0];

                }
            }
            ShowSuggestion();
        } else
        {
            if (input.text.Length < 2)
            {
                suggestion = "";
                isAutocomplete = true;
            }
        }
    }

    void ShowSuggestion()
    {
        textSuggestion.text = suggestion;
    }

    void SelectInput()
    {
        input.Select();
        input.ActivateInputField();
    }

    void GoToHistoryMarker()
    {
        input.text = history[history.Count - historyMarker];
        SelectInput();
        StartCoroutine(DoAfter(() =>
        {
            input.caretPosition = input.text.Length;
        }));
    }

    void Show()
    {
        stateSaved = SnapState();
        LoadState(stateConsole);
        isVisible = true;
        suggestion = "";
        textSuggestion.text = "";
        isAutocomplete = true;
        body.SetActive(true);
        SelectInput();
        CallActionsOfType(ActionType.OnShow);
    }

    void Hide()
    {
        LoadState(stateSaved);
        isVisible = false;
        body.SetActive(false);
        bodyA.gameObject.SetActive(false);
        CallActionsOfType(ActionType.OnHide);
    }

    State SnapState()
    {
        return new State()
        {
            cursorVisible = Cursor.visible,
            cursorMode = Cursor.lockState
        };
    }

    void LoadState(State state)
    {
        Cursor.visible = state.cursorVisible;
        Cursor.lockState = state.cursorMode;
    }

    void AddButton(string label)
    {
        GameObject b = Instantiate(Resources.Load("ConsoleButton", typeof(GameObject)) as GameObject,
            bodyB.transform);
        Button bB = b.GetComponent<Button>();
        b.GetComponentInChildren<Text>().text = label;
    }

    public void Process (string data)
    {
        var dataAll = Regex.Matches(data, @"[\""|\'].+?[\""|\']|[^ ]+")
                .Cast<Match>()
                .Select(m => m.Value)
                .ToList();
        if (dataAll.Count > 0)
        {
            Command command = FindCommandByName(dataAll[0].ToLower().Trim());
            if (command != null)
            {
                if (dataAll.Count >= command.arguments + 1)
                {
                    if (command.action != null)
                    {
                        command.action(dataAll);
                    }
                    if (command.toClearAfter)
                    {
                        input.text = "";
                        SelectInput();
                    }
                    if (command.response != "")
                    {
                        Say(command.response);
                    }
                    if (command.toHideAfter)
                    {
                        Hide();
                    }
                } else
                {
                    Say("Command needs at least " + command.arguments + " arguments");
                    SelectInput();
                }
            } else
            {
                Say("Command " + dataAll[0] + " not recognized");
                SelectInput();
            }
        }
    }

    public static void Say(object what, bool forceShow = false)
    {
        Say(what.ToString(), forceShow);
    }

    public static void Say(string what, bool forceShow = false)
    {
        if (what[0] == '\"' || what[0] == '\'')
            what = what.Substring(1);
        if (what.Last() == '\"' || what.Last() == '\'')
            what = what.Substring(0, what.Length - 1);
        Console.a.textA.text = what;
        Console.a.bodyA.gameObject.SetActive(true);
        if (forceShow)
            Console.a.Show();
    }

    public static void Register(Command c)
    {
        Console.a.commands.Add(c);
        Console.a.names.Add(c.name);
    }

    public static void RegisterAction(ActionType type, System.Action action)
    {
        actions.Add(new ConsoleAction()
        {
            type = type,
            action = action
        });
    }

    public static void RegisterWatcher(Watcher watcher)
    {
        Console.a.watchers.Add(watcher);
    }
    
    void CallActionsOfType(ActionType type)
    {
        foreach(var a in actions)
        {
            if (a.type == type)
                a.action();
        }
    }

    Command FindCommandByName(string name)
    {
        return commands.Find((c) => { return (c.name == name); });
    }

    IEnumerator DoAfter(System.Action action)
    {
        yield return new WaitForFixedUpdate();
        action();
    }

    IEnumerator DoAfter(System.Action action, float time)
    {
        yield return new WaitForSecondsRealtime(time);
        action();
    }

    #region Defaults

    void RegisterDefaults()
    {
        stateConsole = new State()
        {
            cursorVisible = true,
            cursorMode = CursorLockMode.None
        };

        RegisterWatcher(new Watcher()
        {
            pattern = "<b>Time:</b> {0}s",
            isHidden = true,
            updater = () =>
            {
                return (Time.realtimeSinceStartup).ToString("0");
            }
        });

        RegisterWatcher(new Watcher()
        {
            pattern = "<b>Screen size:</b> {0}",
            isHidden = true,
            updater = () =>
            {
                return Screen.width + "x" + Screen.height;
            }
        });

        Register(new Command()
        {
            name = "say",
            argumentsText = "text or \"text text\"",
            arguments = 1,
            action = (s) => {
                Say(s[1]);
            }
        });

        Register(new Command()
        {
            name = "hide",
            toClearAfter = false,
            action = (s) =>
            {
                Hide();
            }
        });

        Register(new Command()
        {
            name = "quit",
            toClearAfter = false,
            action = (s) =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                UnityEngine.Application.Quit();
#endif
            }
        });

        Register(new Command()
        {
            name = "screenshot",
            argumentsText = "(filename)",
            action = (s) =>
            {
                string filename = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop)
                    + "\\" + (s.Count > 1 ? s[1] : "Screenshot") + ".png";
                body.SetActive(false);
                StartCoroutine(DoAfter(() =>
                {
                    ScreenCapture.CaptureScreenshot(filename);
                    StartCoroutine(DoAfter(() =>
                    {
                        Say(filename + " captured!", true);
                    }, 0.5f));
                }, 0.2f));
            }
        });

        Register(new Command()
        {
            name = "help",
            action = (s) =>
            {
                Say(System.String.Join(", ", names.ToArray()));
            }
        });

        Register(new Command()
        {
            name = "reload_scene",
            action = (s) =>
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            }
        });

        /* Register(new Command()
        {
            name = "save_watchers",
            action = (s) =>
            {
                string filename = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop)
                    + "\\" + Application.productName + "-" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "-watchers.txt";
                logList.Clear();
                foreach (var w in watchers)
                    logList.Add(w.ToString());
                File.WriteAllLines(filename, logList.ToArray());
                Say("Saved to " + filename);
            }
        }); */
    }

    #endregion
}

}