using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class Command {
    public string name;
    public string argumentsText;
    public string response = "";
    public int arguments = 0;
    public bool toClearAfter = true;
    public bool toHideAfter = false;
    public System.Action<List<string>> action;
}

public class Watcher
{
    public string pattern;
    public bool isHidden = false;
    public System.Func<string> updater;

    public string ToString(bool rich = false)
    {
        string u = updater();
        string r = pattern.Replace("{0}", u);
        if (!rich)
            r = Regex.Replace(r, "<.*?>", string.Empty);
        return r;
    }
}