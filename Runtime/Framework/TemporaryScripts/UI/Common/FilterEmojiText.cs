using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
public class FilterEmojiText : MonoBehaviour {
    private InputField inputField;

    // Use this for initialization
    void Start () {
        inputField = GetComponent<InputField>();
        inputField.onValidateInput += delegate (string text, int charIndex, char addedChar)
        {
            return ValidateInput(text, charIndex, addedChar);
        };
    }

    char ValidateInput(string text, int charIndex, char addedChar)
    {
        if (char.GetUnicodeCategory(addedChar) == System.Globalization.UnicodeCategory.Surrogate)
        {
            return '\0';
        }
        return addedChar;
    }
}
