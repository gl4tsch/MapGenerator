using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_MapGen : MonoBehaviour
{
    [SerializeField] MapGenerator mapGen;

    [Header("UI References")]
    [SerializeField] TMP_InputField seedInput;
    [SerializeField] Button randomizeButton;

    private void Start()
    {
        seedInput.SetTextWithoutNotify(mapGen.Seed.ToString());

        seedInput.onEndEdit.AddListener(OnSeedInput);
        randomizeButton.onClick.AddListener(OnRandomizeButtonClicked);
    }

    void OnSeedInput(string input)
    {
        int seed = int.Parse(input);
        mapGen.Seed = seed;
        mapGen.GenerateNewMap();
    }

    void OnRandomizeButtonClicked()
    {
        seedInput.SetTextWithoutNotify(mapGen.RandomizeSeed().ToString());
        mapGen.GenerateNewMap();
    }
}
