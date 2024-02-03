using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BuffSystem : MonoBehaviour
{
    [SerializeField]
    List<UniversalEffect> effects = new List<UniversalEffect>();

    void Update()
    {
        if (effects.Count > 0)
            foreach (UniversalEffect effect in effects) 
        {
            if (effect.Depretiated)
            {
                effect.ReverseEffect();
            }
            else 
            {
                effect.Update();
            }
        }

        effects.RemoveAll(item => item.Depretiated);
    }

    private void FixedUpdate()
    {
        if(effects.Count > 0)
        foreach(UniversalEffect effect in effects) 
        {
            if (!effect.Depretiated)
                effect.FixedUpdate();
        }
    }

    public void AddEffect(UniversalEffect newEffect) 
    {
        UniversalEffect toAdd = newEffect;

        foreach(var effect in effects) 
        {
            if(effect.GetType() == newEffect.GetType()) // Если вновь добавляется тот же эффект - обновляем время.
            {
                effect.ReverseEffect();
                effect.Depretiated = true;
                toAdd = effect.MergeSimilar(newEffect);
            }
        }

        effects.Add(toAdd);
    }
}
