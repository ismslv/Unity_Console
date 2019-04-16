using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FMLHT {

public class CommandSwitch
{
    public string[] commands;
    Switch state;

    public CommandSwitch(string[] data) {
        commands = data;
        state = new Switch(data.Length - 1);
    }

    public void Switch() {
        state++;
    }

    public string Command {
        get {
            return commands[state.State];
        }
    }
}

public class Switch {
    public int State;
    int a;
    int b;
    public Switch(int a_, int b_) {
        State = a = a_;
        b = b_;
    }
    public Switch(Vector2Int v) {
        State = a = v.x;
        b = v.y;
    }
    public Switch(int b_) {
        State = a = 0;
        b = b_;
    }
    public Switch AddTo(int val) {
        State = State + val;
        if (State > b) {
            State = a;
        } else if (State < a) {
            State = b;
        }
        return this;
    }
    public void Add(int val) {
        State = State + val;
        if (State > b) {
            State = a;
        } else if (State < a) {
            State = b;
        }
    }
    public static Switch operator +(Switch obj, int val) {
        return obj.AddTo(val);
    }
    public static Switch operator -(Switch obj, int val) {
        return obj.AddTo(-val);
    }
    public static Switch operator ++(Switch obj) {
        return obj.AddTo(1);
    }
    public static Switch operator --(Switch obj) {
        return obj.AddTo(-1);
    }
}

}