using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FMLHT {

public class CommandRegisterer : MonoBehaviour
{
    public CommandVisual[] commands;

    void Start()
    {
        RegisterCommands();
    }

    void RegisterCommands() {
        foreach (var c in commands) {
            Console.Register(new Command() {
                name = c.name,
                arguments = c.argumentsQ,
                argumentsText = c.commentary,
                toClearAfter = true,
                toHideAfter = c.toHideAfter,
                action = (a) => {c.action.Invoke();}
            });
        }
    }

    [System.Serializable]
    public struct CommandVisual {
        public string name;
        public int argumentsQ;
        public string commentary;
        public bool toHideAfter;
        public UnityEvent action;
    }
}

}