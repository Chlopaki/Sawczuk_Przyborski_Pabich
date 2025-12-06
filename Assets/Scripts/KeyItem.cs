using UnityEngine;

// Definicja kluczy dostêpnych w grze
public enum KeyColor
{
    Red = 0,
    Green = 1,
    Blue = 2
}

public class KeyItem : MonoBehaviour
{
    // Konkretny kolor klucza w unity
    public KeyColor keyColor;
}