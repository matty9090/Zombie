using UnityEngine.Events;

public enum EResource { Wood, Stone };

public class Resources
{
    public int Wood {
        get {
            return mWood;
        }

        set {
            mWood = value;
            ResourcesChangedEvent.Invoke();
        }
    }

    public int Stone {
        get {
            return mStone;
        }

        set {
            mStone = value;
            ResourcesChangedEvent.Invoke();
        }
    }

    private int mWood = 0;
    private int mStone = 0;

    public UnityEvent ResourcesChangedEvent = new UnityEvent();
}
