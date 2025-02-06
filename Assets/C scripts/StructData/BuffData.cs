using System;

public enum BuffType
{
    Vision, Swim,
}

[Serializable]
public class BuffBase
{
    public BuffType buff;
    public virtual void Update() { }
}