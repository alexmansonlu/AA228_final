// Inner classes to match the structure of the action received from the Python client
[System.Serializable]
public class ActionData
{
    public string actionType;
    public ActionDataDetails data;
}

[System.Serializable]
public class ActionDataDetails
{
    public int index;
    public int colorIndex;
}