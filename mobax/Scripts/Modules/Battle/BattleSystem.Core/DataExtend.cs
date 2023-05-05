using System.Collections.Generic;

public partial class SkillRow
{
    private int skillID = -1;
    public int SkillID
    {
        get
        {
            if (skillID >= 0)
            {
                return skillID;
            }
            return -1;
        }
        set { skillID = value; }
    }

    private List<int[]> _aff;

    public List<int[]> aff
    {
        get
        {
            if (_aff == null)
            {
                _aff = new List<int[]>();
            }
            return _aff;
        }
        set { }
    }
}

public partial class BuffRow
{
    private int buffID = -1;
    public int BuffID
    {
        get
        {
            if (buffID >= 0)
            {
                return buffID;
            }
            return -1;
        }
        set { buffID = value; }
    }
}