using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    private Character mCharacter;

    public void ProvideCharacter(Character character)
    {
        mCharacter = character;
        mCharacter.HealthChangedEvent.AddListener(HealthChanged);
    }

    void HealthChanged()
    {
        if (mCharacter)
        {
            Vector3 scale = transform.localScale;
            scale.x = (float)mCharacter.Health / (float)mCharacter.MaxHealth;
            transform.localScale = scale;
        }
    }
}
