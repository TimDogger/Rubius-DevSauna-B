using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UserLogin : MonoBehaviourPunCallbacks
{
    public InputField nameField;
    public GameObject enterNamePanel;
    public Dropdown roleDropdown;
    public GameObject arCamera;

    private void Start()
    {
        enterNamePanel.SetActive(true);
        roleDropdown.options.Clear();
        var roles = Enum.GetValues(typeof(Roles)).Cast<Roles>().ToList();
        foreach (var role in roles)
        {
            Dropdown.OptionData newOption = new Dropdown.OptionData(role.ToString());
            newOption.text = role.ToString();
            roleDropdown.options.Add(newOption);
        }
        roleDropdown.RefreshShownValue();
    }
    public void OnNameButtonClicked()
    {
        switch ((Roles)roleDropdown.value)
        {
            case Roles.DISPATCHER:
                {
                    arCamera.SetActive(false);
                    break;
                }
            case Roles.OPERATOR:
                {
                    arCamera.SetActive(true);
                    break;
                }
        }
        string userNickname = nameField.text + " | " + (Roles) roleDropdown.value;
        gameObject.SendMessage("SetUserRole", roleDropdown.value);
        gameObject.SendMessage("ConnectToMasterLobby", userNickname);
        enterNamePanel.SetActive(false);
    }   

}
