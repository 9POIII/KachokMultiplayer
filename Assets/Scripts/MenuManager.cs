using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class MenuManager : MonoBehaviourPunCallbacks
{
    public static MenuManager Instance;

    [SerializeField] private List<Menu> menus;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void OpenMenu(string menuName)
    {
        foreach (Menu menu in menus)
        {
            if (menu.menuName == menuName) menu.Open();
            else menu.Close();
        }
    }
}
