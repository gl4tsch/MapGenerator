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
    [SerializeField] TMP_InputField heightMapFrequency;
    [SerializeField] TMP_InputField heightLevelsInput;
    [SerializeField] Toggle sideCountToggle;
    [SerializeField] Toggle cellBorderToggle;

    void Start()
    {
        InitUiElements();

        newButton.onClick.AddListener(OnNewButtonClicked);
        refreshButton.onClick.AddListener(OnRefreshButtonClicked);
        seedInput.onEndEdit.AddListener(OnSeedInput);
        mapSizeInput.onEndEdit.AddListener(OnMapSizeInput);
        heightMapFrequency.onEndEdit.AddListener(OnHeightFrequencyInput);
        heightLevelsInput.onEndEdit.AddListener(OnHeightLevelInput);
        sideCountToggle.onValueChanged.AddListener(OnSideCountToggle);
        cellBorderToggle.onValueChanged.AddListener(OnCellBorderToggle);
    }

    void InitUiElements()
    {
        seedInput.SetTextWithoutNotify(mapGen.Seed.ToString());
        mapSizeInput.SetTextWithoutNotify(mapGen.MapSize.ToString());
        heightMapFrequency.SetTextWithoutNotify((mapGen.HeightMapFrequency * 10f).ToString());
        heightLevelsInput.SetTextWithoutNotify(mapGen.NumElevationLevels.ToString());
        sideCountToggle.SetIsOnWithoutNotify(mapGen.ShowNeighbourCounts);
        cellBorderToggle.SetIsOnWithoutNotify(mapGen.DrawCellBorders);
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
        if (mapGen.Seed == seed) return;

        mapGen.Seed = seed;
        mapGen.GenerateMap();
    }

    void OnMapSizeInput(string input)
    {
        float size = float.Parse(input);
        if (mapGen.MapSize == size) return;
        mapGen.MapSize = size;
        mapGen.GenerateMap();
    }

    void OnHeightFrequencyInput(string input)
    {
        float modifiedInput = float.Parse(input) / 10f;
        if (mapGen.HeightMapFrequency == modifiedInput) return;

        mapGen.HeightMapFrequency = modifiedInput;
        mapGen.GenerateMap();
    }

    void OnHeightLevelInput(string input)
    {
        int heightLevels = int.Parse(input);
        if (mapGen.NumElevationLevels == heightLevels) return;

        mapGen.NumElevationLevels = heightLevels;
        mapGen.GenerateMap();
    }

    void OnSideCountToggle(bool on)
    {
        mapGen.ShowNeighbourCounts = on;
    }

    void OnCellBorderToggle(bool on)
    {
        mapGen.DrawCellBorders = on;
    }
}
