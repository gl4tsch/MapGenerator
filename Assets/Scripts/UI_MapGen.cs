using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_MapGen : MonoBehaviour
{
    [SerializeField] MapGenerator mapGen;

    [Header("UI References")]
    [SerializeField] Button newButton;
    [SerializeField] Button refreshButton;
    [SerializeField] TMP_InputField seedInput;
    [SerializeField] TMP_InputField mapSizeInput;
    [SerializeField] TMP_InputField cellSizeInput;
    [SerializeField] Toggle sideCountToggle;

    void Start()
    {
        InitUiElements();

        newButton.onClick.AddListener(OnNewButtonClicked);
        refreshButton.onClick.AddListener(OnRefreshButtonClicked);
        seedInput.onEndEdit.AddListener(OnSeedInput);
        mapSizeInput.onEndEdit.AddListener(OnMapSizeInput);
        cellSizeInput.onEndEdit.AddListener(OnCellSizeInput);
        sideCountToggle.onValueChanged.AddListener(OnSideCountToggle);
    }

    void InitUiElements()
    {
        seedInput.SetTextWithoutNotify(mapGen.Seed.ToString());
        mapSizeInput.SetTextWithoutNotify(mapGen.MapSize.ToString());
        cellSizeInput.SetTextWithoutNotify(mapGen.PoissonDiscRadius.ToString());
        sideCountToggle.SetIsOnWithoutNotify(mapGen.ShowNeighbourCounts);
    }

    void OnNewButtonClicked()
    {
        seedInput.SetTextWithoutNotify(mapGen.RandomizeSeed().ToString());
        mapGen.GenerateMap();
    }

    void OnRefreshButtonClicked()
    {
        mapGen.GenerateMap();
    }

    void OnSeedInput(string input)
    {
        int seed = int.Parse(input);
        mapGen.Seed = seed;
        mapGen.GenerateMap();
    }

    void OnMapSizeInput(string input)
    {
        mapGen.MapSize = float.Parse(input);
        mapGen.GenerateMap();
    }

    void OnCellSizeInput(string input)
    {
        mapGen.PoissonDiscRadius = float.Parse(input);
    }

    void OnSideCountToggle(bool on)
    {
        mapGen.ShowNeighbourCounts = on;
    }
}
