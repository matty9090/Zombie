using UnityEngine;
using UnityEngine.UI;

public class UnlockPopup : MonoBehaviour
{
    [SerializeField] Text ObjectName = null;
    [SerializeField] Text ObjectDesc = null;

    public string Name {
        set {
            ObjectName.text = value.ToUpper();
        }
    }

    public string Desc {
        set {
            ObjectDesc.text = value;
        }
    }

    private void Update()
    {

    }

    public void Close()
    {
        GetComponent<Animator>().SetTrigger("Close");
    }

    public void CloseFinished()
    {
        
    }
}
