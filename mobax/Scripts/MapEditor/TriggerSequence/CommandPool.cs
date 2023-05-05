using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

public class Command
{
    public int ID;
    public string Name;
}

public class CommandPool : Single<CommandPool>
{
    int Sequence = 1;
    Dictionary<int,Command> Commands = new Dictionary<int,Command>();
    public int AddCommand (Command command)
    {
        var currentId = Sequence;
        command.ID = currentId;
        Commands.Add(currentId, command);
        Sequence++;
        return currentId;
    }

    public Command GetCommand (int id)
    {
        if (Commands.ContainsKey(id))
            return Commands[id];
        return null;
    }
    public void RemoveId (int id)
    {
        if (Commands.ContainsKey(id))
            Commands.Remove(id);
    }

    public void RemoveByName (string name)
    {
        var ids = new List<int>();
        foreach (var cmd in Commands.Values) {
            if (cmd.Name == name) {
                ids.Add(cmd.ID);
            }
        }
        foreach (var id in ids) {
            Commands.Remove(id);
        }
    }
    public Command GetByName(string name)
    {
        var ids = new List<int>();
        foreach (var cmd in Commands.Values) {
            if (cmd.Name == name) {
                ids.Add(cmd.ID);
            }
        }
        
        if (ids.Count > 0) {
            return GetCommand (ids[0]);
        }
        return null;
    }
}
